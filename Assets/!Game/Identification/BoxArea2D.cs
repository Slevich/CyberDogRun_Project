using System;
using UnityEngine;

public class BoxArea2D : MonoBehaviour
{
    #region Fields
    [Header("Horizontal box size"), SerializeField, Range(0f, 10f)]
    private float _horizontalSize = 1f;
    [Header("Vertical box size"), SerializeField, Range(0f, 10f)]
    private float _verticalSize = 1f;
    #endregion
    
    #region Properties
    public Vector3 Center => transform.position;
    public Vector2 Size { get; private set; }
    #endregion
    
    #region Methods
    private void Awake()
    {
        Size = new Vector2(_horizontalSize, _verticalSize);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.greenYellow;
        Gizmos.DrawWireCube(Center, new Vector2(_horizontalSize, _verticalSize));
    }
#endif
    #endregion
}
