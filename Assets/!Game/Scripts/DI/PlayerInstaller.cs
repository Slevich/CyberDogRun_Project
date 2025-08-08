using UnityEngine;
using Zenject;

public class PlayerInstaller : MonoInstaller
{
    #region Fields
    [Header("Movable player body."), SerializeField]
    private Transform _movableTransform;
    
    [Header("Player body sprite animator."), SerializeField]
    private Animator _animator;
    #endregion
    
    #region Methods
    public override void InstallBindings()
    {
        this.Container
            .Bind<InputMovement>()
            .FromComponentInHierarchy()
            .AsSingle();
        
        this.Container
            .Bind<PlayerCollisionIdentifier>()
            .FromComponentInHierarchy()
            .AsSingle();
        
        this.Container
            .Bind<Rigidbody2D>()
            .FromComponentInHierarchy()
            .AsSingle();
        
        this.Container
            .Bind<LevelEndingOnObstacleCollision>()
            .FromComponentInHierarchy()
            .AsSingle();
        
        this.Container
            .Bind<CircleArea2D>()
            .FromComponentInHierarchy()
            .AsSingle();
        
        this.Container
            .Bind<Transform>()
            .FromInstance(_movableTransform)
            .AsSingle();
        
        this.Container
            .Bind<Animator>()
            .FromInstance(_animator)
            .AsSingle();
    }
    #endregion

}
