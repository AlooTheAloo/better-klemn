using UnityEditor.Build;

namespace Chroma
{
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    
    [InitializeOnLoad]
    public class GameBuilder
    {
        static GameBuilder()
        {
            if (!File.Exists(Application.persistentDataPath + Constantes.FICHIER_BUILD_SETTINGS))
                File.WriteAllText(Application.persistentDataPath + Constantes.FICHIER_BUILD_SETTINGS,
                    JsonUtility.ToJson(new BuildConfig(BuildType.CLIENT, isDev, chromaWebApiBaseUrl, lightApiBaseUrl)));
        }
        
        private static bool isDev;
        private static string chromaWebApiBaseUrl = "http://localhost:3000";
        private static string lightApiBaseUrl = "http://localhost:5000";
        
        [MenuItem("Chroma Tools/Build Serveur")]
        public static void BuildServer()
        {
            string path = EditorUtility.SaveFolderPanel("Choisir location du serveur", "", "");
            if (path == "") return;
            BuildPipeline.BuildPlayer(creerBuildOptions(path + "/serveur.exe"));
            File.WriteAllText(path + Constantes.FICHIER_BUILD_SETTINGS, getBuildConfigEnJSON(BuildType.SERVER, isDev, chromaWebApiBaseUrl, lightApiBaseUrl));
            Debug.Log("Build du serveur r�ussi!");
        }

        [MenuItem("Chroma Tools/Build Client")]
        public static void BuildClient()
        {
            string path = EditorUtility.SaveFolderPanel("Choisir location du client", "", "");
            if (path == "") return;
            BuildPipeline.BuildPlayer(creerBuildOptions(path + "/client.exe"));
            File.WriteAllText(path + Constantes.FICHIER_BUILD_SETTINGS, getBuildConfigEnJSON(BuildType.CLIENT, isDev, chromaWebApiBaseUrl, lightApiBaseUrl));
            Debug.Log("Build du client réussi!");
        }
        
        
        [MenuItem("Chroma Tools/Build Attente")]
        public static void BuildAttente()
        {
            string path = EditorUtility.SaveFolderPanel("Choisir location du client", "", "");
            if (path == "") return;
            BuildPipeline.BuildPlayer(creerBuildOptions(path + "/clientAttente.exe"));
            File.WriteAllText(path + Constantes.FICHIER_BUILD_SETTINGS, getBuildConfigEnJSON(BuildType.ATTENTE, isDev, chromaWebApiBaseUrl, lightApiBaseUrl));
        }

        [MenuItem("Chroma Tools/Changer mode �diteur vers serveur")]
        public static void SetEditeurServeur()
        {
            File.WriteAllText(Application.persistentDataPath + Constantes.FICHIER_BUILD_SETTINGS,
                    JsonUtility.ToJson(new BuildConfig(BuildType.SERVER, isDev, chromaWebApiBaseUrl, lightApiBaseUrl)));
        }

        [MenuItem("Chroma Tools/Changer mode �diteur vers client")]
        public static void SetEditeurClient()
        {
            File.WriteAllText(Application.persistentDataPath + Constantes.FICHIER_BUILD_SETTINGS,
                    JsonUtility.ToJson(new BuildConfig(BuildType.CLIENT, isDev, chromaWebApiBaseUrl, lightApiBaseUrl)));
        }
        
        private static BuildPlayerOptions creerBuildOptions(string path)
        {
            BuildPlayerOptions options = new BuildPlayerOptions();
            options.scenes = getScenePaths();
            options.locationPathName = path;
            options.target = BuildTarget.StandaloneWindows;
            System.Windows.Forms.DialogResult dr = System.Windows.Forms.MessageBox.Show("Voulez-vous activer le mode de build d�veloppement ?", 
                "Mode build d�velopement", System.Windows.Forms.MessageBoxButtons.YesNo);

            var devMode = dr == System.Windows.Forms.DialogResult.Yes;
            
            options.options = devMode ? BuildOptions.Development : BuildOptions.None;
            isDev = devMode;
            
            return options;
        }

        private static string[] getScenePaths()
        {
            // Chercher toutes les sc�nes
            string[] scenePaths = new string[SceneManager.sceneCountInBuildSettings];
            for (int i = 0; i < scenePaths.Length; i++)
            {
                scenePaths[i] = SceneUtility.GetScenePathByBuildIndex(i);
            }
            
            return scenePaths;
        }

        private static string getBuildConfigEnJSON(BuildType type, bool isDev, string chromaWebApiBaseUrl, string lightApiBaseUrl)
        {
            return JsonUtility.ToJson(new BuildConfig(type, isDev, chromaWebApiBaseUrl, lightApiBaseUrl));
        }
    }

}
