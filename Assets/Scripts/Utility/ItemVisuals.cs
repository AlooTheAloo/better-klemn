using System;
using System.Collections.Generic;
using System.Globalization;
using Chroma.Editor;
using Nova;
using NovaSamples.Effects;
using System.Linq;
using NovaSamples.UIControls;
using UnityEngine;
using System.Drawing.Printing;

public class ScoreVisuals : ItemVisuals
{
    public TextBlock Position;
    public TextBlock TeamName;
    public TextBlock ScoreValue;
}

public class MapVisuals : ItemVisuals
{
    public TextBlock diff;
    public TextBlock ar;
    public TextBlock time;
    public TextBlock bpm;
    public TextBlock creatorName;
    public TextBlock artistName;
    public MapSelectMapObject obj;
    public TextBlock mapName;
    public BlurEffect background;
}

public class EditorGroupVisuals : ItemVisuals
{
    public UIBlock2D arrierePlan;
    public TextBlock nomGroupe;

    public void Bind(string groupe)
    {
        nomGroupe.Text = groupe;
    }
}

public class EditorMapVisuals : ItemVisuals
{
    [Header("Components")] public UIBlock ContentRoot;
    public TextBlock MapName;
    public UIBlock2D ColorBand;
    public UIBlock2D MapImage;

    public void Bind(MapCollectionMap data)
    {
        ContentRoot.gameObject.SetActive(true);
        MapName.Text = data.mapMetadonnees.titre;
        ColorBand.Color = data.couleur;
        MapImage.SetImage(data.imageCover);
    }
}

public class EditorLightObjectVisuals : ItemVisuals
{
    [Header("Components")] public TextBlock title;
    public TextBlock description;
    public GameObject iconeCercle;
    public GameObject iconeCarre;

    [Header("Scripts")] public ItemLumiereEditeur itemLumiere;

    [HideInInspector] public LightObjectInitiaialisePacket packet;

    public void Bind(LightObjectInitiaialisePacket packet)
    {
        this.packet = packet;
        var data = packet.lumiereData;

        title.Text = packet.lumiereData.nomLumiere;
        var cnt = data.groupes.Length;
        description.Text = $"{cnt} groupe {(cnt == 1 ? "" : "s")} | projecteur #{data.projectorID + 1}";

        if (data.type == ShapeActorType.RECTANGLE)
        {
            iconeCarre.SetActive(true);
            iconeCercle.SetActive(false);
            iconeCarre.GetComponent<UIBlock2D>().Color =
                (packet.initialState.listeEtats.Where(x => x is ColorLightState).First() as ColorLightState).lightColor;
        }
        else
        {
            iconeCarre.SetActive(false);
            iconeCercle.SetActive(true);
            iconeCercle.GetComponent<UIBlock2D>().Color =
                (packet.initialState.listeEtats.Where(x => x is ColorLightState).First() as ColorLightState).lightColor;
        }
    }
}

public class EditorLightEffectVisuals : ItemVisuals
{
    public UIBlock2D ArrierePlan;
    public TextBlock NomEffet;

    private Color _couleurOriginale;
    private Color _couleurSelectionee;


    public void Bind(int index)
    {
        NomEffet.Text = $"Effet #{index + 1}";
        _couleurOriginale = ArrierePlan.Color;
        _couleurSelectionee = new Color(0x2C / 255f, 0xB9 / 255f, 0xFB / 255f);
    }

    public void SetSelected(bool selected)
    {
        ArrierePlan.Color = selected ? _couleurSelectionee : _couleurOriginale;
    }
}


public class MultiSelectDropdownVisuals : UIControlVisuals
{
    [Header("Visuels r�duits")] [Tooltip("Le TextBlock pour afficher le nombre d'options s�lectionn�es.")]
    public TextBlock SelectedCount = null;

    [Tooltip("L'�l�ment visuel d'arri�re-plan � changer lorsque la liste d�roulante est press�e et rel�ch�e.")]
    public UIBlock2D Background = null;

    [Tooltip("L'�l�ment visuel � faire pivoter lorsque la liste d�roulante est �tendue ou r�duite.")] [SerializeField]
    private UIBlock2D DropdownArrow = null;

    [Header("Visuels �tendus")]
    [Tooltip("La racine visuelle du contenu � afficher lorsque la liste d�roulante est �tendue.")]
    [SerializeField]
    private UIBlock ExpandedViewRoot = null;

    [Tooltip("La ListView utilis�e pour afficher les diff�rentes options de la liste d�roulante.")]
    public ListView OptionsView = null;

