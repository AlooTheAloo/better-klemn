using UnityEngine;
using UnityEngine.SceneManagement;

public class CommonAudioListener : MonoBehaviour
{
    private static CommonAudioListener i;
    private void Awake()
    {
        if (i != null)
        {
            Destroy(gameObject);
        }
        else
        {
            i = this;
            DontDestroyOnLoad(gameObject);
        }

        SceneManager.sceneLoaded += SupprimerAudioListener;
    }

    private void SupprimerAudioListener(Scene scene, LoadSceneMode mode)
    {
        var listeners =FindObjectsOfType<AudioListener>();
        foreach (var listener in listeners)
        {
            if (listener.gameObject.scene.buildIndex != (int) Scenes.DONT_DESTROY_ON_LOAD)
            {
                Destroy(listener);
            }
        }
    }
}
