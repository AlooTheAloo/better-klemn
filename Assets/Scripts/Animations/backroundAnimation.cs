using Nova;
using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class backroundAnimation : MonoBehaviour
{
    [SerializeField]
    private UIBlock2D bgBlock;

    private Coroutine bgColorRoutine;

    private void Awake()
    {
        bgColorRoutine = StartCoroutine(ChangerBGColor(0.05f));
    }

    private IEnumerator ChangerBGColor(float speed)
    {
        float bgColorTimer = 0;
        while (true)
        {
            yield return null;
            bgColorTimer += Time.deltaTime * speed;
            bgColorTimer %= 1;
            bgBlock.Gradient.Color = Color.HSVToRGB(bgColorTimer, 0.75f, 0.8f);
        }
    }
}

