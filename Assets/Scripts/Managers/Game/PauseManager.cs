using UnityEngine;
using System;
using Shapes;
using TMPro;
using NovaSamples.UIControls;
using UnityEngine.SceneManagement;
using Nova;

public class PauseManager : MonoBehaviour
{
    public static event Action onRetour; 
    public static event Action<bool> EnPause;
    [SerializeField] KeyCode Pause;
    [SerializeField] UIBlock2D backgroundMenu;

    public bool enPause = false;

    private void Start() {
        affichageMenu(enPause);
    }

    private void Update() {
        
        if (Input.GetKeyDown(Pause)) {
            lancerPause();
        }

        if (enPause) {
            if (Input.GetKeyDown(KeyCode.R)) {
                lancerPause();
            }
            else if (Input.GetKeyDown(KeyCode.M)) {
                onRetour?.Invoke();
            }
        }

    }

    private void OnEnable() {
        enPause = false;
        affichageMenu(false);
    }

    private void OnDisable() {
        enPause = false;
        affichageMenu(false);
    }

    private void affichageMenu(bool actif) {
        backgroundMenu.gameObject.SetActive(actif);
    }

    private void lancerPause() {
        enPause = !enPause;
        EnPause.Invoke(enPause);
        affichageMenu(enPause);
    }


}
