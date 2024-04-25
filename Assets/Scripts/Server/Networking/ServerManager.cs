namespace Chroma.Server
{
    using System;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using Riptide;
    using System.Collections;

    public class ServerManager : MonoBehaviour
    {
        private static ServerManager instance;
        const int FETCH_INTERVAL = 2;
        Server server;
        public static event Action<InterfaceInfo?> onGetIP;
        public static event Action onGetConnection;


        private void Start()
        {
            Message.MaxPayloadSize = 100000;
            if(BuildManager.build.buildType == BuildType.CLIENT)
            {
                Destroy(gameObject);
            }
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }
            else instance = this;

            server = new Server();
            server.Start((ushort)ConstantesReseaux.PORT, (ushort)ConstantesReseaux.MAX_PEERS);
            DontDestroyOnLoad(this);
            StartCoroutine(fetchAdresseIP());
            server.ClientConnected += (_, _) => onGetConnection?.Invoke();
            server.ClientDisconnected += (_, _) => retournerAuMenuServeur();
        }

        private void FixedUpdate()
        {
            server.Update();
        }

        public void retournerAuMenuServeur()
        {
            SceneManager.LoadScene((int)Scenes.MENU_SERVER);
        }

        IEnumerator fetchAdresseIP()
        {
            while (true)
            {
                yield return new WaitForSeconds(FETCH_INTERVAL);
                if (SceneManager.GetActiveScene().buildIndex == (int)Scenes.MENU_SERVER)
                {
                    onGetIP?.Invoke(IPUtil.GetIP(ADDRESSFAM.IPv4));
                }
            }
        }

        public static void SendMessage(Message m)
        {
            if(BuildManager.build.buildType == BuildType.CLIENT)
            {
                throw new InvalidOperationException("Un ServerManager.SendMessage ne peut �tre appell� sur un build client");
            }
            instance.server.SendToAll(m); // ceci envoie le message � tous les clients
        }

        // Ceci arr�te le serveur lorsque l'application est quitt�e
        private void OnApplicationQuit()
        {
            server.Stop(); 
        }
    }
}
