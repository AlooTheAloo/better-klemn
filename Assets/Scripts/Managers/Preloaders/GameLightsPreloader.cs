using Riptide;
using System;
using UnityEngine;

public struct GameLightingData
{
    public bool serverACK;
    public GameLightingData(bool serverACK)
    {
        this.serverACK = serverACK;
    }
}

public class GameLightsPreloader : Preloader<GameLightingData>, IPreloader
{
    static Action<GameLightingData> onCompleteAction;
    internal Map map;

    public GameLightsPreloader(Map map)
    {
        this.map = map;
    }

    public override IPreloader CommencerChargement(Action<GameLightingData> onComplete)
    {
        onCompleteAction = onComplete;

        if (!ClientManager.HasConnection())
        {
            onComplete?.Invoke(new GameLightingData(false));
            return this;
        }
        
        if(map.listeGroupes.Count == 0 && map.ListeObjetLumieres.Count == 0){
            onComplete?.Invoke(new GameLightingData(false));
            return this;
        }

        // Envoyer longueurs
        Message lenMessage = Message.Create(MessageSendMode.Reliable, ClientToServerCalls.GROUPS_OBJECTS_LENGTH);
        lenMessage.AddInt(map.listeGroupes.Count);
        lenMessage.AddInt(map.ListeObjetLumieres.Count);
        ClientManager.SendMessage(lenMessage);

        Debug.Log("groupes ; " + map.listeGroupes.Count);
        Debug.Log("obj" + map.ListeObjetLumieres.Count);


        foreach (var groupe in map.listeGroupes)
        {
            // Envoyer groupes
            Message groupMessage = Message.Create(MessageSendMode.Reliable, ClientToServerCalls.ADD_GROUPS);
            groupMessage.AddString(groupe);
            ClientManager.SendMessage(groupMessage);
        }

        foreach(var objet in map.ListeObjetLumieres)
        {
            // Envoyer objets
            Message objectsMessage = Message.Create(MessageSendMode.Reliable, ClientToServerCalls.ADD_OBJECTS);
            objectsMessage.AddSerializable(objet);
            ClientManager.SendMessage(objectsMessage);
        }
        return this;
    }

    [MessageHandler((ushort) ServerToClientCalls.ACK_GROUPS_OBJECTS)]
    private static void Net_Ack_Groups_Objects(Message message)
    {
        Debug.Log("Got a NET ACK");
        onCompleteAction?.Invoke(new GameLightingData(true));
    }

}
