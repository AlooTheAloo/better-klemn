using System;
using System.Collections;
using System.Collections.Generic;
using Chroma.Server;
using Nova;
using Riptide;
using UnityEngine;

[Serializable]
struct UISongStats
{
    public TextBlock mapTitle;
    public TextBlock mapArtist;
    public TextBlock mapMapper;
    public TextBlock mapTimer;
    public UIBlock2D mapImage;
}


public class WaitingRoomNetworkManager : MonoBehaviour
{
    [SerializeField] private UIBlock2D DisconnectedPanel;
    [SerializeField] private UIBlock2D LogoParent;
    [SerializeField] private UIBlock2D EquipesParent;
    [SerializeField] private UIBlock2D SongDataParent;

    [SerializeField] private TextBlock teamName;
    [SerializeField] private UISongStats stats;

    public List<Texture2D> mapImages = new();
    
    private Client client;
    private static bool isWaitingRoom;
    private static event Action<WaitingRoomState> onGetStateChange;
    private static event Action<string> onGetTeamName;
    private static event Action<GameStatus> onGetGameStatus;

    private static event Action<int> onGetMapID;
    
    private void Start()
    {
        isWaitingRoom = true;
        Connecter($"{ConstantesReseaux.DEFAULT_IP}");
    }

    private void OnEnable()
    {
        client = new Client();
        client.Connected += OnConnection;
        client.Disconnected += OnDisconnect;
        onGetStateChange += setState;
        onGetTeamName += GetTeamName;
        onGetGameStatus += ShowGameStatus;
        onGetMapID += SetMapImage;
    }

    private void ShowGameStatus(GameStatus obj)
    {
        stats.mapTitle.Text = obj.mapName;
        stats.mapArtist.Text = obj.mapArtist;
        stats.mapMapper.Text = obj.mapMapper;
        SetMapImage(obj.mapID);

        StartCoroutine(CountTimer(obj.estimatedTimeMap));
    }

    private void SetMapImage(int id)
    {
        stats.mapImage.gameObject.SetActive(id != -1);
        if (id != -1)
        {
            stats.mapImage.SetImage(mapImages[id - 1]);
        }
    }

    private IEnumerator CountTimer(float timer)
    {
        while (timer > 0)
        {
            timer--;
            yield return new WaitForSeconds(1);
            TimeSpan time = TimeSpan.FromSeconds(timer);
            print(timer);
            print(time.ToString("mm':'ss"));
            stats.mapTimer.Text = "Fin de la chanson dans " + time.ToString("mm':'ss");
        }        
        stats.mapTimer.Text = "";

    }
    

    private void OnDisconnect(object sender, DisconnectedEventArgs e)
    {
        LogoParent.gameObject.SetActive(false);
        EquipesParent.gameObject.SetActive(false);
        SongDataParent.gameObject.SetActive(false);
        DisconnectedPanel.gameObject.SetActive(true);
        
    }

    private void OnDisable()
    {
        onGetStateChange -= setState;
        onGetTeamName -= GetTeamName;
        onGetGameStatus -= ShowGameStatus;
        client.Connected -= OnConnection;
        client.Disconnected -= OnDisconnect;
        onGetMapID -= SetMapImage;
    }

    private void GetTeamName(string teamName)
    {
        this.teamName.Text = teamName;
    }
    
    private void Connecter(string ip)
    {
        client.Connect(ip, 5);
    }

    private void OnConnection(object sender, EventArgs args)
    {
        DisconnectedPanel.gameObject.SetActive(false);
    }

    private void Update()
    {
        client.Update();
    }

    [MessageHandler((ushort) ServerToClientCalls.SET_WAITING_ROOM_STATE)]
    private static void SetWaitingRoomMode(Message message)
    {
        if (!isWaitingRoom) return;
        onGetStateChange?.Invoke((WaitingRoomState) message.GetInt());
    }

    [MessageHandler((ushort)ServerToClientCalls.WAITINGROOM_TEAMNAME)]
    private static void SetName(Message message)
    {
        if (!isWaitingRoom) return;
        onGetTeamName?.Invoke(message.GetString());
    }

    [MessageHandler((ushort)ServerToClientCalls.WAITINGROOM_STATUS)]
    private static void SetStatus(Message message)
    {
        if (!isWaitingRoom) return;
        onGetGameStatus?.Invoke(message.GetSerializable<GameStatus>());
    }

    #region Server relays

    [MessageHandler((ushort) ClientToServerCalls.WAITINGROOM_STATE)]
    private static void Server_Waitingroomstate(ushort cid, Message message)
    {
        print("Setting state :zamn:");
        Message rep = Message.Create(MessageSendMode.Reliable, (ushort) ServerToClientCalls.SET_WAITING_ROOM_STATE); 
        rep.Add(message.GetInt());
        ServerManager.SendMessage(rep);
    }

    [MessageHandler((ushort)ClientToServerCalls.WAITINGROOM_TEAMNAME)]
    private static void Server_SetName(ushort cid, Message message)
    {
        Message rep = Message.Create(MessageSendMode.Reliable, (ushort) ServerToClientCalls.WAITINGROOM_TEAMNAME); 
        rep.Add(message.GetString());
        ServerManager.SendMessage(rep);
    }

    [MessageHandler((ushort)ClientToServerCalls.WAITINGROOM_STATUS)]
    private static void Server_SetStatus(ushort cid, Message message)
    {
        print("Sending new status");
        Message rep = Message.Create(MessageSendMode.Reliable, (ushort) ServerToClientCalls.WAITINGROOM_STATUS); 
        rep.Add(message.GetSerializable<GameStatus>());
        ServerManager.SendMessage(rep);
    }

    [MessageHandler((ushort) ClientToServerCalls.WAITINGROOM_IMAGE)]
    private static void Server_SetImage(ushort cid, Message message)
    {
        print("Got image");
        print(message.GetString());
    }
    
    
    #endregion
    
    private void setState(WaitingRoomState state)
    {
        bool showLogo = false;
        bool showSongData = false;
        bool showEquipes = false;
        switch (state)
        {
            case WaitingRoomState.SHOW_NEXT_TEAM:
                showEquipes  = true;
                teamName.Text = "";
                break;
            case WaitingRoomState.SHOW_SONG_STATS:
                showSongData = true;
                stats.mapTimer.Text = "";
                break;
            case WaitingRoomState.LOGO:
                showLogo = true;
                break;
        }
        LogoParent.gameObject.SetActive(showLogo);
        SongDataParent.gameObject.SetActive(showSongData);
        EquipesParent.gameObject.SetActive(showEquipes);
    }
}

public enum WaitingRoomState
{
    SHOW_NEXT_TEAM,
    SHOW_SONG_STATS,
    LOGO
}

public struct GameStatus : IMessageSerializable
{
    public string mapName;
    public string mapArtist;
    public string mapMapper;
    public float estimatedTimeMap;
    public int mapID;

    public GameStatus(string mapName, string mapArtist, string mapMapper, float time, int mapID)
    {
        this.mapName = mapName;
        this.mapArtist = mapArtist;
        this.mapMapper = mapMapper;
        this.estimatedTimeMap = time;
        this.mapID = mapID;
    }
    
    public void Serialize(Message message)
    {
        message.AddString(mapName);
        message.AddString(mapArtist);
        message.AddString(mapMapper);
        message.AddFloat(estimatedTimeMap);
        message.AddInt(mapID);
    }

    public void Deserialize(Message message)
    {
        mapName = message.GetString();
        mapArtist = message.GetString();
        mapMapper = message.GetString();
        estimatedTimeMap = message.GetFloat();
        mapID = message.GetInt();
    }
}
