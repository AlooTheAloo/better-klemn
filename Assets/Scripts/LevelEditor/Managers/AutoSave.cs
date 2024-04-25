using System;
using System.Collections;
using System.Collections.Generic;
using Nova;
using UnityEngine;

public class AutoSave : MonoBehaviour
{

    [SerializeField] private float saveInterval;
    [SerializeField] private TextBlock saveTextBlock;
    [SerializeField] private float animationDurationSaveText = 3f;
    
    void Start()
    {
        saveTextBlock.Color = new Color(saveTextBlock.Color.r, saveTextBlock.Color.g, saveTextBlock.Color.b, 0);   
        StartCoroutine(RegularSave());
        EditorHotkeys.saveChanges += FadeOutSaveText;
    }

    private void OnDisable()
    {
        EditorHotkeys.saveChanges -= FadeOutSaveText;
    }

    private void FadeOutSaveText()
    {
        StartCoroutine(FadeOutSaveTextRoutine(animationDurationSaveText));
    }
    
    
    private IEnumerator RegularSave()
    {
        if (saveInterval != 0f)
        {
            while (true)
            {
                EditorHotkeys.saveChanges.Invoke();
                yield return new WaitForSeconds(saveInterval);
            }
        }
        
    }

    private IEnumerator FadeOutSaveTextRoutine(float seconds)
    {
        float timer = 0;
        saveTextBlock.Color = new Color(saveTextBlock.Color.r, saveTextBlock.Color.g, saveTextBlock.Color.b, 1);   
        while (timer < seconds)
        {
            float opacite = 1 - (timer / seconds);
            saveTextBlock.Color = new Color(saveTextBlock.Color.r, saveTextBlock.Color.g, saveTextBlock.Color.b, opacite);
            timer += Time.deltaTime;
            yield return null;
        }
        
    }
}
