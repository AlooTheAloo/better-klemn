using System;
using System.Collections;
using System.Collections.Generic;
using Nova;
using UnityEngine;
using UnityEngine.Serialization;

public class CreditSceneManager : MonoBehaviour
{
    [SerializeField] private Camera sceneCamera;
    public static event Action<Camera> onCreditSceneManagerLoaded; 
    private float timer = 25;
    
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CreditSceneRoutine());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            timer = 0;
        }
    }


    private IEnumerator CreditSceneRoutine()
    {
        while (timer > 0)
        {
            yield return null;
            timer -= Time.deltaTime;
        }
        StartCoroutine(TeamReadyManager.LoadFromSceneToScene(Scenes.CREDIT, Scenes.INTRO));
    }

    private void OnEnable()
    {
        onCreditSceneManagerLoaded?.Invoke(sceneCamera);
    }
}
