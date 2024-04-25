using Shapes;
using System.Collections;
using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class LifeManager : MonoBehaviour
{
    const int MAX_HP_SETTING = 10;
    const int MULTIPLICATEUR_HP_PARFAIT = 3;
    const int MULTIPLICATEUR_HP_BON = 3;
    const int MULTIPLICATEUR_HP_OK = 3;



    [SerializeField] public int tempsAvantDefaite;
    [SerializeField] private int MAX_HEALTH;
    [SerializeField] private float longueurAnimation;
    [SerializeField] private Rectangle barreVie;
    [SerializeField] public AnimationCurve curveAnimationBarreVie;
    [SerializeField] private TextMeshPro texteDefaite;
    
    [SerializeField] private Volume transitionVolume;

    private Coroutine coroutineActive;

    public static bool isLosing;
    

    public static event Action Defaite;

    private float largeurBarre;
    private float comboErreurs = 0;

    public static LifeManager _lifeManager;



    private float vieActuelle {
        get {  return _vieActuelle; }
        set
        {
            if(value > MAX_HEALTH)
            {
                value = MAX_HEALTH;
            }

            _vieActuelle = value;
            if(coroutineActive != null)
            {
                StopCoroutine(coroutineActive);
            }

            coroutineActive = StartCoroutine(AnimerBarreVie(value));
        }
    }
    private float _vieActuelle;

    private void OnEnable()
    {
        Alley.onJugement += DeterminerVie;
        largeurBarre = barreVie.Width;
        vieActuelle = MAX_HEALTH;
        isLosing = false;
        _lifeManager = this;
    }

    private void OnDisable()
    {
        Alley.onJugement -= DeterminerVie;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            vieActuelle -= 20;
        }
    }


    private void DeterminerVie(int joueur, Precision precision, Alley a) {

        var hpSetting = GameManager.Instance.gameNoteData.map.metadonnees.hp;
        var hpBoost = MAX_HP_SETTING + 1 - hpSetting;

        switch (precision) {
            case Precision.PARFAIT:
                comboErreurs = 0;
                vieActuelle += hpBoost * MULTIPLICATEUR_HP_PARFAIT;
                break;
            case Precision.BIEN:
                comboErreurs = 0;
                vieActuelle += hpBoost * MULTIPLICATEUR_HP_BON;
                break;
            case Precision.OK:
                comboErreurs = 0;
                vieActuelle += hpBoost * MULTIPLICATEUR_HP_OK;
                break;
            case Precision.RATE:
                comboErreurs++;
                vieActuelle -= hpSetting * comboErreurs;
                break;
        }

        //En cas de défaite
        if (vieActuelle <= 0 && !isLosing) {
            Defaite?.Invoke();
            StartCoroutine(AnimerDefaite());
        }

    }

    private IEnumerator AnimerBarreVie(float progresFinal)
    {
        float progresInitial = GetProgresBarre();
        float compteur = 0;
        while (compteur <= 1)
        {
            SetProgresBarre(Mathf.Lerp(progresInitial, progresFinal, curveAnimationBarreVie.Evaluate(compteur)));
            compteur += Time.deltaTime / longueurAnimation;
            yield return new WaitForEndOfFrame();
        }
        SetProgresBarre(progresFinal);
        coroutineActive = null;
    }

    private IEnumerator AnimerDefaite()
    {
        isLosing = true;
        StartCoroutine(PerformGrayScale());
        StartCoroutine(AfficherTextDefaite());
        yield return new WaitForSeconds(tempsAvantDefaite / 2f);
        StartCoroutine(BlackoutTransition());

        yield return new WaitForSeconds(tempsAvantDefaite / 2f);
        SceneManager.LoadScene((int) Scenes.ENDGAME);
        
    }

    private IEnumerator PerformGrayScale()
    {
        ColorAdjustments colorGrading = transitionVolume.profile.TryGet<ColorAdjustments>(out var tmp) ? tmp : null;
        
        float timer = 0f; 
        float startScale = colorGrading.saturation.value; 
        float endScale = -100f;

        float duration = tempsAvantDefaite / 2f;
        while (timer < duration)
        {
            float t = timer / duration; 

            
            float scale = Mathf.Lerp(startScale, endScale, curveAnimationBarreVie.Evaluate(t));

          
            colorGrading.saturation.value = scale;

            timer += Time.unscaledDeltaTime; 

            yield return new WaitForFixedUpdate();
        }

        colorGrading.saturation.value = colorGrading.saturation.min;
        
        
        yield return null;
    }

    private IEnumerator AfficherTextDefaite()
    {
        while (texteDefaite.alpha < 1) {
                    texteDefaite.alpha += Time.fixedDeltaTime * 0.75f;
                    yield return new WaitForFixedUpdate();
        }
        yield return null;
    }

    private IEnumerator BlackoutTransition()
    {

           float timer = 0f;
           float startScale = 100f;
           float endScale = 0f;
        
        
        ChannelMixer colorMixer = transitionVolume.profile.TryGet<ChannelMixer>(out var tmp) ? tmp : null;

        float duration = tempsAvantDefaite / 2f;
        while (timer < duration)
        {
            float t = timer / duration; 

            
            float scale = Mathf.Lerp(startScale, endScale, curveAnimationBarreVie.Evaluate(t));

            colorMixer.redOutRedIn.value = scale;
            colorMixer.blueOutBlueIn.value = scale;
            colorMixer.greenOutGreenIn.value = scale;

            timer += Time.unscaledDeltaTime; 

            yield return new WaitForFixedUpdate();
        }
        
        yield return new WaitForFixedUpdate();
    }

    void SetProgresBarre(float progres)
    {
        barreVie.Width = progres / MAX_HEALTH * largeurBarre;
    }

    float GetProgresBarre()
    {
        return barreVie.Width / largeurBarre * MAX_HEALTH;
    }

    private void OnApplicationQuit()
    {
        ResetColours();
    }

    private void ResetColours()
    {
        ChannelMixer colorMixer = transitionVolume.profile.TryGet<ChannelMixer>(out var tmp) ? tmp : null;
        colorMixer.redOutRedIn.value = 100f;
        colorMixer.blueOutBlueIn.value = 100f;
        colorMixer.greenOutGreenIn.value = 100f;
    }
}