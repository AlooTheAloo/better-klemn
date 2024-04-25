using System.IO;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public static BuildConfig build;   
    private void Awake()
    {
        string path = "";
        if (Application.isEditor)
        {
            path = $"{Application.persistentDataPath}{Constantes.FICHIER_BUILD_SETTINGS}";
        }
        else
        {
            path = $"{Application.dataPath}/..{Constantes.FICHIER_BUILD_SETTINGS}";
        }
        build = (BuildConfig) JsonUtility.FromJson(File.ReadAllText(path), typeof(BuildConfig));
    }
}
