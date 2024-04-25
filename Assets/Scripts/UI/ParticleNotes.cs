using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine;

public class ParticleNotes : MonoBehaviour
{
    private void OnEnable()
    {
        Alley.onJugement += DetermineParticleColour;
    }

    private void OnDisable()
    {
        Alley.onJugement -= DetermineParticleColour;
    }

    private void DetermineParticleColour(int Joueur , Precision precision, Alley allee)
    {
        if (precision == Precision.RATE) return;
        DoParticleEffect(allee);
    }

    private void DoParticleEffect(Alley a)
    {
        var mainModule = a.clickParticles.main;

        Color alleeSprite = a.alleeSprite.Color;
        alleeSprite.a = 1;
        
        mainModule.startColor = alleeSprite;

        a.clickParticles.Play();
    }
    
}
