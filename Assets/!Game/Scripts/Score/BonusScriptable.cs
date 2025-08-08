using UnityEngine;

[CreateAssetMenu(fileName = "Bonus_Scriptable", menuName = "ScriptableObjects/NewBonus", order = 1)]
public class BonusScriptable : ScriptableObject
{
    [field: Header("Bonus type."), SerializeField]
    public BonusType Type { get; set; } = BonusType.Invincibility;
    
    [field: Header("Bonus duration in seconds."), SerializeField, Range(0f, 60f)]
    public float Duration { get; set; } = 5f;
}

public enum BonusType
{
    Invincibility,
    GameSlowdown,
    GameAcceleration
}