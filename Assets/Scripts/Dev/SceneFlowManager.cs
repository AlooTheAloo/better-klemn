using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFlowManager : MonoBehaviour
{
    [SerializeField]
    static SceneFlowManager i;

    private void Awake()
    {
        if (i != null || !Application.isEditor)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        i = this;
        if(SceneManager.GetActiveScene().buildIndex != (int)Scenes.SCENELOADER)
        {
            SceneManager.LoadScene((int)Scenes.SCENELOADER, LoadSceneMode.Single);
        }
        
        
    }
}
