using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    internal static float plateauBase;
    
    private void Start()
    {
        plateauBase = DeterminerPlateau();
    }

    private void OnEnable()
    {
        NoteManager.i.onMapOver += LoadFinalScorePanelCoroutine;
        PauseManager.onRetour += QuitterApresPauseMenuRoutine;
    }
    private void OnDisable()
    {
        NoteManager.i.onMapOver -= LoadFinalScorePanelCoroutine;
        PauseManager.onRetour -= QuitterApresPauseMenuRoutine;
    }

    private void LoadFinalScorePanelCoroutine()
    {
        StartCoroutine(LoadFinalScorePanel(3));
    }
    
    private void QuitterApresPauseMenuRoutine()
    {
        StartCoroutine(LoadFinalScorePanel(0));
    }

    private IEnumerator LoadFinalScorePanel(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        SceneManager.UnloadSceneAsync((int)Scenes.GAME);
        SceneManager.LoadScene((int)Scenes.TRANSITION_SCENE, LoadSceneMode.Additive);
    }

    /// <summary>
    /// Utilise la difficulte generale de la carte pour determiner le premier plateau de temps ou le joueur peut avoir
    /// une note parfait
    /// </summary>
    /// <returns>Le premier plateau de temps de la carte</returns>
    private float DeterminerPlateau()
    {
        float difficulteGenerale = GameManager.Instance.gameNoteData.map.metadonnees.od;
        float diffTolerance = Constantes.MAX_MS_TOLERANCE - Constantes.MIN_MS_TOLERANCE;
        float diffOd = (Constantes.MAX_OD - Constantes.MIN_OD) - difficulteGenerale;
        return ((diffTolerance / Constantes.MAX_OD) * diffOd + Constantes.MIN_MS_TOLERANCE)
            / Constantes.MS_DANS_SECONDE;

    }
}
