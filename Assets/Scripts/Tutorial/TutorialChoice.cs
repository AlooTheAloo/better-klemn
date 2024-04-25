using System;
using System.Collections;
using ChromaWeb.Database;
using Nova;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialChoice : MonoBehaviour
{
    [SerializeField] private TextBlock texteJoueurHote;
    
    bool dejaChoisi = false;
    [SerializeField]
    KeyCode choixOui;

    [SerializeField]
    KeyCode choixNon;

    [SerializeField] private Camera tutorialCamera;
    public static event Action<Camera> onTutorialChoiceEntered;


    private bool debloque = false;

    void Update()
    {
        if (dejaChoisi || !debloque) return;
    
        if (Input.GetKeyDown(choixNon)) 
        {
            dejaChoisi = true;
            StartCoroutine(TeamReadyManager.LoadFromSceneToScene(Scenes.TUTORIAL_CHOICE, Scenes.SONG_SELECT));
        } 
        else if (Input.GetKeyDown(choixOui))
        {
            dejaChoisi = true;
            var map = MapCollection.i.Tutoriel();
            if (!map.HasValue) 
            {
                StartCoroutine(TeamReadyManager.LoadFromSceneToScene(Scenes.TUTORIAL_CHOICE, Scenes.SONG_SELECT));
            }
            else 
            {
                GameManager.mapSelectionnee = map.Value;
                GameManager.Instance.TutorielEnCours = true;
                SceneManager.LoadScene((int)Scenes.LOADING, LoadSceneMode.Additive);
            }

        }
    }

    private void Awake()
    {
        LightsClient.SetLedMode(LedLightState.Pulse);
        TeamInfo? equipe = QueueManager.i.GetCurrentTeam();
        if (equipe.HasValue)
        {
            texteJoueurHote.Text = $"{equipe.Value.NomPremierJoueur} est l'hôte";
        }
        
        StartCoroutine(DebloquerApresTransition(1f));
        onTutorialChoiceEntered?.Invoke(tutorialCamera);
    }


    private IEnumerator DebloquerApresTransition(float longueurTransition)
    {
        yield return new WaitForSeconds(longueurTransition);
        debloque = true;
    }
    
}
