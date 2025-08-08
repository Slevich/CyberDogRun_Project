using System;
using UnityEngine;

public abstract class MovementDataContainerBase : MonoBehaviour
{
    #region Fields
    [field: Header("Movement speed modifier."),
            Tooltip("An modifier by which the change in the object's position per frame is multiplied."),
            SerializeField,
            Range(0f, 100f)]
    public float Speed { get; set; } = 1f;

    [field: Header("Current world movement direction."),
            Tooltip("Current direction of movement in global coordinates."),
            SerializeField,
            ReadOnly]
    public Vector3 MovementDirection { get; set; } = Vector3.zero;

    [field: Header("Object is currently moving?"),
            Tooltip("Is it currently in movement?"),
            SerializeField,
            ReadOnly]
    public bool IsMoving { get; set; } = false;

    #endregion
    
    #region Methods
    private void OnDisable() => ResetValues();
    protected abstract void ResetValues();
    #endregion

}
