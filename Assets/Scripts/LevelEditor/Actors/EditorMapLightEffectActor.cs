using System;
using System.Collections.Generic;
using Chroma.Editor;
using Nova;
using UnityEngine;

public class EditorMapLightEffectActor : MonoBehaviour
{
    private const float ViewportSkew = -0.5f;
    
    private const KeyCode TouchePasDeSnap = KeyCode.LeftControl;

    public GroupeEffets GroupeEffets;
    public UIBlock2D acteur;
    private EditorAlleyManager _alleyManager;
    private EditorTimeManager _timeManager;
    [SerializeField] private TextBlock label;

    private bool _enTrainDeBouger;
    
    private bool _estSelectionne;
    
    public bool EstSelectionne
    {
        get => _estSelectionne;
        set
        {
            _estSelectionne = value;
            acteur.Color = _estSelectionne ? Color.gray : Color.white;
            //print("Bitches the new color is now " + acteur.Color + " and my name is " + gameObject.name);
        }
    }
    public static event Action<bool, GroupeEffets , EditorMapLightEffectActor, bool> OnEffectSelected;

    public EditorMapLightEffectActor Initialiser(GroupeEffets groupeEffets, EditorAlleyManager alleyManager,
        EditorTimeManager timeManager, bool selectionne)
    {
        GroupeEffets = groupeEffets;
        _alleyManager = alleyManager;
        _timeManager = timeManager;
        label.Text = groupeEffets.ToString();
        EstSelectionne = selectionne;
        return this;
    }
    
    public void RefreshText()
    {
        label.Text = GroupeEffets.ToString();
    }

    private void OnEnable()
    {
        acteur.AddGestureHandler<Gesture.OnPress>(OnActeurPress);

        acteur.AddGestureHandler<Gesture.OnDrag>(OnActeurDrag);

        acteur.AddGestureHandler<Gesture.OnRelease>(OnActeurRelease);

        acteur.AddGestureHandler<Gesture.OnClick>(OnActeurClick);

        EditorAlleyManager.OnEffetsColles += Deselectionner;
    }

    private void OnDisable()
    {
        acteur.RemoveGestureHandler<Gesture.OnPress>(OnActeurPress);

        acteur.RemoveGestureHandler<Gesture.OnDrag>(OnActeurDrag);

        acteur.RemoveGestureHandler<Gesture.OnRelease>(OnActeurRelease);

        acteur.RemoveGestureHandler<Gesture.OnClick>(OnActeurClick);
        
        EditorAlleyManager.OnEffetsColles -= Deselectionner;
    }

    private void OnActeurClick(Gesture.OnClick evt)
    {
        if (_timeManager.isPlaying) return;
        EstSelectionne = !EstSelectionne;
        OnEffectSelected?.Invoke(EstSelectionne, GroupeEffets , this, Input.GetKey(KeyCode.LeftShift));
        _enTrainDeBouger = false;
        evt.Consume();
    }

    private float initialPosition;
    private void OnActeurPress(Gesture.OnPress evt)
    {
        if (_timeManager.isPlaying) return;
        _enTrainDeBouger = true;
        initialPosition = _alleyManager.PositionToSecondes(evt.Target.Position.X.Raw);
        evt.Consume();
    }
    private void OnActeurDrag(Gesture.OnDrag evt)
    {
        if (!_enTrainDeBouger) return;
        
        var positionSouris = (Camera.main!.WorldToViewportPoint(evt.PointerPositions.Current).x +
                              ViewportSkew) * _alleyManager.alleeParents.Size.X.Raw;
        
        var deltaX = 0f;
        if (Input.GetKey(TouchePasDeSnap))
        { 
            deltaX = positionSouris - acteur.Position.X.Raw; // same delta we're applying to all effets
        }
        else
        {
            deltaX = _alleyManager.SnapX(positionSouris) - acteur.Position.X.Raw; // same delta we're applying to all effets
        }
        
        
        var effetsActorsABouger = new Dictionary<GroupeEffets,EditorMapLightEffectActor>(_alleyManager._effetsMapSelectionnes);
        if (!effetsActorsABouger.ContainsValue(this))
        {
            effetsActorsABouger.Add(GroupeEffets.Clone(), this);
        }

        foreach (var kvp in effetsActorsABouger)
        {
            kvp.Value.acteur.Position.X.Raw += deltaX;
            ICommand command = new MoveEffectCommand(_alleyManager, kvp.Value.initialPosition,
                _alleyManager.PositionToSecondes(kvp.Value.acteur.Position.X.Raw), kvp.Value);
            NoteInvoker.AddCommand(command);
            
        }
        
        evt.Consume();
    }

    private void OnActeurRelease(Gesture.OnRelease evt)
    {
        if (!_enTrainDeBouger) return;
        ICommand command = new MoveEffectCommand(_alleyManager, initialPosition,
            _alleyManager.PositionToSecondes(evt.Target.Position.X.Raw), this);
        print($" on acteur release pos : {evt.Target.Position.X.Raw}");
        NoteInvoker.AddCommand(command);
    }
    
    public void Deselectionner()
    {
        print("deselectionner is called");
        EstSelectionne = false;
    }
    
    public void Selectionner()
    {
        print("selectionner");
        EstSelectionne = true;
    }
}