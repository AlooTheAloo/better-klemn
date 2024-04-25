using Chroma.Server;
using Riptide;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ServerLightingManager : MonoBehaviour
{

    [Header("Refs")]
    [SerializeField] [SceneObjectsOnly] private List<Transform> lightingParents;

    // Dataf
    [ShowInInspector] Dictionary<string, List<ObjetLumiereActor>> objetsLumieres = new Dictionary<string, List<ObjetLumiereActor>>();
    Dictionary<long, List<EffetLumineux>> effetsEnAttente = new();


    // Actions
    private static event Action<LightingEffectGroup, EffetLumineux> onGetEffet;
    private static event Action<string> onGetGroupe;
    private static event Action<LightObjectInitiaialisePacket> onGetObjets;
    private static event Action<int, int> onGetLongeurs;
    private static event Action<LightingEffectGroup> onGetEffetGroupe;

    int longueurGroupes, longueurObjets;
    int groupesNetCount, objetsNetCount;

    #region MonoBehviour Callbacks
    private void OnEnable()
    {
        onGetGroupe += CreerGroupe;
        onGetObjets += CreateObjectLumineuxFromPacket;
        onGetLongeurs += SetLongueurs;
        onGetEffet += AjouterEffet;
        onGetEffetGroupe += CreerGroupeEffets;
    }
    private void OnDisable()
    {
        onGetGroupe -= CreerGroupe;
        onGetObjets -= CreateObjectLumineuxFromPacket;
        onGetLongeurs -= SetLongueurs;
        onGetEffet -= AjouterEffet;
        onGetEffetGroupe -= CreerGroupeEffets;
    }

    #endregion

    #region Network Calls

    [MessageHandler((ushort) ClientToServerCalls.AJOUTER_GROUPE_EFFETS)]
    private static void NetPreparerLongueurEffets(ushort clientID, Message message)
    {
        onGetEffetGroupe?.Invoke(message.GetSerializable<LightingEffectGroup>());
    }

    [MessageHandler((ushort) ClientToServerCalls.GROUPS_OBJECTS_LENGTH)]
    private static void NetPreparerLongueurObjets(ushort clientID, Message message)
    {
        onGetLongeurs?.Invoke(message.GetInt(), message.GetInt());
    }

    [MessageHandler((ushort) ClientToServerCalls.ADD_GROUPS)]
    private static void NetAjouterGroupes(ushort clientID, Message message)
    {
        string groupes = message.GetString();
        onGetGroupe?.Invoke(groupes);
    }

    [MessageHandler((ushort)ClientToServerCalls.ADD_OBJECTS)]
    private static void NetAjouterObjets(ushort clientID, Message message)
    {
        LightObjectInitiaialisePacket packet = message.GetSerializable<LightObjectInitiaialisePacket>();
        string conc = "Ajout de l'objet  " + packet.lumiereData.nomLumiere;
        foreach(var obj in packet.lumiereData.groupes)
        {
            conc += " " + obj + ",";
        }
        print(conc);

        onGetObjets?.Invoke(packet);
    }

    [MessageHandler((ushort)ClientToServerCalls.AJOUTER_EFFET_LUMINEUX)]
    private static void NetAjouterEffet(ushort clientID, Message message) {
        LightingEffectGroup groupe = message.GetSerializable<LightingEffectGroup>();
        EffetLumineux effet = message.GetSerializable<EffetLumineux>();
        onGetEffet?.Invoke(groupe, effet);
    }
    #endregion

    #region Listener functions
    private void SetLongueurs(int longueurGroupes, int longueurObjets)
    {
        print("objetlumieres is empty. proof : " + objetsLumieres.Count);
        this.longueurGroupes = longueurGroupes;
        this.longueurObjets = longueurObjets;
    }
    private void CreerGroupe(string groupe)
    {
        print("added groupe " + groupe);
        objetsLumieres.Add(groupe, new List<ObjetLumiereActor>());
        groupesNetCount++;
        VerifierLongueurs();
    }

    private void CreateObjectLumineuxFromPacket(LightObjectInitiaialisePacket packet)
    {
        var nouvelObjetLumineux = 
            (packet.lumiereData.type == ShapeActorType.RECTANGLE ? 
            PoolManager.i.ServerPools.rectangleObjectPool : 
            PoolManager.i.ServerPools.circleObjectPool)
            .Spawn(lightingParents[packet.lumiereData.projectorID]).GetComponent<ObjetLumiereActor>();
        nouvelObjetLumineux.Initialiser(packet);
        foreach (var groupe in nouvelObjetLumineux.ObjetData.groupes)
        {
            if (objetsLumieres.ContainsKey(groupe))
            {
                objetsLumieres[groupe].Add(nouvelObjetLumineux);
            }
            else
            {
                objetsLumieres.Add(groupe, new List<ObjetLumiereActor>() { nouvelObjetLumineux });
            }
        }
        objetsNetCount++;
        VerifierLongueurs();
    }


    void VerifierLongueurs()
    {
        print($"Current amount : ({objetsNetCount + groupesNetCount} / {longueurGroupes + longueurObjets})");
        if(objetsNetCount >= longueurObjets && groupesNetCount >= longueurGroupes)
        {
            Message messageACK = Message.Create(MessageSendMode.Reliable, ServerToClientCalls.ACK_GROUPS_OBJECTS);
            ServerManager.SendMessage(messageACK);
        }
    }


    private void AjouterEffet(LightingEffectGroup groupe, EffetLumineux obj)
    {
        effetsEnAttente[groupe.id].Add(obj);
        if (effetsEnAttente[groupe.id].Count >= groupe.count)
        {
            StartCoroutine(AppliquerListeEffets(effetsEnAttente[groupe.id]));
        }
    }

    private IEnumerator AppliquerListeEffets(List<EffetLumineux> effets)
    {
        foreach(var effet in effets.OrderBy(x => x.Order))
        {
            StartCoroutine(AppliquerEffet(effet));
            yield return null;
        }
    }

    IEnumerator AppliquerEffet(EffetLumineux effet)
    {
        float timer = 0;

        while(timer < effet.Decalage)
        {
            yield return null;
            timer += Time.deltaTime;
        }

        foreach(var groupe in effet.GroupesCible)
        {
            objetsLumieres[groupe].ForEach(x =>
            {
                x.SetEffet(effet);
            });
        }
    }

    private void CreerGroupeEffets(LightingEffectGroup obj)
    {
        effetsEnAttente.Add(obj.id, new());
    }

    #endregion

}

