using System;
using System.IO;
using System.Linq;
using Nova;
using UnityEngine;
using SFB;
using NovaSamples.UIControls;
using System.Reflection;
using UnityEngine.SceneManagement;

namespace Chroma.Editor
{
    public enum FichierRessource
    {
        Chanson,
        ArrierePlan,
        Couverture
    }

    [Serializable]
    public struct PanneauxSettings
    {
        [SerializeField] internal BoutonPanneau ressources;
        [SerializeField] internal BoutonPanneau metadonnees;
        [SerializeField] internal BoutonPanneau difficulte;
    }

    [Serializable]
    public struct BoutonPanneau
    {
        [SerializeField] internal UIBlock panneau;
        [SerializeField] internal UIBlock bouton;
    }

    [Serializable]
    public struct BoutonsDifficulte
    {
        [SerializeField] internal UIBlock[] difficulte;
        [SerializeField] internal UIBlock[] OD;
        [SerializeField] internal UIBlock[] AR;
        [SerializeField] internal UIBlock[] HP;
    }

    public class SettingsManager : MonoBehaviour
    {
        [SerializeField] internal UIBlock boutonSauvegarder;
        [SerializeField] internal UIBlock BPMPanel;
        [SerializeField] internal UIBlock mainPanel;
        [SerializeField] internal PanneauxSettings panneauxBoutons;
        [SerializeField] internal LumieresEditeur lumieresManager;

        internal static event Action<EditorMapSettings, EditorMapSettings> onSettingsChanged;
        internal static event Action<string> onSongUpdated;

        internal EditorMapSettings parametresPrecedents;
        internal EditorMapSettings parametresActuels;

        const string TEXTEDEFAUTFICHIER = "Téléverser...";
        [Header("Ressources refs")]
        [SerializeField] TextBlock texteChanson;
        [SerializeField] TextBlock texteVideo;
        [SerializeField] TextBlock texteArrierePlan;
        [SerializeField] TextBlock texteCouverture;

        [Header("Métadonnées refs")]
        [SerializeField] TextField field_titreChanson;
        [SerializeField] TextField field_artiste;
        [SerializeField] TextField field_mappeur;
        [SerializeField] TextField field_debut;
        [SerializeField] TextField field_fin;
        [SerializeField] TextField field_BPM;
        [SerializeField] TextField field_offset;

        [Header("Difficulté refs")]
        [SerializeField] BoutonsDifficulte boutonsDifficulte;
        [SerializeField] private Color couleurBoutonSelectionne;
        [SerializeField] private Color couleurBoutonDeselectionne;

        [HideInInspector] public bool panneauBPMActif { 
            get
            {
                return BPMPanel.gameObject.activeSelf;
            }
            set {
                throw new InvalidOperationException("Impossible de changer la valeur de 'panneauBPMActif' au runtime, puisqu'elle est décidée par l'état d'un objet dans la scène !");
            }
        }

        private void Awake()
        {
            var panneaux = typeof(PanneauxSettings).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Select(x => (BoutonPanneau)x.GetValue(panneauxBoutons));
            foreach (var panneau in panneaux)
            {
                panneau.bouton.GetComponent<Button>().OnClicked.AddListener(() =>
                {
                    if (!panneau.bouton.Color.Equals(Color.gray))
                        SetPanneauActif(panneau);
                });
            }

            CreerListenersBoutonsDiff();
            SetPanneauActif(panneauxBoutons.ressources);
        }


        private void OnEnable()
        {
            EditorMapManager.OnWriteFiles += SetSettings;
            TermineBPMFinderState.OnBpmFound += ArreterMesurerBPM;
            EditorHotkeys.saveChanges += Sauvegarder;
        }

        private void OnDisable()
        {
            EditorMapManager.OnWriteFiles -= SetSettings;
            TermineBPMFinderState.OnBpmFound -= ArreterMesurerBPM;
            EditorHotkeys.saveChanges -= Sauvegarder;
        }

