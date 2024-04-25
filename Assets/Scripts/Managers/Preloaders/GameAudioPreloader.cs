using System;
using System.Collections;
using UnityEngine;

public interface IGameData {}

public struct AudioData : IGameData
{
    public AudioClip clip;
    public AudioData(AudioClip clip){ 
        this.clip = clip;
    }
}

public class GameAudioPreloader : Preloader<AudioData>
{
    string path;

    public GameAudioPreloader(string path)
    {
        this.path = path;
    }

    public override IPreloader CommencerChargement(Action<AudioData> onComplete)
    {
        GameManager.Instance.StartCoroutine(chargerMusiqueCoroutine(onComplete));
        return this;
    }

    public IEnumerator chargerMusiqueCoroutine(Action<AudioData> onComplete)
    {
        yield return new WaitForSeconds(8);
        onComplete?.Invoke(new AudioData(GameManager.mapSelectionnee.audio));
    }

}

