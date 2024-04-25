using Riptide;
using Shapes;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class ScaleLightState : LightState
{
    public Vector2 tailleOriginal;
    public Vector2 taille;

    public override void Serialize(Message message)
    {
        base.Serialize(message);
        if (actif)
        {
            message.Add(taille); 
        }    
    }

    public override void Deserialize(Message message)
    {
        base.Deserialize(message);
        if(actif)
        {
            taille = message.GetVector2();
        }
    }

    public Vector2 getDeltaAtFrame(float deltaTime, float currentTime, float animationLength, AnimationCurve courbe)
    {
        if (animationLength == 0)
        {
            return taille;
        }

        if (mesure == StateMeasurement.ABSOLUTE)
        {
            return Vector2.Lerp(tailleOriginal, taille, courbe.Evaluate(currentTime / animationLength));
        }
        else
        {
            return Vector2.Lerp(Vector2.zero, taille, courbe.Evaluate(currentTime / animationLength) - courbe.Evaluate((currentTime - deltaTime) / animationLength));
        }
    }

    public override IEnumerator AppliquerEffet(float time, ShapeRenderer target, AnimationCurve courbe, Action onCompleted)
    {
        float scaleFactor = target is Disc ? 0.5f : 1f;
        float originalSizeFactor = target is Disc ? 2f : 1f;

        tailleOriginal = target.transform.localScale * originalSizeFactor;
        float timer = 0;
        while (timer < time)
        {
            if (mesure == StateMeasurement.ABSOLUTE)
            {
                Vector2 delta = getDeltaAtFrame(Time.deltaTime, timer, time, courbe);
                delta.x *= scaleFactor;
                delta.y *= scaleFactor;
                target.transform.localScale =  delta;
            }
            else
            {
                Vector3 delta = getDeltaAtFrame(Time.deltaTime, timer, time, courbe);
                delta.x *= scaleFactor;
                delta.y *= scaleFactor;
                target.transform.localScale += delta;
            }

            timer += Time.deltaTime;
            yield return null;
        }
        yield return null;
        if (mesure == StateMeasurement.ABSOLUTE)
        {
            target.transform.localScale = ((Vector3) taille) * scaleFactor;
        }
        else if (time == 0)
        {
            target.transform.localScale += ((Vector3) taille) * scaleFactor;
        }

        onCompleted?.Invoke();
    }

    public ScaleLightState(Vector2? taille, StateMeasurement mesure) : base(mesure)
    {
        actif = taille != null;
        if (actif)
        {
            this.taille = (Vector2) taille;
        }
    }

    public ScaleLightState() { }
}
