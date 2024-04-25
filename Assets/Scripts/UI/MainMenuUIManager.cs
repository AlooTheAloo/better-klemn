 using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;
using UnityEngine.SceneManagement;
using NovaSamples.UIControls;
using Riptide;
using SFB;

public enum MainMenuPanel
{
    FormulaireIP,
    ConnectionEnCours,
    FormulaireModeDeJeu
}

public class MainMenuUIManager : MonoBehaviour
{
    internal static event Action<string> onConnectWithIPRequested;
    internal static event Action onConnectWithoutIPRequested;
    internal static event Action onConnectWithoutLightsRequested;

    [SerializeField] private TextField ipTextField;
    
    [SerializeField] private Transform ipFormulairePanel;
    [SerializeField] private Transform connectionEnCoursPanel;
    [SerializeField] private Transform modeDeJeuFormulairePanel;

    private void OnEnable()
    {
        ClientManager.OnConnected += OnConnected;
        ClientManager.OnDisconnected += OnDisconnected;
    }

    private void OnDisable()
    {
        ClientManager.OnConnected -= OnConnected;
        ClientManager.OnDisconnected -= OnDisconnected;
    }


    public void OnConnected()
    {
        SetActivePanel(MainMenuPanel.FormulaireModeDeJeu);
    }

    public void OnDisconnected()
    {
        SetActivePanel(MainMenuPanel.FormulaireIP);
    }

    public void ConnecterAIP()
    {
        print("connection vers ip..");
        SetActivePanel(MainMenuPanel.ConnectionEnCours);
        onConnectWithIPRequested?.Invoke(ipTextField.Text);
    }
    public void ConnecterAIPDefaut()
    {
        onConnectWithoutIPRequested?.Invoke();
        SetActivePanel(MainMenuPanel.ConnectionEnCours);
    }

    public void ConnecterSansLumieres()
    {
        onConnectWithoutLightsRequested?.Invoke();
        SetActivePanel(MainMenuPanel.FormulaireModeDeJeu);
    }

    public void SetActivePanel(MainMenuPanel panel)
    {
        ipFormulairePanel.gameObject.SetActive(false);
        connectionEnCoursPanel.gameObject.SetActive(false);
        modeDeJeuFormulairePanel.gameObject.SetActive(false);

        switch (panel)
        {
            case MainMenuPanel.FormulaireIP:
                ipFormulairePanel.gameObject.SetActive(true);
                break;
            case MainMenuPanel.ConnectionEnCours:
                connectionEnCoursPanel.gameObject.SetActive(true);
                break;
            case MainMenuPanel.FormulaireModeDeJeu:
                modeDeJeuFormulairePanel.gameObject.SetActive(true);
                break;
            default:
                Debug.LogError("Quelqu'un a appel� SetActivePanel sans ajouter le panneau aux options dans le switch");
                break;
        }
    }
    
    // called through the inspector
    public void LoadTeamReady()
    {
        Message m = Message.Create(MessageSendMode.Reliable, ClientToServerCalls.CHANGE_SCENE);
        m.AddInt((int)Scenes.INTRO_SERVER);
        ClientManager.SendMessage(m);
        print("Sent message");
        SceneManager.LoadScene((int) Scenes.INTRO);
    }

    public void LoadEditorSongSelect()
    {
        print("loading scene with index " + (int)Scenes.EDITOR_SONG_SELECT);
        SceneManager.LoadScene((int) Scenes.EDITOR_SONG_SELECT);
    }

    private void Awake()
    {
        SetActivePanel(MainMenuPanel.FormulaireIP);

    }
}
