using UnityEngine;
using UnityEngine.Pool;

public class LayerPoolObject : MonoBehaviour
{
    [field: SerializeField, ReadOnly]
    public LevelBlockLayerDataContainer DataContainer { get; set; }
    [field: SerializeField, HideInInspector]
    public LayerPool ParentPool { get; set; }
    [field: SerializeField, ReadOnly]
    public bool IsInPool  { get; set; } = false;

    public void ReturnToPool()
    {
        if(ParentPool == null)
            return;

        if (IsInPool)
            return;
        
        ParentPool.ReturnObjectToPool(this);
    }
}
