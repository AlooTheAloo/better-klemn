using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AccuracyAnimation : MonoBehaviour
{
    [SerializeField] AnimationCurve curve;
    [SerializeField] TextMeshPro text;
    [SerializeField] float animationDuration = 0.5f;
    [SerializeField] float animationStrength = 500f;

    
    // Start is called before the first frame update
    void Start()
    {
        curve.Evaluate(0);
        OnGetScore();
        print("meow");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Button("On")]
    void OnGetScore()
    {
        StartCoroutine(AnimatedText());
    }

    private IEnumerator AnimatedText()
    {
       // yield return new WaitForSeconds(2f);
        
        float elapseTime = 0f;
        float endValueText = animationStrength;

        while (elapseTime < animationDuration)
        {
            
            text.characterSpacing = curve.Evaluate(elapseTime / animationDuration)* animationStrength;
            elapseTime += Time.deltaTime;

            
            yield return null;
        }

        
        text.characterSpacing = endValueText;
    }

}
