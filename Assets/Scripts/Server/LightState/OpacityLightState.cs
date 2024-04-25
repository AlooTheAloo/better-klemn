using Riptide;
using Shapes;
using System;
using System.Collections;
using System.Threading;
using TMPro;
using UnityEngine;

public class OpacityLightState : LightState
{
    public float opacity;
    public float originalOpacity;

    public override void Serialize(Message message)
    {
        base.Serialize(message);
        if (actif) message.Add(opacity);
    }

    public override void Deserialize(Riptide.Message message)
    {
        base.Deserialize(message);
        if (actif) opacity = message.GetFloat();
    }

    public float getDeltaAtFrame(float deltaTime, float currentTime, float animationLength, AnimationCurve courbe)
    {
        if (animationLength == 0)
        {
            return opacity;
        }
       
        if (mesure == StateMeasurement.ABSOLUTE)
        {
            return Mathf.Lerp(originalOpacity, opacity, courbe.Evaluate(currentTime / animationLength));
        }
        else
        {
            return Mathf.Lerp(0, opacity, courbe.Evaluate(currentTime / animationLength) - courbe.Evaluate((currentTime - deltaTime) / animationLength));
        }
    }


    public override IEnumerator AppliquerEffet(float time, ShapeRenderer target, AnimationCurve courbe, Action onCompleted)
    {
        originalOpacity = target.Color.a;
        float timer = 0;

        while (timer < time)
        {

            float delta = getDeltaAtFrame(Time.deltaTime, timer, time, courbe);
            Color c = target.Color;

            if (mesure == StateMeasurement.ABSOLUTE)
            {
                target.Color = new Color(c.r, c.g, c.b, delta);
            }
            else
            {
                target.Color += new Color(c.r, c.g, c.b, c.a + delta);
            }

            timer += Time.deltaTime;
            yield return null;
        }
        if (mesure == StateMeasurement.ABSOLUTE)
        {
            target.Color = new Color(target.Color.r, target.Color.g, target.Color.b, opacity);
        }
        else if (time == 0)
        {
            target.Color = new Color(target.Color.r, target.Color.g, target.Color.b, target.Color.a + opacity);
        }

        onCompleted?.Invoke();
    }

    public OpacityLightState(float? opacity, StateMeasurement mesure) : base(mesure) 
    {
        actif = opacity != null;
        if (actif)
        {
            this.opacity = (float) opacity;
        }
    }

    public OpacityLightState() { }
}
