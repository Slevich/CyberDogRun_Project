using UnityEngine;

public interface IBounded
{
    public Vector3 Center { get; }
    public Vector3 Size { get; }
    public bool InBorders { get; set; }
    public Transform ObjectTransform { get; }
}
