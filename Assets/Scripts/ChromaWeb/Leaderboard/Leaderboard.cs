using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ChromaWeb.Database;
using Nova;
using UnityEngine;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] private UIBlock leaderboardBackground;
    [SerializeField] private TextBlock mapName;
    [SerializeField] private TextBlock artistName;
    [SerializeField] private ListView leaderboardListView;
    [SerializeField] private Color[] colors;
    [SerializeField] private AnimationCurve changerCouleurBackgroundCurve;
    [SerializeField] private float changerCouleurBackgroundTemps;
    [SerializeField] private TextBlock scoreTracker;
    [SerializeField] private Camera leaderboardCamera;

    private List<ScoreInfo> _scoreList = new List<ScoreInfo>();
    private List<TeamInfo> _teamInfos = new List<TeamInfo>();
    private MapCollectionMap _mapSelectionee;
    private ScoreApiClient _scoreApiClient;
    private TeamApiClient _teamApiClient;

    private Coroutine _coroutineBackground;

    private static Leaderboard leaderboard;
    
    private void Awake()
    {
        if(leaderboard){
            Destroy(gameObject); 
            Destroy(leaderboardCamera);
            return;
        }

        leaderboard = this;
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(leaderboardCamera);
        _scoreApiClient = new ScoreApiClient(Constantes.API_BASE_URL);
        _teamApiClient = new TeamApiClient(Constantes.API_BASE_URL);
        leaderboardListView.AddDataBinder<ScoreInfo, ScoreVisuals>(BindScores);
        for (int i = 0; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
        }
    }

    private void OnSongChange(int mapIndex)
    {
        leaderboardListView.SetDataSource(new List<ScoreInfo>());
        _mapSelectionee = MapCollection.i.mapsAffichees[mapIndex];
        mapName.Text = _mapSelectionee.mapMetadonnees.titre;
        artistName.Text = _mapSelectionee.mapMetadonnees.artiste;
        if(_coroutineBackground != null) StopCoroutine(_coroutineBackground);
        _coroutineBackground = StartCoroutine(ChangerCouleurBackground(_mapSelectionee.couleur));
        LoadScores();
    }

    private async void LoadScores()
    {
        _scoreList = await _scoreApiClient.GetScoreInfoAsync(_mapSelectionee.mapMetadonnees.chansonId);
        _scoreList.Sort((x, y) => y.ScoreValue.CompareTo(x.ScoreValue));
        _teamInfos = await _teamApiClient.ListTeamInfoAsync();
        leaderboardListView.SetDataSource(_scoreList);
        DisableScoreTracker();
    }

    private async void LoadScores(int mapID)
    {
        _scoreList = await _scoreApiClient.GetScoreInfoAsync(mapID);
        _scoreList.Sort((x, y) => y.ScoreValue.CompareTo(x.ScoreValue));
        _teamInfos = await _teamApiClient.ListTeamInfoAsync();
        leaderboardListView.SetDataSource(_scoreList);
        DisableScoreTracker();
    }
    
    private void BindScores(Data.OnBind<ScoreInfo> evt, ScoreVisuals visuals, int index)
    {
        visuals.Position.Text = (index + 1).ToString();
        visuals.TeamName.Text = _teamInfos.Find(x => x.TeamId == evt.UserData.TeamId).TeamName;
        visuals.ScoreValue.Text = evt.UserData.ScoreValue.ToString();
        if(index < colors.Length) visuals.Position.Parent.Color= colors[index];
        visuals.TeamName.Parent.Color = _mapSelectionee.couleur;
        Color couleurMap = _mapSelectionee.couleur;
        (float hue, float saturation, float value) couleurHSV;
        Color.RGBToHSV(couleurMap, out couleurHSV.hue, out couleurHSV.saturation, out couleurHSV.value);
        couleurHSV.value = 0.5f;
        Color targetColor = Color.HSVToRGB(couleurHSV.hue, couleurHSV.saturation, couleurHSV.value);
        visuals.ScoreValue.Parent.Color = targetColor;
        visuals.TeamName.Parent.Color = targetColor;
        if(index >= colors.Length)visuals.Position.Parent.Color = targetColor;
    }

    private void OnEnable()
    {
        SongSelectManager.onMapSelectedChanged += OnSongChange;
        GameManager.onGameLoaded += OnGameLoaded;
        StatsGameOver.onStatsSceneLoaded += LoadScores;
    }
    
    private void OnDisable()
    {
        SongSelectManager.onMapSelectedChanged -= OnSongChange;
        GameManager.onGameLoaded -= OnGameLoaded;
        StatsGameOver.onStatsSceneLoaded -= LoadScores;
    }

    IEnumerator ChangerCouleurBackground(Color nouvelleCouleur)
    {
        float timer = 0.0f;
        while (timer < changerCouleurBackgroundTemps)
        {
            leaderboardBackground.Color = Color.Lerp(leaderboardBackground.Color, nouvelleCouleur,
                changerCouleurBackgroundCurve.Evaluate(timer / changerCouleurBackgroundTemps));
            yield return null;
            timer += Time.deltaTime;
        }
        leaderboardBackground.Color = nouvelleCouleur;
    }

    private void OnGameLoaded()
    {
        scoreTracker.Text = string.Empty;
        scoreTracker.gameObject.SetActive(true);
    }


    private void DisableScoreTracker()
    {
        scoreTracker.gameObject.SetActive(false);
    }

}
