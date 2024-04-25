using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using Chroma.Editor;
using Nova;
using NovaSamples.UIControls;
using UnityEngine;

/// <summary>
///     Classe représentant le panneau des effets lumineux.
/// </summary>
public class LightEffectPannel : MonoBehaviour
{
    [SerializeField] private UIBlock2D panel;
    [SerializeField] private ListView effetListView;
    [SerializeField] private Button buttonAdd;
    [SerializeField] private Button buttonRemove;
    [SerializeField] private Button buttonDeselect;
    [SerializeField] private EditorLightEffectPanel editorLightEffectPanel;


    private GroupeEffets _groupeEffets;
    private int _currentIndex;
    private EditorLightEffectVisuals _selectedVisuals;

    private bool IsSelecting => _currentIndex != -1;

    public static event Action OnLightPanelClose;
    internal static Action<EffetLumineux> OnEffectSelected;

    bool panelOpened;


    /// <summary>
    ///     Initialise le panneau des effets lumineux avec un groupe d'effets donné.
    /// </summary>
    /// <param name="groupeEffets">Le groupe d'effets à utiliser.</param>
    public void Initialiser(GroupeEffets groupeEffets)
    {
        panelOpened = true;
        panel.gameObject.SetActive(true);
        _groupeEffets = groupeEffets;
        _currentIndex = -1;
        effetListView.SetDataSource(_groupeEffets.EffetsLumineux);
        buttonRemove.enabled = false;
    }

    /// <summary>
    ///     Termine l'édition des effets lumineux et retourne le groupe d'effets modifié.
    /// </summary>
    /// <returns>Le groupe d'effets modifié.</returns>
    public GroupeEffets Terminer()
    {
        panel.gameObject.SetActive(false);
        return _groupeEffets;
    }

    private void BindEffets(Data.OnBind<EffetLumineux> evt, EditorLightEffectVisuals visuals, int index)
    {
        visuals.Bind(index);
    }

    /// <summary>
    ///     Ajoute un nouvel effet au groupe d'effets.
    /// </summary>
    private void AddEffect()
    {
        _groupeEffets.AddNewEffet();
        if (_currentIndex != -1) DeselectEffect();
        effetListView.Refresh();
    }

    /// <summary>
    ///     Supprime l'effet sélectionné du groupe d'effets.
    /// </summary>
    private void RemoveEffect()
    {
        _groupeEffets.RemoveEffet(_currentIndex);
        if (_currentIndex != -1) DeselectEffect();
        effetListView.Refresh();
    }

    /// <summary>
    ///     Désélectionne l'effet actuellement sélectionné.
    /// </summary>
    private void DeselectEffect()
    {
        _currentIndex = -1;
        _selectedVisuals?.SetSelected(false);
        buttonRemove.enabled = false;
        editorLightEffectPanel.ResetFields();
    }

    private void MoveEffetUp(int index)
    {
        if (index == 0) return;
        _groupeEffets.EffetsLumineux[index].Order--;
        _groupeEffets.EffetsLumineux[index - 1].Order++;
        (_groupeEffets.EffetsLumineux[index], _groupeEffets.EffetsLumineux[index - 1]) = (
            _groupeEffets.EffetsLumineux[index - 1], _groupeEffets.EffetsLumineux[index]);
        DeselectEffect();
        effetListView.Refresh();
    }

    private void MoveEffetDown(int index)
    {
        if (index == _groupeEffets.EffetsLumineux.Count - 1) return;
        _groupeEffets.EffetsLumineux[index].Order++;
        _groupeEffets.EffetsLumineux[index + 1].Order--;
        (_groupeEffets.EffetsLumineux[index], _groupeEffets.EffetsLumineux[index + 1]) = (
            _groupeEffets.EffetsLumineux[index + 1], _groupeEffets.EffetsLumineux[index]);
        DeselectEffect();
        effetListView.Refresh();
    }

    private void ClosePannel()
    {
       
        if (_currentIndex != -1) DeselectEffect();
        if (panelOpened) OnLightPanelClose?.Invoke();
        panelOpened = false;
    }