    [Tooltip("Si faux, la liste d�roulante s'�tendra vers le haut.")] [SerializeField]
    private bool expandDown = true;

    [Header("Couleurs des lignes de la vue des options")]
    [Tooltip(
        "La couleur d'arri�re-plan par d�faut des �l�ments de la liste d�roulante. Chaque �l�ment de rang pair aura cette couleur.")]
    public Color DefaultRowColor;

    [Tooltip(
        "La couleur d'arri�re-plan alternative des �l�ments de la liste d�roulante. Chaque �l�ment de rang impair aura cette couleur.")]
    public Color AlternatingRowColor;

    public bool IsExpanded => ExpandedViewRoot.gameObject.activeSelf;
    public event Action<List<string>> OnOptionsChanged = null;

    private MultiSelectDropDownData DataSource { get; set; }

    private bool _eventHandlersRegistered = false;

    public void Expand(MultiSelectDropDownData dataSource)
    {
        ExpandedViewRoot.Alignment.Y = expandDown ? VerticalAlignment.Top : VerticalAlignment.Bottom;

        // Mettez en cache la source de donn�es actuelle pour pouvoir la mettre � jour en 
        // cas de s�lection d'une nouvelle option dans la liste d�roulante.
        DataSource = dataSource;

        EnsureEventHandlersRegistered();

        ExpandedViewRoot.gameObject.SetActive(true);

        OptionsView.SetDataSource(dataSource.Options);
        OptionsView.Refresh();

        // Mettez � jour la rotation de la fl�che de la liste d�roulante
        DropdownArrow.transform.localRotation = Quaternion.Euler(0, 0, 90);
    }

    public void Collapse()
    {
        // D�sactivez la liste des options s�lectionnables
        ExpandedViewRoot.gameObject.SetActive(false);

        // Mettez � jour la rotation de la fl�che de la liste d�roulante
        DropdownArrow.transform.localRotation = Quaternion.identity;
    }

    public void InitSelectionCount(string count)
    {
        SelectedCount.Text = count;
    }

    private void EnsureEventHandlersRegistered()
    {
        if (_eventHandlersRegistered) return;

        // Enregistrez le data binder
        OptionsView.AddDataBinder<string, ToggleVisuals>(BindOption);

        // Enregistrez les gestionnaires de gestes
        OptionsView.AddGestureHandler<Gesture.OnClick, ToggleVisuals>(HandleOptionSelected);
        OptionsView.AddGestureHandler<Gesture.OnHover, ToggleVisuals>(ToggleVisuals.HandleHovered);
        OptionsView.AddGestureHandler<Gesture.OnUnhover, ToggleVisuals>(ToggleVisuals.HandleUnhovered);
        OptionsView.AddGestureHandler<Gesture.OnPress, ToggleVisuals>(ToggleVisuals.HandlePressed);
        OptionsView.AddGestureHandler<Gesture.OnRelease, ToggleVisuals>(ToggleVisuals.HandleReleased);
        OptionsView.AddGestureHandler<Gesture.OnCancel, ToggleVisuals>(ToggleVisuals.HandlePressCanceled);

        _eventHandlersRegistered = true;
    }

    private void BindOption(Data.OnBind<string> evt, ToggleVisuals option, int index)
    {
        option.Label.Text = evt.UserData;
        option.IsOnIndicator.gameObject.SetActive(DataSource.SelectedIndexes.Contains(index));
        var defaultColor = index % 2 == 0 ? DefaultRowColor : AlternatingRowColor;
        option.Background.Color = defaultColor;
        option.DefaultColor = defaultColor;
        option.PressedColor = PressedColor;
        option.HoveredColor = HoveredColor;
    }

    private void HandleOptionSelected(Gesture.OnClick evt, ToggleVisuals option, int index)
    {
        if (!DataSource.SelectedIndexes.Contains(index))
        {
            DataSource.SelectedIndexes.Add(index);
            option.IsOnIndicator.gameObject.SetActive(true);
        }

        else
        {
            DataSource.SelectedIndexes.Remove(index);
            option.IsOnIndicator.gameObject.SetActive(false);
        }

        SelectedCount.Text = DataSource.SelectedIndexes.Count.ToString(CultureInfo.InvariantCulture);
        OnOptionsChanged?.Invoke(DataSource.CurrentSelections);
    }

