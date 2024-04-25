using Nova;
using NovaSamples.Effects;
using Riptide;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StatsGameOver : MonoBehaviour
{
    [Header("Global")] 
    
    private AudioSource musique;

    private bool alreadyPressed;
    
    [SerializeField]
    List<TextBlock> elementsGlobaux;

    [SerializeField]
    TextBlock scoreTotal;

    [SerializeField]
    TextBlock precisionGlobale;

    [SerializeField]
    TextBlock nomEquipe;

    [SerializeField]
    TextBlock texteEncouragement;

    [SerializeField]
    Camera mainCamera;

    [Header("Infos joueurs")]
    [SerializeField]
    List<PlayerScoreBlocks> listePlayerScoreBlocks;

    [Header("Map")]
    [SerializeField]
    TextBlock mapName;

    [SerializeField]
    TextBlock mapArtiste;

    public BlurEffect blurEffect;

    public static event Action<Camera> onStatsSceneLoad;
    public static event Action<int> onStatsSceneLoaded;

    const int MIN_PLAGE = 60;
    const int MAX_PLAGE = 100;

    [SerializeField]
    List<Grade> listeGrades;

    [Serializable]
    public struct Grade
    {
        public string grade;

        [MinMaxSlider(MIN_PLAGE, MAX_PLAGE, true)]
        public Vector2 plage;

#nullable enable
        public Texture2D? clipMask;
#nullable disable
        public Color color;

        public string[] commentaire;
    }

    [Serializable]
    public struct PlayerScoreBlocks
    {
        public TextBlock playerName;
        
        public TextBlock precision;

        public TextBlock maxCombo;

        public TextBlock parfait;

        public TextBlock bien;

        public TextBlock ok;

        public TextBlock rate;
    }

    private void onEnable() {
        alreadyPressed = false;
    }

    private void Start()
    {
        Message m = Message.Create(MessageSendMode.Reliable, ClientToServerCalls.WAITINGROOM_STATE);
        m.AddInt((int)WaitingRoomState.LOGO);
        ClientManager.SendMessage(m);
        
        musique = FindObjectOfType<AudioSource>();
        onStatsSceneLoad?.Invoke(mainCamera);
        onStatsSceneLoaded?.Invoke(GameManager.mapSelectionnee.mapMetadonnees.chansonId);

        var r = new System.Random();

        float precisionJoueur = StaticStats.precision;

        Grade precisionGrade = PrecisionCalculator(precisionJoueur);

        foreach (TextBlock block in elementsGlobaux)
        {
            block.Color = precisionGrade.color;
            if (precisionGrade.clipMask != null)
            {
                var c = block.gameObject.AddComponent<ClipMask>();
                c.Mask = precisionGrade.clipMask;
            }
        }

        precisionGlobale.Text = precisionGrade.grade;
        scoreTotal.Text = $"{StaticStats.score}";
        texteEncouragement.Text = precisionGrade.commentaire[r.Next(precisionGrade.commentaire.Length)];

        blurEffect.InputTexture = GameManager.mapSelectionnee.imageArriere;
        blurEffect.Reblur();

        mapName.Text = GameManager.mapSelectionnee.mapMetadonnees.titre;
        mapArtiste.Text = GameManager.mapSelectionnee.mapMetadonnees.artiste;
        nomEquipe.Text = StaticStats.equipe.TeamName;


        var i = 0;
        foreach (var block in listePlayerScoreBlocks)
        {
            block.playerName.Text =
                i % 2 == 0 ? StaticStats.equipe.NomPremierJoueur : StaticStats.equipe.NomSecondJoueur;
            
            block.precision.Text = $"{StaticStats.precisions[i]} %";
            block.maxCombo.Text = $"x{StaticStats.meilleursCombos[i]}";
            block.parfait.Text = $"{StaticStats.nombreNotesParfait[i]}";
            block.bien.Text = $"{StaticStats.nombreNotesBien[i]}";
            block.ok.Text = $"{StaticStats.nombreNotesOk[i]}";
            block.rate.Text = $"{StaticStats.nombreNotesRatees[i]}";

            i++;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(Constantes.TOUCHES[0, 0]) && !alreadyPressed)
        {
            alreadyPressed = true;
            if (StaticStats.equipe.NbTours - 1 <= 0)
            {
                TeamReadyManager.hasGameEnded = false;

                Message m = Message.Create(MessageSendMode.Reliable, ClientToServerCalls.CHANGE_SCENE);
                m.AddInt((int) Scenes.INTRO_SERVER);
                ClientManager.SendMessage(m);

                StartCoroutine(TeamReadyManager.LoadFromSceneToScene(Scenes.ENDGAME, Scenes.CREDIT));
                
            }
            else
            {
                if (GameManager.mapSelectionnee.mapMetadonnees.chansonId != -1)
                    StaticStats.equipe.NbTours--;
                
                TeamReadyManager.StartGame();

            }

            StartCoroutine(lowerMusique());
        }
    }


    // Baisse le volume de la musique 1 -> 0 en 1 seconde
    private IEnumerator lowerMusique()
    {
        float timer = 1f;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
            musique.volume = timer;
        }
        Destroy(mainCamera.gameObject.GetComponent<AudioListener>());
    }
    

    public Grade PrecisionCalculator(float precision)
    {
        foreach (Grade grade in listeGrades)
        {
            if (precision <= grade.plage.y && precision >= grade.plage.x)
            {
                return grade;
            }
        }
        return listeGrades.Last();
    }
}
