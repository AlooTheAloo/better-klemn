using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct AudioEffet
{
    public AudioClip clip;
    public SFX sfx;
}

[Serializable]
public enum SFX
{
    OPTION_CANCEL,
    OPTION_HOVER,
    OPTION_SELECT,
    OPTION_SONGSELECT,
    MUSIC_SCORE
}

public class SoundEffectManager : MonoBehaviour
{
    [SerializeField] public List<AudioEffet> effetList;

    [SerializeField]
    AudioSource sfxSource;

    public static SoundEffectManager i;


    public void Awake()
    {
        if (i == null)
        {
            i = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SongSelectManager.onMapEnter += () => PlaySFX(SFX.OPTION_SONGSELECT, 1, false);
        SongSelectManager.onMapSelectedChanged += _ => PlaySFX(SFX.OPTION_HOVER, 1, false);
        StatsGameOver.onStatsSceneLoad += _ => PlaySFX(SFX.MUSIC_SCORE, 1, false);
    }

    private void OnDisable()
    {
        SongSelectManager.onMapEnter -= () => PlaySFX(SFX.OPTION_SONGSELECT, 1, false);
        SongSelectManager.onMapSelectedChanged -= _ => PlaySFX(SFX.OPTION_HOVER, 1, false);
        StatsGameOver.onStatsSceneLoad -= _ => PlaySFX(SFX.MUSIC_SCORE, 1, false);
    }


    public void PlaySFX(SFX type, float volume, bool loop)
    {
        if (sfxSource != null)
        {
            AudioEffet effet = effetList.Find(x => x.sfx == type);

            sfxSource.clip = effet.clip;
            sfxSource.volume = volume;
            sfxSource.loop = loop;

            sfxSource.Play();
        }
    }
}
