using System;
using UnityEngine;
using UnityEngine.Networking;

public class LightsClient
{
    private static string Lightserverurl = "";
    private static LedLightState _state = LedLightState.Manual;


    public static void Intro()
    {
        MakeWebRequest($"intro");
    }
    
    public static void SetReady(int player, bool ready)
    {
        MakeWebRequest($"setready/{player}/{ready}");
    }
    
    public static void SetLedMode(LedLightState state)
    {
        MakeWebRequest($"setmode/{(int)state}");
        _state = state;
    }

    public static void ChangeLedColor(Color color)
    {
        MakeWebRequest($"changecolor/{(int) (color.r * 255)}/{(int)(color.g * 255)}/{(int) (color.b * 255)}/{(int) (color.a)}");
    }
    
    public static void AddLine(Color color)
    {
        if (_state != LedLightState.Lineup)
        {
            throw new InvalidOperationException("Impossible de mettre des lignes lorsque le mode n'est pas 'LINEUP'");
        }
        MakeWebRequest($"addLine/{(int) (color.r * 255)}/{(int) (color.g * 255)}/{(int) (color.b * 255)}");
    }
    
    public static void Pulse(Color color, int lightID)
    {
        if (_state != LedLightState.Pulse)
        {
            throw new InvalidOperationException("Impossible de faire un pulse lorsque le mode n'est pas 'PULSE'");
        }
        MakeWebRequest($"pulse/{(int) (color.r * 255)}/{(int) (color.g * 255)}/{(int) (color.b * 255)}/{lightID}");
    }
    
    private static void MakeWebRequest(string parameters)
    {
        if (string.IsNullOrEmpty(Lightserverurl)) Lightserverurl = Constantes.LIGHT_API_BASE_URL;
        
        UnityWebRequest req = UnityWebRequest.Get($"{Lightserverurl}/{parameters}");
        Debug.Log($"sent request : {Lightserverurl}/{parameters}");
        req.SendWebRequest().completed += _ =>
        {
            req.Dispose();
        };
    }

    public static void ActiverDMX(int color, int pattern)
    {
        MakeWebRequest($"dmx/kiai/{color}/{pattern}");
    }
    
    public static void DesactiverDMX()
    {
        MakeWebRequest("dmx/stopkiai");
    }
    
}

public enum LedLightState
{
    Manual = 0,
    Lineup = 1,
    Pulse = 2,
    INTRO = 3,
}
