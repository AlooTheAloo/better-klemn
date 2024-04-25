using System;
using System.Globalization;
using System.Linq;
using UnityEngine;
using Nova;
using Nova.TMP;
using NovaSamples.Effects;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class EditorMapSelectManager : MonoBehaviour
{
    [SerializeField] private GridView editorMapSelectGridView;
    [SerializeField] private EditorMapSelectDetails editorMapSelectDetails;

    [SerializeField] [Tooltip("Une longueur pour configurer la hauteur de chaque Map.")]
    private Length gridRowHeight;

    [SerializeField] [Tooltip("Un ensemble de longueurs pour configurer l'espacement entre les Maps.")]
    private LengthRect gridRowPadding;

    public static MapCollectionMap? SelectedMap = null;
    
    private void Start()
    {
        InitializeGrid(editorMapSelectGridView);
    }

    private void InitializeGrid(GridView grid)
    {
        SubscribeToEvents(grid);

        if (grid.DataSourceItemCount == 0)
        {
            grid.SetDataSource(MapCollection.i.mapsAffichees);
        }

        MapCollectionMap initialMap = MapCollection.i.mapsAffichees.First();
        SelectedMap = initialMap;
        editorMapSelectDetails.SetDetails(initialMap);
    }

    private void SubscribeToEvents(GridView grid)
    {
        grid.SetSliceProvider(ProvideGridSlice);
        grid.AddDataBinder<MapCollectionMap, EditorMapVisuals>(HandleBind);
        grid.AddGestureHandler<Gesture.OnClick, EditorMapVisuals>(HandleClick);
    }

    private void UnsubscribeToEvents(GridView grid)
    {
        grid.ClearSliceProvider();
        grid.RemoveDataBinder<MapCollectionMap, EditorMapVisuals>(HandleBind);
        grid.RemoveGestureHandler<Gesture.OnClick, EditorMapVisuals>(HandleClick);
    }

    private void HandleBind(Data.OnBind<MapCollectionMap> evt, EditorMapVisuals target, int index) =>
        target.Bind(evt.UserData);

    private void HandleClick(Gesture.OnClick evt, EditorMapVisuals target, int index)
    {
        
        if (IsSecondaryButton(evt.Interaction))
        {
            return;
        }
        MapCollectionMap clickedMap = MapCollection.i.mapsAffichees[index];
        editorMapSelectDetails.SetDetails(clickedMap);
        SelectedMap = clickedMap;
    }

    private void ProvideGridSlice(int sliceIndex, GridView gridView, ref GridSlice2D gridSlice)
    {
        gridSlice.AutoLayout.AutoSpace = true;
        gridSlice.Layout.AutoSize.Y = AutoSize.Expand;
        gridSlice.Layout.Size.Y = gridRowHeight;
        gridSlice.Layout.Padding.XY = gridRowPadding;
    }

    private static bool IsSecondaryButton(Interaction.Update interaction)
    {
        if (interaction.UserData is InputData inputData)
        {
            return inputData.SecondaryButtonDown;
        }

        return false;
    }

    public void SendToEditor(bool isCreateNewMap)
    {
        SelectedMap = isCreateNewMap ? null : SelectedMap;
        SceneManager.LoadSceneAsync((int) Scenes.EDITOR);
    }

    public void QuitterMapSelect()
    {
        SceneManager.LoadSceneAsync((int)Scenes.MENU);
    }

    private void OnDisable()
    {
        UnsubscribeToEvents(editorMapSelectGridView);
    }
}

[Serializable]
public struct EditorMapSelectDetails
{
    [SerializeField] internal TextBlock editorMapSelectMapName;
    [SerializeField] internal TextBlock editorMapSelectMapArtist;
    [SerializeField] internal UIBlock2D editorMapSelectMapBackground;
    [SerializeField] internal TextBlock editorMapSelectMapTime;
    [SerializeField] internal TextBlock editorMapSelectMapBpm;
    [SerializeField] internal TextBlock editorMapSelectMapAR;
    [SerializeField] internal UIBlock2D editorMapSelectDetailsBackground;
    [SerializeField] internal UIBlock2D editorMapSelectDetailsColorBand;
    [SerializeField] internal TextBlock editorMapSelectDetailsEditButtonLabel;

    internal void SetDetails(MapCollectionMap mapDetails)
    {
        editorMapSelectMapName.Text = mapDetails.mapMetadonnees.titre;
        editorMapSelectMapArtist.Text = mapDetails.mapMetadonnees.artiste;
        editorMapSelectMapTime.Text = (mapDetails.mapMetadonnees.duree.fin - mapDetails.mapMetadonnees.duree.debut).ToString(CultureInfo.InvariantCulture);
        editorMapSelectMapBpm.Text = mapDetails.mapMetadonnees.bpm.ToString();
        editorMapSelectMapAR.Text = mapDetails.mapMetadonnees.ar.ToString(CultureInfo.InvariantCulture);
        editorMapSelectMapBackground.SetImage(mapDetails.imageCover);
        editorMapSelectDetailsBackground.Color = mapDetails.couleur;
        editorMapSelectDetailsColorBand.Color = mapDetails.couleur;
        editorMapSelectDetailsEditButtonLabel.Color = mapDetails.couleur;
    }
}