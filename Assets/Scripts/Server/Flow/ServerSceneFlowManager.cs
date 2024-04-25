using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Chroma.Server {
    public class ServerSceneFlowManager : MonoBehaviour
    {
        [MessageHandler((ushort) ClientToServerCalls.CHANGE_SCENE)]
        public static void onSceneChange(ushort clientID, Message message)
        {
            SceneManager.LoadScene(message.GetInt());
        }
    }
}
