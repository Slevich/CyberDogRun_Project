using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniRx;
using Zenject;

public class PlayerCollisionIdentifier : MonoBehaviour
{
    #region Fields
    private BoxArea2D _box;
    
    private bool _inProgress = false;
    private CancellationTokenSource _cancellationTokenSource;
    #endregion

    #region Properties
    public ISubject<CollisionTagContainer[]> SphereCollisionsDetected { get; private set; } = new Subject<CollisionTagContainer[]>();
    public ISubject<CollisionTagContainer[]> RaysCollisionsDetected { get; private set; } = new Subject<CollisionTagContainer[]>();
    public Vector3 CasterPosition => _box != null ? _box.Center : transform.position;
    #endregion

    #region Methods
    [Inject]
    public void Construct(BoxArea2D box)
    {
        _box = box;
    }
    
    private async void StartCasting()
    {
        if (_box == null)
        {
            Debug.LogError("No CircleArea2D to cast sphere!");
            return;
        }
        
        if(_inProgress)
            return;

        if(_cancellationTokenSource == null)
            _cancellationTokenSource = new CancellationTokenSource();
        
        _inProgress = true;

        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            RaycastHit2D[] sphereHits = Physics2D.BoxCastAll(_box.Center, _box.Size, 0f, Vector2.zero);
            List<CollisionTagContainer> sphereCollisions = new List<CollisionTagContainer>();
            
            if (sphereHits.Length > 0)
            {
                foreach (RaycastHit2D hit in sphereHits)
                {
                    Component tagContainerComponent = ComponentsSearcher.GetSingleComponentOfTypeFromObjectAndChildren(hit.collider.gameObject, typeof(CollisionTagContainer));

                    if (tagContainerComponent != null)
                    {
                        sphereCollisions.Add((CollisionTagContainer)tagContainerComponent);
                    }
                }
            }
            
            SphereCollisionsDetected.OnNext(sphereCollisions.ToArray());

            float distance = 100f;
            Vector2 rayStartPosition = new Vector2(_box.Center.x, _box.Center.y - (distance / 2));
            RaycastHit2D[] rayHits = Physics2D.RaycastAll(rayStartPosition, Vector2.up, distance);
            Debug.DrawRay(rayStartPosition, Vector2.up * distance, Color.red);
            List<CollisionTagContainer> rayCollisions = new List<CollisionTagContainer>();
            
            if (rayHits.Length > 0)
            {
                foreach (RaycastHit2D hit in rayHits)
                {
                    Component tagContainerComponent = ComponentsSearcher.GetSingleComponentOfTypeFromObjectAndChildren(hit.collider.gameObject, typeof(CollisionTagContainer));
                    
                    if (tagContainerComponent != null)
                    {
                        CollisionTagContainer tagContainer = (CollisionTagContainer)tagContainerComponent;
                        
                        if(sphereCollisions.Contains(tagContainer))
                            continue;
                        
                        rayCollisions.Add(tagContainer);
                    }
                }
            }
            
            RaysCollisionsDetected.OnNext(rayCollisions.ToArray());
            
            try
            {
                await UniTask.WaitForEndOfFrame(cancellationToken: _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException exception)
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
                break;
            }
        }
        
        _inProgress = false;
    }

    private void StopCasting()
    {
        if (_box == null)
        {
            Debug.LogError("No CircleArea2D to cast sphere!");
            return;
        }
        
        if(!_inProgress)
            return;
        
        if(_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();
        }
        
        _inProgress = false;
    }

    private void OnEnable() => StartCasting();
    private void OnDisable() => StopCasting();
    #endregion
}
