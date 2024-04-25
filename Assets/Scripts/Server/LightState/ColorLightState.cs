using Riptide;
using Shapes;
using System;
using System.Collections;
using UnityEngine;

public class ColorLightState : LightState
{
    public Color initColor;
    public Color lightColor;

    public Color getDeltaAtFrame(float currentTime, float animationLength, AnimationCurve courbe)
    {
       return Color.Lerp(initColor, lightColor, animationLength == 0 ? 1 : courbe.Evaluate(currentTime / animationLength));
    }


    public override void Deserialize(Message message)
    {
        base.Deserialize(message);
        if(actif)
        {
            float[] color = message.GetFloats();
            lightColor = new Color(color[0], color[1], color[2]);
        }
    }

    public override void Serialize(Message message)
    {
        base.Serialize(message);
        if(actif)
            message.AddFloats(new float[] { lightColor.r, lightColor.g, lightColor.b });
    }

    public override IEnumerator AppliquerEffet(float time, ShapeRenderer target, AnimationCurve courbe, Action onCompleted)
    {
        if (mesure == StateMeasurement.RELATIVE) throw new InvalidOperationException("Une animation de couleur ne peut pas �tre relative");
        initColor = target.Color;
        float timer = 0;
        while (timer < time)
        {
            Color newColor = getDeltaAtFrame(timer, time, courbe);
            newColor.a = target.Color.a;
            target.Color = newColor;

            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        yield return null;
        lightColor.a = target.Color.a;
        target.Color = lightColor;
        onCompleted?.Invoke();
    }

    

    public ColorLightState()
    {
        actif = false;
    }

    public ColorLightState(string lightColor, StateMeasurement mesure) : base(mesure)
    {
        if (lightColor.Contains("#"))
        {
            lightColor = lightColor.Replace("#", "");
        }
        actif = lightColor != null;
        if (actif)
        {
            ColorUtility.TryParseHtmlString("#" + lightColor, out this.lightColor);
        }
    }
}
