using Nova;
using UnityEngine;

public class ScoreTracker : MonoBehaviour
{
    private TextBlock _scoreTextBlock;
    
    private void Awake()
    {
        _scoreTextBlock = GetComponent<TextBlock>();
    }
    
    private void OnEnable()
    {
       ScoreManager.OnScoreChange += OnScoreChanged;
    }
    
    private void OnDisable()
    {
        ScoreManager.OnScoreChange -= OnScoreChanged;
    }
    
    private void OnScoreChanged(ulong score)
    {
        _scoreTextBlock.Text = $"{score} pts";
    }
}
