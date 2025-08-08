using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

public static class LayersPoolsCreator
{
    private static string baseAssetsPath => Application.dataPath;
    private static string layersAssetsPath => "/!Game/Prefabs/Environment/Multiple";
    public static string FullAssetsPath => baseAssetsPath + layersAssetsPath;
    private static string prefabExtension => "*.prefab";
    
    #if UNITY_EDITOR
    public static List<GameObject> LoadAllPrefabs()
    {
        if (!Directory.Exists(FullAssetsPath))
        {
            Debug.LogError("Directory not found: " + FullAssetsPath);
            return new List<GameObject>();
        }
        
        List<GameObject> prefabs = new List<GameObject>();
        string[] prefabPaths = Directory.GetFiles(FullAssetsPath, prefabExtension, SearchOption.AllDirectories);

        foreach (string path in prefabPaths)
        {
            string assetPath = path.Replace("\\", "/").Replace(Application.dataPath, "Assets");
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab != null) prefabs.Add(prefab);
        }
        return prefabs;
    }
    #endif
    
    public static List<LayerPool> CreateLayersPools(List<GameObject> prefabs, string[] layerNames, Transform parent)
    {
        if(prefabs == null || prefabs.Count == 0)
            return new List<LayerPool>();
        
        List<LevelBlockLayerDataContainer> layersData = new List<LevelBlockLayerDataContainer>();
        
        foreach (GameObject prefab in prefabs)
        {
            Component component =
                ComponentsSearcher.GetSingleComponentOfTypeFromObjectAndChildren(prefab,
                    typeof(LevelBlockLayerDataContainer));

            if (component != null)
            {
                layersData.Add((LevelBlockLayerDataContainer)component);
            }
        }
        
        if(layersData.Count == 0)
            return new List<LayerPool>();
        
        List<LayerPool> _layers = new List<LayerPool>();
        
        foreach (string name in layerNames)
        {
            GameObject layerObject = new GameObject();
            layerObject.transform.SetParent(parent);
            layerObject.transform.localPosition = Vector3.zero;
            layerObject.name = $"{name}_layer";
            
            LayerPool newLayerPool = layerObject.AddComponent<LayerPool>();
            newLayerPool.Name = name;
            newLayerPool.ObjectsParent = layerObject.transform;
            newLayerPool.Layer = (BlockLayers)Enum.Parse(typeof(BlockLayers), name);
            _layers.Add(newLayerPool);
        }
        
        if(_layers.Count == 0)
            return new List<LayerPool>();

        foreach (LayerPool layerPool in _layers)
        {
            IEnumerable<LevelBlockLayerDataContainer> layerDataContainers = layersData.Where(data => data.Data.Sort == layerPool.Layer);
            
            if(layerDataContainers.Count() == 0)
                continue;

            layerPool.BlockLayerPrefabs = layerDataContainers.ToList();
        }
        
        return _layers;
    }
}
