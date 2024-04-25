using System.Collections;
using System.Collections.Generic;
using Chroma.Editor;
using UnityEngine;

public class TrailChangedCommand : ICommand
{
    private EditorAlleyManager _alleyManager;
    private HoldNoteData _holdNoteData;
    private EditorHoldNoteActor _holdNoteActor;
    private float _originalTrailSize;
    private float _newTrailSize;
    public TrailChangedCommand(EditorAlleyManager alleyManager , HoldNoteData holdNoteData ,
        EditorHoldNoteActor holdNoteActor , float originalTrailSize , float newTrailSize)
    {
        _alleyManager = alleyManager;
        _holdNoteData = holdNoteData;
        _holdNoteActor = holdNoteActor;
        _originalTrailSize = originalTrailSize;
        _newTrailSize = newTrailSize;
    }
    public void Execute()
    {
        _holdNoteActor.trail.Size.X.Raw = _newTrailSize;
        _holdNoteData!.duree = _holdNoteActor.LongueurToDuree(_holdNoteActor.trail);
        _alleyManager.UpdateNotes();
    }

    public void Undo()
    {
        _holdNoteActor.trail.Size.X.Raw = _originalTrailSize;
        _holdNoteData!.duree = _holdNoteActor.LongueurToDuree(_holdNoteActor.trail);
        _alleyManager.UpdateNotes();
    }
}
