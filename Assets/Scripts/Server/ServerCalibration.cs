using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerCalibration : MonoBehaviour
{
    const KeyCode CONTINUE_KEYCODE = KeyCode.Return;
    private void Update()
    {
        if (Input.GetKeyDown(CONTINUE_KEYCODE))
        {
            SceneManager.LoadScene((int) Scenes.MENU_SERVER);
        }
    }

    private void Start()
    {
        foreach (var display in Display.displays)
        {
            display.Activate();
        }
    }
}
