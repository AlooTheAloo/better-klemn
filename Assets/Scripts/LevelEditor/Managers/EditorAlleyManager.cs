using System;
using System.Collections.Generic;
using System.Linq;
using Chroma.Editor.Commands;
using Lean.Pool;
using Nova;
using NovaSamples.UIControls;
using UnityEngine;

class CopiedNoteActor
{
    public EditorNoteActor acteur;
    public float timeDelta;

    public CopiedNoteActor(EditorNoteActor acteur, float timeDelta)
    {
        this.acteur = acteur;
        this.timeDelta = timeDelta;
    }
}

class CopiedEffetActor
{
    public EditorMapLightEffectActor acteur;
    public float timeDelta;

    public CopiedEffetActor(EditorMapLightEffectActor acteur, float timeDelta)
    {
        this.acteur = acteur;
        this.timeDelta = timeDelta;
    }
}

namespace Chroma.Editor
{
    [Serializable]
    public struct AllesPourJoueur
    {
        [SerializeField] public UIBlock2D[] allees;
    }

    /// <summary>
    /// Gestionnaire des allées pour l'éditeur.
    /// </summary>
    public class EditorAlleyManager : MonoBehaviour
    {
        private const float DefaultLongueurHoldNote = 5.0f;
        private const float ZoomIncrement = 10f;
        private const float ScrubIncrement = 25f;

        private const float MinZoom = 10;
        private const float MaxZoom = 400;

        public float LongueurBeat { get; private set; } = 40;


        [SerializeField] public AllesPourJoueur[] alleesPourJoueur;
        [SerializeField] internal UIBlock2D alleeParents;
        [SerializeField] private UIBlock2D ticksParent;
        [SerializeField] internal UIBlock2D effetsLumineuxParent;
        [SerializeField] internal UIBlock2D ligneJugement;

        private readonly List<UIBlock2D> _ticks = new();
        public readonly List<EditorNoteActor> _noteActors = new();
        private readonly List<EditorMapLightEffectActor> _mapEffects = new();

        internal Dictionary<NoteData, EditorNoteActor> NoteActorsSelected = new();
        private Dictionary<NoteData, CopiedNoteActor> _noteActorsCopied = new();

        internal List<NoteData> _notes = new();

        internal Dictionary<GroupeEffets, EditorMapLightEffectActor> _effetsMapSelectionnes = new();
        internal Dictionary<GroupeEffets, CopiedEffetActor> _effetsMapCopies = new();


        internal List<GroupeEffets> _mapGroupeEffets = new();

        [SerializeField] private LeanGameObjectPool holdEditorNotes;
        [SerializeField] private LeanGameObjectPool clickEditorNotes;
        [SerializeField] private LeanGameObjectPool ticksPool;
        [SerializeField] private LeanGameObjectPool mapEffectsPool;

        [SerializeField] private EditorTimeManager editorTimeManager;
        [SerializeField] private UIBlock2D creationPanel;
        [SerializeField] private Button noteEffectButton;
        [SerializeField] private LightEffectPannel lightEffectPannel;
        [SerializeField] private UIBlock2D[] deselectAllees;

        private float _positionMaxGauche;
        private float _positionMaxDroite;

        private bool _isEditingMapEffects = true;
        private int _mapGroupeEffetsIndex;

        private bool _allowScroll = true;

        private ICommand updateNotesCommand;
        private NoteInvoker _noteInvoker;
        private const float ClickNoteDuree = 0f;
        

        private bool ModificationEffetDisponible =>
            (NoteActorsSelected.Count == 1 && _effetsMapSelectionnes.Count == 0) ||
            (NoteActorsSelected.Count == 0 && _effetsMapSelectionnes.Count == 1);

        internal static event Action OnEffetsColles;


