using Riptide;
using Riptide.Utils;
using System;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    public static event Action OnConnected;
    public static event Action OnDisconnected;
    private static ClientManager instance;

    const int MAX_CONNECTION_ATTEMPTS = 1;

    private Client client;

    internal void ConnectionAIPParDefault()
    {
        Connecter(ConstantesReseaux.DEFAULT_IP);
    }

    internal void ConnecterAIPParticuliere(string ip)
    {
        Connecter($"{ip}:{ConstantesReseaux.PORT}");
    }

    private void Connecter(string ip)
    {
        bool connecting = client.Connect(ip, MAX_CONNECTION_ATTEMPTS);
        if (!connecting) OnDisconnected?.Invoke();
    }

    private void ConnecterSansLumieres()
    {
    }

    private void Awake()
    {
        if(BuildManager.build.buildType == BuildType.SERVER || instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        client = new Client();

        client.Connected += (_, _) => OnConnected.Invoke();
        client.ConnectionFailed += (_, _) => { OnDisconnected.Invoke(); };
        OnConnected += onClientConnected;
        OnDisconnected += onConnectionFailed;
    }

    private void OnEnable()
    {
        MainMenuUIManager.onConnectWithIPRequested += ConnecterAIPParticuliere;
        MainMenuUIManager.onConnectWithoutIPRequested += ConnectionAIPParDefault;
        MainMenuUIManager.onConnectWithoutLightsRequested += ConnecterSansLumieres;
    }

    private void OnDisable()
    {
        MainMenuUIManager.onConnectWithIPRequested -= ConnecterAIPParticuliere;
        MainMenuUIManager.onConnectWithoutIPRequested -= ConnectionAIPParDefault;
        MainMenuUIManager.onConnectWithoutLightsRequested -= ConnecterSansLumieres;
    }


    private void FixedUpdate()
    {
        client?.Update();
    }

    private void onClientConnected()
    {
    }

    private void onConnectionFailed()
    {
    }

    public static bool HasConnection()
    {
        return instance.client.IsConnected;
    }

    public static void SendMessage(Message message)
    {
        if (BuildManager.build.buildType == BuildType.SERVER)
            return;
        if (instance == null)
            return;
        if (instance.client == null)
            return;
        Client client = instance.client;
        if (client.IsConnected)
        {
            client.Send(message);
        }
    }
}
