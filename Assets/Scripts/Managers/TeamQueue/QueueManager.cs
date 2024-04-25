using ChromaWeb.Database;
using Nova;
using System;
using System.Collections;
using System.Threading.Tasks;
using Riptide;
using UnityEngine;
using UnityEngine.SceneManagement;


public class QueueManager : MonoBehaviour
{
    public static QueueManager i;

    public static Action<TeamInfo> onFindTeam;
    
    private TeamApiClient _teamApiClient;
    public TeamInfo? currentTeam = null;
    private int NbToursDuServeur = 0;

    private const int POLLING_DELAI_TEAMINFO = 4;
    private const int POLLING_DELAI_NBTOURS = 2;


    private event Action hearBeatTeamInfo;
    private event Action hearBeatNbTours;


    public static event Action OnNextTeamFound;
    public static event Action OnNoTeamFound;

    

    private void Start()
    {
    
        if (i)
        {
            Destroy(this);
            return;
        }
        i = this;
        _teamApiClient = new TeamApiClient(Constantes.API_BASE_URL);
        print("Creating teamapiclient with ip " + Constantes.API_BASE_URL);
        
        
        DontDestroyOnLoad(this);
        StartCoroutine(PollingRoutineTeamInfo(POLLING_DELAI_TEAMINFO));
        StartCoroutine(PollingRoutineNbTours(POLLING_DELAI_NBTOURS));
        
        hearBeatNbTours += GetNombreToursFromServ;
        hearBeatTeamInfo += GetTeam;

        hearBeatTeamInfo?.Invoke();
        hearBeatNbTours?.Invoke();
    }

    private void OnEnable() {
        SceneManager.sceneLoaded += Add;
        hearBeatNbTours?.Invoke();
    }

    public void Add(Scene s, LoadSceneMode mode){
        if(s.buildIndex != (int) Scenes.INTRO) return;
        GoNextTeam();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= Add;
        hearBeatNbTours -= GetNombreToursFromServ;
        hearBeatTeamInfo -= GetTeam;
    }

    private async void GetTeam()
    {
        await GetTeamInfos();
    }

    public TeamInfo? GetCurrentTeam()
    {
        return currentTeam;
    }

    public string GetCurrentTeamName()
    {
        return currentTeam.Value.TeamName;
    }

    private bool ouiOuNon;
    
    private async Task GetTeamInfos()
    {
        print("getting team info");

        if (currentTeam != null && !ouiOuNon) return;
        ouiOuNon = false;
        print("getting team info");
        TeamInfo? listTeamInfo = await _teamApiClient.GetNextTeamAsync();
        if (listTeamInfo != null)
        {
            print("Its in !");
            currentTeam = listTeamInfo.Value;
            onFindTeam?.Invoke(currentTeam.Value);

            StaticStats.equipe = (TeamInfo) currentTeam;
            if (NbToursDuServeur != Constantes.NOMBRE_TOURS)
            {
                StaticStats.equipe.NbTours = NbToursDuServeur;
            }
            OnNextTeamFound?.Invoke();
        }
        else
        {
            print("Its not in !");
            OnNoTeamFound?.Invoke();
        }
    }
    public void GoNextTeam()
    {
        ouiOuNon = true;
        hearBeatTeamInfo?.Invoke();
    }

    private async void GetNombreToursFromServ()
    {
        try
        {
            NbToursDuServeur = await _teamApiClient.GetNombreTours();
            if (NbToursDuServeur != Constantes.NOMBRE_TOURS)
            {
                print($"Nouveau nombre de tours pour la prochaine équipe à : {NbToursDuServeur}");
                Constantes.NOMBRE_TOURS = NbToursDuServeur;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            print(e);
        }
        print("Nbtours is " + Constantes.NOMBRE_TOURS);
    }

    IEnumerator PollingRoutineTeamInfo(float seconds)
    {
        while (true)
        {
            hearBeatTeamInfo?.Invoke();
            yield return new WaitForSeconds(seconds);
        }
    }
    IEnumerator PollingRoutineNbTours(float seconds)
    {
        while (true)
        {
            hearBeatNbTours?.Invoke();
            yield return new WaitForSeconds(seconds);
        }
    }
}
