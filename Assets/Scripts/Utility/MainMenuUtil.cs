using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using System;
using UnityEditor;

public class MainMenuUtil : MonoBehaviour
{


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    static void createMapPath()
    {
        if (!Directory.Exists(Application.persistentDataPath + "/Maps"))
            Directory.CreateDirectory(Application.persistentDataPath + "/Maps");

        if (!File.Exists(Application.persistentDataPath + Constantes.FICHIER_BUILD_SETTINGS))
            File.WriteAllText(Application.persistentDataPath + Constantes.FICHIER_BUILD_SETTINGS,
                JsonUtility.ToJson(new BuildConfig(BuildType.CLIENT, false, "http://localhost:3000", "http://localhost:5000")));
    }

    public static string getMapsDirectory()
    {
        return Application.persistentDataPath
            + Path.DirectorySeparatorChar
            + "Maps"
            + Path.DirectorySeparatorChar;
    }

    private void Awake()
    {
        Constantes.API_BASE_URL = BuildManager.build.chromaWebApiBaseUrl;
        Constantes.LIGHT_API_BASE_URL = BuildManager.build.lightApiBaseUrl;



        Debug.Log(Constantes.API_BASE_URL);
        Debug.Log(Constantes.LIGHT_API_BASE_URL);
    }
}
