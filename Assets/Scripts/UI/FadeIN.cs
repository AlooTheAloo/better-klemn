using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FadeIN : MonoBehaviour
{
    [SerializeField] private Volume colorVolume;
    [SerializeField] public AnimationCurve curveFadeIn;
    [SerializeField] public float transitionTime;
    void Start()
    {
        TeamReadyManager.hasGameEnded = true;
        StartCoroutine(BeginFade(curveFadeIn,
            transitionTime));
    }

    public IEnumerator BeginFade(AnimationCurve curve , float transitionTime)
    {
        float timer = 0f;
        float startScale = -200f;
        float endScale = 100f;


        ChannelMixer colorMixer = colorVolume.profile.TryGet<ChannelMixer>(out var tmp) ? tmp : null;

        float duration = transitionTime;
        while (timer < duration)
        {
            float t = timer / duration; 

            
            float scale = Mathf.Lerp(startScale, endScale, curve.Evaluate(t));

            colorMixer.redOutRedIn.value = scale;
            colorMixer.blueOutBlueIn.value = scale;
            colorMixer.greenOutGreenIn.value = scale;

            timer += Time.unscaledDeltaTime; 

            yield return new WaitForFixedUpdate();
        }
        yield return null;
    }

    private void OnApplicationQuit()
    {
        ResetColours();
    }

    private void ResetColours()
    {
        ChannelMixer colorMixer = colorVolume.profile.TryGet<ChannelMixer>(out var tmp) ? tmp : null;
        colorMixer.redOutRedIn.value = 0f;
        colorMixer.blueOutBlueIn.value = 0f;
        colorMixer.greenOutGreenIn.value = 0f;
    }
    
}