        private void SetSettings(EditorMapSettings settings)
        {
            parametresPrecedents = settings;
            parametresActuels = settings;
        }

        private void CreerListenersBoutonsDiff()
        {
            // ... si seulement les pointeurs existaient :(

            foreach (var bouton in boutonsDifficulte.AR)
                bouton.AddGestureHandler<Gesture.OnClick>((evt) => {
                parametresActuels.difficulte.diff_velocite = 
                    (ushort) (boutonsDifficulte.AR.ToList().IndexOf(bouton) + 1); RafraichirUI(); });

            foreach (var bouton in boutonsDifficulte.OD)
                bouton.AddGestureHandler<Gesture.OnClick>((evt) => {
                    parametresActuels.difficulte.diff_precision = 
                    (ushort) (boutonsDifficulte.OD.ToList().IndexOf(bouton) + 1); RafraichirUI();
                });

            foreach (var bouton in boutonsDifficulte.HP)
                bouton.AddGestureHandler<Gesture.OnClick>((evt) => {
                    parametresActuels.difficulte.diff_vie = 
                    (ushort)(boutonsDifficulte.HP.ToList().IndexOf(bouton) + 1); RafraichirUI();
                });

            foreach (var bouton in boutonsDifficulte.difficulte)
                bouton.AddGestureHandler<Gesture.OnClick>((evt) => {
                    parametresActuels.difficulte.diff_generale = 
                    (ushort)(boutonsDifficulte.difficulte.ToList().IndexOf(bouton) + 1); RafraichirUI();
                });
        }

        private void Start()
        {
            InitialiseSettings();
        }

        public void InitialiseSettings() {
            Metadonnees meta = EditorMapManager.MapData.MapVisualData.mapMetadonnees;
            FichiersMap fichiers = meta.fichiers;
            //meta.ar = 1;
            //meta.od = 1;
            //meta.hp = 1;
            //meta.difficulte = 1;

            // Ressources
            parametresActuels.ressources.FromFileStruct(fichiers);

            // Metadata
            parametresActuels.metadata.FromMetadataStruct(meta);

            // Difficulté
            parametresActuels.difficulte.FromMetadataStruct(meta);

            parametresPrecedents = parametresActuels;

            RafraichirUI();
        }

        public void Sauvegarder()
        {
            if (boutonSauvegarder.Color == Color.gray) return;
            onSettingsChanged?.Invoke(parametresPrecedents, parametresActuels);
            parametresPrecedents = parametresActuels;
            
        }

        public void Reinitialiser()
        {
            parametresActuels = parametresPrecedents;
            RafraichirUI();
        }

        public bool VerifierNonNull(params string[] texts)
        {
            foreach(string text in texts) {
                if (string.IsNullOrEmpty(text)) return false;
            }
            return true;
        }

        public bool VerifierNonNull(params float?[] valeurs)
        {
            foreach (var valeur in valeurs)
            {
                if (valeur is null) 
                    return false;
            }
            return true;
        }

        public void RafraichirUI()
        {
            // Section fichiers
            texteArrierePlan.Text = Path.GetFileName(parametresActuels.ressources.cheminArrierePlan) ?? TEXTEDEFAUTFICHIER;
            texteChanson.Text = Path.GetFileName(parametresActuels.ressources.cheminChanson) ?? TEXTEDEFAUTFICHIER;
            texteVideo.Text = Path.GetFileName(parametresActuels.ressources.cheminVideo) ?? TEXTEDEFAUTFICHIER;
            texteCouverture.Text = Path.GetFileName(parametresActuels.ressources.cheminCouverture) ?? TEXTEDEFAUTFICHIER;

            // Section metadonnées
            field_titreChanson.Text = parametresActuels.metadata.titre;
            field_artiste.Text = parametresActuels.metadata.artiste;
            field_mappeur.Text = parametresActuels.metadata.mappeur;
            field_debut.Text = parametresActuels.metadata.debut.ToString();
            field_fin.Text = parametresActuels.metadata.fin.ToString();
            field_BPM.Text = parametresActuels.metadata.BPM.ToString();
            field_offset.Text = parametresActuels.metadata.offsetMenu.ToString();


            // Section difficulté
            var d = parametresActuels.difficulte;

            ActiverBoutonDifficulte(boutonsDifficulte.AR, boutonsDifficulte.AR[d.diff_velocite - 1]);
            ActiverBoutonDifficulte(boutonsDifficulte.OD, boutonsDifficulte.OD[d.diff_precision - 1]);
            ActiverBoutonDifficulte(boutonsDifficulte.difficulte, boutonsDifficulte.difficulte[d.diff_generale - 1]);
            ActiverBoutonDifficulte(boutonsDifficulte.HP, boutonsDifficulte.HP[d.diff_vie - 1]);

            bool ressources = VerifierRessources();
            bool metadonnees = VerifierMetadonnees();
            SetBoutonBarre(boutonSauvegarder, !(ressources && metadonnees));

        }

