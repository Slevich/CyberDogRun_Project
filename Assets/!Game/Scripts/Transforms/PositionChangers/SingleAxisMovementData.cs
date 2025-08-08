using System;
using UnityEditor;
using UnityEngine;

public class SingleAxisMovementData : DirectiveMovementData
{
    #region Fields
    private Vector3 _worldAxisDirection = Vector3.zero;
    private Vector3 _movementDirectionUnclamped = Vector3.zero;
    #endregion

    #region Properties
    [field: Header("Local axis orientation of the movement."),
            Tooltip("The local axis along which movement occurs in global coordinates."),
            SerializeField]
    public Axes LocalAxis { get; set; } = Axes.X;

    [field: Header("Local direction of the movement."),
            Tooltip("The direction of the movement along the axis."),
            SerializeField]
    public AxisMovementType MovementType { get; set; } = AxisMovementType.Negative;

    public Vector3 MovementDirectionUnclamped
    {
        get
        {
            if (_movementDirectionUnclamped == Vector3.zero)
            {
                CalculateWorldAxisDirection();
            }

            switch (MovementType)
            {
                case AxisMovementType.Negative:
                    _movementDirectionUnclamped = -(_worldAxisDirection * Speed * Time.fixedDeltaTime);
                    break;
                
                case AxisMovementType.Positive:
                    _movementDirectionUnclamped = _worldAxisDirection * Speed * Time.fixedDeltaTime;
                    break;
            }
            
            return _movementDirectionUnclamped;
        }
        private set
        {
            MovementDirection = value.normalized;
            _movementDirectionUnclamped = value;
            IsMoving = value != Vector3.zero;
        }
    }
    #endregion

    #region Methods
    protected override void ResetValues()
    {
        base.ResetValues();
        MovementDirectionUnclamped = Vector3.zero;
    }

    private void CalculateWorldAxisDirection()
    {
        switch (LocalAxis)
        {
            case Axes.X:
                _worldAxisDirection = transform.right;
                break;

            case Axes.Y:
                _worldAxisDirection = transform.up;
                break;

            case Axes.Z:
                _worldAxisDirection = transform.forward;
                break;
        }
    }
    #endregion
}