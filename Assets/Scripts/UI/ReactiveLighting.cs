using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nova;
using UnityEngine;

public class ReactiveLighting : MonoBehaviour
{
    [Header("Param�tres d'animation")]
    [SerializeField] private int targetIntensity; //magic number
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private float tempsAnimation;

    private Coroutine[] flashRoutines = new Coroutine[Constantes.NOMBRE_JOUEURS];

    [SerializeField] private UIBlock2D[] blockFlash;
    
    private void OnEnable()
    {
        Alley.onJugement += DeterminerIntensite;
    }

    private void OnDisable()
    {        
        Alley.onJugement -= DeterminerIntensite;
    }

    private void DeterminerIntensite(int joueur, Precision precision, Alley a)
    {
        if (precision != Precision.RATE)
        {
            AppelFlash(joueur , 100);

        }
    }

    private void AppelFlash(int joueur, int intensite)
    {
        if (flashRoutines[joueur] != null)
        {
            StopCoroutine(flashRoutines[joueur]);
        }
        flashRoutines[joueur] = StartCoroutine(Flash(joueur , intensite));
    }

    private const float MAX_OPAC_FLASH = 0.75f;
    private const float LONGUEUR_OBJET_FLASH = 400;
    
    private IEnumerator Flash(int joueur, int targetIntensity)
    {
        var couleur = Color.white;
        couleur.a = targetIntensity * MAX_OPAC_FLASH / 100f;
        blockFlash[joueur].Color = couleur;
        blockFlash[joueur].Size.X.Raw = LONGUEUR_OBJET_FLASH * Mathf.Lerp(MAX_OPAC_FLASH, 1f,couleur.a);

        float timer = 0;
        while (timer < tempsAnimation)
        {
            yield return null;
            timer += Time.deltaTime;

            couleur.a = Mathf.Lerp(targetIntensity, 0, curve.Evaluate(timer / tempsAnimation)) * MAX_OPAC_FLASH / 100;
            blockFlash[joueur].Color = couleur;
            blockFlash[joueur].Size.X.Raw = LONGUEUR_OBJET_FLASH * Mathf.Lerp(0.5f, 1f,couleur.a);
        }
        couleur.a = 0;
        blockFlash[joueur].Size.X.Raw = LONGUEUR_OBJET_FLASH * Mathf.Lerp(0.5f, 1f,couleur.a);
        blockFlash[joueur].Color = couleur;
    }

}
