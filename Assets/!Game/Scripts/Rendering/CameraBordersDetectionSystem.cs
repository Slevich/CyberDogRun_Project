using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniRx;

public class CameraBordersDetectionSystem : MonoBehaviour
{
    #region Fields
    [Header("Min distance to detect exit."), 
     Tooltip("The minimum distance between the object boundaries and the camera boundaries for exit detection."), 
     SerializeField, 
     Range(-5f, 5f)]
    private float _boundsAdditionalDifference = 0f;
    
    private GameObject[] _boundedComponentsHolders = new GameObject[] {};
    
    private List<IBounded> _boundedObjects = new List<IBounded>();
    private CancellationTokenSource _cancellationTokenSource;
    private static CameraBordersDetectionSystem _instance;
    
    private Transform _cameraTransform;
    private float screenAspect = 0f;
    private float cameraHeight = 0f;
    private float cameraWidth = 0f;
    public Subject<IBounded[]> BoundedObjectsChanged { get; private set; }

    #endregion
    
    #region Properties
    public static CameraBordersDetectionSystem Instance
    {
        get
        {
            if(_instance == null)
                _instance =  FindFirstObjectByType<CameraBordersDetectionSystem>();
            
            return _instance;
        }
    }
    
    public Bounds CameraBounds { get; private set; }
    #endregion
    
    #region Methods
    public void SetBoundedObjects(GameObject[] BoundedObjects)
    {
        if (BoundedObjects == null || BoundedObjects.Length == 0)
        {
            _boundedComponentsHolders = Array.Empty<GameObject>();
            return;
        }
        
        _boundedComponentsHolders = BoundedObjects;
        
        foreach (GameObject obj in _boundedComponentsHolders)
        {
            Component boundedComponent = ComponentsSearcher.GetSingleComponentOfTypeFromObjectAndChildren(obj, typeof(IBounded));

            if (boundedComponent != null)
            {
                IBounded bounded = boundedComponent as IBounded;
                _boundedObjects.Add(bounded);
            }
        }
        
        Camera mainCamera = Camera.main;
        _cameraTransform = mainCamera.transform;
        screenAspect = (float)Screen.width / Screen.height;
        cameraHeight = mainCamera.orthographicSize * 2;
        cameraWidth = cameraHeight * screenAspect;
                
        if(BoundedObjectsChanged == null)
            BoundedObjectsChanged = new Subject<IBounded[]>();
        
        TryToStartDetection();
    }
    
    private async void StartDetection()
    {
        if(_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            return;
        
        _cancellationTokenSource =  new CancellationTokenSource();
        
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            bool somethingChanged = false;
            
            foreach (IBounded boundedObject in _boundedObjects)
            {
                bool isInBorders = IsObjectInCameraBorders(boundedObject);

                if (boundedObject.InBorders != isInBorders)
                {
                    boundedObject.InBorders = isInBorders;
                    somethingChanged = true;
                }
            }
            
            if (somethingChanged)
                BoundedObjectsChanged.OnNext(_boundedObjects.ToArray());
            
            try
            {
                await UniTask.WaitForEndOfFrame(_cancellationTokenSource.Token);
            }
            catch (OperationCanceledException exception)
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
                
                if (BoundedObjectsChanged != null)
                {
                    BoundedObjectsChanged.Dispose();
                    BoundedObjectsChanged = null;
                }
                
                break;
            }
        }
    }

    // ДОДЕЛАТЬ!!!
    private bool IsObjectInCameraBorders(IBounded boundedObject)
    {
        Vector3 objectCenter = boundedObject.Center;
        Vector3 objectSize = boundedObject.Size;
        Bounds objectBounds = new Bounds(objectCenter, objectSize);
        
        Vector3 cameraPosition = _cameraTransform.position;
        Vector3 cameraCenter = new Vector3(cameraPosition.x, cameraPosition.y, 0);
        Vector3 cameraSize = new Vector3(cameraWidth, cameraHeight, 0);
        CameraBounds = new Bounds(cameraCenter, cameraSize);
        
        Vector3 cameraExitSize = new Vector3(cameraWidth + _boundsAdditionalDifference, cameraHeight + _boundsAdditionalDifference, 0);
        Bounds cameraExitBounds = new Bounds(cameraCenter, cameraExitSize);
        bool boundedIntersectsCameraBounds = cameraExitBounds.Intersects(objectBounds);

        return boundedIntersectsCameraBounds;
    }

    public void StopDetection()
    {
        if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();
        }
    }

    private void TryToStartDetection()
    {
        if (_boundedObjects.Count > 0)
        {
            StartDetection();
        }
    }
    
    private void OnDisable() => StopDetection();
    #endregion
}