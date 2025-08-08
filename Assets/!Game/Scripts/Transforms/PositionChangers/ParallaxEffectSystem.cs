using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class ParallaxEffectSystem : MonoBehaviour
{
    #region Fields

    [Header("Parallax speed modifier"), SerializeField, Range(0f, 100f)]
    private float _parallaxSpeed = 1f;
    
    private List<IParallaxEffector> _parallaxEffectors = new List<IParallaxEffector>();
    private static ParallaxEffectSystem _instance;
    private CancellationTokenSource _cancellationTokenSource;
    private bool _inProcess =  false;
    private Camera _camera;
    #endregion
    
    #region Properties

    public static ParallaxEffectSystem Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<ParallaxEffectSystem>();
                
            return _instance;
        }
    }
    #endregion

    #region Methods
    private void Awake()
    {
        if(_instance != null && _instance != this)
        {
            Destroy(this);
        }
        
        _camera = Camera.main;
    }

    public void FindParallaxEffectors(GameObject EffectorsParent)
    {
        if(EffectorsParent == null)
            return;
        
        Component[] parallaxEffectorsComponents = ComponentsSearcher.GetComponentsOfTypeFromObjectAndAllChildren(EffectorsParent, typeof(IParallaxEffector));
        
        if(parallaxEffectorsComponents.Length == 0)
            return;

        foreach (Component effectorComponent in parallaxEffectorsComponents)
        {
            _parallaxEffectors.Add((IParallaxEffector)effectorComponent);
        }
        
        if(_parallaxEffectors.Count == 0)
            return;
        
        StartExecution();
    }
    
    private async void StartExecution()
    {
        if(_inProcess)
            return;
        
        _inProcess = true;
        
        if(_cancellationTokenSource == null)
            _cancellationTokenSource = new CancellationTokenSource();

        while (_inProcess || (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested))
        {
            foreach (IParallaxEffector effector in _parallaxEffectors)
            {
                Transform effectorTransform = effector.EffectedTransform;
                float parallaxSpeed = effector.ParallaxSpeed * Time.timeScale;
                Vector3 xAxisTransformDirection = effectorTransform.right * -parallaxSpeed;
                effectorTransform.position += xAxisTransformDirection * Time.fixedDeltaTime;
            }

            try
            {
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

    private void MoveSingleEffector(IParallaxEffector effector, float delta)
    {
        Transform effectorTransform = effector.EffectedTransform;
        Vector3 newPosition = transform.localPosition;
        float effectorSpeed = effector.ParallaxSpeed;
        newPosition.x -= delta * effectorSpeed;
        effectorTransform.localPosition = newPosition;
    }
    
    public void StopExecution()
    {
        if(!_inProcess)
            return;

        if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();
        }
        
        _inProcess = false;
    }

    private void OnEnable()
    {
        if (_parallaxEffectors.Count == 0 || _inProcess)
            return;
        
        StartExecution();
    }

    private void OnDisable() => StopExecution();
    #endregion
}

public interface IParallaxEffector
{
    public float ParallaxSpeed { get; set; }
    public Transform EffectedTransform { get; set; }
}