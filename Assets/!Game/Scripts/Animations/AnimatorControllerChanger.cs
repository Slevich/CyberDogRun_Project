using System;
using UnityEngine;
using Zenject;

public class AnimatorControllerChanger : MonoBehaviour
{
    #region Fields
    [SerializeField]
    private Animator _animator;
    #endregion

    #region Properties

    private void Awake()
    {
        if(_animator == null && TryGetComponent(out Animator animator))
            _animator = animator;
    }

    public void ManageAnimatorTriggers(MovementState state)
    {
        switch (state)
        {
            case MovementState.Idling:
                break;
            
            case MovementState.Falling:
                SetAnimatorTrigger("Falling");
                break;
            
            case MovementState.Jumping:
                SetAnimatorTrigger("Jumping");
                break;
        }
    }
    
    private void SetAnimatorTrigger(string triggerName)
    {
        if(_animator == null)
            return;
        
        _animator.SetTrigger(triggerName);
    }
    #endregion
}
