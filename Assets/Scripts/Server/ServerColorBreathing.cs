using Nova;
using Riptide;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerColorBreathing : MonoBehaviour
{
    [SerializeField] private UIBlock2D[] ecrans;
    public static event Action<float> onBreathingDataReceivedStart;
    Coroutine routineBreathing;

    [MessageHandler((ushort) ClientToServerCalls.SYNCHRONISE_BREATHING)]
    public static void SyncBreathing(ushort client, Message message)
    {
        float speed = message.GetFloat();
        onBreathingDataReceivedStart?.Invoke(speed);
    }

    private void OnEnable()
    {
        onBreathingDataReceivedStart += StartBreathing;
    }

    private void OnDisable()
    {
        if(routineBreathing != null)
        {
            StopCoroutine(routineBreathing);
        }
        onBreathingDataReceivedStart -= StartBreathing;
    }

    public void StartBreathing(float speed)
    {
        if (routineBreathing != null)
        {
            StopCoroutine(routineBreathing);
        }
        routineBreathing = StartCoroutine(Breathing(speed));
    }

    IEnumerator Breathing(float speed)
    {
        print("Breathing with speed " + speed);                                                             
        float bgColorTimer = 0;
        while (true)
        {
            print(bgColorTimer);
            yield return null;
            bgColorTimer += Time.deltaTime * speed;
            bgColorTimer %= 1;
            foreach(var ecran in ecrans)
            {
                ecran.Color = Color.HSVToRGB(bgColorTimer, 1, 1);
            }
        }
    }

}
