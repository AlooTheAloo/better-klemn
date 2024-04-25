using Nova;
using Shapes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chroma.Editor
{

    [Serializable]
    public struct VisualisateurPanneaux {
        [SerializeField] public UIBlock2D panneauAvantChargement;
        [SerializeField] public UIBlock2D panneauChargement;
        [SerializeField] public UIBlock2D panneauAffichage;
    }

    class VisualisateurConstantes
    {
        public static readonly float SIMULATION_FPS = 60;
        public static readonly float SIMULATION_TIME_DELTATIME = 1 / SIMULATION_FPS;
        public static readonly float SIMULATION_X_SCALE = 8.66f;
        public static readonly float SIMULATION_Y_SCALE = 8.89f;
        public static readonly float BAR_SCALE = -7.12f;

    }

    public sealed class VisualisateurLumieres : MonoBehaviour
    {
        [SerializeField] VisualisateurPanneaux panneaux;
        [SerializeField] private UIBlock2D panneauVisualisationLumieres;
        [SerializeField] private TextBlock nombreGroupes;
        [SerializeField] private TextBlock nombreEffets;
        [SerializeField] private TextBlock nombreObjets;
        [SerializeField] private EditorMapManager mapManager;
        [SerializeField] private CourbesSupportees courbes;
        [SerializeField] private TextBlock etatLoading;
        [SerializeField] private TextBlock compteurLoading;
        [SerializeField] private UIBlock2D fillLoading;

        [SerializeField] private UIBlock leftProjector;
        [SerializeField] private UIBlock rightProjector;
        [SerializeField] private GameObject ellipsePrefab;
        [SerializeField] private GameObject rectanglePrefab;

        [SerializeField] private UIBlock2D progressBar;
        [SerializeField] private TextBlock timeText;
        [SerializeField] private UIBlock2D fillBar;
        [SerializeField] private UIBlock2D playButton;
        [SerializeField] private UIBlock2D pauseButton;

        [SerializeField] private AudioSource sourceVisualisateur;

        List<RepresentationObjet> objets = new List<RepresentationObjet>();
        int shownFrame = 0;

        private void Start()
        {
            SetPanneauActif(panneaux.panneauAvantChargement);
        }

        private void OnEnable()
        {
            progressBar.AddGestureHandler<Gesture.OnPress>(OnSeekVisualisateur);
            progressBar.AddGestureHandler<Gesture.OnDrag>(OnDragVisualisateur);
        }

        private void OnDisable()
        {
            progressBar.RemoveGestureHandler<Gesture.OnPress>(OnSeekVisualisateur);
            progressBar.RemoveGestureHandler<Gesture.OnDrag>(OnDragVisualisateur);
        }


        public void Update()
        {
            if (sourceVisualisateur.isPlaying)
            {
                SetVisualisateurTime(sourceVisualisateur.time);
            }
        }

        public void TogglePlayPause()
        {
            if (sourceVisualisateur.isPlaying)
            {
                pauseButton.gameObject.SetActive(false);
                playButton.gameObject.SetActive(true);
                sourceVisualisateur.Pause();
            }
            else
            {
                pauseButton.gameObject.SetActive(true);
                playButton.gameObject.SetActive(false);
                sourceVisualisateur.Play();
            }
        }

        public void ResetVisualisateur()
        {
            SetPanneauActif(panneaux.panneauAvantChargement);
            StartCoroutine(DelayedDestroy());
        }

        IEnumerator DelayedDestroy()
        {
            yield return new WaitForSeconds(0.1f);

        }

        public void FermerVisualisateur()
        {
            panneauVisualisationLumieres.gameObject.SetActive(false);    
        }

        private void OnSeekVisualisateur(Gesture.OnPress evt)
        {
            float ratio = (evt.PointerWorldPosition.x / VisualisateurConstantes.BAR_SCALE - 1) / -2;
            sourceVisualisateur.time = (sourceVisualisateur.clip.length * ratio);
            SetVisualisateurTime(sourceVisualisateur.clip.length * ratio);
        }

        private void OnDragVisualisateur(Gesture.OnDrag evt)
        {
            float ratio = (evt.PointerPositions.Current.x / VisualisateurConstantes.BAR_SCALE - 1) / -2;
            sourceVisualisateur.time = Mathf.Clamp((sourceVisualisateur.clip.length * ratio), 0, sourceVisualisateur.clip.length);
            SetVisualisateurTime(sourceVisualisateur.clip.length * ratio);
        }


        private void SetVisualisateurTime(float time)
        {
            int frame = Mathf.FloorToInt(time / VisualisateurConstantes.SIMULATION_TIME_DELTATIME);
            if (shownFrame == frame) return;
            if (time >= sourceVisualisateur.clip.length || time < 0) return;

            float ratio = time / sourceVisualisateur.clip.length;
            fillBar.Size.X.Percent = ratio;
            timeText.Text = frame.ToString();
            LoadFrame(frame);
        }

        public void OuvrirPanneau()
        {
            var mapData = EditorMapManager.MapData;
            panneauVisualisationLumieres.gameObject.SetActive(true);
            nombreGroupes.Text = mapData.ListeGroupes.Count.ToString();
            nombreObjets.Text = mapData.ListeObjetLumieres.Count.ToString();
            nombreEffets.Text = (mapData.ListeEffets.Sum(x => x.EffetsLumineux.Count) +
                mapData.Notes.Sum(x => x.listeEffets.EffetsLumineux.Count)).ToString();
        }

        public void FermerPanneau()
        {
            panneauVisualisationLumieres.gameObject.SetActive(false);
        }

        public void ChargerRepresentation()
        {
            StartCoroutine(ChargerRepresentationRoutine());
            SetPanneauActif(panneaux.panneauChargement);
        }

        Dictionary<string, List<EffetLumineux>> effetsdict = new Dictionary<string, List<EffetLumineux>>();

        public void AjouterEffetAuDictionnaire(string key, EffetLumineux effet)
        {
            if (effetsdict.ContainsKey(key))
            {
                effetsdict[key].Add(effet);
            }
            else
            {
                effetsdict.Add(key, new List<EffetLumineux>() { effet });
            }
        }

        public IEnumerator ChargerRepresentationRoutine()
        {

            foreach (var objet in objets)
            {
                Destroy(objet.acteur.gameObject);
            }
            objets.Clear();

            effetsdict.Clear();
            compteurLoading.Text = "";
            etatLoading.Text = "Initialisation...";
            fillLoading.Size.X.Percent = 0;

            yield return null;

            // Effets "par défaut"
            foreach (var groupeEffets in EditorMapManager.MapData.ListeEffets)
            {
                foreach (var effetLumineux in groupeEffets.EffetsLumineux)
                {
                    foreach (var groupe in effetLumineux.GroupesCible)
                    {
                        var effetClone = effetLumineux.Clone();
                        effetClone.Decalage += groupeEffets.TempsDebut;
                        AjouterEffetAuDictionnaire(groupe, effetClone); // ...
                        yield return null;
                    }
                }
            }

            // Effets notes
            foreach (var note in EditorMapManager.MapData.Notes)
            {
                foreach (var effet in note.listeEffets.EffetsLumineux)
                {
                    foreach (var groupe in effet.GroupesCible)
                    {
                        var effetClone = effet.Clone();
                        effetClone.Decalage += note.tempsDebut;
                        AjouterEffetAuDictionnaire(groupe, effetClone);
                        yield return null;
                    }
                }
            }

            int i = 0;
            etatLoading.Text = "Compilation...";

            foreach (var item in EditorMapManager.MapData.ListeObjetLumieres)
            {
                compteurLoading.Text = $"{i} / {EditorMapManager.MapData.ListeObjetLumieres.Count}";
                fillLoading.Size.X.Percent = i / (float) EditorMapManager.MapData.ListeObjetLumieres.Count;

                List<EffetLumineux> effets = new();

                foreach (var groupe in item.lumiereData.groupes)
                {
                    if(effetsdict.ContainsKey(groupe))
                        effets.AddRange(effetsdict[groupe]);
                }

                var effetsTries = effets.Select(x =>
                {
                    return new Tuple<int, EffetLumineux>(Mathf.RoundToInt(x.Decalage / VisualisateurConstantes.SIMULATION_TIME_DELTATIME) + x.Order, x);
                }).OrderBy(item => item.Item1).ToList();


                RepresentationObjet obj = new RepresentationObjet(courbes);

                obj.CreerSimulationImages(
                    item,
                    new Queue<Tuple<int, EffetLumineux>>(effetsTries),
                    EditorMapManager.MapData.MapVisualData.mapMetadonnees.duree.dureeTemps
                );

                i++;
                objets.Add(obj);
                yield return null;
            }
            TermineChargement();
        }

        public void TermineChargement()
        {
            SetPanneauActif(panneaux.panneauAffichage);
            foreach(var obj in objets)
            {
                GameObject nouvelObjetLumineux = Instantiate(obj.packet.lumiereData.type == ShapeActorType.ELLIPSE ? ellipsePrefab : rectanglePrefab,
                    obj.packet.lumiereData.projectorID == 0 ? leftProjector.transform : rightProjector.transform);
                obj.InitialiserObjet(nouvelObjetLumineux);
            }
        }

        public void LoadFrame(int frame)
        {
            foreach(var obj in objets)
            {
                obj.LoadFrame(frame);
            }
        }

        public void SetPanneauActif(UIBlock2D panneau)
        {
            foreach (var fieldInfo in panneaux.GetType().GetFields()) {
                (fieldInfo.GetValue(panneaux) as UIBlock2D).gameObject.SetActive(false);
            }
            panneau.gameObject.SetActive(true);
        }
    }

    public sealed class RepresentationObjet
    {
        internal List<ObjectFrame> images;
        internal LightObjectInitiaialisePacket packet;
        internal UIBlock2D acteur;

        private Dictionary<Type, List<LightStateSimulation>> runningStates = new Dictionary<Type, List<LightStateSimulation>>();
        private ObjectFrame initialCondition;
        private CourbesSupportees courbes;

        public RepresentationObjet(CourbesSupportees courbes)
        {
            this.courbes = courbes;
            images = new List<ObjectFrame>();
        }

        private void EnleverEtatsDeType(LightState etat)
        {
            Type stateType = etat.GetType();
            if (!runningStates.ContainsKey(stateType)) return;
            runningStates[stateType].Clear();
        }

        private void EnleverEtat(LightStateSimulation etat)
        {
            Type stateType = etat.lightState.GetType();
            if (!runningStates.ContainsKey(stateType)) return;
            runningStates[stateType].Remove(etat);
        }


        private void AjouterEtat(LightStateSimulation etat)
        {
            Type stateType = etat.lightState.GetType();
            if (runningStates.ContainsKey(stateType))
            {
                runningStates[stateType].Add(etat);
            }
            else runningStates.Add(stateType, new List<LightStateSimulation>() { etat });
        }

        internal void AjouterEffetSurObjet(EffetLumineux effet, ObjectFrame firstFrame, int frameCnt)
        {
            foreach (var etat in effet.State.listeEtats)
            {
                if (!etat.actif) continue;
                if (etat.mesure == StateMeasurement.ABSOLUTE)
                {
                    EnleverEtatsDeType(etat); // Enlever tous les �tats actifs
                }

                if (etat is PositionLightState pls)
                    pls.originalPosition = firstFrame.position;

                if (etat is OpacityLightState ols)
                    ols.originalOpacity = firstFrame.opacity;

                if (etat is ScaleLightState sls)
                    sls.tailleOriginal = firstFrame.scale;

                if (etat is ColorLightState cls)
                    cls.initColor = firstFrame.color;

                AjouterEtat(new LightStateSimulation(etat, courbes.GetCourbeFromLabel(effet.CourbeAnimation), effet.Duree, firstFrame, frameCnt));
            }


        }

        internal void EnleverEtatsInvalides(int frame)
        {
            foreach (var etatAEnlever in runningStates.Values.SelectMany(list => list.Where(x => {
                
                return x.longueur <= (frame - x.initialFrame) * VisualisateurConstantes.SIMULATION_TIME_DELTATIME;
            }).ToList()))
            {
                EnleverEtat(etatAEnlever);
            }
        }

        internal void CreerSimulationImages(LightObjectInitiaialisePacket data, Queue<Tuple<int, EffetLumineux>> effets, float tempsMap)
        {
            

            initialCondition = ListeEtatVersObjectFrame(data.initialState.listeEtats);
            packet = data;
            for (int i = 0; i < VisualisateurConstantes.SIMULATION_FPS * tempsMap; i++) 
            {
                ObjectFrame ancienEtat = i == 0 ? initialCondition : images.Last();
                if (effets.Count > 0)
                {
                    while (effets.Peek().Item1 == i)
                    {
                        var effet = effets.Dequeue().Item2;
                        AjouterEffetSurObjet(effet, ancienEtat, i);
                        if (effets.Count == 0) break;
                    }
                }

                var nouvelEtat = CalculerEtatObjet(i, ancienEtat);

                images.Add(nouvelEtat);
                EnleverEtatsInvalides(i);

            }
        }


        internal ObjectFrame CalculerEtatObjet(int image, ObjectFrame previousFrame)
        {
            float d = VisualisateurConstantes.SIMULATION_TIME_DELTATIME;
            ObjectFrame currentObjectFrame = previousFrame.Clone();

            currentObjectFrame = GererPosition(image, d, currentObjectFrame);
            currentObjectFrame = GererOpacite(image, d, currentObjectFrame);
            currentObjectFrame = GererScale(image, d, currentObjectFrame);
            currentObjectFrame = GererCouleur(image, d, currentObjectFrame);

            return currentObjectFrame;
        }
        

        internal ObjectFrame GererPosition(int image, float d, ObjectFrame currentObjectFrame)
        {
            if (runningStates.TryGetValue(typeof(PositionLightState), out var listPosition))
            {
                listPosition.ForEach(x =>
                {
                    Vector2 delta = (x.lightState as PositionLightState).getDeltaAtFrame(d, (image - x.initialFrame) * d, x.longueur, x.courbe);
                    SetMeasureData(ref currentObjectFrame.position, delta, x.lightState.mesure);
                });
            }
            return currentObjectFrame;
        }

        internal ObjectFrame GererOpacite(int image, float d, ObjectFrame currentObjectFrame)
        {
            if (runningStates.TryGetValue(typeof(OpacityLightState), out var listOpacite))
            {
                listOpacite.ForEach(x =>
                {
                    float delta = (x.lightState as OpacityLightState).getDeltaAtFrame(d, (image - x.initialFrame) * d, x.longueur, x.courbe);
                    SetMeasureData(ref currentObjectFrame.opacity, delta, x.lightState.mesure);
                });
            }
            return currentObjectFrame;
        }

        internal ObjectFrame GererScale(int image, float d, ObjectFrame currentObjectFrame)
        {
            if (runningStates.TryGetValue(typeof(ScaleLightState), out var listScale))
            {
                listScale.ForEach(x =>
                {
                    Vector2 delta = (x.lightState as ScaleLightState).getDeltaAtFrame(d, (image - x.initialFrame) * d, x.longueur, x.courbe);
                    SetMeasureData(ref currentObjectFrame.scale, delta, x.lightState.mesure);
                });
            }
            return currentObjectFrame;
        }



        internal ObjectFrame GererCouleur(int image, float d, ObjectFrame currentObjectFrame)
        {
            if (runningStates.TryGetValue(typeof(ColorLightState), out var listScale))
            {
                listScale.ForEach(x =>
                {
                    currentObjectFrame.color = (x.lightState as ColorLightState).getDeltaAtFrame((image - x.initialFrame) * d, x.longueur, x.courbe);
                });
            }
            return currentObjectFrame;
        }

        private void SetMeasureData<T>(ref T dataTarget, T dataSource, StateMeasurement mesure)
        {
            if (mesure == StateMeasurement.ABSOLUTE)
            {
                dataTarget = dataSource;
            }
            else
            {
                dataTarget = Add(dataTarget, dataSource);
            }
        }

        public T Add<T>(T number1, T number2)
        {
            dynamic a = number1;
            dynamic b = number2;
            return a + b;
        }

        internal void LoadFrame(int frame)
        {
            SetDataOnObject(images[frame]);
        }


        internal void SetDataOnObject(ObjectFrame data)
        {
            acteur.Position.X.Raw = data.position.x * VisualisateurConstantes.SIMULATION_X_SCALE;
            acteur.Position.Y.Raw = data.position.y * VisualisateurConstantes.SIMULATION_Y_SCALE;

            acteur.Size.X.Raw = data.scale.x * VisualisateurConstantes.SIMULATION_X_SCALE;
            acteur.Size.Y.Raw = data.scale.y * VisualisateurConstantes.SIMULATION_Y_SCALE;

            acteur.Color = new Color(data.color.r, data.color.g, data.color.b, data.opacity);
        }

        internal void InitialiserObjet(GameObject nouvelObjetLumineux)
        {
            acteur = nouvelObjetLumineux.GetComponent<UIBlock2D>();
            SetDataOnObject(initialCondition);
        }


        internal ObjectFrame ListeEtatVersObjectFrame(LightState[] liste)
        {
            Vector2 position = Vector2.zero;
            Vector2 scale = Vector2.zero;
            Color couleur = Color.black;
            float opacite = 0;

            // Initialisation
            foreach (var etat in liste)
            {
                if (etat is PositionLightState pls) position = pls.targetPosition;
                if (etat is ScaleLightState sls) scale = sls.taille;
                if (etat is ColorLightState cls) couleur = cls.lightColor;
                if (etat is OpacityLightState ols) opacite = ols.opacity;
            }
            return new ObjectFrame(position, scale, couleur, opacite);
        }

    }

    internal struct LightStateSimulation
    {
        internal LightState lightState;
        internal AnimationCurve courbe;
        internal float longueur;
        internal ObjectFrame initialCondition;
        internal int initialFrame;

        internal LightStateSimulation(LightState lightstate, AnimationCurve courbe, float longueur, ObjectFrame initialCondition, int initialFrame)
        {
            this.lightState = lightstate;
            this.courbe = courbe;
            this.longueur = longueur;
            this.initialCondition = initialCondition;
            this.initialFrame = initialFrame;
        }
    }

    public struct ObjectFrame : ICloneable<ObjectFrame>
    {
        public Vector2 position;
        public Vector2 scale;
        public Color color;
        public float opacity;

        public ObjectFrame(Vector2 position, Vector2 scale, Color color, float opacity)
        {
            this.position = position;
            this.scale = scale;
            this.color = color;
            this.opacity = opacity;
        }

        public ObjectFrame Clone()
        {
            return new ObjectFrame(position, scale, color, opacity);
        }
    }

}
