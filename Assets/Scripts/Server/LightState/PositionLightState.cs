using Riptide;
using Shapes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PositionLightState : LightState
{
    public Vector2 originalPosition;
    public Vector2 targetPosition;

    public Vector2 getDeltaAtFrame(float deltaTime, float currentTime, float animationLength, AnimationCurve courbe)
    {
        if (animationLength == 0)
        {
            return targetPosition;
        }

        if (mesure == StateMeasurement.ABSOLUTE)
            return Vector2.Lerp(originalPosition, targetPosition, courbe.Evaluate(currentTime / animationLength));
        else
        {
            return  (Vector3)Vector2.Lerp(Vector2.zero, targetPosition,
                courbe.Evaluate(currentTime / animationLength) - courbe.Evaluate((currentTime - deltaTime) / animationLength));
        }
    }

    public override IEnumerator AppliquerEffet(float time, ShapeRenderer target, AnimationCurve courbe, Action onCompleted)
    {
        Debug.Log("Bitches heres the hashcode " + GetHashCode() + " for object " + target);

        originalPosition = target.transform.localPosition;

        float timer = 0;
        while (timer < time)
        {
           
            Vector2 delta = getDeltaAtFrame(Time.deltaTime, timer, time, courbe);
            Debug.Log("For object " + target + " the originalposition " + originalPosition + " and the targetposition " + targetPosition);
            Debug.Log("For object " + target + " we are moving with a delta of " + delta);
            if (mesure == StateMeasurement.ABSOLUTE)
                target.transform.localPosition = delta;
            else
            {
                target.transform.localPosition += new Vector3(delta.x, delta.y, 0);
            }

            timer += Time.deltaTime;
            yield return null;
        }
        yield return null;
        if (mesure == StateMeasurement.ABSOLUTE)
        {
            target.transform.localPosition = targetPosition;
        }
        else if (time <= 0)
        {
            target.transform.localPosition += (Vector3)targetPosition;
        }
        onCompleted?.Invoke();
        

    }

    public override void Deserialize(Message message)
    {
        base.Deserialize(message);
        if(actif)
            targetPosition = message.GetVector2();
    }

    public override void Serialize(Message message)
    {
        base.Serialize(message);
        if (actif)
            message.AddVector2(targetPosition);
    }


    public PositionLightState(Vector2? position, StateMeasurement mesure) : base(mesure)
    {
        actif = position != null;
        if (actif)
            targetPosition = (Vector2) position;
    }

    public PositionLightState() {}
}
