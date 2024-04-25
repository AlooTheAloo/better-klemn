using ChromaWeb.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour {

    [Header("Refs")]
    [SerializeField] List<TextMeshPro> textesCombo;
    [SerializeField] TextMeshPro texteScore;
    [SerializeField] TextMeshPro textePrecision;

    [Header("Animations")]
    [SerializeField] float dureeAnimationScore = 0.25f;
    [SerializeField] AnimationCurve animationScoreCourbe;

    private ulong scoreParfait, score, scoreAAfficher, scoreSansCombo;
    private float precision;
    private int[] combos, meilleursCombos, nombreNotesTotal, nombreNotesRatees;

    private Coroutine coroutineEnCours;
    
    public static Action<ulong> OnScoreChange;

    private ScoreApiClient _scoreApiClient;


    private void OnEnable() {

        StaticStats.Reset();
        combos = new int[Constantes.NOMBRE_JOUEURS];
        meilleursCombos = new int[Constantes.NOMBRE_JOUEURS];
        nombreNotesTotal = new int[Constantes.NOMBRE_JOUEURS];
        nombreNotesRatees = new int[Constantes.NOMBRE_JOUEURS];

        for (int i = 0; i < combos.Length; i++) {
            combos[i] = 0;
            meilleursCombos[i] = 0;
            nombreNotesTotal[i] = 0;
            nombreNotesRatees[i] = 0;
        }

        Alley.onJugement += AjouterScore;
        Alley.onJugement += TraiterCombo;
        NoteManager.i.onMapOver += SendFinalScore;
    }

    private void OnDisable() {
        Alley.onJugement -= AjouterScore;
        Alley.onJugement -= TraiterCombo;
        NoteManager.i.onMapOver -= SendFinalScore;
    }

    private void Start() {
        score = 0;
        scoreAAfficher = 0;
        scoreParfait = 0;
        precision = 100;
    }


    private void Awake()
    {
        _scoreApiClient = new ScoreApiClient(Constantes.API_BASE_URL);
    }


    private void SendFinalScore()
    {
        print($"Score final pour l'équipe {StaticStats.equipe.TeamName} avec un score de {StaticStats.score}");
        Map currentMap = GameManager.Instance.gameNoteData.map;
        if (currentMap.metadonnees.chansonId != -1) // si le tutoriel
        {
            InsertScoreInfo scoreInfo = new(
            StaticStats.score,
            currentMap.metadonnees.chansonId,
            StaticStats.equipe.TeamId);
            _scoreApiClient.AddScoreInfoAsync(scoreInfo);
        }
    }

    public void AjouterScore(int joueur, Precision precision, Alley a) {

        nombreNotesTotal[joueur]++;
        StaticStats.nombreNotesTotal[joueur] = nombreNotesTotal[joueur];
        float scoreParfaitBrut = Constantes.POINTS_PRECISION[0];
        scoreParfait += (ulong)Mathf.Round(scoreParfaitBrut);
        
        ulong scoreBrut = Constantes.POINTS_PRECISION[(int)precision];
        scoreSansCombo += scoreBrut;
        ulong scoreAAjouter = scoreBrut;

        if (combos[joueur] > 0) {
            scoreAAjouter = (ulong)Mathf.Round(scoreBrut + (combos[joueur] * Constantes.MULTIPLICATEUR_COMBO * scoreBrut));
        }

        if (coroutineEnCours != null) {
            StopCoroutine(coroutineEnCours);
            scoreAAfficher = score;
        }

        if (scoreParfait != 0) {
            double precisionBrute = 100 * (double)scoreSansCombo / scoreParfait;
            this.precision = (float)Math.Round(precisionBrute, 2);
            StaticStats.precision = this.precision;
        }

        coroutineEnCours = StartCoroutine(AnimerScore(score, score + scoreAAjouter));
        score += scoreAAjouter;

        StaticStats.score = score;

    }

    const int paddingScore = 10;
    private void OnGUI() {
        texteScore.text = $"{PadScore(scoreAAfficher, paddingScore)}";
        textePrecision.text = $"{precision}%";
        for (int i = 0; i < Constantes.NOMBRE_JOUEURS; i++) {
            textesCombo[i].text = $"{combos[i]}x";
        }
    }

    private string PadScore(ulong score, int nombrePadding)
    {
        // string used in Format() method 
        string stringPadding = "{0:";
        for (int i = 0; i < nombrePadding; i++)
        {
            stringPadding += "0";
        }
        stringPadding += "}";

        string value = string.Format(stringPadding, score);

        return value;
    }

    private IEnumerator AnimerScore(ulong scoreInitial, ulong nouveauScore) {

        float compteur = 0f;

        while (compteur < dureeAnimationScore) {
            compteur += Time.deltaTime;
            scoreAAfficher = (ulong)Mathf.Round(Mathf.Lerp(scoreInitial, nouveauScore, animationScoreCourbe.Evaluate(compteur / dureeAnimationScore)));
            yield return new WaitForEndOfFrame();
        }
        scoreAAfficher = nouveauScore;
        OnScoreChange?.Invoke(scoreAAfficher);
    }

    private void TraiterCombo(int joueur, Precision precision, Alley a) {

        switch (precision) {

            case Precision.PARFAIT:
                StaticStats.nombreNotesParfait[joueur]++; break;

            case Precision.BIEN:
                StaticStats.nombreNotesBien[joueur]++; break;

            case Precision.OK:
                StaticStats.nombreNotesOk[joueur]++; break;

            case Precision.RATE:
                combos[joueur] = 0;
                nombreNotesRatees[joueur]++;
                StaticStats.nombreNotesRatees[joueur]++;
                break;
        }

        if (precision != Precision.RATE) {
            combos[joueur]++;
        }

        if (combos[joueur] > meilleursCombos[joueur])
        {
            meilleursCombos[joueur] = combos[joueur];
            StaticStats.meilleursCombos[joueur] = combos[joueur];
        }

    }

}