        private void ActiverBoutonDifficulte(UIBlock[] listeTarget, UIBlock bouton)
        {
            foreach(var btn in listeTarget)
            {
                btn.Color = couleurBoutonDeselectionne;
            }
            bouton.Color = couleurBoutonSelectionne;
        }


        private bool VerifierMetadonnees()
        {
            var m = parametresActuels.metadata;

            if (VerifierNonNull(m.titre, m.artiste, m.mappeur) && VerifierNonNull(m.debut, m.fin, m.BPM, m.offsetMenu))
            {
                SetBoutonBarre(panneauxBoutons.difficulte.bouton, false);
                return true;
            }
            else
            {
                SetBoutonBarre(panneauxBoutons.difficulte.bouton, true);
                return false;
            }
        }

        private bool VerifierRessources()
        {
            var r = parametresActuels.ressources;

            if (VerifierNonNull(r.cheminCouverture, r.cheminChanson, r.cheminArrierePlan))
            {
                SetBoutonBarre(panneauxBoutons.metadonnees.bouton, false);
                return true;
            }
            else
            {
                SetBoutonBarre(panneauxBoutons.metadonnees.bouton, true);
                SetBoutonBarre(panneauxBoutons.difficulte.bouton, true);
                return false;
            }
        }

        public void SetBoutonBarre(UIBlock boutton, bool estBarre)
        {
            if(!boutton.Color.Equals(couleurBoutonSelectionne))
               boutton.GetComponent<TextBlock>().Color = estBarre ? Color.gray : Color.white;
        }

        public void SetBoutonSelectionne(UIBlock boutonTarget, bool estSelectionne)
        {
            if (estSelectionne)
            {
                boutonTarget.Color = couleurBoutonSelectionne;
            }
            if (!estSelectionne && !boutonTarget.Color.Equals(Color.gray))
                boutonTarget.Color = Color.white;
            
        }

        public void SetPanneauActif(BoutonPanneau panneauActif)
        {
            BoutonPanneau[] panneaux = 
                typeof(PanneauxSettings)
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Select(x => (BoutonPanneau) x.GetValue(panneauxBoutons))
                .ToArray();
            
            foreach (var panneauInactif in panneaux)
            {
                panneauInactif.panneau.gameObject.SetActive(false);
                SetBoutonSelectionne(panneauInactif.bouton, false);
            }

            panneauActif.panneau.gameObject.SetActive(true);
            SetBoutonSelectionne(panneauActif.bouton, true);

        }

        public void SoumettreMetadonnees() {
            parametresActuels.metadata.titre = field_titreChanson.Text;
            parametresActuels.metadata.artiste = field_artiste.Text;
            parametresActuels.metadata.mappeur = field_mappeur.Text;

            float debut;
            if (float.TryParse(field_debut.Text, out debut))
            {
                parametresActuels.metadata.debut = debut;
            };

            float fin;
            if (float.TryParse(field_fin.Text, out fin))
            {
                parametresActuels.metadata.fin = fin;
            };

            float bpm;
            if (float.TryParse(field_BPM.Text, out bpm))
            {
                parametresActuels.metadata.BPM = bpm;
            };

            float offset;
            if (float.TryParse(field_offset.Text, out offset))
            {
                parametresActuels.metadata.offsetMenu = offset;
            };
            
            RafraichirUI();
        }

