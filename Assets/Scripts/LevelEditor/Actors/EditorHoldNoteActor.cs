using Chroma.Editor;
using Chroma.Editor.Commands;
using Nova;
using UnityEngine;

/// <summary>
/// Classe repr�sentant un acteur de note dans l'�diteur.
/// </summary>
public class EditorHoldNoteActor : EditorNoteActor
{

    [SerializeField] internal  UIBlock2D trail;
    

    private bool _enTrainDeResizeTrail;

    /// <summary>
    /// Initialise cet acteur de note dans l'�diteur avec des donn�es de note sp�cifiques.
    /// </summary>
    /// <param name="data">Les donn�es de la note � utiliser pour l'initialisation.</param>
    /// <param name="alleyManager">Le gestionnaire d'all�es de l'�diteur.</param>
    /// <param name="editorTimeManager">Le gestionnaire du temps de l'�diteur.</param>
    /// <returns>Cet acteur de note initialis�.</returns>
    public override EditorNoteActor Initialiser(NoteData data, EditorAlleyManager alleyManager,
        EditorTimeManager editorTimeManager)
    {
        base.Initialiser(data as HoldNoteData, alleyManager, editorTimeManager);
        trail.Size.X.Raw = DureeToLongueur((data as HoldNoteData)!.duree);
        trail.Color = TrouverCouleurAllee();
        return this;
    }

    /// <summary>
    /// G�re les actions � effectuer lorsque cet acteur est activ�.
    /// </summary>
    private void OnEnable()
    {
        acteur.AddGestureHandler<Gesture.OnPress>(OnNotePress);
        
        acteur.AddGestureHandler<Gesture.OnClick>(OnClick);

        acteur.AddGestureHandler<Gesture.OnRelease>(OnNoteRelease);

        acteur.AddGestureHandler<Gesture.OnDrag>(OnDrag);

        trail.AddGestureHandler<Gesture.OnPress>(OnTrailPress);
        
        trail.AddGestureHandler<Gesture.OnClick>(OnTrailClick);

        trail.AddGestureHandler<Gesture.OnRelease>(OnTrailRelease);

        trail.AddGestureHandler<Gesture.OnDrag>(OnTrailDrag);
    }

    /// <summary>
    /// G�re les actions � effectuer lorsque cet acteur est d�sactiv�.
    /// </summary>
    private void OnDisable()
    {
        acteur.RemoveGestureHandler<Gesture.OnPress>(OnNotePress);
        
        acteur.RemoveGestureHandler<Gesture.OnClick>(OnClick);

        acteur.RemoveGestureHandler<Gesture.OnRelease>(OnNoteRelease);

        acteur.RemoveGestureHandler<Gesture.OnDrag>(OnDrag);

        trail.RemoveGestureHandler<Gesture.OnPress>(OnTrailPress);
        
        trail.RemoveGestureHandler<Gesture.OnClick>(OnTrailClick);

        trail.RemoveGestureHandler<Gesture.OnRelease>(OnTrailRelease);

        trail.RemoveGestureHandler<Gesture.OnDrag>(OnTrailDrag);

        if (_enTrainDeBouger)
        {
            _enTrainDeBouger = false;
        }
    }

    /// <summary>
    /// G�re les actions � effectuer lorsque la note est press�e.
    /// </summary>
    /// <param name="evt">L'�v�nement de pression de la note.</param>
    private void OnNotePress(Gesture.OnPress evt)
    {
        if (timeManager.isPlaying) return;
        _enTrainDeBouger = true;
        print("D�but du d�placement");
        originalAlleyData = alleesManager.GetAlleeData(acteur.Parent);
        originalTime = notedata.tempsDebut;
        evt.Consume();
    }

    /// <summary>
    /// G�re les actions � effectuer lorsque la note est rel�ch�e.
    /// </summary>
    /// <param name="evt">L'�v�nement de rel�chement de la note.</param>
    private void OnNoteRelease(Gesture.OnRelease evt)
    {
        if (!_enTrainDeBouger) return;
        
        float startTime = notedata.tempsDebut;
        float endTime = alleesManager.PositionToSecondes(evt.Target.Position.X.Raw);
        trail.Color = TrouverCouleurAllee();
        _enTrainDeBouger = false;
        
        alleesManager.MoveNotes(endTime - startTime, this);
        evt.Consume();
    }