    /// <summary>
    ///     Gère l'événement de pression sur un effet dans la liste.
    /// </summary>
    /// <param name="evt">L'événement de pression.</param>
    /// <param name="target">L'effet visuel sur lequel la pression a eu lieu.</param>
    /// <param name="index">L'index de l'effet dans la liste.</param>
    private void HandlePress(Gesture.OnPress evt, EditorLightEffectVisuals target, int index)
    {
        if (index == _currentIndex) return;
        var previousIndex = _currentIndex;
        _currentIndex = index;
        if (previousIndex != -1) _selectedVisuals.SetSelected(false);

        target.SetSelected(true);
        _selectedVisuals = target;
        buttonRemove.enabled = true;
        print("Calling oneffetselected");
        OnEffectSelected?.Invoke(_groupeEffets.EffetsLumineux[index]);
    }

    /// <summary>
    ///     Confirme les modifications apportées à un effet et désactive l'éditeur.
    /// </summary>
    private void ConfirmEffect(EffetLumineux effetModifie)
    {
        if(_currentIndex != -1)
            _groupeEffets.EffetsLumineux[_currentIndex] = effetModifie;

        for(int i =0; i < _groupeEffets.EffetsLumineux.Count; i++)
        {
            _groupeEffets.EffetsLumineux[i].Order = (ushort) i;
        }
    }


    private void OnEnable()
    {
        editorLightEffectPanel.EnableEditor();
        

        buttonAdd.OnClicked.AddListener(AddEffect);
        buttonRemove.OnClicked.AddListener(RemoveEffect);
        buttonDeselect.OnClicked.AddListener(DeselectEffect);
        EditorLightEffectPanel.OnEffectConfirmed += ConfirmEffect;
        effetListView.AddDataBinder<EffetLumineux, EditorLightEffectVisuals>(BindEffets);
        effetListView.AddGestureHandler<Gesture.OnPress, EditorLightEffectVisuals>(HandlePress);
    }

    private void OnDisable()
    {
        editorLightEffectPanel.DisableEditor();

        buttonAdd.OnClicked.RemoveListener(AddEffect);
        buttonRemove.OnClicked.RemoveListener(RemoveEffect);
        buttonDeselect.OnClicked.RemoveListener(DeselectEffect);
        EditorLightEffectPanel.OnEffectConfirmed -= ConfirmEffect;
        effetListView.RemoveDataBinder<EffetLumineux, EditorLightEffectVisuals>(BindEffets);
        effetListView.RemoveGestureHandler<Gesture.OnPress, EditorLightEffectVisuals>(HandlePress);
    }

    private void Update()
    {
        if (!panelOpened) return;
        if (Input.GetKeyDown(KeyCode.UpArrow) && IsSelecting)
            MoveEffetUp(_currentIndex);
        else if (Input.GetKeyDown(KeyCode.DownArrow) && IsSelecting) MoveEffetDown(_currentIndex);

        if (Input.GetKeyDown(KeyCode.Escape)) ClosePannel();

    }
}

/// <summary>
///     Structure représentant le panneau d'édition des effets lumineux.
/// </summary>
[Serializable]
internal struct EditorLightEffectPanel
{
    [SerializeField] private EditorLightEffectBaseInfo effectBaseInfo;
    [SerializeField] private EditorLightEffectTransitionInfo effectTransitionInfo;
    [SerializeField] private EditorLightEffectStateInfo effectStateInfo;
    [SerializeField] private Button buttonConfirm;

    internal static event Action<EffetLumineux> OnEffectConfirmed;

    internal void EnableEditor()
    {
        LightEffectPannel.OnEffectSelected += FromEffet;
        effectTransitionInfo.SubscribeToEvents();
        effectStateInfo.SubscribeToEvents();
        buttonConfirm.OnClicked.AddListener(ToEffet);
    }

    internal void DisableEditor()
    {
        LightEffectPannel.OnEffectSelected = null;

        effectTransitionInfo.UnsubscribeFromEvents();
        effectStateInfo.UnsubscribeFromEvents();
        buttonConfirm.OnClicked.RemoveListener(ToEffet);
    }

