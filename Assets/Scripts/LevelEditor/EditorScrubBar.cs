using System;
using Nova;
using UnityEngine;

namespace Chroma.Editor
{

    public class EditorScrubBar : MonoBehaviour
    {
        private const float LEFT_POINT = -8.32f;
        private const float RIGHT_POINT = -3.37f;

        public static event Action<float> onScrub;
    
        [SerializeField] private UIBlock2D block;
        private void OnEnable()
        {
            block.AddGestureHandler<Gesture.OnDrag>(OnDrag);
            block.AddGestureHandler<Gesture.OnPress>(OnPress);
        }

        private void OnPress(Gesture.OnPress evt)
        {
            float ratio = Mathf.InverseLerp(LEFT_POINT, RIGHT_POINT, evt.PointerWorldPosition.x);
            onScrub?.Invoke(ratio);
        }
        
        private void OnDrag(Gesture.OnDrag evt)
        {
            float ratio = Mathf.InverseLerp(LEFT_POINT, RIGHT_POINT, evt.PointerPositions.Current.x);
            onScrub?.Invoke(ratio);
        }
    }
    
}
