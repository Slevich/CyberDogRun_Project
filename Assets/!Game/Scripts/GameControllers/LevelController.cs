using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class LevelController : MonoBehaviour
{
    #region Fields
    [Header("Delay before level started is seconds."), SerializeField]
    private float _startDelay = 0f;
    
    [Header("Delay before level ended is seconds."), SerializeField]
    private float _endDelay = 0f;
    [Header("Event after ending delay."), SerializeField]
    private UnityEvent _onEndEvent;
    
    private bool _alreayEntered = false;
    
    private CancellationTokenSource _cancellationTokenSource;
    private static LevelController _instance;
    #endregion
    
    #region Properties

    public static LevelController Instance
    {
        get
        {
            if(_instance == null)
                _instance = FindObjectOfType<LevelController>();
            
            return _instance;
        }
    }
    #endregion
    
    #region Methods
    private void Start()
    {
        if(_instance != null &&  _instance != this)
            Destroy(gameObject);
    }

    public async void EntryPoint()
    {
        if(_alreayEntered)
            return;
        
        _alreayEntered = true;
        InputProvider.UIInputEnabled = true;
        
        if (_startDelay > 0)
        {
            if(_cancellationTokenSource != null &&  !_cancellationTokenSource.IsCancellationRequested)
                return;
        
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_startDelay), cancellationToken: _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException exception)
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }
        
        LevelBlocksManager.Instance.StartExecution();
        InputProvider.PlayerInputEnabled = true;
    }

    public async void ExitPoint()
    {
        if(!_alreayEntered)
            return;
        
        if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();
        }
                
        _alreayEntered = false;
        
        LevelBlocksManager.Instance.StopExecution();
        InputProvider.UIInputEnabled = false;
        InputProvider.PlayerInputEnabled = false;
        
        if (_endDelay > 0)
        {
            if(_cancellationTokenSource != null &&  !_cancellationTokenSource.IsCancellationRequested)
                return;
        
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_endDelay), cancellationToken: _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException exception)
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }

        _onEndEvent?.Invoke();
    }
    
    private void OnEnable() => EntryPoint();

    private void OnDisable() => ExitPoint();
    #endregion
}
