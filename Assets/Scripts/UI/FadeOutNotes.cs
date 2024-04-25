using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Pool;
using Shapes;
using Sirenix.Utilities;
using UnityEngine;

public class FadeOutNotes : MonoBehaviour
{

    [SerializeField] private GameObject noteTrail;
    private Rectangle trailComponent;

    private Rectangle shapeComponent;




    [SerializeField] private AnimationCurve fadeCurve; 

    [SerializeField] private float fadeDuration;

    private bool isLongNote;

    public static event Action<GameNoteActor> onFadeOutComplete;
    

    public Alley thisAlley;
    public float vitesseMovement;

    public void BeginFade(bool isLongNote)
    {
        this.isLongNote = isLongNote;
        shapeComponent = gameObject.GetComponent<Rectangle>();
        if (isLongNote)
        {
             trailComponent = noteTrail.GetComponent<Rectangle>();
        }

        // Start the fade out coroutine
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float timer = 0f; 

        
        while (timer < fadeDuration / vitesseMovement)
        {
            
            float t = timer / (fadeDuration / vitesseMovement);

            
            float currentOpacity = Mathf.Lerp(1, 0, fadeCurve.Evaluate(t));

           
            Color sColor = shapeComponent.Color;
            sColor.a = currentOpacity;
            
            shapeComponent.Color = sColor;
            if (isLongNote)
            {
                Color tColor = trailComponent.Color;
                tColor.a = currentOpacity;
                trailComponent.Color = tColor;
            }
            
            
            timer += Time.deltaTime;
            

            yield return null;
        }

        var actor = gameObject.GetComponent<GameNoteActor>();
        thisAlley.notesDansAllee.Remove(actor);
        onFadeOutComplete?.Invoke(actor);
    }
}
