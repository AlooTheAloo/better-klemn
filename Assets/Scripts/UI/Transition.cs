using Nova;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class Transition : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Camera loadingSceneCamera;
    private Camera currentCamera;


    public void OnEnable()
    {
        IntroScene.onLoadIntroScene += SetCamera;
        CreditSceneManager.onCreditSceneManagerLoaded += SetCamera;
        StatsGameOver.onStatsSceneLoad += SetCamera;
        SongSelectManager.onSongSelectEntered += SetCamera;
        TutorialChoice.onTutorialChoiceEntered += SetCamera;
        TeamReadyManager.onTeamReadyEntered += SetCamera;
    }

    public void OnDisable()
    {
        CreditSceneManager.onCreditSceneManagerLoaded -= SetCamera;
        StatsGameOver.onStatsSceneLoad -= SetCamera;
        SongSelectManager.onSongSelectEntered -= SetCamera;
        TutorialChoice.onTutorialChoiceEntered -= SetCamera;
        TeamReadyManager.onTeamReadyEntered -= SetCamera;
    }

    public void SetCamera(Camera camera)
    {
        this.currentCamera = camera;
        var camdata = currentCamera.GetUniversalAdditionalCameraData();
        var stack = camdata.cameraStack;
        stack.Add(loadingSceneCamera);
    }
    

    private void Update()
    {
        if (currentCamera != Camera.main && Camera.main != loadingSceneCamera)
        {
            if (Camera.main.GetUniversalAdditionalCameraData().cameraStack == null)
            {
                return;
            }
            SetCamera(Camera.main);
        }
    }
    public void TermineAnimationTransitionIN()
    {
        if (TeamReadyManager.hasGameEnded) return;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if(SceneManager.GetSceneAt(i).buildIndex == (int) Scenes.GAME)
            {
                SceneManager.UnloadSceneAsync((int)Scenes.GAME);
                SceneManager.LoadScene((int)Scenes.ENDGAME, LoadSceneMode.Additive);
            }
        }
    }

    public void TermineAnimationTransitionOUT()
    {
        try
        {
            currentCamera.GetUniversalAdditionalCameraData().cameraStack.Remove(loadingSceneCamera);
        }
        catch (Exception e) // L'éventualité où la scène s'est unload plus rapidement que l'animation
        {
            Console.WriteLine(e);
        }

        SceneManager.UnloadSceneAsync((int)Scenes.TRANSITION_SCENE);
    }
}
