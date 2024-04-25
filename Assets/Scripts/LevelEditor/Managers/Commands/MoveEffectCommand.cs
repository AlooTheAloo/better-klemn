using System.Collections;
using System.Collections.Generic;
using Chroma.Editor;
using UnityEngine;

public class MoveEffectCommand : ICommand
{
    private EditorAlleyManager _alleyManager;
    private float _initialPosition;
    private float _endPosition;
    private EditorMapLightEffectActor _mapLightEffectActor;
    public MoveEffectCommand(EditorAlleyManager alleyManager , float initialPosition ,
        float endPosition , EditorMapLightEffectActor mapLightEffectActor)
    {
        _alleyManager = alleyManager;
        _initialPosition = initialPosition;
        _endPosition = endPosition;
        _mapLightEffectActor = mapLightEffectActor;
    }

    public void Execute()
    {
        _mapLightEffectActor.GroupeEffets.TempsDebut = _endPosition;
        _alleyManager.UpdateNotes();
    }

    public void Undo()
    {
        _mapLightEffectActor.GroupeEffets.TempsDebut = _initialPosition;
        _alleyManager.UpdateNotes();
    }
}
