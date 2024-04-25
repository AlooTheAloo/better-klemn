using Riptide;
using Shapes;
using System;
using System.Collections;
using System.Drawing.Printing;
using UnityEngine;


public abstract class LightState : IMessageSerializable, ICloneable<LightState>
{
    public bool actif;
    public Coroutine effetRoutine;
    public StateMeasurement mesure;

    public virtual void Deserialize(Message message) {
        actif = message.GetBool();
        if (actif) mesure = (StateMeasurement) message.GetUShort();
    }

    public virtual void Serialize(Message message) {
        message.AddBool(actif);
        if (actif) message.AddUShort((ushort) mesure);
    }

    public abstract IEnumerator AppliquerEffet(float time, ShapeRenderer target, AnimationCurve courbe, Action onComplete);

    public virtual void Stop() {
        GameManager.Instance.StopCoroutine(effetRoutine);
    }

    public LightState Clone()
    {
        LightState newOne = MemberwiseClone() as LightState;
        Debug.Log("Just cloned. " + newOne.GetHashCode() + " is the new one, " + GetHashCode() + " is old one");
        return newOne;
    }

    public LightState(StateMeasurement mesure)
    {
        this.mesure = mesure;
    }

    public LightState()
    {
        actif = false;
    }
}
