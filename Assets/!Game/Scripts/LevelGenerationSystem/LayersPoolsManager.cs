using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class LayersPoolsManager : MonoBehaviour
{
    #region Fields
    [Header("Layers of the blocks."), Tooltip(""), SerializeField]
    private List<LayerPool> _layersPools = new List<LayerPool>();
    [SerializeField, HideInInspector]
    private string[] _layersNames = new string[] {};
    
    [SerializeField]
    private bool _loading = false;
    #endregion

    #region Properties
    public bool HasPools => _layersPools.Count > 0;
    #endregion

    #region Methods
    #if UNITY_EDITOR
    [ExecuteInEditMode]
    public void UpdateLayers()
    {
        if(_loading)
            return;
        
        if (_layersPools.Count > 0)
        {
            foreach (LayerPool layerPool in _layersPools)
            {
                if(layerPool == null)
                    continue;
                
                layerPool.DestroyAllPoolObjects();
                DestroyImmediate(layerPool.gameObject);
            }
            
            _layersPools.Clear();
        }
        
        _loading = true;
        _layersNames = Enum.GetNames(typeof(BlockLayers));

        try
        {
            List<GameObject> prefabs = LayersPoolsCreator.LoadAllPrefabs();

            if (prefabs.Count == 0)
            {
                Debug.LogError("Error while loading prefabs!");
                _loading = false;
                return;
            }
            
            _layersPools = LayersPoolsCreator.CreateLayersPools(prefabs, _layersNames, transform);
        }
        catch (Exception exc)
        {
            Debug.LogError(exc.Message);
            _loading = false;
            return;
        }

        if (_layersPools.Count > 0)
        {
            foreach (LayerPool layerPool in _layersPools)
            {
                layerPool.EachPrefabClonesAmount = LevelBlocksManager.Instance.BlocksCount;
                layerPool.PrepareObjectsInPool();
            }
        }
        
        _loading = false;
    }
    #endif

    public LayerPoolObject GetObjectFromLayer(BlockLayers Layer)
    {
        if(_layersPools.Count == 0)
            return null;

        IEnumerable<LayerPool> pools = _layersPools.Where(layerPool => layerPool.Name == Layer.ToString());

        if (pools.Count() == 0)
            return null;
        
        LayerPool pool = pools.First();
        return pool.GetObjectFromPool();
    }
    
    private void OnDestroy()
    {
        if(_layersPools.Count == 0)
            return;

        foreach (LayerPool layerPool in _layersPools)
        {
            layerPool.DestroyAllPoolObjects();
        }
    }

    #endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(LayersPoolsManager))]
public class LayersObjectsPoolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        serializedObject.Update();
        
        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Loading prefabs from path: " + LayersPoolsCreator.FullAssetsPath);
        // bool updateLayersButtonPressed = GUILayout.Button("Update Layers!");
        // bool loadingState = serializedObject.FindProperty("_loading").boolValue;
        // if (updateLayersButtonPressed && !loadingState)
        // {
        //     LayersPoolsManager poolManager = (LayersPoolsManager)target;
        //     poolManager.UpdateLayers();
        // }
        
        serializedObject.ApplyModifiedProperties();
    }
}
#endif