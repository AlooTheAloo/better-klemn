using System;
using Chroma.Editor;
using Chroma.Editor.Commands;
using Nova;
using UnityEngine;

public class EditorClickNoteActor : EditorNoteActor
{
    private void OnEnable()
    {
        acteur.AddGestureHandler<Gesture.OnClick>(OnClick);

        acteur.AddGestureHandler<Gesture.OnPress>(OnPress);

        acteur.AddGestureHandler<Gesture.OnDrag>(OnDrag);

        acteur.AddGestureHandler<Gesture.OnRelease>(OnRelease);
    }

    private void OnDisable()
    {
        acteur.RemoveGestureHandler<Gesture.OnClick>(OnClick);

        acteur.RemoveGestureHandler<Gesture.OnPress>(OnPress);

        acteur.RemoveGestureHandler<Gesture.OnDrag>(OnDrag);

        acteur.RemoveGestureHandler<Gesture.OnRelease>(OnRelease);

        if (_enTrainDeBouger)
        {
            _enTrainDeBouger = false;
        }
    }

    
    private ushort joueur;
    
    
    private void OnPress(Gesture.OnPress evt)
    {
        if (timeManager.isPlaying)
            return;
        
        originalAlleyData = alleesManager.GetAlleeData(acteur.Parent);
        _enTrainDeBouger = true;
        evt.Consume();
    }


    private void OnRelease(Gesture.OnRelease evt)
    {
        if (!_enTrainDeBouger)
            return;
        UpdateNotePos(evt.Target.Position.X.Raw);
        evt.Consume();
    }

    private void UpdateNotePos(float position)
    {
        AlleeData? data = alleesManager.GetAlleeData(acteur.Parent);

        Debug.Log(data.HasValue);
        if (data.HasValue)
        {
            endAlleyData = data;
            float startTime = notedata.tempsDebut;
            float endTime = alleesManager.PositionToSecondes(position);
            alleesManager.MoveNotes(endTime - startTime, this);
        }
    }
}