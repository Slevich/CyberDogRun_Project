using System;
using UnityEngine;

public class CircleArea2D : MonoBehaviour
{
    #region Properties
    [field: Header("Sphere radius"), SerializeField, Range(0f, 10f)]
    public float Radius { get; set; } = 1f;
    public Vector3 Center => transform.position;
    #endregion
    
    #region Methods
    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.greenYellow;
        Gizmos.DrawWireSphere(Center, Radius);
    }
#endif
    #endregion
}
