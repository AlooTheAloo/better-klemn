using System.Collections;
using Lean.Pool;
using Shapes;
using UnityEngine;

public class HoldNoteActor : GameNoteActor
{
    [SerializeField] private Rectangle note;
    [SerializeField] private Rectangle trail;
    internal float tempsDebut;
    internal float dureeNote;

    internal void OnRelease(Alley alley , float vitesseMovement)
    {
        FadeOutNotes fadeOutNotes = gameObject.GetComponent<FadeOutNotes>();
        fadeOutNotes.thisAlley = alley;
        fadeOutNotes.vitesseMovement = vitesseMovement;
        fadeOutNotes.BeginFade(true);
    }

    

    internal HoldNoteActor ChangerTailleTrailDeNotedata(HoldNoteData noteData)
    {
        trail.Height = Constantes.AR_MULTIPLICATEUR * GameManager.Instance.gameNoteData.map.metadonnees.ar * noteData.duree;
        return this;
    }

    internal void ChangerTailleTrail(float taille) {
        trail.Height = taille;
    }
    internal void ChangerCouleurtrail(Color couleur)
    {
        trail.Color = couleur;
    }

    public override void OnMiss(Alley alley , float vitesseMovement)
    {
        FadeOutNotes fadeOutNotes = gameObject.GetComponent<FadeOutNotes>();
        fadeOutNotes.thisAlley = alley;
        fadeOutNotes.vitesseMovement = vitesseMovement;
        
        fadeOutNotes.BeginFade(true);
    }
}

