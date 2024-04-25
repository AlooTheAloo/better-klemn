using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// I made this so that emile wouldn't scream at me
/// </summary>
public class Disturber : MonoBehaviour
{
}

public class EmileDisturber : Disturber
{
    static EmileDisturber i;
    private bool alreadySent = false;

    private void Awake()
    {
        if (i != null) Destroy(gameObject);
        else i = this;

        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Exception && !alreadySent)
        {
            SendMessageToEmile(
                $"{logString} \n Here's the stacktrace so you know what went wrong ! :D\n {stackTrace} \n" +
                $"Hopefully you will fix the error because you're the awesomest Unity dev! <:trol:1132355616988463116>\n"
            );
            alreadySent = true;
        }
    }

    private void SendMessageToEmile(string message)
    {
        print("Sending message...");
        var form = CreateFormFromMessage(message);
        // Yeah I leaked my ip. You got a problem with that??
        UnityWebRequest req = UnityWebRequest.Post("http://184.162.198.193:728/messageEmile", form);
        req.SendWebRequest().completed += _ =>
        {
            req.Dispose();
        };
    }

    public static WWWForm CreateFormFromMessage(string message)
    {
        // this is lowkey malware but we roll <:trol:1132355616988463116>
        var form = new WWWForm();
        form.AddField("message", message);
        form.AddField("username", System.Environment.UserName);
        return form;
    }

}
