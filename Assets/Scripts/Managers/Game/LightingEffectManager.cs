using System;
using Chroma.Server;
using Riptide;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LightingEffectManager : MonoBehaviour
{
    long idGenerator = 0;

    void Start()
    {
        LightsClient.SetLedMode(LedLightState.Pulse);
        
        GameManager.Instance.gameNoteData.map.ListeEffets.ForEach(x =>
        {
            TimeManager.ActionATemps(x.TempsDebut, () =>
            {
                Message messageGroupe = Message.Create(MessageSendMode.Reliable, ClientToServerCalls.AJOUTER_GROUPE_EFFETS);
                LightingEffectGroup group = new LightingEffectGroup(idGenerator, x.EffetsLumineux.Count);
                messageGroupe.Add(group);
                ClientManager.SendMessage(messageGroupe);

                x.EffetsLumineux.OrderBy(x => x.Order).ToList().ForEach(x =>
                {
                    Message messageEffet = Message.Create(MessageSendMode.Reliable, ClientToServerCalls.AJOUTER_EFFET_LUMINEUX);
                    messageEffet.Add(group);
                    messageEffet.Add(x);
                    ClientManager.SendMessage(messageEffet);
                });
                idGenerator++;
            });
        });
    }


    private void OnEnable()
    {
        Alley.onNoteClick += EnvoyerEffetsPourNote;
    }

    private void OnDisable()
    {
        Alley.onNoteClick -= EnvoyerEffetsPourNote;
    }


    private void EnvoyerEffetsPourNote(NoteData data)
    {
       

        
        
        if (data.listeEffets.EffetsLumineux.Count == 0) return; // Aucun effet � envoyer


        Message messageGroupe = Message.Create(MessageSendMode.Reliable, ClientToServerCalls.AJOUTER_GROUPE_EFFETS);
        LightingEffectGroup group = new LightingEffectGroup(idGenerator, data.listeEffets.EffetsLumineux.Count);
        messageGroupe.Add(group);
        ClientManager.SendMessage(messageGroupe);
        data.listeEffets.EffetsLumineux.OrderBy(x => x.Order).ToList().ForEach(x =>
        {
            Message messageEffet = Message.Create(MessageSendMode.Reliable, ClientToServerCalls.AJOUTER_EFFET_LUMINEUX);
            messageEffet.Add(group);
            messageEffet.Add(x);
            ClientManager.SendMessage(messageEffet);
        });
        idGenerator++;
    }
}

public struct LightingEffectGroup : IMessageSerializable
{
    public long id;
    public ushort count;

    public LightingEffectGroup(long id, int count)
    {
        this.id = id;
        this.count = (ushort) count;
    }

    public void Deserialize(Message message)
    {
        id = message.GetLong();
        count = message.GetUShort();
    }

    public void Serialize(Message message)
    {
        message.AddLong(id);
        message.AddUShort(count);
    }
}