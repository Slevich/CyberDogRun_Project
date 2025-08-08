using System;
using DG.Tweening;
using UnityEngine;

public class SpriteAnimation : MonoBehaviour
{
    #region Fields
    [Header("Target color."), SerializeField]
    private Color _targetColor;
    [Header("Target color change duration."), SerializeField]
    private float _colorDuration = 1f;
    [Header("Color changing ease."), SerializeField]
    private Ease _colorEase = Ease.Linear;
    
    private Color _startColor;
    private SpriteRenderer _spriteRenderer;
    private Tween _currentTween;
    #endregion

    #region Methods

    private void Awake()
    {
        if(_spriteRenderer == null && TryGetComponent(out SpriteRenderer spriteRenderer))
            _spriteRenderer = spriteRenderer;
        
        _startColor =  _spriteRenderer.color;
    }


    public void StartColorFlashing()
    {
        if(_spriteRenderer == null)
            return;
        
        StopCurrentAnimation();
        
        _currentTween = _spriteRenderer.DOColor(_targetColor, _colorDuration).SetLoops(-1, LoopType.Yoyo);
        _currentTween.SetEase(_colorEase);
        _currentTween.Play();
    }

    public void StopCurrentAnimation()
    {
        if (_currentTween != null)
        {
            _currentTween.Kill();
            _currentTween = null;
        }
    }

    public void SetStartColor()
    {
        if(_spriteRenderer == null)
            return;
        
        _spriteRenderer.color = _startColor;
    }
    #endregion
}
