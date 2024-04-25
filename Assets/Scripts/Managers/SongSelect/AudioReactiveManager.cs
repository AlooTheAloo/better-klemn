using System;
using System.Linq;

namespace Managers.SongSelect
{
    using System.Collections;
    using System.Collections.Generic;
    using Nova;
    using UnityEngine;

    public class AudioReactiveManager : MonoBehaviour
    {
        private AudioSource source;
        private float[] samples;
        private const int SAMPLE_SIZE = 1024;
        
        [SerializeField] private List<UIBlock2D> objectsLists = new();
        private Dictionary<UIBlock2D, Tuple<float, float>> dictionary = new();

        private Coroutine analyzerCoroutine;
        
        private void OnEnable()
        {
            weights = new float[50];
            for (int i = 0; i < 25; i++)
            {
                weights[i] = 1;
            }
            
            for (int i = 25; i < 50; i++)
            {
                weights[i] = 0.5f;
            }
            
            foreach (var obj in objectsLists)
            {
                dictionary.Add(obj, new Tuple<float, float>(obj.Size.X.Value, obj.Size.Y.Value));
            }
            SongSelectAudioManager.onSelectedSongSet += SongSelectAudioManagerOnonSelectedSongSet;
            SongSelectManager.onMapEnter += StopAnalyzer;
        }
        
        private void OnDisable()
        {
            dictionary.Clear();
            SongSelectAudioManager.onSelectedSongSet -= SongSelectAudioManagerOnonSelectedSongSet;
            SongSelectManager.onMapEnter -= StopAnalyzer;
        }

        private void StopAnalyzer()
        {
            if (analyzerCoroutine != null)
                StopCoroutine(analyzerCoroutine);
        }
        
        private void SongSelectAudioManagerOnonSelectedSongSet(AudioSource obj)
        {
            source = obj;
            analyzerCoroutine = StartCoroutine(UpdateAnalyser());
        }

        private const float TICK = 0.0f;
        private const float BASE_SCALE_MUL = 1f;
        private const float MAX_SCALE_MUL = 3f;

        IEnumerator UpdateAnalyser()
        {
            while (true)
            {
                yield return null;
                yield return new WaitForSeconds(TICK);
                float rms = AnalyseSound();
                foreach (var kvp in dictionary)
                {
                    kvp.Key.Size.X.Value = Mathf.Lerp(kvp.Value.Item1 * BASE_SCALE_MUL, kvp.Value.Item1 * MAX_SCALE_MUL, rms);
                    kvp.Key.Size.Y.Value = Mathf.Lerp(kvp.Value.Item2 * BASE_SCALE_MUL, kvp.Value.Item2 * MAX_SCALE_MUL, rms);              
                }
            }
        }
    
        float AnalyseSound()
        {
            samples = new float[SAMPLE_SIZE];
            source.GetOutputData(samples, 0);
            int i = 0;
            float sum = 0;
            for (; i < SAMPLE_SIZE; i++)
            {
                sum += samples[i] * samples[i];
            }
            var rms = Mathf.Sqrt(sum / SAMPLE_SIZE);
            queueOfRms.Enqueue(rms);
            while (queueOfRms.Count > weights.Length)
            {
                queueOfRms.Dequeue();
            }
            return weightedAverage();
        }

        private Queue<float> queueOfRms = new();

        private float[] weights = { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1 };

        private float weightedAverage()
        {
            var queueAsList = queueOfRms.Reverse().ToArray();
            float sum = 0;
            float sumOfWeights = 0;
            for (int i = 0; i < queueAsList.Length; i++)
            {
                sum += weights[i] * queueAsList[i];
                sumOfWeights += weights[i];
            }
            return sum / sumOfWeights;
        }
    }
}