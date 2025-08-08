using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

public class LevelEndingOnObstacleCollision : MonoBehaviour
{
    #region Fields
    [Header("Event on player obstacle collision.")]
    public UnityEvent _onPlayerObstacleCollisionEvent;
    
    private PlayerCollisionIdentifier _playerCollisionIdentifier;
    #endregion

    #region Properties

    public bool Blocked { get; set; } = false;

    #endregion
    
    #region Methods

    [Inject]
    public void Construct(PlayerCollisionIdentifier CollisionIdentifier)
    {
        _playerCollisionIdentifier = CollisionIdentifier;

        if (_playerCollisionIdentifier != null)
        {
            _playerCollisionIdentifier.SphereCollisionsDetected
                .Where(tags => tags.Any(tag => tag.Tag == CollisionTags.Obstacle))
                .Subscribe(_ => CallLevelEnding())
                .AddTo(this);
        }
    }

    private void CallLevelEnding()
    {
        if(Blocked)
            return;
        
        _onPlayerObstacleCollisionEvent?.Invoke();
        LevelController.Instance.ExitPoint();
    }
    #endregion
}