    /// <summary>
    /// Une m�thode utilitaire pour restaurer l'�tat visuel de l'objet <see cref="MultiSelectDropdownVisuals"/> 
    /// lorsqu'il n'est pas survol�.
    /// </summary>
    /// <param name="evt">L'�v�nement de rel�chement.</param>
    /// <param name="visuals">Les <see cref="ItemVisuals"/> recevant l'�v�nement de rel�chement.</param>
    public static void HandleUnhovered(Gesture.OnUnhover evt, MultiSelectDropdownVisuals visuals)
    {
        if (evt.Receiver.transform.IsChildOf(visuals.ExpandedViewRoot.transform))
            // La hi�rarchie est survol�e, ce qui se produira si un des objets dans visuals.OptionsView
            // est survol�, mais nous ne voulons pas mettre en surbrillance l'�l�ment d'arri�re-plan
            // dans ce contexte.
            return;

        ButtonVisuals.HandleUnhovered(evt, visuals);
    }

    /// <summary>
    /// Une m�thode utilitaire pour indiquer un �tat visuel survol� de l'objet <see cref="MultiSelectDropdownVisuals"/>.
    /// </summary>
    /// <param name="evt">L'�v�nement de pression.</param>
    /// <param name="visuals">Les <see cref="ItemVisuals"/> recevant l'�v�nement de pression.</param>
    public static void HandleHovered(Gesture.OnHover evt, MultiSelectDropdownVisuals visuals)
    {
        if (evt.Receiver.transform.IsChildOf(visuals.ExpandedViewRoot.transform))
            // La hi�rarchie est survol�e, ce qui se produira si un des objets dans visuals.OptionsView
            // est survol�, mais nous ne voulons pas mettre en surbrillance l'�l�ment d'arri�re-plan
            // dans ce contexte.
            return;

        ButtonVisuals.HandleHovered(evt, visuals);
    }

    /// <summary>
    /// Une m�thode utilitaire pour restaurer l'�tat visuel de l'objet <see cref="MultiSelectDropdownVisuals"/> 
    /// lorsque son geste actif (probablement une pression) est annul�.
    /// </summary>
    /// <param name="evt">L'�v�nement d'annulation.</param>
    /// <param name="visuals">Les <see cref="ItemVisuals"/> recevant l'�v�nement d'annulation.</param>
    public static void HandlePressCanceled(Gesture.OnCancel evt, MultiSelectDropdownVisuals visuals)
    {
        if (evt.Receiver.transform.IsChildOf(visuals.ExpandedViewRoot.transform))
            // La hi�rarchie est survol�e, ce qui se produira si un des objets dans visuals.OptionsView
            // est survol�, mais nous ne voulons pas mettre en surbrillance l'�l�ment d'arri�re-plan
            // dans ce contexte.
            return;

        ButtonVisuals.HandlePressCanceled(evt, visuals);
    }

    /// <summary>
    /// Une m�thode utilitaire pour restaurer l'�tat visuel de l'objet <see cref="MultiSelectDropdownVisuals"/> 
    /// lorsque son geste actif (probablement une pression) est rel�ch�.
    /// </summary>
    /// <param name="evt">L'�v�nement de rel�chement.</param>
    /// <param name="visuals">Les <see cref="ItemVisuals"/> recevant l'�v�nement de rel�chement.</param>
    public static void HandleReleased(Gesture.OnRelease evt, MultiSelectDropdownVisuals visuals)
    {
        if (evt.Receiver.transform.IsChildOf(visuals.ExpandedViewRoot.transform))
            // La hi�rarchie est rel�ch�e, ce qui se produira si un des objets dans visuals.OptionsView
            // est rel�ch�, mais nous ne voulons pas mettre en surbrillance l'�l�ment d'arri�re-plan
            // dans ce contexte.
            return;

        ButtonVisuals.HandleReleased(evt, visuals);
    }

    /// <summary>
    /// Une m�thode utilitaire pour indiquer un �tat visuel press� de l'objet <see cref="MultiSelectDropdownVisuals"/>.
    /// </summary>
    /// <param name="evt">L'�v�nement de pression.</param>
    /// <param name="visuals">Les <see cref="ItemVisuals"/> recevant l'�v�nement de pression.</param>
    public static void HandlePressed(Gesture.OnPress evt, MultiSelectDropdownVisuals visuals)
    {
        if (evt.Receiver.transform.IsChildOf(visuals.ExpandedViewRoot.transform))
            // La hi�rarchie est press�e, ce qui se produira si un des objets dans visuals.OptionsView
            // est press�, mais nous ne voulons pas mettre en surbrillance l'�l�ment d'arri�re-plan
            // dans ce contexte.
            return;

        ButtonVisuals.HandlePressed(evt, visuals);
    }
}

