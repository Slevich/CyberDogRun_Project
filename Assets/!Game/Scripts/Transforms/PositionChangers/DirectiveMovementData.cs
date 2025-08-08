using UnityEngine;

public class DirectiveMovementData : MovementDataContainerBase
{
    protected override void ResetValues()
    {
        MovementDirection = Vector3.zero;
        IsMoving = false;
    }
}
