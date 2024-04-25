using Nova;
using System;
using UnityEngine;

namespace Chroma.Editor
{
    public class EditorTimeManager : MonoBehaviour
    {
        [Header("Play/Pause button")]
        [SerializeField] private UIBlock2D pauseButton;
        [SerializeField] private UIBlock2D playButton;

        public static event Action<float> OnTimeChanged;
        KeyCode _playPauseCommandTouche = KeyCode.Space;
        [SerializeField] AudioSource source;
        [SerializeField] AudioSource sourceBPM;
        [SerializeField] AudioSource sourceVisualisation;

        [SerializeField] ActivePanelManager activePanelManager;

        public bool isPlaying
        {
            get { return source.isPlaying; }
            private set { throw new InvalidOperationException("Il est impossible de changer isPlaying au runtime"); }
        }

        public float editorTime
        {
            get
            {
                if (source.isPlaying)
                    return source.time;
                else return lastTime;
            }
            set
            {
                if(value < 0)
                {
                    value = 0; 
                }

                if (value >= source.clip.length)
                {
                    value = source.clip.length - 0.01f;
                }
                source.time = value;
                lastTime = value;
                OnTimeChanged?.Invoke(value);
            }
        }

        private float lastTime;

        private void OnEnable()
        {
            EditorMapManager.OnMapChanged += LoadClip;
            SettingsManager.onSongUpdated += LoadClipBPM;
            EditorScrubBar.onScrub += Scrub;
        }
        private void OnDisable()
        {
            EditorMapManager.OnMapChanged -= LoadClip;
            SettingsManager.onSongUpdated -= LoadClipBPM;
            EditorScrubBar.onScrub -= Scrub;
        }

        public void Scrub(float ratio)
        {
            editorTime = source.clip.length * ratio;
        }
        

        public void LoadClip(EditorMap map)
        {
            if (EditorMapManager.MapData.MapVisualData.audio != null)
            {
                source.clip = EditorMapManager.MapData.MapVisualData.audio;
                sourceBPM.clip = EditorMapManager.MapData.MapVisualData.audio;
                sourceVisualisation.clip = EditorMapManager.MapData.MapVisualData.audio;
            }
        }

        public void LoadClipBPM(string url)
        {
            sourceBPM.clip = new AudioLoader().LoadDataSync(url).clip;
        }

        private void Update()
        {
            if (Input.GetKeyDown(_playPauseCommandTouche) && activePanelManager.PanneauTopActif)
            {
                TogglePlayState();
            }
            
            if(!activePanelManager.PanneauTopActif)
            {
                Pause();
            }

            

            if (source.isPlaying)
            {
                lastTime = source.time;
                OnTimeChanged?.Invoke(source.time);
            }
            else
            {
                Pause();
            }
        }

        public void TogglePlayState()
        {
            if (source.isPlaying)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }


        public void Pause()
        {
            pauseButton.gameObject.SetActive(false);
            playButton.gameObject.SetActive(true);
            source.Pause();
        }

        public void Play()
        {
            pauseButton.gameObject.SetActive(true);
            playButton.gameObject.SetActive(false);
            source.Play();
        }
    }


}
