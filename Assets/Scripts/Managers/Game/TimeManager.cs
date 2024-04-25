using Nova;
using Shapes;
using System;
using System.Collections;
using ChromaWeb.Database;
using Riptide;
using UnityEngine;
using UnityEngine.Audio;

public class TimeManager : MonoBehaviour
{
    public float mapBPM;
    float bpmOffset = 0;

    public static TimeManager i;

    [SerializeField] private PauseManager pauseManager;
    
    [SerializeField]
    private AnimationCurve timeCurve;

    [HideInInspector]
    public AudioSource source;

    [Header("Défaite audio")]

    [SerializeField]
    AudioMixerGroup master;

    [Header("Progrès visuel")]

    [SerializeField]
    UIBlock2D progresVisuel;

    [SerializeField]
    Color couleurDepart;

    [SerializeField]
    Color couleurFin;

    [SerializeField]
    TextBlock tempsRestantText;


    private float tempsMap;

    private bool animationMort;

    private void Start()
    {
        source.clip = GameManager.Instance.audioData.clip;
        source.outputAudioMixerGroup = master;
        StartCoroutine(commencerApresDelai());
    }

    private void OnEnable()
    {
        PauseManager.EnPause += SetPaused;
        LifeManager.Defaite += LifeManagerOnDefaite;
    }

    private void OnDisable()
    {
        PauseManager.EnPause -= SetPaused;
        LifeManager.Defaite -= LifeManagerOnDefaite;
        master.audioMixer.SetFloat("PitchBend", 1);
    }

    private void SetPaused(bool pause)
    {
        if (pause)
        {
            LightsClient.DesactiverDMX();
            source.Pause();
        }
        else
        {
            source.UnPause();
        }
    }

    private IEnumerator commencerApresDelai()
    {
        SourceTime = -3; // TODO : Crer Constante
        while (SourceTime <= 0)
        {
            yield return new WaitForEndOfFrame();
            SourceTime += Time.deltaTime;
        }
        
        Message m = Message.Create(MessageSendMode.Reliable, ClientToServerCalls.WAITINGROOM_STATE);
        m.AddInt((int)WaitingRoomState.SHOW_SONG_STATS);
        ClientManager.SendMessage(m);

        Metadonnees map = GameManager.mapSelectionnee.mapMetadonnees;
        print("Maptitle : " + map.titre);
        print("Map artist : " + map.artiste);
        print("Mappeur : " + map.mappeur);
        print("Temps : " + (map.duree.fin - map.duree.debut));

        GameStatus status = new GameStatus(map.titre, map.artiste, map.mappeur, map.duree.fin - map.duree.debut, map.chansonId);
        Message m2 = Message.Create(MessageSendMode.Reliable, ClientToServerCalls.WAITINGROOM_STATUS);
        m2.AddSerializable(status);
        ClientManager.SendMessage(m2);

        tempsMap = GameManager.mapSelectionnee.mapMetadonnees.duree.dureeTemps;
        mapBPM = GameManager.mapSelectionnee.mapMetadonnees.bpm;
        
        source.Play();

        while (!animationMort)
        {
            yield return new WaitForEndOfFrame();
            if(!pauseManager.enPause) SourceTime += Time.deltaTime;
        }
    }

private float _sourceTime;

    private void LifeManagerOnDefaite()
    {
        LightsClient.DesactiverDMX();
        StartCoroutine(SlowTime());
    }

    private void Update()
    {
        if (source.isPlaying && !LifeManager.isLosing)
        {
            AnimerProgres();
        }
    }

    private IEnumerator SlowTime()
    {
        animationMort = true;

        float timer = 0f;
        float startTime = SourceTime;
        float endTime = SourceTime + LifeManager._lifeManager.tempsAvantDefaite / 4f;

        while (timer < LifeManager._lifeManager.tempsAvantDefaite / 2f)
        {
            float t = timer / (LifeManager._lifeManager.tempsAvantDefaite / 2f);
            SourceTime = Mathf.Lerp(startTime, endTime, timeCurve.Evaluate(t));
            master.audioMixer.SetFloat("PitchBend", timeCurve.Evaluate(1 - t));

            timer += Time.unscaledDeltaTime;

            yield return null;
        }
        source.Stop();
        master.audioMixer.SetFloat("PitchBend", 0);
    }

    private float SourceTime
    {
        get
        {
             return _sourceTime;
        }
        set { _sourceTime = value; }
    }

    public float tempsSecondes
    {
        get { return SourceTime; }
        set
        {
            throw new System.InvalidOperationException(
                "TempsSecondes est contrôlé par un AudioSource, et ne peut pas être modifié pendant le runtime"
            );
        }
    }

    internal float tempsBPM
    {
        get { return (tempsSecondes - bpmOffset) / Constantes.SECONDES_DANS_MINUTE * mapBPM; }
        private set
        {
            throw new System.InvalidOperationException(
                "TempsBPM est contrôlé par tempsSecondes, qui ne peut pas être modifié pendant le runtime"
            );
        }
    }

    private void Awake()
    {
        if (i)
            Destroy(this);
        else
            i = this;

        source = gameObject.AddComponent<AudioSource>();
    }

    private void AnimerProgres() 
    {
        try {
            float pourcentageTermine = tempsSecondes / tempsMap;
            progresVisuel.RadialFill.FillAngle = -pourcentageTermine * 360; // - pour avoir un remplissage dans le sens horaire
            progresVisuel.Color = Color.Lerp(couleurDepart, couleurFin, pourcentageTermine);

            DateTime time = new(0);
            float tempsRestant =  tempsMap - tempsSecondes;
            time = time.AddSeconds(tempsRestant);
            tempsRestantText.Text = $"{time:mm:ss}";
        }
        catch {} // Quand l'objet se fait supprimer pour l'animation de changement de scène (misinformation)
    }

    public void ActionDansBeats(float beats, Action action)
    {
        ActionABeats(beats + i.tempsBPM, action);
    }

    public static void ActionABeats(float beats, Action action)
    {
        
        i.StartCoroutine(
            SimpleRoutines.WaitUntil(
                () => i.tempsBPM >= beats,
                () =>
                {
                    action?.Invoke();
                }
            )
        );
    }

    public static void ActionATemps(float temps, System.Action action)
    {
        i.StartCoroutine(
            SimpleRoutines.WaitUntil(
                () => i.tempsSecondes >= temps,
                () =>
                {
                    action?.Invoke();
                }
            )
        );
    }


    /// <summary>
    /// Retourne la différence entre un temps passé en paramètre et le temps actuel de la map. (en secondes)
    /// </summary>
    public static float TempsDiffSecondes(float secondes)
    {
        return secondes - i.tempsSecondes;
    }
}
