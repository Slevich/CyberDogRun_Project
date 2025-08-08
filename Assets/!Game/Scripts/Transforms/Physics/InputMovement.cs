using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Zenject;

public class InputMovement : MonoBehaviour
{
    #region Fields
    private Transform _movableTransform;
    
    [Header("Jump height."), 
     Tooltip("The height of the jump along the Y axis in world coordinates."), 
     SerializeField, 
     Range(0f, 10f)]
    private float _jumpHeight = 3f;
    
    [Header("Jump speed."), 
     Tooltip("The speed modifier of the jump along the Y axis in world coordinates."), 
     SerializeField, 
     Range(0f, 100f)]
    private float _jumpSpeed = 3f;
    
    [Header("Fall speed."), 
     Tooltip("The speed modifier of the fall along the Y axis in world coordinates."), 
     SerializeField, 
     Range(0f, 100f)]
    private float _fallSpeed = 3f;
    
    private CancellationTokenSource _cancellationTokenSource;
    private bool _inProcess = false;
    
    [Space(10), Header("Body is on the ground?"), SerializeField, ReadOnly]
    private bool _isGrounded = false;
    
    [SerializeField, Header("Event on change current movement state.")]
    private UnityEvent<MovementState> _onChangeMovementStateEvent;
    
    private PlayerCollisionIdentifier _collisionIdentifier;
    private Vector3 _startPosition;
    private bool _settingStartPosition = true;
    #endregion

    #region Properties

    private bool hasTransform
    {
        get
        {
            if(_movableTransform == null)
                Debug.LogError("Movable transform is null!");
            
            return _movableTransform != null;
        }
    }

    [field: SerializeField, HideInInspector]
    public bool Blocked { get; set; } = false;

    #endregion
    
    #region Methods
    [Inject]
    public void Construct(Transform MovableTransform, PlayerCollisionIdentifier CollisionIdentifier)
    {
        _movableTransform = MovableTransform;
        _collisionIdentifier = CollisionIdentifier;
        _startPosition = _movableTransform.position;
    }

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        if(_movableTransform == null)
            return;
        
        if(_collisionIdentifier == null)
            return;
        
        _collisionIdentifier.SphereCollisionsDetected
            .Subscribe(tags => GroundDetection(tags))
            .AddTo(this);
        
        _onChangeMovementStateEvent?.Invoke(MovementState.Idling);
    }
    
    private async void Jumping(InputAction.CallbackContext context)
    {
        if(!hasTransform)
            return;
        
        if(_inProcess || !_isGrounded)
            return;
        
        if(_cancellationTokenSource == null)
            _cancellationTokenSource = new CancellationTokenSource();
        
        _inProcess = true;
        _onChangeMovementStateEvent?.Invoke(MovementState.Jumping);
        
        if(_settingStartPosition)
            _movableTransform.position = _startPosition;
        
        Vector3 newPosition = _startPosition;
        float currentYAxisValue = newPosition.y;
        float targetYAxisValue = currentYAxisValue + _jumpHeight;
        float jumpPositionChange = _jumpSpeed * Time.timeScale * Time.fixedDeltaTime;

        while ((currentYAxisValue < targetYAxisValue) && (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested) && !Blocked)
        {
            try
            {
                currentYAxisValue += jumpPositionChange;
                newPosition.y = currentYAxisValue;
                _movableTransform.position = newPosition;
                
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

    private async void Falling()
    {
        if(!hasTransform)
            return;
        
        if(_inProcess || _isGrounded)
            return;
        
        if(_cancellationTokenSource == null)
            _cancellationTokenSource = new CancellationTokenSource();
        
        _inProcess = true;
        
        Vector3 newPosition = _movableTransform.transform.position;
        float currentYAxisValue = newPosition.y;
        float jumpPositionChange = _fallSpeed * Time.timeScale * Time.fixedDeltaTime;

        while ((!_isGrounded) && (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested) && !Blocked)
        {
            try
            {
                currentYAxisValue -= jumpPositionChange;
                newPosition.y = currentYAxisValue;
                _movableTransform.position = newPosition;
                
                await UniTask.WaitForFixedUpdate(_cancellationTokenSource.Token);
            }
            catch (OperationCanceledException exception)
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
                break;
            }
        }
        
        _onChangeMovementStateEvent?.Invoke(MovementState.Falling);
        
        if(_settingStartPosition)
            _movableTransform.position = _startPosition;
        
        _settingStartPosition = true;
        _inProcess = false;
    }

    private void GroundDetection(CollisionTagContainer[] tags)
    {
        if(tags.Length == 0)
            _isGrounded = false;
        else
            _isGrounded = tags.Any(tag => tag.Tag == CollisionTags.Ground);
        
        if(!_isGrounded)
            Falling();
    }

    private void OnEnable()
    {
        if (!hasTransform)
        {
            return;    
        }
        
        InputProvider.JumpInputAction.started += Jumping;
    }

    private void OnDisable()
    {
        if (!hasTransform)
        {
            return;    
        }
        
        InputProvider.JumpInputAction.started -= Jumping;
        StopExecution();
    }

    public void StopExecution(bool SetStartPositionOnStop = false)
    {
        _settingStartPosition = SetStartPositionOnStop;
        
        if (_inProcess && _cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();
        }
    }
    #endregion
}

public enum MovementState
{
    Idling,
    Jumping,
    Falling
}