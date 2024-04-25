using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class AudioLoader
{
    public Action<AudioData> onSuccess;
    public AudioLoader LoadData(string path)
    {
        GameManager.Instance.StartCoroutine(GetRequest(path));
        return this;
    }

    public AudioData LoadDataSync(string path)
    {
        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.WAV))
        {
            request.SendWebRequest();
            while(!request.isDone) {} // Lord forgive me for I have sinned
            AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
            return new AudioData(clip);
        }
    }

    IEnumerator GetRequest(string url)
    {
        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV))
        {
            // Send the request and wait for a response
            yield return request.SendWebRequest();

            AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
            onSuccess?.Invoke(new AudioData(clip));
        }
    }
}
