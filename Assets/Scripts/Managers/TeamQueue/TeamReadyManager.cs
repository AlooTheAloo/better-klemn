using Nova;
using Riptide;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeamReadyManager : MonoBehaviour
{
    [Header("Texte prêts")]
    [SerializeField]
    string readyMsg;

    [SerializeField]
    string notReadyMsg;

    [Header("Refs")]
    [SerializeField] 
    private TextBlock playerOneName;

    [SerializeField]
    private AudioSource source;


    [SerializeField]
    private TextBlock playerTwoName;

    [SerializeField] 
    private TextBlock teamName;

    
    [SerializeField] 
    private UIBlock2D overlay;
    
    [SerializeField]
    TextBlock p1ReadyBox;

    [SerializeField]
    TextBlock p2ReadyBox;

    [SerializeField]
    UIBlock2D bgBlock;

    [SerializeField]
    UIBlock2D square1;

    [SerializeField]
    UIBlock2D square2;



    [SerializeField]
    private bool isDev;

    int _readyPlayers = 0;
    bool _readyP1 = false;
    bool _readyP2 = false;


    public static bool hasGameEnded = false;

    public static event Action allPlayersReady;
    public static event Action<bool, int> playerReady;
    public static event Action OnNbToursChange;
    public static event Action<Camera> onTeamReadyEntered;

    [SerializeField] private Camera equipeCamera; 
    
    private Coroutine timerRoutine;

    [Header("Animation Prêt")]
    [SerializeField] private float animationLength = 0.125f;
    [SerializeField] private AnimationCurve animationCurve;
    [SerializeField] private float animationStrength;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color unselectedColor;

    int ReadyPlayers
    {
        get { return _readyPlayers; }
        set
        {
            if (value >= Constantes.NOMBRE_JOUEURS){
                hasGameEnded = false;
                allPlayersReady?.Invoke();
            }
            _readyPlayers = value;
        }
    }

    const float ANIMATION_SPEED = 0.05f;

    private void Awake()
    {
        if (!Application.isEditor && !BuildManager.build.isDev)
        {
            isDev = false;
        }

        var team = QueueManager.i.GetCurrentTeam().Value;
        playerOneName.Text = team.NomPremierJoueur;
        playerTwoName.Text = team.NomSecondJoueur;
        teamName.Text = team.TeamName;
        

        StartCoroutine(animateOverlay());
        
        LightsClient.SetLedMode(LedLightState.Lineup);
        StartCoroutine(StartBreathingInTime(1f));


    }

    private IEnumerator animateOverlay()
    {
        overlay.Color = Color.white;
        
        float timer = 1f;

        while (timer > 0f)
        {
            yield return null;
            timer -= Time.deltaTime;
            Color c = overlay.Color;
            c.a = timer;
            overlay.Color = c;
        }
        
    } 
    

    private IEnumerator StartBreathingInTime(float time)
    {
        yield return new WaitForSeconds(time);
        Message message = Message.Create(MessageSendMode.Reliable, ClientToServerCalls.SYNCHRONISE_BREATHING);
        message.AddFloat(ANIMATION_SPEED);
        ClientManager.SendMessage(message);
        StartCoroutine(ChangerBGColor(ANIMATION_SPEED));
        StartCoroutine(PulseAvecMusique());
    }


    private const float MUSIQUEBPM = 30f;
    private IEnumerator PulseAvecMusique()
    {
        while (true)
        {
            yield return new WaitForSeconds(60 / MUSIQUEBPM);
            LightsClient.AddLine(Color.HSVToRGB(bgColorTimer, 1, 1));
        }
    }

    private void StopTimerRoutine()
    {
        if(timerRoutine != null)
            StopCoroutine(timerRoutine);
    }

    
    private void OnEnable()
    {
        onTeamReadyEntered?.Invoke(equipeCamera);
        allPlayersReady += FadeVolume;
        allPlayersReady += StartGame;
        allPlayersReady += StopTimerRoutine;
        playerReady += ToggleReadyBox;
        playerReady += PlayerReadyStateChange;
    }

    private void OnDisable()
    {
        allPlayersReady -= FadeVolume;
        allPlayersReady -= StartGame;
        allPlayersReady -= StopTimerRoutine;
        playerReady -= ToggleReadyBox;
        playerReady -= PlayerReadyStateChange;
    }
    

    private void ChangeTeam()
    {
        QueueManager.i.GoNextTeam();
    }

    public static void StartGame()
    {
        Message m = Message.Create(MessageSendMode.Reliable, ClientToServerCalls.CHANGE_SCENE);
        m.AddInt((int)Scenes.SONG_SELECT_SERVER);
        ClientManager.SendMessage(m);
        OnNbToursChange?.Invoke();



        PoolManager.i.ClientPools.clickNotePool.DespawnAll(true);
        PoolManager.i.ClientPools.holdNotePool.DespawnAll(true);
        GameManager.Instance.TutorielEnCours = false;

        if (hasGameEnded)
        {
            GameManager.Instance.StartCoroutine(LoadFromSceneToScene(Scenes.ENDGAME, Scenes.SONG_SELECT));
            GameManager.mapSelectionnee = MapCollection.i.mapsAffichees[0];
            SongSelectManager.MapSelectionneeIndex = 0;
        }
        else
        {
            GameManager.Instance.StartCoroutine(LoadFromSceneToScene(Scenes.TEAM_READY, Scenes.TUTORIAL_CHOICE));
        }

    }
    
    public void FadeVolume(){
        StartCoroutine(FadeVolumeRoutine());
    }

    public IEnumerator FadeVolumeRoutine(){ 
        float timer = 1;
        while(timer > 0){ 
            print("Set volume to " + timer);
            source.volume = timer;
            timer -= Time.deltaTime * 2;
            yield return null;
        }
        source.volume = 0;
    }


    public static IEnumerator LoadFromSceneToScene(Scenes sceneToUnload, Scenes sceneToLoad)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if(SceneManager.GetSceneAt(i).buildIndex == (int) Scenes.TRANSITION_SCENE)
            {
                SceneManager.UnloadSceneAsync((int)Scenes.TRANSITION_SCENE);
            }
        }
        
        SceneManager.LoadScene((int)Scenes.TRANSITION_SCENE, LoadSceneMode.Additive);
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene((int)sceneToLoad, LoadSceneMode.Additive);
        yield return new WaitForSeconds(0.5f);
        SceneManager.UnloadSceneAsync((int)sceneToUnload);
        yield return new WaitForSeconds(1f);
        hasGameEnded = false;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.C)) SceneManager.LoadScene((int) Scenes.MENU); // im lazy i know
        
        
        if (ReadyPlayers != Constantes.NOMBRE_JOUEURS)
        {
            if (Input.GetKeyDown(Constantes.TOUCHES[0, 0]))
            {
                _readyP1 = !_readyP1;
                playerReady?.Invoke(_readyP1, 1);
            }
            else if (Input.GetKeyDown(Constantes.TOUCHES[1, 0]))
            {
                _readyP2 = !_readyP2;
                playerReady?.Invoke(_readyP2, 2);
            }
        }
    }

    private Coroutine[] routines = new Coroutine[Constantes.NOMBRE_JOUEURS];

    private void ToggleReadyBox(bool ready, int player)
    {
        if (routines[player - 1] != null)
        {
            StopCoroutine(routines[player - 1]);
        }
        
        if (player == 1)
        {
            p1ReadyBox.Text = ready ? readyMsg : notReadyMsg;
        }
        else
        {
            p2ReadyBox.Text = ready ? readyMsg : notReadyMsg;
        }

        LightsClient.SetReady(player-1, ready);
        routines[player - 1] = StartCoroutine(ChangerVisuelsBoite(ready, player == 1 ? square1 : square2));

    }



    private IEnumerator ChangerVisuelsBoite(bool ready, UIBlock2D block)
    {
        Color originalColor = block.Color;
        Color targetColor = ready ? selectedColor : unselectedColor;

        Vector3 originalSize = block.transform.localScale;
        Vector3 targetSize = ready ? new Vector3(animationStrength, animationStrength, 1) : Vector3.one;
        

        float timer = 0;
        while(timer < animationLength)
        {
            yield return null;
            timer += Time.deltaTime;
            float proportion = animationCurve.Evaluate(timer / animationLength);
            block.Color = Color.Lerp(originalColor, targetColor, proportion);
            block.transform.localScale = Vector3.Lerp(originalSize, targetSize, proportion);
        }

    }

    private void PlayerReadyStateChange(bool ready, int player)
    {
        ReadyPlayers += ready ? 1 : -1;
    }



    float bgColorTimer = 0;
    IEnumerator ChangerBGColor(float speed)
    {
        while (true)
        {
            yield return null;
            bgColorTimer += Time.deltaTime * speed;
            bgColorTimer %= 1;
            bgBlock.Gradient.Color = Color.HSVToRGB(bgColorTimer, 0.75f, 0.8f);
        }
    }
}