        private void OnEnable()
        {
            EditorTimeManager.OnTimeChanged += TempsChange;
            EditorNoteActor.OnSelectedChanged += OnNoteSelectedChanged;
            NoteCreationOptionsManager.OnClickNoteSelected += AddClickNote;
            NoteCreationOptionsManager.OnHoldNoteSelected += AddHoldNote;
            NoteCreationOptionsManager.OnGroupEffectSelected += AddLightEffect;
            EditorMapLightEffectActor.OnEffectSelected += OnEffetSelectionne;
            LightEffectPannel.OnLightPanelClose += FermerPanneauEffets;

            EditorHotkeys.deleteBtnPressed += DeleteSelectedActors;
            EditorHotkeys.selectAllNotes += SelectAllNotes;
            EditorHotkeys.deselectAllNotes += DeselectAllNotes;
            EditorHotkeys.spawnClickNote += AddClickNote;
            EditorHotkeys.spawnHoldNote += AddHoldNote;
            EditorHotkeys.zoomComboPressed += ModifyBeatLength;

            EditorHotkeys.undoChange += Undo;
            EditorHotkeys.redoChange += Execute;

            noteEffectButton.OnClicked.AddListener(ModifierEffetsNote);
            foreach (var deselectAllee in deselectAllees)
            {
                deselectAllee.AddGestureHandler<Gesture.OnRelease>(OnNoteAlleeRelease);
            }
            
            effetsLumineuxParent.AddGestureHandler<Gesture.OnClick>(OnEffetAlleeRelease);
        }

     
        private void OnDisable()
        {
            EditorTimeManager.OnTimeChanged -= TempsChange;
            EditorNoteActor.OnSelectedChanged -= OnNoteSelectedChanged;
            NoteCreationOptionsManager.OnClickNoteSelected -= AddClickNote;
            NoteCreationOptionsManager.OnHoldNoteSelected -= AddHoldNote;
            NoteCreationOptionsManager.OnGroupEffectSelected -= AddLightEffect;
            EditorMapLightEffectActor.OnEffectSelected -= OnEffetSelectionne;
            LightEffectPannel.OnLightPanelClose -= FermerPanneauEffets;

            EditorHotkeys.deleteBtnPressed -= DeleteSelectedActors;
            EditorHotkeys.selectAllNotes -= SelectAllNotes;
            EditorHotkeys.deselectAllNotes -= DeselectAllNotes;
            EditorHotkeys.spawnClickNote -= AddClickNote;
            EditorHotkeys.spawnHoldNote -= AddHoldNote;
            EditorHotkeys.zoomComboPressed -= ModifyBeatLength;

            EditorHotkeys.undoChange -= Undo;
            EditorHotkeys.redoChange -= Execute;

            noteEffectButton.OnClicked.RemoveListener(ModifierEffetsNote);
            foreach (var deselectAllee in deselectAllees)
            {
                deselectAllee.RemoveGestureHandler<Gesture.OnRelease>(OnNoteAlleeRelease);
            }
            effetsLumineuxParent.RemoveGestureHandler<Gesture.OnClick>(OnEffetAlleeRelease);

        }

        private void Start()
        {
            _noteInvoker = new NoteInvoker();
            _notes = EditorMapManager.MapData.Notes;
            _mapGroupeEffets = EditorMapManager.MapData.ListeEffets;
            UpdateNotes();
        }


        private void Execute() => _noteInvoker.Execute();
        private void Undo() => _noteInvoker.Undo();

        private void OnNoteAlleeRelease(Gesture.OnRelease evt)
        {
            DeselectAllNotes();
            evt.Consume();
        }

        private void OnEffetAlleeRelease(Gesture.OnClick evt)
        {
            DeselectAllEffets();
            _effetsMapSelectionnes.Clear();
            evt.Consume();
        }

        private void DeselectAllEffets()
        {
            foreach (var kvp in _effetsMapSelectionnes)
            {
                kvp.Value.Deselectionner();
            }
        }
        
        private void ModifyBeatLength(int variation)
        {
            SetLongueurBeat(Mathf.Clamp(LongueurBeat + ZoomIncrement * variation, MinZoom,
                MaxZoom));
            Debug.Log(LongueurBeat);
        }

        private void CopyNotesSelected()
        {
            _noteActorsCopied.Clear();

            if (NoteActorsSelected.Count == 0) return;
            float premier = NoteActorsSelected.Min(x => x.Value.notedata.tempsDebut);
            foreach (var kvp in NoteActorsSelected)
            {
                _noteActorsCopied.Add(kvp.Key, new(kvp.Value, kvp.Value.notedata.tempsDebut - premier));
            }
            

        }

