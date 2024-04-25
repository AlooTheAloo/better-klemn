

using Shapes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Xml;
using UnityEngine;


public abstract class ObjetLumiereActor : MonoBehaviour
{
    [SerializeField] CourbesSupportees courbesSupportees;
    [SerializeField] string[] groups;




    public ObjetLumiereData ObjetData { get; private set; }

    public Dictionary<Type, List<LightState>> runningStates = new Dictionary<Type, List<LightState>>();

    public abstract ShapeRenderer GetShape();

    public virtual void SetEffet(EffetLumineux effet) {

        foreach (var etat in effet.State.listeEtats)
        {
            if (!etat.actif) continue;

            var clone = etat.Clone();

            if(clone.mesure == StateMeasurement.ABSOLUTE)
            {
                ClearEtatOfType(clone); // Enlever tous les états actifs
            }
            AjouterEtat(clone);

            AnimationCurve courbe = courbesSupportees.GetCourbeFromLabel(effet.CourbeAnimation);
            Coroutine routine = StartCoroutine(clone.AppliquerEffet(effet.Duree, GetShape(), courbe, () =>
            {
                EnleverEtat(clone);
            }));
            clone.effetRoutine = routine;
        }

    }

    private void AjouterEtat(LightState etat)
    {
        Type stateType = etat.GetType();
        if (runningStates.ContainsKey(stateType))
        {
            runningStates[stateType].Add(etat);
        }
        else runningStates.Add(stateType, new List<LightState>() { etat });
    }

    private void EnleverEtat(LightState etat)
    {
        Type stateType = etat.GetType();
        if(runningStates.ContainsKey(stateType))
            runningStates[stateType].Remove(etat);
    }

    private void ClearEtatOfType(LightState etat)
    {
        Type stateType = etat.GetType();

        if (!runningStates.ContainsKey(stateType)) return;

        foreach (var stateAStop in runningStates[stateType])
        {
            stateAStop.Stop();
        }
        runningStates[etat.GetType()].Clear();
    }

    public virtual void Initialiser(LightObjectInitiaialisePacket packet)
    {
        transform.localPosition
            = Vector3.zero;
        transform.localScale = Vector3.zero;

        ObjetData = packet.lumiereData;
        print("Initialisation de l'effet...");
        SetEffet(
        new EffetLumineux
        {
            Decalage = 0,
            Duree = 0,
            CourbeAnimation = "LINEAR",
            GroupesCible = new string[0],
            State = packet.initialState,
            Order = 0
        });


        groups = ObjetData.groupes;

    }


    public bool isInteresting()
    {
        return groups.Length == 1 && groups.First() == "CrazyRecL";
    }



}

