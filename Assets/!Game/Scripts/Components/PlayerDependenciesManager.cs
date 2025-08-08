using UnityEngine;

public class PlayerDependenciesManager : MonoBehaviour
{
    #region Fields
    private Animator _animator;
    private AnimatorControllerChanger _animatorController;
    private Rigidbody _rigidbody;
    private InputMovement _movement;
    private PlayerCollisionIdentifier _collisionIdentifier;
    private CircleArea2D _circleArea2D;
    #endregion


    #region Methods

    
    private void FindAllComponents()
    {
        Component animatorComponent = FindChildComponent<Animator>(gameObject);
        if (animatorComponent != null)
            _animator = (Animator)animatorComponent;
        
        Component movementComponent = FindChildComponent<InputMovement>(gameObject);
        if (movementComponent != null)
            _movement = (InputMovement)movementComponent;
        
        Component animatorControllerComponent = FindChildComponent<AnimatorControllerChanger>(gameObject);
        if (animatorControllerComponent != null)
            _animatorController = (AnimatorControllerChanger)animatorControllerComponent;
    }

    private void InjectDependencies()
    {
        if (_animator != null && _animatorController != null && _movement != null)
        {
            
        }
    }

    private Component FindChildComponent<T>(GameObject obj) where T : Component
    {
        return ComponentsSearcher.GetSingleComponentOfTypeFromObjectAndChildren(obj, typeof(T));
    }

    #endregion
}