    internal void ResetFields()
    {
        effectBaseInfo.ResetFields();
        effectTransitionInfo.ResetFields();
        effectStateInfo.ResetFields();
    }

    private void FromEffet(EffetLumineux effetLumineux)
    {
        Debug.Log("Fromeffet");
        effectBaseInfo.FromEffet(effetLumineux);
        effectTransitionInfo.FromEffet(effetLumineux);
        effectStateInfo.FromEffet(effetLumineux);
    }

    private void ToEffet()
    {
        Debug.Log("ToEffet");
        var newEffet = new EffetLumineux();
        var (groupes, duree, decalage) = effectBaseInfo.ToEffet();
        var courbe = effectTransitionInfo.ToEffet();
        var state = effectStateInfo.ToEffet();
        newEffet.Decalage = decalage;
        newEffet.Duree = duree;
        newEffet.GroupesCible = groupes;
        newEffet.CourbeAnimation = courbe;
        newEffet.State = state;

        OnEffectConfirmed?.Invoke(newEffet);
    }
}

/// <summary>
///     Structure représentant les informations de base d'un effet lumineux dans l'éditeur.
/// </summary>
[Serializable]
internal struct EditorLightEffectBaseInfo
{
    [SerializeField] private TextField dureeBlock;
    [SerializeField] private TextField decalageBlock;
    [SerializeField] private LumieresEditeur lumieresEditeur;
    [SerializeField] private MultiSelectDropdown groupesDropdown;
    
    private List<string> GroupesDisponibles => lumieresEditeur.Groupes;

    internal void FromEffet(EffetLumineux effetLumineux)
    {

        dureeBlock.Text = effetLumineux.Duree.ToString(CultureInfo.InvariantCulture);
        Debug.Log($"duree block texte set as : {dureeBlock.Text}");
        decalageBlock.Text = effetLumineux.Decalage.ToString(CultureInfo.InvariantCulture);
        var groupesSelectionnes = effetLumineux.GroupesCible.ToList();
        groupesDropdown.SetOptions(GroupesDisponibles, groupesSelectionnes);
        
        dureeBlock.enabled = true;
        decalageBlock.enabled = true;
        groupesDropdown.enabled = true;
        
        dureeBlock.GetComponent<Interactable>().enabled = true;
        decalageBlock.GetComponent<Interactable>().enabled = true;
        groupesDropdown.GetComponent<Interactable>().enabled = true;
    }

    internal (string[], float, float) ToEffet()
    {
        var groupes = groupesDropdown.GetSelectedOptions().ToArray();
        var dureeParse = float.TryParse(dureeBlock.Text, out var duree);
        var decalageParse = float.TryParse(decalageBlock.Text, out var decalage);
        return (groupes, dureeParse ? duree : 0.0f, decalageParse ? decalage : 0.0f);
    }

    internal void ResetFields()
    {
        dureeBlock.Text = string.Empty;
        decalageBlock.Text = string.Empty;
        
        dureeBlock.enabled = false;
        decalageBlock.enabled = false;
        
        dureeBlock.GetComponent<Interactable>().enabled = false;
        decalageBlock.GetComponent<Interactable>().enabled = false;
    }
}

/// <summary>
///     Structure représentant les informations de transition d'un effet lumineux dans l'éditeur.
/// </summary>
[Serializable]
internal struct EditorLightEffectTransitionInfo
{
    [SerializeField] private List<UIBlock2D> transitionBlocks;
    [SerializeField] private Color originalColor;
    [SerializeField] private Color selectedColor;
    [SerializeField] private CourbesSupportees courbesSupportees;

    private const string CourbeDefault = "LINEAR";

    internal void FromEffet(EffetLumineux effetLumineux)
    {
        foreach (var block in transitionBlocks)
        {
            block.Color = originalColor;
            block.GetComponent<Interactable>().enabled = true;
        }

        if (effetLumineux.CourbeAnimation == string.Empty) return;
        var courbe = effetLumineux.CourbeAnimation;
        var index = courbesSupportees.courbeLabels.FindIndex(x => x.label == courbe);
        transitionBlocks[index].Color = selectedColor;
    }

