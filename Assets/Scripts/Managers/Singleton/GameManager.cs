using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;
using System;
using System.Collections.Generic;
using Riptide;
using UnityEngine.Video;

public class GameManager : MonoBehaviour
{
    [HideInInspector] public GameNoteData gameNoteData;
    [HideInInspector] public AudioData audioData;
    [HideInInspector] public ImageData imageData;
    [HideInInspector] public string videoUrl;

    private int preloaderCompleteCount = 0;
    internal List<IPreloader> preloaders = new();

    public static GameManager Instance;
    public static BuildConfig buildConfig;
    public static event Action onGameLoaded;
    public static int _mapSelectionneeIndex;

    [HideInInspector]
    public bool TutorielEnCours;
    

    //so that we dont unnecessarily notify the leaderboard
    public static event Action OnUILoad;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _mapSelectionneeIndex = 0;
        TutorielEnCours = false;
}

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnStartLoading; // TODO : please no 
        _mapSelectionneeIndex = 0;
        TutorielEnCours = false;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnStartLoading; // TODO : please no 
    }


    void OnStartLoading(Scene b, LoadSceneMode mode)
    {
        if (b.buildIndex != (int) Scenes.LOADING) return;
        if (TeamReadyManager.hasGameEnded) return;
        if (TutorielEnCours) mapSelectionnee = MapCollection.i.Tutoriel().Value;
        Message changeSceneMessage = Message.Create(MessageSendMode.Reliable, ClientToServerCalls.CHANGE_SCENE);
        changeSceneMessage.AddInt((int)Scenes.GAME_SERVER);
        ClientManager.SendMessage(changeSceneMessage);

        preloaderCompleteCount = 0;
        preloaders.Clear();

        preloaders.Add(new GameNotePreloader().CommencerChargement((gameNoteData) =>
        {
            preloaders.Add(new GameLightsPreloader(gameNoteData.map).CommencerChargement((x) =>
            {
                PreloaderComplete();
            }));
                this.gameNoteData = gameNoteData;
                PreloaderComplete();
            })
        );

        preloaders.Add(new GameAudioPreloader(gameNoteData.map.metadonnees.fichiers.audio).CommencerChargement((audioData) =>
        {
            this.audioData = audioData;
            PreloaderComplete();
        }));

        preloaders.Add(new GameImagePreloader().CommencerChargement((imageData) =>
        {
            this.imageData = imageData;
            PreloaderComplete();
        }));

        videoUrl = mapSelectionnee.mapMetadonnees.fichiers.video;
    }

    private void PreloaderComplete()
    {
        preloaderCompleteCount++;
        if (preloaderCompleteCount == preloaders.Count)
        {
            if (!TeamReadyManager.hasGameEnded)
            {
                SceneManager.LoadScene((int)Scenes.GAME, LoadSceneMode.Additive);
                if (Instance.TutorielEnCours) {
                    SceneManager.UnloadSceneAsync((int)Scenes.TUTORIAL_CHOICE);
                } else {
                    SceneManager.UnloadSceneAsync((int)Scenes.SONG_SELECT);
                }
                onGameLoaded?.Invoke();
            }
            else
            {
                if (Instance.TutorielEnCours) {
                    SceneManager.UnloadSceneAsync((int)Scenes.TUTORIAL_CHOICE);
                } else {
                    SceneManager.LoadScene((int)Scenes.SONG_SELECT, LoadSceneMode.Additive);
                    SceneManager.UnloadSceneAsync((int)Scenes.ENDGAME);
                }
            }
            OnUILoad?.Invoke();

        }

    }

    public static MapCollectionMap mapSelectionnee {
        get {
            if (Instance.TutorielEnCours)
            {
                return MapCollection.i.Tutoriel().Value;
            }
            else
            {
                if (_mapSelectionneeIndex < 0) {
                    return MapCollection.i.mapsAffichees[_mapSelectionneeIndex+1];
                } else if (_mapSelectionneeIndex+1 > MapCollection.i.mapsAffichees.Count) {
                        return MapCollection.i.mapsAffichees[_mapSelectionneeIndex-1];
                } else {
                    return MapCollection.i.mapsAffichees[_mapSelectionneeIndex];
                }
                
            }
        }
        set { }
    }
}

public struct BuildConfig
{
    public BuildType buildType;
    public bool isDev;
    public string chromaWebApiBaseUrl;
    public string lightApiBaseUrl;

    public BuildConfig(BuildType type, bool isDev, string chromaWebApiBaseUrl, string lightApiBaseUrl)
    {
        buildType = type;
        this.isDev = isDev;
        this.chromaWebApiBaseUrl = chromaWebApiBaseUrl;
        this.lightApiBaseUrl = lightApiBaseUrl;
    }
}

public enum BuildType
{
    CLIENT,
    SERVER,
    ATTENTE
}