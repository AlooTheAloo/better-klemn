using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour
{

    private void OnEnable()
    {
        NoteManager.i.onMapOver += I_onMapOver;

    }

    private void I_onMapOver()
    {
    }

    private void OnDisable()
    {
        NoteManager.i.onMapOver -= I_onMapOver;
    }
}
