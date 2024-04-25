using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using static UnityEngine.GraphicsBuffer;

public class CameraMovementManager : MonoBehaviour
{
    [SerializeField]
    AnimationCurve curve;

    [SerializeField]
    Camera gameSceneCamera;

    public float tempsAnimation;
    public float targetSize;
    public float IntervalleBeats;

    private Action OnCameraMovementStarted;

    private void OnEnable()
    {
        OnCameraMovementStarted += () => TimeManager.i.ActionDansBeats(IntervalleBeats, () => StartCoroutine(BoomCamera()));
        TimeManager.ActionABeats(0, () => StartCoroutine(BoomCamera()));
    }

    private void OnDisable()
    {
        OnCameraMovementStarted -= () => TimeManager.i.ActionDansBeats(IntervalleBeats, () => StartCoroutine(BoomCamera()));
    }

    IEnumerator BoomCamera()
    {
        OnCameraMovementStarted?.Invoke();
        float tailleInitiale = gameSceneCamera.orthographicSize;
        float timer = 0;
        while (timer < tempsAnimation)
        {
            gameSceneCamera.orthographicSize = Mathf.Lerp(
                tailleInitiale,
                targetSize,
                curve.Evaluate(timer / tempsAnimation)
            );
            yield return null;
            timer += Time.deltaTime;
        }
    }
}
