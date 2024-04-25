using NovaSamples.Effects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Video;

public class ImagePlacer : MonoBehaviour
{
    [SerializeField]
    BlurEffect effect;

    [SerializeField]
    RenderTexture renderTexture;

    private VideoPlayer player;

    private void Start()
    {
        getImage();
        player.Pause();
        StartCoroutine(SimpleRoutines.WaitUntil(() => TimeManager.i.tempsSecondes >= 0,
            () =>
            {
                player.Play();
            }));
    }

    private void OnEnable()
    {
        PauseManager.EnPause += PauseVideo;
        LifeManager.Defaite += () => PauseVideo(true);
    }

    private void OnDisable()
    {
        PauseManager.EnPause -= PauseVideo;
        LifeManager.Defaite -= () => PauseVideo(true);        
    }

    private void getImage()
    {
        if (GameManager.Instance.videoUrl != "")
        {
            player = gameObject.AddComponent<VideoPlayer>();
            player.url = GameManager.Instance.videoUrl;
            player.renderMode = VideoRenderMode.RenderTexture;
            effect.InputTexture = renderTexture;
            player.targetTexture = renderTexture;

            player.isLooping = true;
            player.Play();

            for (ushort i = 0; i < 2; i++)
            {
                player.SetDirectAudioMute(i, true);
            }
        }
        else
        {
            effect.InputTexture = GameManager.Instance.imageData.image;
        }
    }

    private void PauseVideo(bool paused)
    {
        if (player == null)
            return;

        if (paused)
        {
            player.Pause();
        }
        else
        {
            player.Play();
        }
    }
}
