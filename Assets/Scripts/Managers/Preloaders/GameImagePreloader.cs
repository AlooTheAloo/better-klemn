using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public struct ImageData : IGameData
{
    public Texture2D image;
    public Color imageCouleur;
    
    public ImageData(Texture2D image, Color imageCouleur){ 
        this.image = image;
        this.imageCouleur = imageCouleur;
    }
}

public class GameImagePreloader : Preloader<ImageData>
{
    public override IPreloader CommencerChargement(Action<ImageData> onComplete)
    {
        GameManager.Instance.StartCoroutine(chargerMusiqueCoroutine(onComplete));
        return this;
    }

    public IEnumerator chargerMusiqueCoroutine(Action<ImageData> onComplete)
    {
        yield return new WaitForSeconds(1);
        onComplete?.Invoke(new ImageData(GameManager.mapSelectionnee.imageArriere, 
            GameManager.mapSelectionnee.couleur));
    }
}

