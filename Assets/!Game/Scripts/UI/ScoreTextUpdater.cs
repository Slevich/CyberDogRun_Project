using System;
using TMPro;
using UnityEngine;

public class ScoreTextUpdater : MonoBehaviour
{
    #region Fields
    [SerializeField]
    private bool _autoUpdate = false;
    
    private ScoreCounter _scoreCounter;
    private TextMeshProUGUI  _scoreText;
    #endregion


    #region Methods

    private void Awake()
    {
        if(_scoreText == null && TryGetComponent(out TextMeshProUGUI scoreText))
            _scoreText = scoreText;
    }

    public void UpdateText()
    {
        if (_scoreText != null)
            _scoreText.text = ScoreCounter.Instance.CurrentScore.ToString();
    }
    
    private void AutoUpdateText(int score)
    {
        if(_scoreText != null)
            _scoreText.text = score.ToString();
    }
    
    private void OnEnable()
    {
        if(_autoUpdate && _scoreText != null && ScoreCounter.Instance != null)
            ScoreCounter.Instance.ScoreChanged += (score) => AutoUpdateText(score);
    }

    private void OnDisable()
    {
        if(_autoUpdate && _scoreText != null && ScoreCounter.Instance != null)
            ScoreCounter.Instance.ScoreChanged -= (score) => AutoUpdateText(score);
    }
    #endregion
    
}
