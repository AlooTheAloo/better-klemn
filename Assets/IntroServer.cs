using System;
using System.Collections;
using System.Collections.Generic;
using Riptide;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Video;

public class IntroServer : MonoBehaviour
{
    [SerializeField] VideoPlayer[] videos;
    
    private static event Action playvideo;

    public void OnEnable(){ 
        playvideo += PlayVideo;
    }

    public void OnDisable(){ 
        playvideo -= PlayVideo;
    }


    public void PlayVideo(){
        print("Playing videos");
        foreach(var v in videos)
            v.Play();
    }


    
    
    [MessageHandler((ushort) ClientToServerCalls.PLAY_INTRO)]
    public static void PlayIntro(ushort cid, Message message){
        print("invoking playinto");
        playvideo?.Invoke();
    }


}