public class ColorSelectorVisuals : UIControlVisuals
{
    [Header("Collapsed Visuals")] [Tooltip("The TextBlock to display the currently selected color.")]
    public TextBlock SelectedColor = null;

    [Tooltip("The UIBlock2D to display the currently selected color.")]
    public UIBlock2D SelectedColorBlock = null;

    [Tooltip("The UIBlock2D that acts as the button to expand the color selector.")]
    public UIBlock2D ExpandButton = null;

    [Header("Expanded Visuals")] [Tooltip("The root visual element of the expanded color selector.")] [SerializeField]
    private UIBlock ExpandedViewRoot = null;

    [Tooltip("The UIBlock2D that acts as the display for the entire color palette.")] [SerializeField]
    public UIBlock2D ColorPalette = null;

    [Tooltip("The UIBlock2D that acts as the pointer for the currently selected color.")] [SerializeField]
    public UIBlock2D ColorPointer = null;

    [Tooltip("The UIBlock2D that acts as the button to collapse the color selector.")] [SerializeField]
    public UIBlock2D CollapseButton = null;

    [Tooltip("Si faux, la palette de couleur s'affichera sur la droite.")] [SerializeField]
    private bool expandLeft = true;

    public event Action<Color> OnColorChanged;

    private Color SelectedColorValue { get; set; }

    private bool _eventHandlersRegistered;

    private bool _isPointerMoving;

    public bool IsExpanded => ExpandedViewRoot.gameObject.activeSelf;

    private const float ViewPortSkew = -0.5f;

    public void Expand(Color currentColor)
    {
        ExpandedViewRoot.Alignment.X = expandLeft ? HorizontalAlignment.Left : HorizontalAlignment.Right;
        EnsureEventHandlersRegistered();
        ExpandedViewRoot.gameObject.SetActive(true);
        SelectedColorValue = currentColor;
        //SetPointerPositionFromColor(currentColor);
    }

    public void Collapse()
    {
        ExpandedViewRoot.gameObject.SetActive(false);
    }

    private void HandleDrag(Gesture.OnDrag evt)
    {
        Debug.Log("In drag and ispointermoving is " + _isPointerMoving);
        if (!_isPointerMoving) return;
        evt.Consume();
        Debug.Log(evt.DragDeltaLocalSpace);
        Vector3 oldPosition = ColorPointer.Position.Raw;
        ColorPointer.Position.Raw += evt.DragDeltaLocalSpace;
        Vector3 newPos = ColorPointer.Position.Raw;
        float maxX = ColorPalette.Size.X.Raw / 2;
        float maxY = ColorPalette.Size.Y.Raw / 2;
        
        Debug.Log("max " + maxX + ", " + maxY);
        Debug.Log("new " + newPos.x + ", " + newPos.y);

        
        if (newPos.x < -maxX || newPos.y < -maxY || newPos.x > maxX || newPos.y > maxY)
        {
            ColorPointer.Position.Raw = oldPosition;
        }
    }

    


    private void HandlePress(Gesture.OnPress evt)
    {
        Debug.Log("ISMOVING");
        _isPointerMoving = true;
        evt.Consume();
    }

    private void HandleRelease(Gesture.OnRelease evt)
    {
        Debug.Log("Released");
        _isPointerMoving = false;
        evt.Consume();
        int width = ColorPalette.Texture.width;
        int height = ColorPalette.Texture.width;

        float ratioX = 1 + (ColorPointer.Position.Raw.x - (ColorPalette.Size.X.Raw / 2)) / ColorPalette.Size.X.Raw;
        float ratioY = 1 + (ColorPointer.Position.Raw.y - (ColorPalette.Size.Y.Raw / 2)) / ColorPalette.Size.Y.Raw;

        SelectedColorValue = ColorPalette.Texture.GetPixel((int) (ratioX * width), (int) (ratioY * height));
        OnColorChanged?.Invoke(SelectedColorValue);
        Debug.Log(SelectedColorValue);
    }

    private void EnsureEventHandlersRegistered()
    {
        if (_eventHandlersRegistered) return;
        
        ColorPalette.AddGestureHandler<Gesture.OnDrag>(HandleDrag);
        ColorPalette.AddGestureHandler<Gesture.OnPress>(HandlePress);
        ColorPalette.AddGestureHandler<Gesture.OnRelease>(HandleRelease);
        _eventHandlersRegistered = true;
    }
}