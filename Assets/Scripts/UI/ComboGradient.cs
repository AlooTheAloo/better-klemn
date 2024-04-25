using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboGradient : MonoBehaviour
{
    //[SerializeField] private Material[] barMaterials;

    //[SerializeField] private Gradient comboGrad; //blanche initallement

    //private ulong[] combos;

    //private int gameNotes;

    //private void OnEnable()
    //{
        
        
    //    Alley.onJugement += AlleyOnonJugement;
        
    //    for (int _instance = 0; _instance < barMaterials.Length; _instance++)
    //    {
    //        barMaterials[_instance].EnableKeyword("_EMISSION");
    //    }

    //    gameNotes = GameNotePreloader.noteCount;
    //}
    
    //private void Start()
    //{
    //    GradientColorKey[] colorKeys = new GradientColorKey[2];
    //    GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];

    //    alphaKeys[0].time = 0 ;
    //    alphaKeys[0].alpha = 1;

    //    alphaKeys[1].time = 1 ;
    //    alphaKeys[1].alpha = 1;
        
    //    colorKeys[0].color = Color.white;
    //    colorKeys[0].time = 0.0f;
    //    colorKeys[1].color = ImageLoaderImages.mainColor;
    //    colorKeys[1].time = 1.0f;
    //    comboGrad.SetKeys(colorKeys , alphaKeys);
    //}

    //private void AlleyOnonJugement(int joueur, Precision precision, Alley alley)
    //{
    //    if (precision == Precision.RATE)
    //    {
    //        return;
    //    }
        
    //    combos = ComboManager.combos;

    //    for (int _instance = 0; _instance < barMaterials.Length; _instance++)
    //    {
    //        barMaterials[_instance].SetColor("_EmissionColor" , comboGrad.Evaluate((gameNotes * 0.25f) / combos[_instance]));
    //    }
        
        
        
    //}
}
