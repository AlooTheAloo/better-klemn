using System;
using System.Collections.Generic;
using Lean.Pool;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    const float MAP_PADDING = 20;
    [SerializeField]
    private Queue<NoteData> notesQueue = new Queue<NoteData>();

    [SerializeField]
    private AlleyContainer alleyContainer;

    private Map map;

    public static NoteManager i;

    public event Action onMapOver;
    private bool mapOver;

    private void Awake()
    {
        i = this;
    }

    private void OnEnable()
    {
        mapOver = false;
        FadeOutNotes.onFadeOutComplete += OnNoteFadeOut;
    }

    private void OnDisable()
    {
        FadeOutNotes.onFadeOutComplete -= OnNoteFadeOut;
    }

    void Start()
    {
        map = GameManager.Instance.gameNoteData.map;
        notesQueue = ReadNotesFromFile(map);
    }

    private void OnNoteFadeOut(GameNoteActor note)
    {
        LeanPool.Despawn(note);
    }

    private void Update()
    {
        if (notesQueue.Count != 0)
        {
            while (notesQueue.Peek().tempsDebut < TimeManager.i.tempsSecondes + MAP_PADDING / GameManager.Instance.gameNoteData.map.metadonnees.ar)
            {
                NoteData nouvelleNote = notesQueue.Dequeue();
                Alley targetAlley = alleyContainer.alleesJoueur[
                    nouvelleNote.joueur
                ].alleesPourJoueur[nouvelleNote.positionNote];
                GameNoteActor acteur;

                if (nouvelleNote is ClickNoteData)
                {
                    acteur = PoolManager.i.ClientPools.clickNotePool
                        .Spawn(targetAlley.transform)
                        .GetComponent<GameNoteActor>();
                }
                else
                {
                    acteur = PoolManager.i.ClientPools.holdNotePool
                        .Spawn(targetAlley.transform)
                        .GetComponent<HoldNoteActor>()
                        .ChangerTailleTrailDeNotedata(nouvelleNote as HoldNoteData);
                }

                acteur.notedata = nouvelleNote;

                targetAlley.AjouterNoteActiveDansAllee(acteur);
                if (notesQueue.Count == 0)
                    break;
            }
        }
        else if (
            !mapOver
            && PoolManager.i.ClientPools.clickNotePool.Spawned == 0
            && PoolManager.i.ClientPools.holdNotePool.Spawned == 0
        )
        {
            LightsClient.DesactiverDMX();
            onMapOver?.Invoke();
            mapOver = true;
        }
    }

    private Queue<NoteData> ReadNotesFromFile(Map map)
    {
        return new Queue<NoteData>(map.Notes);
    }
}
