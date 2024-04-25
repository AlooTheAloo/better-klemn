using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using ChromaWeb.Database;
using Nova;
using Riptide;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class IntroScene : MonoBehaviour
{
    [Header("Animation Temps")]
    [SerializeField]
    private AnimationCurve courbe;

    bool timerIsRunning = false;

    [SerializeField]
    private Camera sceneCamera;

    public static event Action<Camera> onLoadIntroScene;

    [SerializeField]
    private float intensiteAnimation;
    private float scaleDebut;


    [SerializeField] private AudioSource ominous;
    [SerializeField] private AudioSource select;

    [SerializeField]
    TextBlock timerText;

    [SerializeField] 
    private TextBlock teamName;
    
    [SerializeField] 
    private TextBlock appuyerText;

    private bool foundTeam = false;
    
    
    float timer
    {
        get { return _timer; }
        set
        {
            if (Mathf.RoundToInt(value) != TimerDisplay)
            {
                onTimeChange?.Invoke();
                TimerDisplay = Mathf.RoundToInt(value);
            }
            _timer = value;
        }
    }
    
    float _timer;

    int TimerDisplay
    {
        get { return _TimerDisplay; }
        set
        {
            _TimerDisplay = value;
            timerText.Text = timerIsRunning ? $"Passage à une autre équipe dans {value} secondes" : "";
        }
    }

    int _TimerDisplay;
    
    public static event Action timerReachedZero;

    [SerializeField] private VideoPlayer videoPlayer;
    KeyCode[,] touches = {
    {
        KeyCode.A,
        KeyCode.S,
        KeyCode.D,
        KeyCode.F
    },
    {
        KeyCode.H,
        KeyCode.J,
        KeyCode.K,
        KeyCode.L
    } 
    };

    private event Action onTimeChange;


    private Coroutine timerRoutine;
    
    private void AnimerTempsCall()
    {
        timerRoutine = StartCoroutine(AnimerTemps());
    }
    

    private void RunTimer()
    {
        StartCoroutine(ChangerTimer(30));
    }
    bool playing_anim = false;

    IEnumerator ChangerTimer(float seconds)
    {
        timerIsRunning = true;
        timer = seconds;
        while (timer > 0)
        {
            yield return null;
            if(!playing_anim){
                timer -= Time.deltaTime;
            }
        }
        print("Invoking timerreachedzero");
        timerReachedZero?.Invoke();
    }

    private IEnumerator AnimerTemps()
    {
        float timer = 0;
        while (timer < 1)
        {
            timer += Time.deltaTime;
            yield return null;
            float targetScale = Mathf.Lerp(
                intensiteAnimation * scaleDebut,
                scaleDebut,
                courbe.Evaluate(timer)
            );
            timerText.transform.localScale = new Vector3(targetScale, targetScale, targetScale);
        }
    }

    private void Start(){ 
        
    }

    private void Awake()
    {
        LightsClient.SetLedMode(LedLightState.INTRO);
        onLoadIntroScene?.Invoke(sceneCamera);
        Message m1 = Message.Create(MessageSendMode.Reliable, ClientToServerCalls.WAITINGROOM_STATE);
        m1.AddInt((int)WaitingRoomState.LOGO);
        ClientManager.SendMessage(m1);
        scaleDebut = timerText.transform.localScale.x;
    }

    private void OnEnable()
    {
        QueueManager.onFindTeam += onfindteam;
        onTimeChange += AnimerTempsCall;
        timerReachedZero += GoNext;
        QueueManager.OnNoTeamFound += ResetScene;
    }
    
    private void GoNext(){
        QueueManager.i.GoNextTeam();
    }    



    private void OnDisable()
    {
        QueueManager.onFindTeam -= onfindteam;
        onTimeChange -= AnimerTempsCall;
        timerReachedZero -= GoNext;
        QueueManager.OnNoTeamFound -= ResetScene;
    }

    private void ResetScene()
    {
        if(QueueManager.i.GetCurrentTeam() == null) return;
        SceneManager.LoadScene((int)Scenes.INTRO);
        QueueManager.i.currentTeam = null;

    }


    private void onfindteam(TeamInfo team)
    {
        ominous.Play();
        
        appuyerText.Text = "Appuyez sur une touche pour commencer";
        teamName.Text = team.TeamName;
        
        Message m1 = Message.Create(MessageSendMode.Reliable, ClientToServerCalls.WAITINGROOM_STATE);
        m1.AddInt((int)WaitingRoomState.SHOW_NEXT_TEAM);
        ClientManager.SendMessage(m1);

        
        Message m = Message.Create(MessageSendMode.Reliable, ClientToServerCalls.WAITINGROOM_TEAMNAME);
        m.AddString(team.TeamName);
        ClientManager.SendMessage(m);


        foundTeam = true;
        RunTimer();
    }

    private IEnumerator FinishVideo()
    {
        
        Message m = Message.Create(MessageSendMode.Reliable, ClientToServerCalls.WAITINGROOM_STATE);
        m.AddInt((int)WaitingRoomState.LOGO);
        ClientManager.SendMessage(m);

        Message m2 = Message.Create(MessageSendMode.Reliable, ClientToServerCalls.PLAY_INTRO);
        ClientManager.SendMessage(m2);

        ominous.Stop();
        select.Play();

        playing_anim = true;
        videoPlayer.Play();

        float timer = 1;
        bool called = false;
        
        
        while (videoPlayer.isPlaying)
        {

            Color c = Color.white;
            timer -= Time.deltaTime;
            c.a = timer;
            appuyerText.Color = c;
            timerText.Color = c;
            teamName.Color = c;
            if (timer <= -3.2 && !called)
            {
                called = true;
                LightsClient.Intro();
                yield return new WaitForSeconds(4);
                LightsClient.ActiverDMX(0, 155);
            }
            
            yield return null;
        }
        LightsClient.DesactiverDMX();


        Message m3 = Message.Create(MessageSendMode.Reliable, ClientToServerCalls.CHANGE_SCENE);
        m3.AddInt((int) Scenes.TEAM_SELECT_SERVER);
        ClientManager.SendMessage(m3);

        SceneManager.LoadScene((int)Scenes.TEAM_READY);
    }

    private bool videoStarted = false;
    
    private void Update()
    {
        if (videoStarted) return;
        foreach (var touch in touches)
        {
            if (Input.GetKeyDown(touch) && foundTeam)
            {
                videoStarted = true;
                StartCoroutine(FinishVideo());
            }
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            SceneManager.LoadScene((int)Scenes.SONG_SELECT);
            Message m = Message.Create(MessageSendMode.Reliable, ClientToServerCalls.CHANGE_SCENE);
            m.AddInt((int)Scenes.SONG_SELECT_SERVER);
            ClientManager.SendMessage(m);
        }
    }
}
