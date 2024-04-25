using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MapEventsManager : MonoBehaviour
{
    public static MapEventsManager i;
    public List<Evenement> evenementsEnCours = new List<Evenement>();
    private Queue<Evenement> evenementsAVenir = new Queue<Evenement>();

    private void Start()
    {
        if(i != null) Destroy(gameObject);
        else i = this; 
        evenementsAVenir = new Queue<Evenement>(GameManager.Instance.gameNoteData.map.evenements.evenements);
    }

    private void Update()
    {
        if (evenementsAVenir.Count > 0)
        {
            while (evenementsAVenir.Peek().start <= TimeManager.i.tempsSecondes)
            {
                Evenement evt = evenementsAVenir.Dequeue();
                evt.Execute();
                evenementsEnCours.Add(evt);
                if (evenementsAVenir.Count == 0)
                {
                    break;
                }
            }
        }
        
        List<Evenement> evenementsAEnlever = new List<Evenement>();
        
        foreach (var evt in evenementsEnCours)
        {
            if (evt.end <= TimeManager.i.tempsSecondes)
            {
                evt.Cancel();
                evenementsAEnlever.Add(evt);
            }
        }

        foreach (var evt in evenementsAEnlever)
        {
            evenementsEnCours.Remove(evt);
        }
    }
}
