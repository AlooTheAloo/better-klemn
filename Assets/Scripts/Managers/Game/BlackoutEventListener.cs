using System;
using System.Collections;
using System.Collections.Generic;
using Nova;
using UnityEngine;

public class BlackoutEventListener : MonoBehaviour
{
    [SerializeField] private UIBlock2D blackoutObject;
    [SerializeField] private float AnimationLength;
    [SerializeField] private AnimationCurve animationCurve;

    private void OnEnable()
    {
        BlackoutEvenement.onEventStart += StartBlackout;
        BlackoutEvenement.onEventEnd += StopBlackout;
    }

    private void OnDisable()
    {
        BlackoutEvenement.onEventStart -= StartBlackout;
        BlackoutEvenement.onEventEnd -= StopBlackout;        
    }


    private void StartBlackout()
    {
        StartCoroutine(setBlackoutOpacity(1));
    }
    
    private void StopBlackout()
    {
        StartCoroutine(setBlackoutOpacity(0));
    }

    IEnumerator setBlackoutOpacity(float targetOpacity)
    {
        Color color = blackoutObject.Color;
        float originalOpacity = color.a;
        float timer = 0;

        while (timer < AnimationLength)
        {
            color.a = Mathf.Lerp(originalOpacity, targetOpacity, animationCurve.Evaluate(timer / AnimationLength));
            blackoutObject.Color = color;
            yield return null;
            timer += Time.deltaTime;
        }
        color.a = targetOpacity;
        blackoutObject.Color = color;
    }

}
