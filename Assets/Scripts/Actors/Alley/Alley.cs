using Lean.Pool;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Nova;
using Shapes;

public class Alley : MonoBehaviour
{
    [HideInInspector] public List<GameNoteActor> notesDansAllee;
    [HideInInspector] public HoldNoteActor noteAppuyee;
    [HideInInspector] public Rectangle alleeSprite;


    [SerializeField] internal ParticleSystem clickParticles;
    [SerializeField] internal KeyCode entree;
    [SerializeField] internal Touche touche;
    [SerializeField] internal Transform ligneJugement;
    [SerializeField] internal UIBlock2D boutonIcon;


    
    internal event Action<Alley> onButtonClick;
    internal event Action<Alley> onButtonRelease;
    public static event Action<int, Precision, Alley> onJugement;
    public static event Action<NoteData> onNoteClick;

    private bool controlesDesactives = false;
    private bool pressed = false;

    private float derniereVitesse = 0f;

    private const float DELAY_PULSE = 0.1f;
    private static float[] pulseTimer = new float[Constantes.NOMBRE_JOUEURS];

    private void Awake()
    {
        onButtonClick += _ => OnClick();
        onButtonRelease += _ => OnRelease();
        alleeSprite = GetComponent<Rectangle>();
        controlesDesactives = false;
    }

    private void OnEnable() {
        LifeManager.Defaite += onDefaite;
        StartCoroutine(FadeOutKeyIconRoutine(2));
    }
    private void OnDisable() {
        LifeManager.Defaite -= onDefaite;
    }

    private void Update()
    {
        if (controlesDesactives) {
            return;
        }
        
        if (Input.GetKeyDown(entree))
        {
            pressed = true;
            onButtonClick?.Invoke(this);
            touche.OnPress();
        }
        
        if(pressed) {
            if (!Input.GetKey(entree))
            {
                pressed = false;
                onButtonRelease?.Invoke(this);
                touche.OnRelease();
            }
        }
        
    }

    private void onDefaite() {
        controlesDesactives = true;
    }

    internal void UpdateNotes(float vitesseMovement, float tempsActuel)
    {
        derniereVitesse = vitesseMovement;
        List<GameNoteActor> notesToRemove = new List<GameNoteActor>();
        foreach(GameNoteActor note in notesDansAllee)
        {
            
            float targetY = (note.notedata.tempsDebut - tempsActuel) * vitesseMovement + ligneJugement.position.y;
            
            note.transform.position = new Vector2(note.transform.position.x, targetY);
            if (note.notedata.tempsDebut + MapManager.plateauBase * Constantes.DERNIER_PLATEAU_TOLERANCE  < TimeManager.i.tempsSecondes && note.enJeu)
            {
                // Trop tard!
                note.enJeu = false;
               
                notesToRemove.Add(note);
                if (!LifeManager.isLosing)
                {
                     note.OnMiss(this , vitesseMovement);
                     onJugement?.Invoke(note.notedata.joueur, Precision.RATE, this);
                }
            }
        }

        if(noteAppuyee != null && noteAppuyee.enJeu)
        {
            float tempsFin = noteAppuyee.notedata.tempsDebut + ((HoldNoteData)noteAppuyee.notedata).duree;
            float tempsDiff = TimeManager.TempsDiffSecondes(tempsFin);
            if (tempsDiff < 0)
            {
                touche.OnStopGettingScore();
                noteAppuyee.OnRelease(this , vitesseMovement);
                noteAppuyee = null;
            }
            else
                noteAppuyee.ChangerTailleTrail(tempsDiff * Constantes.AR_MULTIPLICATEUR * GameManager.Instance.gameNoteData.map.metadonnees.ar);
        }
    }

