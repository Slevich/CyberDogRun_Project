using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class AxisMovementSystem : MonoBehaviour
{
    #region Fields
    [SerializeField, HideInInspector]
    private List<SingleAxisMovementData> _movementDatas = new List<SingleAxisMovementData> ();
    private CancellationTokenSource _cancellationTokenSource;
    private bool _inProcess = false;
    #endregion
    
    #region Methods
    public void SetMovementData(GameObject[] DataHolders)
    {
        if(DataHolders.Length == 0)
            return;

        _movementDatas.Clear();
        
        foreach (GameObject gameObject in DataHolders)
        {
            Component moveDataComponent = ComponentsSearcher
                .GetSingleComponentOfTypeFromObjectAndChildren(gameObject, typeof(SingleAxisMovementData));
            
            if(moveDataComponent != null)
                _movementDatas.Add((SingleAxisMovementData)moveDataComponent);
        }
        
        if(_movementDatas.Count > 0 && Application.isPlaying)
            MoveAlongAxis();
    }

    public async void MoveAlongAxis()
    {
        if(_inProcess)
            return;
        
        if(_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            return;

        if (_movementDatas.Count == 0)
        {
            return;
        }
        
        _cancellationTokenSource = new CancellationTokenSource();
        _inProcess = true;

        while (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
        {
            try
            {
                foreach (SingleAxisMovementData movementData in _movementDatas)
                {
                    if(!movementData.gameObject.activeInHierarchy)
                        continue;
                    
                    Vector3 currentPosition = movementData.transform.position;
                    Vector3 newPosition = currentPosition + (movementData.MovementDirectionUnclamped * Time.timeScale);
                    movementData.transform.position = newPosition;
                }
            
                await UniTask.WaitForFixedUpdate(_cancellationTokenSource.Token);
            }
            catch (OperationCanceledException exception)
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
                break;
            }
        }
        
        _inProcess = false;
    }
    
    public void StopMovement()
    {
        if(!_inProcess)
            return;
        
        if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();
        }
        
        _inProcess = false;
    }

    private void OnDisable() => StopMovement();
    #endregion
}
