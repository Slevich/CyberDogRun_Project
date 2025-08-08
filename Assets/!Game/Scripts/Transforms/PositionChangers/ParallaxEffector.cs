using UnityEngine;

public class ParallaxEffector : MonoBehaviour, IParallaxEffector
{
    [field: SerializeField, ReadOnly]
    public float ParallaxSpeed { get; set; }
    [field: SerializeField, ReadOnly]
    public Transform EffectedTransform { get; set; }
}
