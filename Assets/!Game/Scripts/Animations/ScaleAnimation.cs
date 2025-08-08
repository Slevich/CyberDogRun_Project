using System;
using DG.Tweening;
using UnityEngine;

public class ScaleAnimation : MonoBehaviour
{
    #region Fields
    [Header("Start local scale."), SerializeField]
    private Vector3 _startScale = Vector3.zero;
    [Header("Scale to start size duration."), SerializeField, Range(0f, 10f)]
    private float _startScaleDuration = 1f;
    [Header("Scale to start ease."), SerializeField]
    private Ease _startScaleEase = Ease.Linear;
    
    [Header("Target local scale."), SerializeField]
    private Vector3 _targetScale = Vector3.one;
    [Header("Scale to target size duration."), SerializeField, Range(0f, 10f)]
    private float _targetScaleDuration = 1f;
    [Header("Scale to start ease."), SerializeField]
    private Ease _targetScaleEase = Ease.Linear;
    
    private Tween _currentTween;
    #endregion

    #region Methods
    private void Awake()
    {
        transform.localScale = _startScale;
    }

    public void SetStartScale() => transform.localScale = _startScale;
    public void SetTargetScale() => transform.localScale = _targetScale;
    
    public void ScaleToStartSize() => StartScaleAnimation(_startScale, _startScaleDuration, _startScaleEase);
    
    public void ScaleToTargetSize() => StartScaleAnimation(_targetScale, _targetScaleDuration, _targetScaleEase);

    private void StartScaleAnimation(Vector3 targetScale, float duration, Ease ease)
    {
        KillCurrentTween();
        _currentTween = transform.DOScale(targetScale, duration);
        _currentTween.SetEase(ease);
        _currentTween.Play();
    }

    private void KillCurrentTween()
    {
        if(_currentTween != null && _currentTween.active)
            _currentTween.Kill();
    }
    #endregion
}