        private void PasteNotesSelected()
        {
            foreach (var kvp in _noteActorsCopied)
            {
                var notedata = kvp.Key;
                if (kvp.Value.acteur is EditorClickNoteActor)
                {
                    AddClickNoteFromCopy(
                        editorTimeManager.editorTime + kvp.Value.timeDelta,
                        (ushort)(notedata.joueur + 1),
                        (ushort)(notedata.positionNote + 1),
                        notedata.listeEffets.Clone()
                    );
                }
                else
                {
                    AddHoldNoteFromCopy(
                        editorTimeManager.editorTime + kvp.Value.timeDelta,
                        (ushort)(notedata.joueur + 1),
                        (ushort)(notedata.positionNote + 1),
                        (notedata as HoldNoteData)!.duree,
                        notedata.listeEffets.Clone()
                    );
                }

                OnNoteSelectedChanged(false, true, kvp.Value.acteur);
            }

        }

        private void OnNoteSelectedChanged(bool clicked, bool deselectAllNotes, EditorNoteActor actor)
        {
            if (clicked)
            {
                if (deselectAllNotes) DeselectAllNotes();
                
                if (NoteActorsSelected.TryAdd(actor.notedata, actor)) // si la note est ajouté correctement
                {
                    actor.acteur.Color = Color.gray; // on change sa couleur en gris
                }
            }
            else
            {
                NoteActorsSelected.Remove(actor.notedata);
                if (deselectAllNotes) DeselectAllNotes();
                actor.acteur.Color = Color.white;
            }

            noteEffectButton.gameObject.SetActive(ModificationEffetDisponible);
        }

        private void OnEffetSelectionne(bool selectionne, GroupeEffets groupeEffets,
            EditorMapLightEffectActor lightEffectActor, bool isLeftShiftPressed)
        {
            
            if (!_effetsMapSelectionnes.ContainsValue(lightEffectActor)  && !isLeftShiftPressed)
            {
                foreach (var effet in _effetsMapSelectionnes.Values)
                {
                    effet.Deselectionner();
                }
                _effetsMapSelectionnes.Clear();
            }

            if (selectionne)
            {
                _effetsMapSelectionnes.TryAdd(groupeEffets, lightEffectActor);
            }
            else
            {
                _effetsMapSelectionnes.Remove(groupeEffets);
            }

            noteEffectButton.gameObject.SetActive(ModificationEffetDisponible);
            
            UpdateNotes();
        }

        private void CopierEffetsSelectionnes()
        {
            _effetsMapCopies.Clear();
            
            if (_effetsMapSelectionnes.Count == 0) return;
            var premier = _effetsMapSelectionnes.Min(x => x.Value.GroupeEffets.TempsDebut);
            foreach (var kvp in _effetsMapSelectionnes)
            {
                _effetsMapCopies.Add(kvp.Key, new(kvp.Value, kvp.Value.GroupeEffets.TempsDebut - premier));
            }

            ShowDictionary(_effetsMapCopies);
        }

        private void ShowDictionary(Dictionary<GroupeEffets, CopiedEffetActor> dic)
        {
            print("BEGINNING PRINT");
            foreach (var kvp in dic)
            {
                print($"KEY : {kvp.Key.TempsDebut} VALUE : {kvp.Value.acteur.name}");
            }
            print("FINISHED");
            
        }

        private void ShowDictionary(Dictionary<GroupeEffets, EditorMapLightEffectActor> dic)
        {
            print("BEGINNING PRINT");
            foreach (var kvp in dic)
            {
                print($"KEY : {kvp.Key.TempsDebut} VALUE : {kvp.Value.acteur.name}");
            }
            print("FINISHED");
            
        }

