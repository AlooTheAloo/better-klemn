using Chroma.Editor;
using Nova;
using System;
using System.Collections.Generic;
using Microsoft.Win32;
using UnityEngine;

public abstract class NoteActor : MonoBehaviour
{
    public NoteData notedata;
}

public abstract class GameNoteActor : NoteActor
{
    public bool enJeu = true;

    private void OnEnable()
    {
        enJeu = true;
    }

    public abstract void OnMiss(Alley alley, float vitesseMovement);
}

public abstract class EditorNoteActor : NoteActor
{
    
    public float originalTime;
    public AlleeData? endAlleyData;
    public AlleeData? originalAlleyData;
    
    
    protected EditorTimeManager timeManager;
    protected EditorAlleyManager alleesManager;
    protected const KeyCode TouchePasDeSnap = KeyCode.LeftControl;

    protected const float VIEWPORT_Y_MAX = 7;
    protected const float VIEWPORT_Y_MIN = -5;
    protected const float VIEWPORT_SKEW = -0.5f;

    public static event Action<bool, bool, EditorNoteActor> OnSelectedChanged;
    public bool clicked
    {
        get { return _clicked; }
        private set
        {
            _clicked = value;
        }
    }
    
    public void SetClicked(bool clicked, bool deselectNotes = true){
        OnSelectedChanged?.Invoke(clicked, deselectNotes, this);
        _clicked = clicked;
    }
    
    
    

    private bool _clicked; // ancienne valeur

    public bool _enTrainDeBouger;

    public UIBlock2D acteur;

    public virtual EditorNoteActor Initialiser(
        NoteData data,
        EditorAlleyManager alleyManager,
        EditorTimeManager editorTimeManager
    )
    {
        timeManager = editorTimeManager;
        alleesManager = alleyManager;
        notedata = data;
        return this;
    }

    protected float CalculerTargetX(float targetX, float actorPosition)
    {
        if (Input.GetKey(TouchePasDeSnap))
        {
            return targetX - actorPosition;
        }
        return alleesManager.SnapX(targetX) - actorPosition;
    }

    protected Transform CalculerTargetY(float targetY)
    {
        Transform alleePlusProche = alleesManager.SnapY(targetY);
        if (alleePlusProche != null)
        {
            return alleePlusProche;
        }
        return null;
    }

    protected void OnClick(Gesture.OnClick evt)
    {
        if (timeManager.isPlaying)
            return;

        bool newvalue = !(clicked && Input.GetKey(KeyCode.LeftShift));
        SetClicked(newvalue, !Input.GetKey(KeyCode.LeftShift));
        evt.Consume();
    }
    protected void OnDrag(Gesture.OnDrag evt)
    {
        if (!_enTrainDeBouger)
            return;


        var deltaX = CalculerTargetX(
            (Camera.main!.WorldToViewportPoint(evt.PointerPositions.Current).x + VIEWPORT_SKEW)
                * alleesManager.alleeParents.Size.X.Raw,
            acteur.Position.X.Raw
        );
        var mouseTargetY = CalculerTargetY(
               Mathf.Lerp(
                   VIEWPORT_Y_MIN,
                   VIEWPORT_Y_MAX,
                   Camera.main.ScreenToViewportPoint(Input.mousePosition).y
               )
           );
        int targetAlleeInt = alleesManager.TransformToAlleeInt(mouseTargetY);

        var currentPositionNote = alleesManager.alleesPourJoueur[notedata.joueur].allees[notedata.positionNote].transform;
        var currentAlleeInt = alleesManager.TransformToAlleeInt(currentPositionNote);

        var deltaY = targetAlleeInt - currentAlleeInt;

        var noteActorsABouger = new Dictionary<NoteData, EditorNoteActor>(alleesManager.NoteActorsSelected);
        if (!noteActorsABouger.ContainsValue(this))
        {
            noteActorsABouger.Add(this.notedata, this);
        }
        
        foreach (var kvp in noteActorsABouger) // l'appliquer aux notes select
        {
            kvp.Value.acteur.Position.X.Raw += deltaX;
            var transform = alleesManager.alleesPourJoueur[kvp.Value.notedata.joueur].allees[kvp.Value.notedata.positionNote].transform;
            currentAlleeInt = alleesManager.TransformToAlleeInt(transform);
            var newPosNoteSelected = currentAlleeInt + deltaY;
            kvp.Value.acteur.transform.SetParent(alleesManager.AlleeIntToTransform(
                    Mathf.Clamp(newPosNoteSelected, 0, Constantes.NOMBRE_TOUCHES * Constantes.NOMBRE_JOUEURS - 1)));
            kvp.Value.acteur.Position.Y.Raw = 0;
        }
        
        evt.Consume();
    }
}