    internal string ToEffet()
    {
        foreach (var block in transitionBlocks)
            if (block.Color == selectedColor)
                return courbesSupportees.courbeLabels[transitionBlocks.IndexOf(block)].label;

        return CourbeDefault;
    }

    internal void ResetFields()
    {
        foreach (var block in transitionBlocks)
        {
            block.Color = originalColor;
            block.GetComponent<Interactable>().enabled = false;
        }
    }

    internal void SubscribeToEvents()
    {
        foreach (var block in transitionBlocks) block.AddGestureHandler<Gesture.OnPress>(OnPressed);
    }

    internal void UnsubscribeFromEvents()
    {
        foreach (var block in transitionBlocks) block.RemoveGestureHandler<Gesture.OnPress>(OnPressed);
    }

    private void OnPressed(Gesture.OnPress evt)
    {
        evt.Consume();
        foreach (var block in transitionBlocks) block.Color = originalColor;

        evt.Target.Color = selectedColor;
    }
}


[Serializable]
internal class MeasureToggleGroup
{
    [SerializeField] public StateMeasurement state;
    [SerializeField] public UIBlock2D parent;
    [SerializeField] UIBlock2D relativeToggle;
    [SerializeField] UIBlock2D absoluteToggle;

    [SerializeField] Color selectedColor;
    [SerializeField] Color deselectedColor;

    public void SetMeasurement(StateMeasurement state)
    {
        if(state == StateMeasurement.RELATIVE)
        {
            relativeToggle.Color = selectedColor;
            absoluteToggle.Color = deselectedColor;
        }
        else
        {
            relativeToggle.Color = deselectedColor;
            absoluteToggle.Color = selectedColor;
        }
        Debug.Log($"inside is {state}");
        this.state = state;
    }




    public void init()
    {
        relativeToggle.AddGestureHandler<Gesture.OnClick>(SetRelative);
        absoluteToggle.AddGestureHandler<Gesture.OnClick>(SetAbsolute);
    }

    private void SetAbsolute(Gesture.OnClick evt)
    {
        SetMeasurement(StateMeasurement.ABSOLUTE);   
    }

    private void SetRelative(Gesture.OnClick evt)
    {
        SetMeasurement(StateMeasurement.RELATIVE);
    }
}


/// <summary>
///     Structure représentant les informations d'état d'un effet lumineux dans l'éditeur.
/// </summary>
[Serializable]
internal struct EditorLightEffectStateInfo
{
    [SerializeField] private Toggle positionToggle;
    [SerializeField] private Toggle scaleToggle;
    [SerializeField] private Toggle colorToggle;
    [SerializeField] private Toggle opacityToggle;

    [Header("Position values")] 
    [SerializeField] private TextField positionXBlock;
    [SerializeField] private TextField positionYBlock;
    [SerializeField] private MeasureToggleGroup positionRelativeToggle;

    [Header("Scale values")] 
    [SerializeField] private TextField scaleXBlock;
    [SerializeField] private TextField scaleYBlock;
    [SerializeField] private MeasureToggleGroup scaleRelativeToggle;

    [Header("Color")] [SerializeField] private TextField colorField;

    [Header("Opacity")] [SerializeField] private TextField opacityField;
    [SerializeField] private MeasureToggleGroup opacityRelativeToggle;


