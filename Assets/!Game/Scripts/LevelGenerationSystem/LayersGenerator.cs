using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class LayersGenerator
{
    #region Properties
    [field: SerializeField, HideInInspector]
    public LayersPoolsManager PoolsManager { get; set; }
    
    [field: SerializeField, HideInInspector]
    public Transform ObjectsTransformParent { get; set; }
    #endregion

    #region Fields
    [SerializeField, HideInInspector]
    protected BlockLayers[] _layers = new BlockLayers[] {};
    
    [SerializeField, ReadOnly]
    protected List<LayerPoolObject> _layerAssets =  new List<LayerPoolObject>();
    
    [SerializeField, ReadOnly]
    protected List<Transform> _blocksLayersTransformsParents = new List<Transform>();
    #endregion
    
    #region Methods
    public virtual void GetLayers()
    {
        Array enumValues = Enum.GetValues(typeof(BlockLayers));
        _layers = enumValues.Cast<BlockLayers>().ToArray();
    }
    
    public virtual void GenerateLayers()
    {
        UpdateContainers();

        if (PoolsManager == null)
        {
            Debug.LogError("PoolsManager is null!");
            return;
        }

        if (_layers.Length == 0)
        {
            Debug.LogError("No Layers Found!");
            return;
        }
        
        for (int i = 0; i < _layers.Length; i++)
        {
            BlockLayers layer = _layers[i];
            LayerPoolObject poolObject = PoolsManager.GetObjectFromLayer(layer);
            
            if(poolObject == null)
                continue;

            string objectLayerName = poolObject.DataContainer.Data.Sort.ToString();
            Transform poolObjectParent = _blocksLayersTransformsParents.Where(parent => parent.transform.name.Contains(objectLayerName)).FirstOrDefault();
            ParallaxEffector effector = poolObjectParent.GetComponent<ParallaxEffector>();
            effector.ParallaxSpeed = poolObject.DataContainer.Data.ParallaxSpeed;
            poolObject.transform.SetParent(poolObjectParent);
            poolObject.transform.localPosition = Vector3.zero;
            poolObject.transform.localRotation = Quaternion.identity;
            _layerAssets.Add(poolObject);
        }
    }
    
    protected virtual void UpdateContainers()
    {
        if(ObjectsTransformParent == null)
            return;
        
        if(_layers.Length == 0)
            return;
        
        if (_blocksLayersTransformsParents.Count == 0)
        {
            int childsCount = ObjectsTransformParent.childCount;

            for (int i = 0; i < childsCount; i++)
            {
                Transform child = ObjectsTransformParent.GetChild(i);
                if(_layers.Any(layer => child.name.Contains(layer.ToString())))
                {
                    _blocksLayersTransformsParents.Add(child);
                }
            }

            if (_blocksLayersTransformsParents.Count != _layers.Length)
            {
                foreach (BlockLayers layer in _layers)
                {
                    GameObject newLayerParentObject = new GameObject();
                    newLayerParentObject.name = layer.ToString() + "Assets_Container";
                    newLayerParentObject.transform.SetParent(ObjectsTransformParent);
                    newLayerParentObject.transform.localPosition = Vector3.zero;
                    newLayerParentObject.transform.localRotation = Quaternion.identity;
                    _blocksLayersTransformsParents.Add(newLayerParentObject.transform);
                    ParallaxEffector effector = (ParallaxEffector)newLayerParentObject.AddComponent<ParallaxEffector>();
                    effector.EffectedTransform = newLayerParentObject.transform;
                }
            }
        }
        else
        {
            foreach (Transform layersObjectsParent in _blocksLayersTransformsParents)
            {
                layersObjectsParent.localPosition = Vector3.zero;
                layersObjectsParent.localRotation = Quaternion.identity;
            }
        }
    }
    
    public virtual void ClearObjects()
    {
        if(_layerAssets.Count == 0)
            return;

        int count = 0;
        for (int i = _layerAssets.Count - 1; i >= 0; i--)
        {
            LayerPoolObject poolObject = _layerAssets[i];
            
            if (poolObject != null)
            {
                poolObject.ReturnToPool();
            }

            _layerAssets.RemoveAt(i);
        }
    }
    #endregion
}