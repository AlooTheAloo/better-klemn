using System;
using Chroma.Editor;
using Discord;
using UnityEngine;

public class DiscordController : MonoBehaviour
{
    public long applicationID;

    private const string chromaEditeur = "chroma_editor";
    private const string gameLogo = "game_logo";

    private long time;

    private static bool instanceExists;
    private Discord.Discord discord;

    private ActivityManager activityManager;

    void Awake() 
    {
        // Transition the GameObject between scenes, destroy any duplicates
        if (!instanceExists)
        {
            instanceExists = true;
            DontDestroyOnLoad(gameObject);
        }
        else if (FindObjectsOfType(GetType()).Length > 1)
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        EditorMapManager.OnMapChanged += UpdateEditorMapDiscordInfo;
        SongSelectManager.onMapSelectedChanged += UpdateMapDiscordInfo;
        GameManager.onGameLoaded += A;
        ScoreManager.OnScoreChange += UpdateInGameEnterDiscordInfo;
    }

    private void UpdateInGameEnterDiscordInfo(ulong score)
    {
        var activity = new Activity
        {
            Details = $"{GameManager.Instance.gameNoteData.map.metadonnees.titre}",
            State = $"Équipe : {StaticStats.equipe.TeamName} Score : {score} pts, Acc : {StaticStats.meilleursCombos[0]}",
            Assets = 
            {
                LargeImage = gameLogo,
                LargeText = $"{GameManager.Instance.gameNoteData.map.metadonnees.titre}"
            },
            Timestamps =
            {
                Start = time
            }
        };
        
        UpdateStatus(activity);
    }

    private void UpdateMapDiscordInfo(int mapIndex)
    {
        var map = MapCollection.i.mapsAffichees[mapIndex];
        var activity = new Activity
        {
            Details = $"Song : {map.nomMap}",
            State = "Selecting Map...",
            Assets = 
            {
                LargeImage = gameLogo
            },
            Timestamps =
            {
                Start = time
            }
        };
        
        UpdateStatus(activity);
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void OnDisable()
    {
        EditorMapManager.OnMapChanged -= UpdateEditorMapDiscordInfo;
        SongSelectManager.onMapSelectedChanged -= UpdateMapDiscordInfo;
        GameManager.onGameLoaded -= A;
        ScoreManager.OnScoreChange -= UpdateInGameEnterDiscordInfo;
    }


    void A()
    {
        UpdateInGameEnterDiscordInfo(0);
    }

    void Start()
    {
        // Log in with the Application ID
        discord = new Discord.Discord(applicationID, (UInt64)CreateFlags.NoRequireDiscord);

        time = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        
        activityManager = discord.GetActivityManager();
    }

    void Update()
    {
        // Destroy the GameObject if Discord isn't running
        try
        {
            discord.RunCallbacks();
        }
        catch
        {
            Destroy(gameObject);
        }
    }


    private void UpdateEditorMapDiscordInfo(EditorMap obj)
    {
        var activity = new Activity
        {
            Details = $"Editing : {obj.MapVisualData.nomMap}",
            State = $"Effects : {obj.ListeEffets.Count}",
            Assets = 
            {
                LargeImage = chromaEditeur,
                LargeText = "Chroma Editor"
            },
            Timestamps =
            {
                Start = time
            }
        };

        UpdateStatus(activity);
    }

    void UpdateStatus(Activity activity)
    {
        // Update Status every frame
        try
        {
            activityManager.UpdateActivity(activity, (res) =>
            {
                if (res != Result.Ok) Debug.LogWarning("Failed connecting to Discord!");
            });
        }
        catch
        {
            // If updating the status fails, Destroy the GameObject
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        discord.Dispose();
    }
}