    internal void Update()
    {
        Debug.Log("Updating obj with UUID " + positionRelativeToggle.GetHashCode());
    }
    internal void FromEffet(EffetLumineux effetLumineux)
    {
        var currentState = effetLumineux.State;
        var activeStates = currentState.listeEtats.Where(x => x.actif).ToList();

        SetToggleState(positionToggle, activeStates, typeof(PositionLightState));
        SetToggleState(scaleToggle, activeStates, typeof(ScaleLightState));
        SetToggleState(colorToggle, activeStates, typeof(ColorLightState));
        SetToggleState(opacityToggle, activeStates, typeof(OpacityLightState));

        SetBlockText(positionXBlock, activeStates, typeof(PositionLightState), state => ((PositionLightState)state).targetPosition.x.ToString(CultureInfo.InvariantCulture));
        SetBlockText(positionYBlock, activeStates, typeof(PositionLightState), state => ((PositionLightState)state).targetPosition.y.ToString(CultureInfo.InvariantCulture));

        SetBlockText(scaleXBlock, activeStates, typeof(ScaleLightState), state => ((ScaleLightState)state).taille.x.ToString(CultureInfo.InvariantCulture));
        SetBlockText(scaleYBlock, activeStates, typeof(ScaleLightState), state => ((ScaleLightState)state).taille.y.ToString(CultureInfo.InvariantCulture));

        SetBlockText(colorField, activeStates, typeof(ColorLightState), state => ColorUtility.ToHtmlStringRGB(((ColorLightState)state).lightColor));

        SetBlockText(opacityField, activeStates, typeof(OpacityLightState), state => ((OpacityLightState)state).opacity.ToString(CultureInfo.InvariantCulture));

        EnableControls();

        positionRelativeToggle.SetMeasurement(GetMeasure(activeStates, typeof(PositionLightState)));
        scaleRelativeToggle.SetMeasurement(GetMeasure(activeStates, typeof(ScaleLightState)));
        opacityRelativeToggle.SetMeasurement(GetMeasure(activeStates, typeof(OpacityLightState)));
    }

    private StateMeasurement GetMeasure(List<LightState> states, Type stateType)
    {
        var state = states.FirstOrDefault(x => x.GetType() == stateType);
        return state?.mesure ?? StateMeasurement.ABSOLUTE;
    }

    private void SetToggleState(Toggle toggle, List<LightState> activeStates, Type stateType, Func<LightState, bool> condition = null)
    {
        toggle.ToggledOn = activeStates.Any(x => x.GetType() == stateType && (condition == null || condition(x)));
    }

    private void SetBlockText(TextField block, List<LightState> activeStates, Type stateType, Func<LightState, string> textSelector)
    {
        var activeState = activeStates.FirstOrDefault(x => x.GetType() == stateType);
        block.Text = (activeState is not null) ? textSelector(activeState) : string.Empty;
    }

    private void EnableControls()
    {
        positionToggle.enabled = scaleToggle.enabled = colorToggle.enabled = opacityToggle.enabled = true;

        positionXBlock.enabled = positionYBlock.enabled = positionRelativeToggle.parent.enabled = true;
        scaleXBlock.enabled = scaleYBlock.enabled = scaleRelativeToggle.parent.enabled = true;
        colorField.enabled = true;
        opacityField.enabled = opacityRelativeToggle.parent.enabled = true;
    }

    internal LightObjectState ToEffet()
    {
        var states = new List<LightState>();
        if (positionToggle.ToggledOn)
        {
            var mesure = positionRelativeToggle.state;
            var xParse = float.TryParse(positionXBlock.Text, out var x);
            var yParse = float.TryParse(positionYBlock.Text, out var y);
            var position = new Vector2(xParse ? x : 0.0f, yParse ? y : 0.0f);
            states.Add(new PositionLightState(position, mesure));
        }

        if (scaleToggle.ToggledOn)
        {
            var mesure = scaleRelativeToggle.state;
            var xParse = float.TryParse(scaleXBlock.Text, out var x);
            var yParse = float.TryParse(scaleYBlock.Text, out var y);
            var scale = new Vector2(xParse ? x : 0.0f, yParse ? y : 0.0f);
            states.Add(new ScaleLightState(scale, mesure));
        }

        if (colorToggle.ToggledOn)
        {
            var color = colorField.Text == string.Empty ? "000000" : colorField.Text;
            states.Add(new ColorLightState($"#{color}", StateMeasurement.ABSOLUTE));
        }

        if (opacityToggle.ToggledOn)
        {
            var mesure = opacityRelativeToggle.state;
            var opParse = float.TryParse(opacityField.Text, out var opacity);
            states.Add(new OpacityLightState(opParse ? opacity : 0.0f, mesure));
        }

        return new LightObjectState(states.ToArray());
    }