    private float originalTrailSize;
    /// <summary>
    /// G�re les actions � effectuer lorsque la longueur de la note est press�e.
    /// </summary>
    /// <param name="evt">L'�v�nement de pression de la longueur de la note.</param>
    private void OnTrailPress(Gesture.OnPress evt)
    {
        evt.Consume();
        if (timeManager.isPlaying) return;
        _enTrainDeResizeTrail = true;
        originalTrailSize = trail.Size.X.Raw;
        print("D�but du redimensionnement, taille d'origine : " + trail.Size.X.Raw);
        print("Duree" + (notedata as HoldNoteData)!.duree);
        evt.Consume();
    }

    /// <summary>
    /// G�re les actions � effectuer lorsque la longueur de la note est rel�ch�e.
    /// </summary>
    /// <param name="evt">L'�v�nement de rel�chement de la longueur de la note.</param>
    private void OnTrailRelease(Gesture.OnRelease evt)
    {
        evt.Consume();
        if (!_enTrainDeResizeTrail) return;
        HoldNoteData data = notedata as HoldNoteData;
        float newTrailSize = trail.Size.X.Raw;
        ICommand command = new TrailChangedCommand(alleesManager , data , this , originalTrailSize , newTrailSize);
        NoteInvoker.AddCommand(command);
        evt.Consume();
    }

    /// <summary>
    /// G�re les actions � effectuer lorsque la longueur de la note est d�plac�e.
    /// </summary>
    /// <param name="evt">L'�v�nement de glissement de la longueur de la note.</param>
    private void OnTrailDrag(Gesture.OnDrag evt)
    {
        evt.Consume();
        if (!_enTrainDeResizeTrail) return;
        if (Input.GetKey(TouchePasDeSnap))
        {
            trail.Size.X.Raw = Mathf.Max(alleesManager.LongueurBeat, 
                (Camera.main!.WorldToViewportPoint(evt.PointerPositions.Current).x + VIEWPORT_SKEW) * 
                alleesManager.alleeParents.Size.X.Raw - acteur.Position.X.Raw);
        }
        else
        {
            trail.Size.X.Raw = Mathf.Max(alleesManager.LongueurBeat, 
                alleesManager.SnapX((Camera.main!.WorldToViewportPoint(evt.PointerPositions.Current).x + VIEWPORT_SKEW) * 
                alleesManager.alleeParents.Size.X.Raw) - acteur.Position.X.Raw);
        }
        evt.Consume();
    }

    private void OnTrailClick(Gesture.OnClick evt)
    {
        evt.Consume();
    }

    /// <summary>
    /// Convertit la longueur en dur�e.
    /// </summary>
    /// <param name="trail">La longueur de la note.</param>
    /// <returns>La dur�e correspondante.</returns>
    public float LongueurToDuree(UIBlock2D trail)
    {
        return EditorAlleyManager.BeatsToSecondes(trail.Size.X.Raw / alleesManager.LongueurBeat, EditorMapManager.MapData.MapVisualData.mapMetadonnees.bpm);
    }

    /// <summary>
    /// Convertit la dur�e en longueur.
    /// </summary>
    /// <param name="duree">La dur�e de la note.</param>
    /// <returns>La longueur correspondante.</returns>
    private float DureeToLongueur(float duree)
    {
        return EditorAlleyManager.SecondesToBeats(duree * alleesManager.LongueurBeat, EditorMapManager.MapData.MapVisualData.mapMetadonnees.bpm);
    }

    /// <summary>
    /// Trouve la couleur de la longueur de la note en fonction de l'all�e.
    /// </summary>
    /// <returns>La couleur de la longueur de la note.</returns>
    private Color TrouverCouleurAllee()
    {
        Color couleurAllee = alleesManager.alleesPourJoueur[notedata.joueur].allees[notedata.positionNote].Color;
        couleurAllee.r = Mathf.Clamp(couleurAllee.r - 0.5f, 0.1f, 1);
        couleurAllee.g = Mathf.Clamp(couleurAllee.g - 0.5f, 0.1f, 1);
        couleurAllee.b = Mathf.Clamp(couleurAllee.b - 0.5f, 0.1f, 1);
        return couleurAllee;
    }

    public void UpdateTrail()
    {
        trail.Size.X.Raw = DureeToLongueur((notedata as HoldNoteData)!.duree);
        trail.Color = TrouverCouleurAllee();
    }
}