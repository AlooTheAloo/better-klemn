using System.Collections.Generic;
using System.Globalization;
using Nova;
using NovaSamples.UIControls;
using UnityEngine.Events;
using UnityEngine;

/// <summary>
/// Un contr�le UI qui r�agit � l'entr�e utilisateur et affiche une liste d'options s�lectionnables.
/// </summary>
public class MultiSelectDropdown : UIControl<MultiSelectDropdownVisuals>
{
    [Tooltip("L'�v�nement d�clench� lorsqu'un nouvel �l�ment est s�lectionn� dans la liste d�roulante.")]
    public UnityEvent<List<string>> OnValueChanged = null;

    [SerializeField] [Tooltip("Les donn�es utilis�es pour peupler la liste d'�l�ments s�lectionnables dans la liste d�roulante.")]
    private MultiSelectDropDownData DropdownOptions = new ();
    
    private MultiSelectDropdownVisuals Visuals => View.Visuals as MultiSelectDropdownVisuals;

    private void OnEnable()
    {
        // S'abonner aux �v�nements souhait�s
        View.UIBlock.AddGestureHandler<Gesture.OnHover, MultiSelectDropdownVisuals>(MultiSelectDropdownVisuals.HandleHovered);
        View.UIBlock.AddGestureHandler<Gesture.OnUnhover, MultiSelectDropdownVisuals>(MultiSelectDropdownVisuals.HandleUnhovered);
        View.UIBlock.AddGestureHandler<Gesture.OnPress, MultiSelectDropdownVisuals>(MultiSelectDropdownVisuals.HandlePressed);
        View.UIBlock.AddGestureHandler<Gesture.OnRelease, MultiSelectDropdownVisuals>(MultiSelectDropdownVisuals.HandleReleased);
        View.UIBlock.AddGestureHandler<Gesture.OnCancel, MultiSelectDropdownVisuals>(MultiSelectDropdownVisuals.HandlePressCanceled);
        View.UIBlock.AddGestureHandler<Gesture.OnClick, MultiSelectDropdownVisuals>(HandleDropdownClicked);
        
        Visuals.OnOptionsChanged += HandleValueChanged;
        InputManager.OnPostClick += HandlePostClick;
        
        Visuals.InitSelectionCount(DropdownOptions.CurrentSelections.Count.ToString(CultureInfo.InvariantCulture));
    }

    private void OnDisable()
    {
        // Se d�sabonner des �v�nements
        View.UIBlock.RemoveGestureHandler<Gesture.OnHover, MultiSelectDropdownVisuals>(MultiSelectDropdownVisuals.HandleHovered);
        View.UIBlock.RemoveGestureHandler<Gesture.OnUnhover, MultiSelectDropdownVisuals>(MultiSelectDropdownVisuals.HandleUnhovered);
        View.UIBlock.RemoveGestureHandler<Gesture.OnPress, MultiSelectDropdownVisuals>(MultiSelectDropdownVisuals.HandlePressed);
        View.UIBlock.RemoveGestureHandler<Gesture.OnRelease, MultiSelectDropdownVisuals>(MultiSelectDropdownVisuals.HandleReleased);
        View.UIBlock.RemoveGestureHandler<Gesture.OnCancel, MultiSelectDropdownVisuals>(MultiSelectDropdownVisuals.HandlePressCanceled);
        View.UIBlock.RemoveGestureHandler<Gesture.OnClick, MultiSelectDropdownVisuals>(HandleDropdownClicked);

        Visuals.OnOptionsChanged -= HandleValueChanged;
        InputManager.OnPostClick -= HandlePostClick;
    }
    
    private void HandleValueChanged(List<string> value)
    {
        OnValueChanged?.Invoke(value);
    }
    
    private void HandlePostClick(UIBlock clickedUIBlock)
    {
        if (!Visuals.IsExpanded)
        {
            return;
        }

        if (clickedUIBlock == null || !clickedUIBlock.transform.IsChildOf(transform))
        {
            // Cliqu� ailleurs, alors supprimez le focus.
            Visuals.Collapse();
        }
        
        print("We clicked something!");
    }

    private void HandleDropdownClicked(Gesture.OnClick evt, MultiSelectDropdownVisuals dropdownControl)
    {
        print(evt.Receiver.gameObject.name);
        if (evt.Receiver.transform.IsChildOf(dropdownControl.OptionsView.transform))
        {
            // L'objet cliqu� n'�tait pas la liste d�roulante elle-m�me, mais plut�t un �l�ment de liste � l'int�rieur de la liste d�roulante.
            // La liste d�roulante elle-m�me g�rera cet �v�nement, nous n'avons donc rien � faire ici.
            return;
        }

        // Basculez l'�tat �tendu de la liste d�roulante au clic

        if (dropdownControl.IsExpanded)
        {
            dropdownControl.Collapse();
        }
        else
        {
            dropdownControl.Expand(DropdownOptions);
        }
        print("Clicked the dropdown!");
    }
    
    public void SetOptions(List<string> options, List<string> currentSelections)
    {
        print("Set options to " + options.Count);
        print("Set current to " + currentSelections.Count);


        DropdownOptions.Options = options;
        DropdownOptions.SelectedIndexes = new List<int>();
        foreach (var selection in currentSelections)
        {
            int index = DropdownOptions.Options.IndexOf(selection);
            if(index != -1)
                DropdownOptions.SelectedIndexes.Add(index);
        }
        if(Visuals.IsExpanded) Visuals.Collapse();
        Visuals.InitSelectionCount(DropdownOptions.CurrentSelections.Count.ToString(CultureInfo.InvariantCulture));
    }
    
    public List<string> GetSelectedOptions()
    {
        return DropdownOptions.CurrentSelections;
    }
}
