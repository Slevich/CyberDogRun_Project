using System;
using UnityEngine;
using UniRx;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;
using Zenject;

public class BonusEarning : MonoBehaviour
{
    #region Fields
    [SerializeField, Header("Event called when current bonus started.")]
    private UnityEvent<BonusType> _onBonusStartedEvent;
    [SerializeField, Header("Event called when current bonus stopped.")]
    private UnityEvent<BonusType> _onBonusEndedEvent;
    
    private PlayerCollisionIdentifier _collisionIdentifier;
    private LevelEndingOnObstacleCollision _obstaclesCollision;
    private BonusExecution _currentBonusExecution;
    private CancellationTokenSource _cancellationTokenSource;
    private float _baseTimeScale = 1f;
    #endregion

    #region Methods
    [Inject]
    public void Construct(PlayerCollisionIdentifier CollisionIdentifier, LevelEndingOnObstacleCollision ObstaclesCollision)
    {
        _collisionIdentifier = CollisionIdentifier;
        _obstaclesCollision = ObstaclesCollision;
        _baseTimeScale = Time.timeScale;
        
        if(_collisionIdentifier !=  null)
            Initialize();
    }
    
    private void Initialize()
    {
        _collisionIdentifier.SphereCollisionsDetected
            .SkipWhile(tags => tags.Length == 0)
            .Where(tags => tags.Any(tag => tag.Tag == CollisionTags.Bonus))
            .Subscribe(tags => Earning(tags))
            .AddTo(this);
    }

    private void Earning(CollisionTagContainer[] tags)
    {
        IEnumerable<CollisionTagContainer> bonusTagsContainers = tags.Where(tag => tag.Tag == CollisionTags.Bonus);
        
        if(bonusTagsContainers.Count() == 0)
            return;
        
        foreach (CollisionTagContainer tagContainer in bonusTagsContainers)
        {
            Component bonusComponent = ComponentsSearcher.GetSingleComponentOfTypeFromObjectAndChildren(tagContainer.gameObject, typeof(BonusContainer));
            
            if(bonusComponent == null)
                continue;
            
            BonusContainer bonusContainer = (BonusContainer)bonusComponent;
            bool applied = TryToApplyBonusEffect(bonusContainer);
            
            if(!applied)
                continue;
            
            Component poolObjectComponent = ComponentsSearcher.GetSingleComponentOfTypeFromObjectAndChildren(tagContainer.gameObject, typeof(LayerPoolObject));

            if (poolObjectComponent != null)
            {
                LayerPoolObject poolObject = (LayerPoolObject)poolObjectComponent;
                poolObject.ReturnToPool();
            }
        }
    }

    private bool TryToApplyBonusEffect(BonusContainer bonusContainer)
    {
        if (bonusContainer == null)
            return false;
        
        StopExecution();

        _currentBonusExecution = new BonusExecution(bonusContainer.Bonus, _obstaclesCollision, _onBonusStartedEvent, _onBonusEndedEvent);
        _currentBonusExecution.StartExecution();
        return true;
    }

    private void StopExecution()
    {
        if (_currentBonusExecution != null && !_currentBonusExecution.Completed)
        {
            _currentBonusExecution.StopExecution();
        }
    }

    private void OnDisable() => StopExecution();
    #endregion
}

public class BonusExecution
{
    private BonusScriptable _bonusScriptable;
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private LevelEndingOnObstacleCollision _obstaclesCollision;
    private Action _onWaitingEndCallback;
    private UniTask _waiting;
    private UnityEvent<BonusType> _onBonusStartedEvent;
    private UnityEvent<BonusType> _onBonusEndedEvent;

    private static readonly float acceleratedTimeScale = 1.3f; 
    private static readonly float slowedTimeScale = 0.75f; 
    private static readonly float baseTimeScale = 1f; 
    
    public bool Completed {get; private set; } = false;
    
    public BonusExecution(BonusScriptable Bonus, LevelEndingOnObstacleCollision ObstaclesCollision, UnityEvent<BonusType> OnBonusStartedEvent, UnityEvent<BonusType> OnBonusEndedEvent)
    {
        _bonusScriptable = Bonus;
        _obstaclesCollision = ObstaclesCollision;
        _onBonusStartedEvent = OnBonusStartedEvent;
        _onBonusEndedEvent = OnBonusEndedEvent;
    }

    public async void StartExecution()
    {
        _onBonusStartedEvent?.Invoke(_bonusScriptable.Type);
        
        switch (_bonusScriptable.Type)
        {
            case BonusType.Invincibility:
                if (_obstaclesCollision != null)
                {
                    _obstaclesCollision.Blocked = true;
                }

                _onWaitingEndCallback = delegate
                {
                    if (_obstaclesCollision != null)
                    {
                        _obstaclesCollision.Blocked = false;
                    }
                };
                break;
            
            case BonusType.GameSlowdown:
                Time.timeScale *= slowedTimeScale;
                
                _onWaitingEndCallback = delegate
                {
                    Time.timeScale /= slowedTimeScale;
                };
                break;
            
            case BonusType.GameAcceleration:
                Time.timeScale *= acceleratedTimeScale;
                
                _onWaitingEndCallback = delegate
                {
                    Time.timeScale /= acceleratedTimeScale;
                };
                break;
        }
        
        try
        {
            _waiting = UniTask.WaitForSeconds(_bonusScriptable.Duration, true, PlayerLoopTiming.FixedUpdate, _cancellationTokenSource.Token);
            await _waiting;
        }
        catch (OperationCanceledException e)
        {
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
            return;
        }
        
        _onWaitingEndCallback?.Invoke();
        _onBonusEndedEvent?.Invoke(_bonusScriptable.Type);
        Completed = true;
    }
    
    public void StopExecution()
    {
        if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();
            _onWaitingEndCallback?.Invoke();
            _onBonusEndedEvent?.Invoke(_bonusScriptable.Type);
        }
    }
}