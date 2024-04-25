using Shapes;
using System;
using System.Collections;
using UnityEngine;

public class Touche : MonoBehaviour
{
    [SerializeField] private Rectangle allee;
    [SerializeField] private Rectangle glow;
    [SerializeField] private Rectangle hitGlow;
    [SerializeField] private AnimationCurve curveAnimation;
    [SerializeField] private float opacity;

    [SerializeField] float tailleGlow;
    [SerializeField] float longueurAnimation;
    [SerializeField] float longueurAnimationHit;

    Coroutine coroutineActive;
    Coroutine coroutineActiveGlow;

    private void Start()
    {
        glow.Height = tailleGlow;
    }

    internal void OnGetScore(bool animerOffImmediatement = true)
    {
        if (coroutineActiveGlow != null)
        {
            StopCoroutine(coroutineActiveGlow);
        }
        SetHitGlowProgres(1);
        if (animerOffImmediatement)
        {
            coroutineActiveGlow = StartCoroutine(AnimateOff(SetHitGlowProgres, longueurAnimationHit));
        }
    }

    internal void OnStopGettingScore()
    {
        if (coroutineActiveGlow != null)
        {
            StopCoroutine(coroutineActiveGlow);
        }
        coroutineActiveGlow = StartCoroutine(AnimateOff(SetHitGlowProgres, longueurAnimationHit));
    }


    internal void OnPress()
    {
        if (coroutineActive != null)
        {
            StopCoroutine(coroutineActive);
        }
        SetIndicateurProgres(1);
    }

    internal void OnRelease()
    {
        if (coroutineActive != null)
        {
            StopCoroutine(coroutineActive);
        }
        coroutineActive = StartCoroutine(AnimateOff(SetIndicateurProgres, longueurAnimation));
    }

    IEnumerator AnimateOff(Action<float> animationAction, float longeur)
    {
        float compteur = 0;
        while (compteur <= 1)
        {
            animationAction(1 - curveAnimation.Evaluate(compteur));
            compteur += Time.deltaTime / longeur;
            yield return new WaitForEndOfFrame();
        }
        animationAction(0);
    }

    private const float COLOR_SKEW = 0.25f;
    
    /// <summary>
    /// Change l'opacité et la position du glow de l'indicateur (prend une valeur entre 0 et 1).
    /// </summary>
    void SetIndicateurProgres(float progres) {
        
        Color.RGBToHSV(allee.Color, out var H, out var S, out var V);
        Color color = Color.HSVToRGB(H, S, Mathf.Max(1, V + COLOR_SKEW));
        
        glow.FillColorStart = new Color(color.r, color.g, color.b, progres * opacity);
        glow.FillColorEnd = new Color(color.r, color.g, color.b, 0);
        glow.FillLinearEnd = new Vector3(0, progres * tailleGlow, 0);
    }

    void SetHitGlowProgres(float progres)
    {   
        hitGlow.FillColorStart = new Color(1, 1, 1, progres * opacity);
        hitGlow.FillLinearEnd = new Vector3(0, Mathf.Lerp(hitGlow.FillLinearStart.y, hitGlow.Height, progres), 0);
    }


}
