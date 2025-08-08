using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class AudioSourceAnimation : MonoBehaviour
{
    #region Fields
    [SerializeField]
    private AudioSource _audioSource;
    [SerializeField]
    private float _duration = 1f;

    private CancellationTokenSource _cancellationTokenSource;
    private bool _inProcess = false;
    #endregion


    #region Methods

    public void SetVolume(float VolumeValue)
    {
        if(_audioSource == null)
            return;
        
        _audioSource.volume = VolumeValue;
    }
    
    public async void LerpVolumeToTarget(float TargetVolume)
    {
        if(_audioSource == null)
            return;
        
        if(_inProcess)
            return;
        
        _inProcess = true;
        
        if(_cancellationTokenSource == null)
            _cancellationTokenSource = new CancellationTokenSource();

        float fixedUpdatesInDuration = _duration / Time.fixedDeltaTime;
        float volumePerFixedUpdate = (TargetVolume - _audioSource.volume) / fixedUpdatesInDuration;

        if(volumePerFixedUpdate == 0)
            return;
        
        while (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
        {
            try
            {
                await UniTask.WaitForFixedUpdate(cancellationToken: _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException exception)
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            float newVolume = _audioSource.volume + volumePerFixedUpdate;
            bool reachValue = false;
            
            if (volumePerFixedUpdate > 0f && newVolume >= TargetVolume)
            {
                reachValue = true;
            }
            else if (volumePerFixedUpdate < 0f && newVolume <= TargetVolume)
            {
                reachValue = true;
            }

            if (reachValue)
            {
                _audioSource.volume = TargetVolume;
                break;
            }
            else
            {
                _audioSource.volume = newVolume;
            }
        }
    }


    public void StopLerping()
    {
        if(!_inProcess)
            return;
        
        if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();
        }
        
        _inProcess = false;
    }

    private void OnDisable() => StopLerping();
    #endregion
}
