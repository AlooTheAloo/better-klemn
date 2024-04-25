using Nova;
using NovaSamples.UIControls;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ColorSelector : UIControl<ColorSelectorVisuals>
{
    public UnityEvent<Color> OnValueChanged = null;

    [SerializeField] private Color currentColor;

    private ColorSelectorVisuals Visuals => View.Visuals as ColorSelectorVisuals;

    private void OnEnable()
    {
        Visuals.ExpandButton.AddGestureHandler<Gesture.OnPress>(HandleExpand);
        Visuals.CollapseButton.AddGestureHandler<Gesture.OnPress>(HandleCollapse);
        Visuals.OnColorChanged += HandleValueChanged;
        SetColor(currentColor);
    }

    private void OnDisable()
    {
        Visuals.ExpandButton.RemoveGestureHandler<Gesture.OnPress>(HandleExpand);
        Visuals.CollapseButton.RemoveGestureHandler<Gesture.OnPress>(HandleCollapse);
        Visuals.OnColorChanged -= HandleValueChanged;
    }

    private void HandleValueChanged(Color value)
    {
        Visuals.SelectedColorBlock.Color = value;
        Visuals.SelectedColor.Text = ColorUtility.ToHtmlStringRGB(value);
        currentColor = value;
        OnValueChanged?.Invoke(value);
    }

    private void HandleExpand(Gesture.OnPress evt)
    {
        Visuals.Expand(currentColor);
    }

    private void HandleCollapse(Gesture.OnPress evt)
    {
        Visuals.Collapse();
    }

    public void SetColor(Color color)
    {
        print("Set coolor to " + color);
        currentColor = color;
        Visuals.SelectedColorBlock.Color = color;
        Visuals.SelectedColor.Text = ColorUtility.ToHtmlStringRGB(color);
    }

    public Color GetColor()
    {
        return Visuals.SelectedColorBlock.Color;
    }
}