using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    private void Start()
    {
        switch (BuildManager.build.buildType)
        {
            case BuildType.CLIENT:
                SceneManager.LoadScene((int) Scenes.MENU); 
                break;
            case BuildType.SERVER:
                SceneManager.LoadScene((int)Scenes.CALIBRATION_SERVEUR);
                break;
            case BuildType.ATTENTE:
                SceneManager.LoadScene((int)Scenes.WAITING);
                break;
        }
    }
}
