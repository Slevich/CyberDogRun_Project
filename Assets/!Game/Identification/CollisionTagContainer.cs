using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CollisionTagContainer : MonoBehaviour
{
    [field: Header("Collision tag to identify."), Tooltip("Tag which uses in collisions identifiers."), SerializeField]
    public CollisionTags Tag { get; set; } = CollisionTags.Ground;
}

public enum CollisionTags
{
    Player,
    Obstacle,
    Bonus,
    Ground
}