        private void CollerEffetsSelectionnes()
        {
            foreach (var eft in _effetsMapSelectionnes.Values)
            {
                print("Hey fuckers i just deselected it :unbontp:");
                eft.Deselectionner();
            }
            
            _effetsMapSelectionnes.Clear();
            foreach (var kvp in _effetsMapCopies)
            {
                var groupeEffet = kvp.Key.Clone();
                groupeEffet.TempsDebut = editorTimeManager.editorTime + kvp.Value.timeDelta;

                AjouterEffetLumineuxCopie(groupeEffet);

                var actor = _mapEffects.FirstOrDefault(x => x.GroupeEffets == groupeEffet);
                if (actor != null)
                {
                    actor.Selectionner();
                }
                _effetsMapSelectionnes.TryAdd(groupeEffet, actor);
            }
            
            UpdateNotes();

        }

        private void TempsChange(float time)
        {
            UpdateNotes();
        }

        private void Update()
        {
            if (Input.mouseScrollDelta != Vector2.zero && _allowScroll)
            {
                if (Input.GetKey(KeyCode.LeftControl))
                    SetLongueurBeat(Mathf.Clamp(LongueurBeat + ZoomIncrement * Input.mouseScrollDelta.y, MinZoom,
                        MaxZoom));
                else
                    SetTemps(editorTimeManager.editorTime +
                             Input.mouseScrollDelta.y * ScrubIncrement * Rationelle(LongueurBeat));
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.C))
            {
                CopyNotesSelected();
                CopierEffetsSelectionnes();
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.V))
            {
                PasteNotesSelected();
                CollerEffetsSelectionnes();
            }
        }

        private void Awake()
        {
            _positionMaxGauche = -ligneJugement.Position.X.Raw - alleeParents.Size.X.Raw / 2;
            _positionMaxDroite = alleeParents.Size.X.Raw / 2 - ligneJugement.Position.X.Raw;
            noteEffectButton.gameObject.SetActive(false);
        }

        private void ModifierEffetsNote()
        {
            if (NoteActorsSelected.Count == 1)
            {
                lightEffectPannel.Initialiser(NoteActorsSelected.First().Key.listeEffets);
                _isEditingMapEffects = false;
            }
            else if (_effetsMapSelectionnes.Count == 1)
            {
                var effetSelectionne = _effetsMapSelectionnes.First().Key;
                lightEffectPannel.Initialiser(effetSelectionne);
                _mapGroupeEffetsIndex = _mapGroupeEffets.IndexOf(effetSelectionne);
                _isEditingMapEffects = true;
            }

            _allowScroll = false;
        }


        private void AddLightEffect()
        {
            ICommand command = new AddEffectCommand(this, new GroupeEffets(tempsDebut: editorTimeManager.editorTime));
            NoteInvoker.AddCommand(command);
        }


        private void FermerPanneauEffets()
        {
            var groupeEffets = lightEffectPannel.Terminer();
            if (_isEditingMapEffects)
            {
                _mapGroupeEffets[_mapGroupeEffetsIndex] = groupeEffets;
            }
            else
            {
                NoteActorsSelected.First().Key.listeEffets = groupeEffets;
            }

            _isEditingMapEffects = true;
            _allowScroll = true;
            UpdateNotes();
        }

        internal void DeselectAllNotes()
        {
            if (NoteActorsSelected.Count > 0)
            {
                NoteActorsSelected.Clear();
                for (int i = 0; i < _noteActors.Count; i++)
                {
                    _noteActors[i].SetClicked(false, false);
                    _noteActors[i].acteur.Color = Color.white;
                }
            }
        }

        private void SelectAllNotes()
        {
            NoteActorsSelected.Clear();
            foreach (var note in _noteActors)
            {
                NoteActorsSelected.Add(note.notedata, note);
                note.SetClicked(true, false);
            }

            Debug.Log(NoteActorsSelected.Count);
            UpdateNotes();
        }

        public void DeleteSelectedActors()
        {
            ICommand command = new DeleteCommand(this);
            NoteInvoker.AddCommand(command);
        }


        public void DisplayCreationPanel()
        {
            creationPanel.gameObject.SetActive(true);
        }

        private void AddClickNote()
        {
            ICommand command = new AddNoteCommand(this, editorTimeManager.editorTime, 1,
                1, ClickNoteDuree, new());
            NoteInvoker.AddCommand(command);
        }

        private void AddHoldNote()
        {
            ICommand command = new AddNoteCommand(this, editorTimeManager.editorTime, 2,
                1, DefaultLongueurHoldNote, new());
            NoteInvoker.AddCommand(command);
        }


        public void AddClickNoteFromCopy(float tempsDebut, ushort joueur, ushort positionNote,
            GroupeEffets listeEffets)
        {
            ICommand command = new AddNoteCommand(this, tempsDebut, joueur, positionNote, ClickNoteDuree, listeEffets);
            NoteInvoker.AddCommand(command);
        }

        public void AddHoldNoteFromCopy(float tempsDebut, ushort joueur, ushort positionNote, float duree,
            GroupeEffets listeEffets)
        {
            ICommand command = new AddNoteCommand(this, tempsDebut, joueur, positionNote, duree, listeEffets);
            NoteInvoker.AddCommand(command);

            UpdateNotes();
        }

        private void AjouterEffetLumineuxCopie(GroupeEffets groupeEffets)
        {
            print($"pasted ge with tempdebut : {groupeEffets.TempsDebut}");
            ICommand command = new AddEffectCommand(this, groupeEffets);
            NoteInvoker.AddCommand(command);
        }
        
        public static float BeatsToSecondes(float beats, float mapBpm, float bpmOffset = 0)
        {
            return beats * Constantes.SECONDES_DANS_MINUTE / mapBpm + bpmOffset;
        }

        public static float SecondesToBeats(float secondes, float bpm, float offset = 0)
        {
            return (secondes - offset) * bpm / Constantes.SECONDES_DANS_MINUTE;
        }

        public void SetLongueurBeat(float val)
        {
            LongueurBeat = val;
            UpdateNotes();
        }

        public void SetTemps(float val)
        {
            editorTimeManager.editorTime = val;
            UpdateNotes();
        }

        public float SnapX(float input)
        {
            // f(x) = [ (x) / s ] * s
            var premierDelta =
                SecondesToBeats(editorTimeManager.editorTime,
                    EditorMapManager.MapData.MapVisualData.mapMetadonnees.bpm) *
                -LongueurBeat; // Le beat le plus à gauche
            premierDelta += ligneJugement.Position.X.Raw;
            premierDelta %= LongueurBeat;

            var left = Mathf.Round(input / LongueurBeat) * LongueurBeat + premierDelta;
            var right = Mathf.Round(input / LongueurBeat) * LongueurBeat + (LongueurBeat + premierDelta);

            return Mathf.Abs(left - input) < Mathf.Abs(right - input) ? left : right;
        }

        public Transform SnapY(float input)
        {
            UIBlock2D candidat = null;
            float? plusPetitDelta = null;
            foreach (var alleeJoueur in alleesPourJoueur)
            foreach (var allee in alleeJoueur.allees)
            {
                var delta = Mathf.Abs(allee.transform.position.y - input);
                if (plusPetitDelta == null)
                {
                    plusPetitDelta = delta;
                    candidat = allee;
                }
                else if (delta < plusPetitDelta)
                {
                    candidat = allee;
                    plusPetitDelta = delta;
                }
            }

            return candidat!.transform;
        }

        internal float PositionToSecondes(float position)
        {
            var deltaPos = position - ligneJugement.Position.X.Raw;
            var deltaBeats = deltaPos / LongueurBeat;
            var deltaSecondes =
                BeatsToSecondes(deltaBeats, EditorMapManager.MapData.MapVisualData.mapMetadonnees.bpm);
            return deltaSecondes + editorTimeManager.editorTime;
        }

        /// <summary>
        /// Trouve la rationnelle c / x
        /// </summary>
        /// <param name="x">L'input de la fonction rationelle </param>
        /// <param name="constante">La dividente</param>
        /// <returns>f(x) où f = c / x</returns>
        private float Rationelle(float x, float constante = 1)
        {
            return constante / x;
        }

        /// <summary>
        /// Obtient la donnée de l'allée en fonction du bloc cible.
        /// </summary>
        /// <param name="targetAllee">Le bloc cible.</param>
        /// <returns>La donnée de l'allée correspondante, ou null si non trouvée.</returns>
        internal AlleeData? GetAlleeData(UIBlock targetAllee)
        {
            for (var joueur = 0; joueur < alleesPourJoueur.Length; joueur++)
            for (var allee = 0; allee < alleesPourJoueur[joueur].allees.Length; allee++)
                if (alleesPourJoueur[joueur].allees[allee] == targetAllee)
                    return new AlleeData(joueur, allee);

            return null;
        }

        internal int TransformToAlleeInt(Transform updatedTransform)
        {
            int index = 0;
            for (int i = 0; i < Constantes.NOMBRE_JOUEURS; i++)
            {
                for (int j = 0; j < alleesPourJoueur[i].allees.Length; j++)
                {
                    if (alleesPourJoueur[i].allees[j].transform == updatedTransform) return index;
                    index++;
                }
            }

            return 0;
        }

        internal Transform AlleeIntToTransform(int alleeInt)
        {
            int index = 0;
            for (int i = 0; i < Constantes.NOMBRE_JOUEURS; i++)
            {
                for (int j = 0; j < alleesPourJoueur[i].allees.Length; j++)
                {
                    if (index == alleeInt) return alleesPourJoueur[i].allees[j].transform;
                    index++;
                }
            }

            return null;
        }


        internal void MoveNotes(float delta, EditorNoteActor trigger)
        {
            var noteActors = new Dictionary<NoteData, EditorNoteActor>(NoteActorsSelected);
            if (!noteActors.ContainsValue(trigger))
            {
                // Je sais pas si c'est nécéssaire de créer un pointeur, mais je le fais quand même trololol
                var targetNote = _notes.First(x => trigger.notedata == x); 
                noteActors.Add(targetNote, trigger);
            }
            

            
            
            List<NoteData> noteDatas = new();
            List<float> startTimes = new();
            List<AlleeData?> newAlleys = new();
            List<AlleeData?> originalAlleys = new();
            foreach (var note in noteActors)
            {
                noteDatas.Add(note.Value.notedata);
                startTimes.Add(note.Value.notedata.tempsDebut);
                newAlleys.Add(GetAlleeData(note.Value.acteur.Parent));
                originalAlleys.Add(note.Value.originalAlleyData);
            }

            ICommand command =
                new MoveNoteCommand(this, noteDatas, startTimes, delta, originalAlleys, newAlleys);
            NoteInvoker.AddCommand(command);
        }

        public void UpdateNotes()
        {
            var bpm = EditorMapManager.MapData.MapVisualData.mapMetadonnees.bpm;
            var notesInView = _notes.Where(x =>
            {
                float deltaBeats;
                if (x is HoldNoteData holdNoteData)
                {
                    deltaBeats = SecondesToBeats(x.tempsDebut + holdNoteData.duree
                                                 - editorTimeManager.editorTime, bpm);
                    return deltaBeats * LongueurBeat > _positionMaxGauche &&
                           deltaBeats * LongueurBeat - SecondesToBeats(holdNoteData.duree, bpm) * LongueurBeat <
                           _positionMaxDroite;
                }

                deltaBeats = SecondesToBeats(x.tempsDebut - editorTimeManager.editorTime, bpm);
                return deltaBeats * LongueurBeat > _positionMaxGauche &&
                       deltaBeats * LongueurBeat < _positionMaxDroite;
            }).ToList();

            foreach (var note in notesInView)
                if (!_noteActors.Select(x => x.notedata).Contains(note))
                {
                    var positionNote = alleesPourJoueur[note.joueur].allees[note.positionNote].transform;
                    _noteActors.Add(
                        (note is HoldNoteData ? holdEditorNotes : clickEditorNotes)
                        .Spawn(positionNote)
                        .GetComponent<EditorNoteActor>()
                        .Initialiser(note, this, editorTimeManager)
                    );
                }


            List<EditorNoteActor> objetsAEnlever = new();
            foreach (var note in _noteActors)
                if (notesInView.Contains(note.notedata))
                {
                    var deltaBeats = SecondesToBeats(note.notedata.tempsDebut - editorTimeManager.editorTime, bpm);

                    Transform parent = alleesPourJoueur[note.notedata.joueur].allees[note.notedata.positionNote]
                        .transform;

                    Transform enfant = note.acteur.transform;
                    enfant.SetParent(parent);
                    Vector3 pos = enfant.localPosition;
                    enfant.localPosition = new Vector3(pos.x, 0, pos.z);
                    note.acteur.Position.X.Raw = ligneJugement.Position.X.Raw + deltaBeats * LongueurBeat;
                    if (note is EditorHoldNoteActor editorNoteActor) editorNoteActor.UpdateTrail();

                    if (NoteActorsSelected.ContainsKey(note.notedata))
                    {
                        note.acteur.Color = Color.gray;
                    }
                    else
                    {
                        note.acteur.Color = Color.white;
                    }
                }
                else
                {
                    objetsAEnlever.Add(note);
                    LeanPool.Despawn(note);
                }

            var effetsInView = _mapGroupeEffets.Where(x =>
            {
                float deltaBeats;
                deltaBeats = SecondesToBeats(x.TempsDebut - editorTimeManager.editorTime, bpm);
                return deltaBeats * LongueurBeat > _positionMaxGauche &&
                       deltaBeats * LongueurBeat < _positionMaxDroite;
            }).ToList();

            foreach (var effet in effetsInView)
                if (!_mapEffects.Select(x => x.GroupeEffets).Contains(effet))
                {
                    _mapEffects.Add(
                        mapEffectsPool.Spawn(effetsLumineuxParent.transform)
                            .GetComponent<EditorMapLightEffectActor>()
                            .Initialiser(effet, this, editorTimeManager, _effetsMapSelectionnes.ContainsKey(effet)));
                }


            foreach (var note in objetsAEnlever) _noteActors.Remove(note);

            List<EditorMapLightEffectActor> effetsAEnlever = new();
            foreach (var effet in _mapEffects)
                if (effetsInView.Contains(effet.GroupeEffets))
                {
                    var deltaBeats = SecondesToBeats(effet.GroupeEffets.TempsDebut - editorTimeManager.editorTime, bpm);
                    effet.acteur.Position.X.Raw = ligneJugement.Position.X.Raw + deltaBeats * LongueurBeat;
                    effet.RefreshText();
                }
                else
                {
                    effetsAEnlever.Add(effet);
                    LeanPool.Despawn(effet);
                }

            foreach (var effet in effetsAEnlever)
            {
                effet.Deselectionner();
                _mapEffects.Remove(effet);
            }


            var beatASpawner =
                SecondesToBeats(editorTimeManager.editorTime, bpm) * -LongueurBeat; // Le beat le plus a gauche

            beatASpawner %= LongueurBeat;
            beatASpawner += ligneJugement.Position.X.Raw;

            while (beatASpawner > ligneJugement.Position.X.Raw + _positionMaxGauche) beatASpawner -= LongueurBeat;

            var i = 0;

            while (beatASpawner < _positionMaxDroite + ligneJugement.Position.X.Raw)
            {
                UIBlock2D tick;
                if (_ticks.Count > i)
                {
                    tick = _ticks[i];
                }
                else
                {
                    tick = ticksPool.Spawn(ticksParent.transform).GetComponent<UIBlock2D>();
                    _ticks.Add(tick);
                }

                tick.Position.X.Raw = beatASpawner;
                beatASpawner += LongueurBeat;
                i++;
            }

            while (i < _ticks.Count)
            {
                LeanPool.Despawn(_ticks[i]);
                _ticks.RemoveAt(i);
            }
        }

        private void OnApplicationQuit()
        {
            NoteInvoker._commandList.Clear();
            NoteInvoker._reverseHistory.Clear();
        }
    }

    /// <summary>
    /// Structure de données pour représenter une allée.
    /// </summary>
    public struct AlleeData
    {
        public readonly int Joueur;
        public readonly int Position;

        /// <summary>
        /// Initialise une nouvelle instance de la structure AlleeData.
        /// </summary>
        /// <param name="joueur">Le numéro du joueur.</param>
        /// <param name="position">La position de l'allée.</param>
        public AlleeData(int joueur, int position)
        {
            Joueur = joueur;
            Position = position;
        }
    }
}