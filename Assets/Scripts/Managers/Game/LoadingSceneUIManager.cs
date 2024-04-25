using Nova;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class LoadingSceneUIManager : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Camera loadingSceneCamera;
    [SerializeField] private Animator animator;
    [SerializeField] private List<UIBlock2D> mainColorBands;
    [SerializeField] UIBlock2D background;
    [SerializeField] TextBlock mapNameAndArtist;

    [SerializeField] TextBlock diffText;
    [SerializeField] TextBlock timeText;
    [SerializeField] TextBlock bpmText;
    [SerializeField] TextBlock arText;

    //default values for any use other than transitioning to a map
    [SerializeField] private Texture2D ChromaBackground;
    [SerializeField] private float animatorSpeed;

    private Camera currentCamera;

    private void OnEnable()
    {
        GameManager.OnUILoad += OnCompleteLoad;
        
    }

    private void OnDisable()
    {
        GameManager.OnUILoad -= OnCompleteLoad;
    }

    private void Start()
    {
        print("hge is " + TeamReadyManager.hasGameEnded);
        if (!TeamReadyManager.hasGameEnded)
        {
            print(GameManager.mapSelectionnee.mapMetadonnees.titre);
            
            LightsClient.SetLedMode(LedLightState.Pulse);
            foreach (var block in mainColorBands)
            {
                block.Color = GameManager.mapSelectionnee.couleur;
            }
            background.SetImage(GameManager.mapSelectionnee.imageArriere);
            mapNameAndArtist.Text = GameManager.mapSelectionnee.nomMap;
                                
            DateTime time = new(0);
            time = time.AddSeconds(GameManager.mapSelectionnee.mapMetadonnees.duree.dureeTemps);

            timeText.Text = $"{time:mm:ss}";
            diffText.Text = GameManager.mapSelectionnee.mapMetadonnees.difficulte.ToString();
            bpmText.Text = GameManager.mapSelectionnee.mapMetadonnees.bpm.ToString();
            arText.Text = GameManager.mapSelectionnee.mapMetadonnees.ar.ToString();
        }

    }

    private void Update()
    {
        if(currentCamera != Camera.main) {
            currentCamera = Camera.main;
            currentCamera.GetUniversalAdditionalCameraData().cameraStack.Add(loadingSceneCamera);
        }
    }

    private void OnCompleteLoad()
    {
        animator.SetBool("isUnStarting" , true);
    }


    public void TermineAnimation()
    {
        currentCamera.GetUniversalAdditionalCameraData().cameraStack.Remove(loadingSceneCamera);
        animator.SetBool("isUnStarting" , false);
        
        SceneManager.UnloadSceneAsync((int) Scenes.LOADING);
    }
}