        public void QuitterEditeur()
        {
            MapCollection.i.ChargerMaps(() =>
            {
                SceneManager.LoadScene((int)Scenes.EDITOR_SONG_SELECT);
            });
        }

        public void MesurerBPM()
        {
            BPMPanel.gameObject.SetActive(true);
            mainPanel.gameObject.SetActive(false);
        }

        public void ArreterMesurerBPM(float BPM)
        {
            mainPanel.gameObject.SetActive(true);
            BPMPanel.gameObject.SetActive(false);
            parametresActuels.metadata.BPM = BPM;
            RafraichirUI();
        }


        public void TeleverserChanson()
        {
            string[] paths = TeleverserFichier(new ExtensionFilter("Chanson", "wav"));
            if (paths.Length > 0)
            {
                parametresActuels.ressources.cheminChanson = paths.First();
                onSongUpdated?.Invoke(paths.First());
            }
            RafraichirUI();
        }

        public void TeleverserVideo()
        {
            string[] paths = TeleverserFichier(new ExtensionFilter("Video", "mp4", "webm"));
            if (paths.Length > 0)
            {
                parametresActuels.ressources.cheminVideo = paths.First();
            }
            RafraichirUI();
        }

        public void TeleverserArrierePlan()
        {
            string[] paths = TeleverserFichier(new ExtensionFilter("Image d'arrière plan", "png", "jpeg", "jpg"));
            if (paths.Length > 0) parametresActuels.ressources.cheminArrierePlan = paths.First();
            RafraichirUI();
        }

        public void TeleverserCouverture()
        {
            string[] paths = TeleverserFichier(new ExtensionFilter("Image de couverture", "png", "jpeg", "jpg"));
            if (paths.Length > 0) parametresActuels.ressources.cheminCouverture = paths.First();

            print("Nouvelle couverture : " + parametresActuels.ressources.cheminCouverture);
            RafraichirUI();
        }


        public string[] TeleverserFichier(ExtensionFilter filtre)
        {
            var extensions = new[]
            {
                filtre
            };
            var paths = StandaloneFileBrowser.OpenFilePanel("Sélectionner un élément", "", extensions, false);
            return paths;
        }
    }

    public struct EditorMapSettings
    {
        public EditorRessources ressources;
        public EditorMetadata metadata;
        public EditorDifficulty difficulte;

    }

    public struct EditorRessources
    {
        public string cheminChanson;
        public string cheminVideo;
        public string cheminArrierePlan;
        public string cheminCouverture;

        public void FromFileStruct(FichiersMap fichiers)
        {
            cheminArrierePlan = fichiers.arrierePlan;
            cheminChanson = fichiers.audio;
            cheminVideo = fichiers.video;
            cheminCouverture = fichiers.imageCouverture;
        }
    }

    public struct EditorMetadata
    {
        public string titre;
        public string artiste;
        public string mappeur;
        public float? debut;
        public float? fin;
        public float? BPM;
        public float? offsetMenu;

        public void FromMetadataStruct(Metadonnees meta)
        {
            titre = meta.titre;
            artiste = meta.artiste;
            mappeur = meta.mappeur;
            debut = meta.duree.debut;
            fin = meta.duree.fin;
            BPM = meta.bpm;
            offsetMenu = meta.audioPreview;
        }
    }

    public struct EditorDifficulty
    {
        public ushort diff_generale;
        public ushort diff_precision;
        public ushort diff_velocite;
        public ushort diff_vie;


        public void FromMetadataStruct(Metadonnees meta)
        {
            diff_velocite = (ushort) meta.ar;
            diff_precision = (ushort) meta.od;
            diff_vie = (ushort) meta.hp;
            diff_generale = (ushort) meta.difficulte;
        }
    }
}