    internal void ResetFields()
    {
        //On vide les champs
        positionToggle.ToggledOn = false;
        scaleToggle.ToggledOn = false;
        colorToggle.ToggledOn = false;
        opacityToggle.ToggledOn = false;

        positionXBlock.Text = string.Empty;
        positionYBlock.Text = string.Empty;
        positionRelativeToggle.SetMeasurement(StateMeasurement.ABSOLUTE);

        scaleXBlock.Text = string.Empty;
        scaleYBlock.Text = string.Empty;
        scaleRelativeToggle.SetMeasurement(StateMeasurement.ABSOLUTE);

        colorField.Text = string.Empty;

        opacityField.Text = string.Empty;
        opacityRelativeToggle.SetMeasurement(StateMeasurement.ABSOLUTE);

        //On désactive les champs
        positionToggle.enabled = false;
        scaleToggle.enabled = false;
        colorToggle.enabled = false;
        opacityToggle.enabled = false;

        positionXBlock.enabled = false;
        positionYBlock.enabled = false;
        positionRelativeToggle.SetMeasurement(StateMeasurement.ABSOLUTE);

        scaleXBlock.enabled = false;
        scaleYBlock.enabled = false;

        colorField.enabled = false;

        opacityField.enabled = false;

        //On arrête l'activation d'interactions
        positionXBlock.GetComponent<Interactable>().enabled = false;
        positionYBlock.GetComponent<Interactable>().enabled = false;
        
        scaleXBlock.GetComponent<Interactable>().enabled = false;
        scaleYBlock.GetComponent<Interactable>().enabled = false;
        
        colorField.GetComponent<Interactable>().enabled = false;
        
        opacityField.GetComponent<Interactable>().enabled = false;
    }

    internal void SubscribeToEvents()
    {
        positionToggle.OnToggled.AddListener(OnPositionToggled);
        scaleToggle.OnToggled.AddListener(OnScaleToggled);
        colorToggle.OnToggled.AddListener(OnColorToggled);
        opacityToggle.OnToggled.AddListener(OnOpacityToggled);

        positionRelativeToggle.init();
        scaleRelativeToggle.init();
        opacityRelativeToggle.init();
    }

    internal void UnsubscribeFromEvents()
    {
        positionToggle.OnToggled.RemoveListener(OnPositionToggled);
        scaleToggle.OnToggled.RemoveListener(OnScaleToggled);
        colorToggle.OnToggled.RemoveListener(OnColorToggled);
        opacityToggle.OnToggled.RemoveListener(OnOpacityToggled);
    }

    private void OnPositionToggled(bool value)
    {
        positionXBlock.enabled = value;
        positionYBlock.enabled = value;
        positionRelativeToggle.parent.enabled = value;
        
        positionXBlock.GetComponent<Interactable>().enabled = value;
        positionYBlock.GetComponent<Interactable>().enabled = value;

        if (!value)
        {
            positionXBlock.Text = string.Empty;
            positionYBlock.Text = string.Empty;
        }
    }

    private void OnScaleToggled(bool value)
    {
        scaleXBlock.enabled = value;
        scaleYBlock.enabled = value;
        scaleRelativeToggle.parent.enabled = value;
        
        scaleXBlock.GetComponent<Interactable>().enabled = value;
        scaleYBlock.GetComponent<Interactable>().enabled = value;

        if (!value)
        {
            scaleXBlock.Text = string.Empty;
            scaleYBlock.Text = string.Empty;
        }
    }

    private void OnColorToggled(bool value)
    {
        colorField.enabled = value;
        
        colorField.GetComponent<Interactable>().enabled = value;

        if (!value) colorField.Text = string.Empty;
    }

    private void OnOpacityToggled(bool value)
    {
        opacityField.enabled = value;
        opacityRelativeToggle.parent.enabled = value;
        
        opacityField.GetComponent<Interactable>().enabled = value;

        if (!value)
        {
            opacityField.Text = string.Empty;
        }
    }
}