using Nova;
using Riptide;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ServerSongSelectManager : MonoBehaviour
{ 
    [SerializeField] private AnimationCurve courbe;
    [SerializeField] private float animationLength;
    [SerializeField] private UIBlock2D[] lumieres;

    Coroutine routine;
    private static event Action<Color> onGetColor;

    [MessageHandler((ushort) ClientToServerCalls.SONG_SELECT_CHANGE_BG)]
    private static void FadeBGColor(ushort clientID, Message m)
    {
        float r = m.GetFloat();
        float g = m.GetFloat();
        float b = m.GetFloat();

        onGetColor?.Invoke(new Color(r, g, b, 1));
    }

    private void OnEnable()
    {
        onGetColor += fadeToColor;
    }

    private void OnDisable()
    {
        onGetColor -= fadeToColor;
    }

    private void fadeToColor(Color c)
    {
        print("Received color " + c);
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(fadeToColorRoutine(c));
    }

    private IEnumerator fadeToColorRoutine(Color c)
    {
        Color couleurOriginale = lumieres.First().Color;
        float timer = 0;
        while(timer < animationLength)
        {
            foreach(var lumiere in lumieres)
            {
                lumiere.Color = Color.Lerp(couleurOriginale, c, courbe.Evaluate(timer / animationLength));
            }
            timer += Time.deltaTime;
            yield return null;
        }
    }
}