    internal void AjouterNoteActiveDansAllee(GameNoteActor note)
    {
        notesDansAllee.Add(note);
        var noteColor = alleeSprite.Color * Constantes.COULEUR_NOTE_MULTIPLICATEUR;
        noteColor.a = 1;
        
        note.GetComponent<Rectangle>().Color = noteColor;
        
        if(note is HoldNoteActor actorNote)
        {
            Color trailColor = noteColor * Constantes.COULEUR_TRAIL_MULTIPLICATEUR;
            trailColor.a = 1;
            actorNote.ChangerCouleurtrail(trailColor);
        }
    }
    internal void OnClick()
    {
        if (notesDansAllee.Count <= 0)
            return;
        List<GameNoteActor> notesJugement = notesDansAllee
            .OrderBy((x) => Mathf.Abs(TimeManager.TempsDiffSecondes(x.notedata.tempsDebut)))
            .Where(x => Mathf.Abs(TimeManager.TempsDiffSecondes(x.notedata.tempsDebut)) < MapManager.plateauBase * (Constantes.DERNIER_PLATEAU_TOLERANCE + 1))
            .ToList();

        if (notesJugement.Count == 0) return;
        GameNoteActor noteJugement = notesJugement.First();

        float diffPrecision = Mathf.Abs(TimeManager.TempsDiffSecondes(noteJugement.notedata.tempsDebut));
        Precision precision = CalculatePrecision(diffPrecision);
        onJugement?.Invoke(noteJugement.notedata.joueur, precision, this);
        
        if(precision != Precision.RATE)
        {
            Color.RGBToHSV(noteJugement.GetComponent<Rectangle>().Color, out var h, out _, out _);
    

            if (Time.time - pulseTimer[noteJugement.notedata.joueur] > DELAY_PULSE)
            {
                pulseTimer[noteJugement.notedata.joueur] = Time.time;
                LightsClient.Pulse(Color.HSVToRGB(h, 1, 1), noteJugement.notedata.joueur);
            }

            
            onNoteClick?.Invoke(noteJugement.notedata);
        }

        noteJugement.transform.localPosition = new Vector2(0, ligneJugement.position.y);

        notesDansAllee.Remove(noteJugement);

        if (noteJugement is HoldNoteActor holdNoteActor)
        {
            noteAppuyee = holdNoteActor;

            holdNoteActor.tempsDebut = TimeManager.i.tempsSecondes;
            holdNoteActor.dureeNote = ((HoldNoteData)holdNoteActor.notedata).duree;
            
            float tempsFin = noteAppuyee.notedata.tempsDebut + ((HoldNoteData)noteAppuyee.notedata).duree;
            float tempsDiff = TimeManager.TempsDiffSecondes(tempsFin);

            noteAppuyee.ChangerTailleTrail(tempsDiff * Constantes.AR_MULTIPLICATEUR *
                                           GameManager.Instance.gameNoteData.map.metadonnees.ar);
            
            touche.OnGetScore(false);
        }
        else
        {
            touche.OnGetScore();
            LeanPool.Despawn(noteJugement);
        }
    }

    internal void OnRelease()
    {
        if (noteAppuyee == null) return;
        touche.OnStopGettingScore();
        noteAppuyee.OnRelease(this , derniereVitesse);

        float differenceTempsFin = Math.Abs(TimeManager.i.tempsSecondes - noteAppuyee.tempsDebut - noteAppuyee.dureeNote);
        if (CalculatePrecision(differenceTempsFin) == Precision.RATE) 
        {
            onJugement?.Invoke(noteAppuyee.notedata.joueur, Precision.RATE, this);
        }
        noteAppuyee = null;
    }


    /// <summary>
    /// Calcul la pr cision d'une note faite par le joueur selon les plateaux de difficultes de la carte
    /// </summary>
    /// <param name="differenceTemps">Difference entre le temps actuel et le temps ideal</param>
    /// <returns>La precision determinee selon le temps de frappe</returns>
    private Precision CalculatePrecision(float differenceTemps)
    {
        if (differenceTemps <= MapManager.plateauBase)
            return Precision.PARFAIT;
        if (differenceTemps <= MapManager.plateauBase * Constantes.DEUXIEME_PLATEAU_TOLERANCE)
            return Precision.BIEN;
        if (differenceTemps <= MapManager.plateauBase * Constantes.DERNIER_PLATEAU_TOLERANCE)
            return Precision.OK;
        return Precision.RATE;
    }


    IEnumerator FadeOutKeyIconRoutine(float seconds)
    {
        yield return new WaitUntil(() => TimeManager.i.tempsSecondes > 0);
        float timer = 0;
        while (timer < seconds)
        {
            boutonIcon.Color = new Color(boutonIcon.Color.r, boutonIcon.Color.g, boutonIcon.Color.b, 1 - timer / seconds);
            timer += Time.deltaTime;
            yield return null;
        }
    }

}
