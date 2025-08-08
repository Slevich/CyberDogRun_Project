using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class LayerPool : MonoBehaviour
{
    [field: SerializeField, ReadOnly] 
    public string Name { get; set; } = "Layer";

    [field: SerializeField, HideInInspector]
    public BlockLayers Layer { get; set; } = BlockLayers.Background;
    
    [field: SerializeField, ReadOnly]
    public List<LevelBlockLayerDataContainer> BlockLayerPrefabs { get; set; } = new List<LevelBlockLayerDataContainer>();

    [field: SerializeField, ReadOnly]
    private List<LayerPoolObject> _allPoolObjects = new List<LayerPoolObject>();

    [field: SerializeField, ReadOnly]
    private List<LayerPoolObject> _insidePoolObjects = new List<LayerPoolObject>();
    
    [field: SerializeField, ReadOnly]
    public Transform ObjectsParent { get; set; }

    [field: SerializeField, HideInInspector]
    public int EachPrefabClonesAmount { get; set; } = 1;
    private int allObjectsCount => _allPoolObjects.Count;
    private int insidePoolObjectsCount => _insidePoolObjects.Count;

    public static readonly Dictionary<BlockLayers, int> PreparedEachItemsPerLayer = new Dictionary<BlockLayers, int>
    {
        {BlockLayers.Background, 1},
        {BlockLayers.BackgroundProps, 1},
        {BlockLayers.Middleground, 1},
        {BlockLayers.MiddlegroundProps, 1},
        {BlockLayers.GroundObstacles, 2},
        {BlockLayers.SkyObstacles, 2},
        {BlockLayers.Bonuses, 1},
        {BlockLayers.Foreground, 1}
    };
    
    public void PrepareObjectsInPool()
    {
        if(BlockLayerPrefabs.Count == 0)
            return;

        int eachCloneAmount = PreparedEachItemsPerLayer[Layer];
        int eachPrefabCloneCount = EachPrefabClonesAmount * eachCloneAmount;
        
        for (int i = 0; i < BlockLayerPrefabs.Count;)
        {
            CreatePoolObject(BlockLayerPrefabs[i]);
            eachPrefabCloneCount--;

            if (eachPrefabCloneCount == 0)
            {
                eachPrefabCloneCount = EachPrefabClonesAmount * eachCloneAmount;
                i++;
            }
        }
    }

    public LayerPoolObject GetObjectFromPool()
    {
        if(BlockLayerPrefabs.Count == 0)
            return null;
        
        LayerPoolObject layerPoolObject = null;
        
        if (insidePoolObjectsCount == 0)
        {
            int randomPrefabIndex = UnityEngine.Random.Range(0, BlockLayerPrefabs.Count - 1);
            layerPoolObject = CreatePoolObject(BlockLayerPrefabs[randomPrefabIndex]);
        }
        else
        {
            List<LayerPoolObject> uniqueObjects = new List<LayerPoolObject>();
            HashSet<string> addedNames = new HashSet<string>();

            foreach (LayerPoolObject poolObject in _insidePoolObjects)
            {
                if (poolObject != null && addedNames.Add(poolObject.name))
                {
                    uniqueObjects.Add(poolObject);
                }
            }

            if (uniqueObjects.Count > 0)
            {
                int randomObjectIndex = UnityEngine.Random.Range(0, uniqueObjects.Count);
                layerPoolObject = uniqueObjects[randomObjectIndex];
            }
            else
            {
                int randomObjectIndex = UnityEngine.Random.Range(0, insidePoolObjectsCount);
                layerPoolObject = _insidePoolObjects[randomObjectIndex];
            }
        }
        
        ReleaseObjectFromPool(layerPoolObject);
        return layerPoolObject;
    }

    private LayerPoolObject CreatePoolObject(LevelBlockLayerDataContainer prefab)
    {
        LevelBlockLayerDataContainer layerObject = MonoBehaviour.Instantiate(prefab, ObjectsParent);
        LayerPoolObject layerPoolObject = ((LayerPoolObject)layerObject.gameObject.AddComponent(typeof(LayerPoolObject)));
        layerPoolObject.DataContainer = layerObject;
        layerPoolObject.ParentPool = this;
        _allPoolObjects.Add(layerPoolObject);
        ReturnObjectToPool(layerPoolObject);
        return layerPoolObject;
    }

    private void ReleaseObjectFromPool(LayerPoolObject layerPoolObject)
    {
        layerPoolObject.gameObject.SetActive(true);
        layerPoolObject.transform.SetParent(null);
        layerPoolObject.IsInPool = false;
        UpdateInsideObjectsList();
    }

    public void ReturnObjectToPool(LayerPoolObject layerPoolObject)
    {
        layerPoolObject.transform.SetParent(ObjectsParent);
        layerPoolObject.transform.localPosition = Vector3.zero;
        layerPoolObject.IsInPool = true;
        UpdateInsideObjectsList();
        layerPoolObject.gameObject.SetActive(false);
    }

    private void UpdateInsideObjectsList()
    {
        if(_allPoolObjects.Count == 0)
            _insidePoolObjects.Clear();
        else
        {
            IEnumerable<LayerPoolObject> insidePoolObjects = _allPoolObjects.Where(poolObject => poolObject.IsInPool);
            
            if(insidePoolObjects.Count() > 0)
                _insidePoolObjects = insidePoolObjects.ToList();
            else
                _insidePoolObjects.Clear();
        }
    }

    public void DestroyAllPoolObjects()
    {
        for (int i = _allPoolObjects.Count - 1; i >= 0; i--)
        {
            LayerPoolObject poolObject = _allPoolObjects[i];
            
            if(poolObject == null)
                continue;
            
            if(!poolObject.IsInPool)
                poolObject.ReturnToPool();

            try
            {
                MonoBehaviour.DestroyImmediate(poolObject.gameObject);
            }
            catch (NullReferenceException exception)
            {
                Debug.LogError(exception.Message);
            }
        }

        _allPoolObjects.Clear();
        _insidePoolObjects.Clear();
    }
}