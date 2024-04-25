using System;
using Nova;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class SongSelectAudioManager : MonoBehaviour
{
    const float MAX_LOWPASS = 20000;
    const float MIN_LOWPASS = 3500;
    const string LOWPASSKEY = "LowPassCutoff";

    public AudioSource selectedSong;

    [Header("Map changée")]
    [SerializeField]
    AnimationCurve fadeOutCurve;

    [SerializeField]
    float fadeLength;

    [Header("Map choisie")]
    [SerializeField]
    AnimationCurve mapEnterfadeOutCurve;

    [SerializeField]
    float mapEnterfadeLength;

    [Header("Audio Low-Pass")]
    [SerializeField]
    AudioMixerGroup groupe;

    [SerializeField]
    float lowPassTemps;

    [SerializeField]
    AnimationCurve lowPassCourbe;


    public static event Action<AudioSource> onSelectedSongSet;

    private void OnEnable()
    {
        SongSelectManager.onMapSelectedChanged += FadeOutSongVol;
        SongSelectManager.onMapEnter += FadeOutSongSelected;
    }

    private void OnDisable()
    {
        SongSelectManager.onMapSelectedChanged -= FadeOutSongVol;
        SongSelectManager.onMapEnter -= FadeOutSongSelected;
        groupe.audioMixer.SetFloat(LOWPASSKEY, MAX_LOWPASS);
    }

    private void FadeOutSongSelected()
    {
        StartCoroutine(FadeOutSongSelectedRoutine());
    }

    private IEnumerator FadeOutSongSelectedRoutine()
    {
        float timer = 0f;

        while (timer < lowPassTemps)
        {
            timer += Time.deltaTime;
            float lerpVal = lowPassCourbe.Evaluate(timer / lowPassTemps);

            groupe.audioMixer.SetFloat(LOWPASSKEY, Mathf.Lerp(MAX_LOWPASS, MIN_LOWPASS, lerpVal));
            yield return null;
        }

        yield return new WaitForSeconds(5); // attente artificielle pendant le loading animation
        StartCoroutine(FadeVolumeRoutine(selectedSong)); // fade out volume final
    }

    private void FadeOutSongVol(int index)
    {
        AudioSource newSong = gameObject.AddComponent<AudioSource>();
        
        
        
        newSong.volume = 0.5f;
        newSong.outputAudioMixerGroup = groupe;
        newSong.clip = GameManager.mapSelectionnee.audio;
        newSong.time = GameManager.mapSelectionnee.mapMetadonnees.audioPreview;
        newSong.loop = true;
        newSong.Play();

        StartCoroutine(FadeVolumeRoutine(selectedSong, newSong));

        selectedSong = newSong;
        onSelectedSongSet?.Invoke(selectedSong);
    }

    private IEnumerator FadeVolumeRoutine(AudioSource currentSong)
    {
        float timer = 0f;

        while (timer < mapEnterfadeLength)
        {
            timer += Time.deltaTime;
            float lerpVal = mapEnterfadeOutCurve.Evaluate(timer / fadeLength);

            currentSong.volume = 1 - lerpVal;

            yield return null;
        }

        Destroy(currentSong);
    }

    private IEnumerator FadeVolumeRoutine(AudioSource sourceOld, AudioSource sourceNew)
    {
        float timer = 0f;

        while (timer < fadeLength)
        {
            timer += Time.deltaTime;
            float lerpVal = fadeOutCurve.Evaluate(timer / fadeLength);

            sourceOld.volume = 1 - lerpVal;
            sourceNew.volume = lerpVal;

            yield return null;
        }

        Destroy(sourceOld);
        sourceNew.volume = 1;
    }
}
