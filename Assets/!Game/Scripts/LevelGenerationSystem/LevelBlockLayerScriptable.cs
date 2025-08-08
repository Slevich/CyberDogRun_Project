using UnityEngine;

[CreateAssetMenu(fileName = "LevelBlockLayer_Scriptable", menuName = "ScriptableObjects/LevelBlockLayerScriptableObject", order = 1)]
public class LevelBlockLayerScriptable : ScriptableObject
{
    [field: Header("Layer name"), Tooltip("The type refers to this layer."), SerializeField]
    public BlockLayers Sort { get; set; }

    [field: Header("Parallax speed."), Tooltip("The speed of movement of the layer within the block."), SerializeField, Range(0, 10f)]
    public float ParallaxSpeed { get; set; } = 0f;
}

public enum BlockLayers
{
    Background,
    BackgroundProps,
    Middleground,
    MiddlegroundProps,
    GroundObstacles,
    SkyObstacles,
    Bonuses,
    Foreground
}

