using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TimescaleAccelerator : MonoBehaviour
{
    #region Field
    [Header("Time scales increment on this value each minute."), SerializeField] 
    private float _timeScaleIncrementPerMinute = 1f;

    private float _timeStepInSeconds = 1f;
    private float _timeStepIncrement = 0f;
    private CancellationTokenSource _cancellationTokenSource;
    private bool _executing = false;
    #endregion

    #region Methods
    private void OnValidate()
    {
        if (_timeStepInSeconds <= 0)
            _timeStepInSeconds = 0.1f;
        else if (_timeStepInSeconds > 60f)
            _timeStepInSeconds = 60f;
    }

    private void Awake()
    {
        _timeStepIncrement = (_timeScaleIncrementPerMinute / 60) * _timeStepInSeconds;
    }

    private async void StartExecution()
    {
        if(_executing)
            return;
        
        _executing = true;
        Time.timeScale = 1f;
        
        if (_cancellationTokenSource == null)
            _cancellationTokenSource = new CancellationTokenSource();

        while (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
        {
            try
            {
                await UniTask.WaitForSeconds(_timeStepInSeconds, true, PlayerLoopTiming.FixedUpdate, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException exception)
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
                break;
            }
            
            Time.timeScale += _timeStepIncrement;
        }
    }

    private void StopExecution()
    {
        if(!_executing)
            return;
        
        if (_cancellationTokenSource != null && _cancellationTokenSource.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();
        }
        
        _executing = false;
        Time.timeScale = 1f;
    }

    private void OnEnable() => StartExecution();
    private void OnDisable() => StopExecution();
    #endregion
}
