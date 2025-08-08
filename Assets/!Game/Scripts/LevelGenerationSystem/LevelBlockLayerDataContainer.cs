using UnityEngine;

public class LevelBlockLayerDataContainer : MonoBehaviour
{
    [field: Header("Scriptable object layer info."), Tooltip("Reference to level block layer scripable object"), SerializeField]
    public LevelBlockLayerScriptable Data { get; set; }
    
    public GameObject ObjectLink => gameObject;
}
