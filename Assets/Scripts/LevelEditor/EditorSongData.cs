using Nova;
using NovaSamples.Effects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chroma.Editor
{
    public class EditorSongData : MonoBehaviour
    {
        float tempsPrecendent;
        [SerializeField] BlurEffect bg;
        [SerializeField] UIBlock2D cover;
        [SerializeField] TextBlock titre;
        [SerializeField] TextBlock artiste;
        [SerializeField] TextBlock temps;
        [SerializeField] UIBlock2D tempsFill;
        
        private void OnEnable()
        {
            EditorTimeManager.OnTimeChanged += onTimeChanged;
            EditorMapManager.OnMapChanged += LoadMapData;
        }

        private void OnDisable()
        {
            EditorTimeManager.OnTimeChanged -= onTimeChanged;
            EditorMapManager.OnMapChanged -= LoadMapData;
        }

        private void onTimeChanged(float time)
        {
            tempsPrecendent = time;
            var durree = EditorMapManager.MapData.MapVisualData.mapMetadonnees.duree;

            TimeSpan spanCurrent = TimeSpan.FromSeconds(time);
            TimeSpan spanFin = TimeSpan.FromSeconds(durree.fin - durree.debut);
            temps.Text = $"{spanCurrent.ToString("mm':'ss")} / {spanFin.ToString("mm':'ss")} ";

            tempsFill.Size.X.Percent = time / (durree.fin - durree.debut);
        }


        private void LoadMapData(EditorMap map)
        {
            Metadonnees meta = map.MapVisualData.mapMetadonnees;
            titre.Text = meta.titre;
            artiste.Text = meta.artiste;
            cover.SetImage(map.MapVisualData.imageCover);
            bg.InputTexture = map.MapVisualData.imageArriere;
            onTimeChanged(tempsPrecendent); // Refresh UI
        }
    }

}
