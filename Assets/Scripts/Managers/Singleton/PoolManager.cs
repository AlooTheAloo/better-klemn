using Lean.Pool;
using Sirenix.Utilities;
using System;
using System.Reflection;
using UnityEngine;

[Serializable]
public struct ServerPools
{
    [SerializeField] internal LeanGameObjectPool rectangleObjectPool;
    [SerializeField] internal LeanGameObjectPool circleObjectPool;
}

[Serializable]
public struct ClientPools
{
    [SerializeField] internal LeanGameObjectPool holdNotePool;
    [SerializeField] internal LeanGameObjectPool clickNotePool;
    [SerializeField] internal LeanGameObjectPool backgroundPool;
}

public class PoolManager : MonoBehaviour
{
    [SerializeField] private ServerPools _serverPools;
    [SerializeField] private ClientPools _clientPools;

    public ServerPools ServerPools
    {
        get { 
            if(BuildManager.build.buildType == BuildType.CLIENT)
            {
                throw new InvalidOperationException("Il est impossible d'acc�der au pools serveur en tant que client");
            }
            else return _serverPools; 
        }
        set {
            throw new InvalidOperationException("Il est impossible de set les pools au runtime");
        }
    }

    public ClientPools ClientPools
    {
        get
        {
            if (BuildManager.build.buildType == BuildType.SERVER)
            {
                throw new InvalidOperationException("Il est impossible d'acc�der au pools clients en tant que serveur");
            }
            else return _clientPools;
        }
        set
        {
            throw new InvalidOperationException("Il est impossible de set les pools au runtime");
        }
    }

    internal static PoolManager i;

    private void Awake()
    {
        if (i) Destroy(this);
        else i = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        DetruirePoolsNonUtilisees();
    }

    private void DetruirePoolsNonUtilisees()
    {
        // Reflection :zamn:
        if (BuildManager.build.buildType == BuildType.SERVER)
        {
            typeof(ClientPools).GetFields(BindingFlags.Instance |
                       BindingFlags.Static |
                       BindingFlags.NonPublic |
                       BindingFlags.Public).ForEach((x) =>
            {
                Destroy(((LeanGameObjectPool) x.GetValue(_clientPools)).gameObject);
            });
        }
        else
        {
            typeof(ServerPools).GetFields(BindingFlags.Instance |
                       BindingFlags.Static |
                       BindingFlags.NonPublic |
                       BindingFlags.Public).ForEach((x) =>
            {
                Destroy(((LeanGameObjectPool)x.GetValue(_serverPools)).gameObject);
            });
        }
    }
}
