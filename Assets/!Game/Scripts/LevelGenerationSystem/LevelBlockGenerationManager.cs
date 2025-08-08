using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

public class LevelBlockGenerationManager : MonoBehaviour
{
    #region Fields
    [SerializeField, HideInInspector] 
    private LevelBlockDataContainer _blockData;
    [SerializeField, HideInInspector]
    private Transform _assetsParent;
    [Space(15), Header("Generator of the static (non-interactable) layers."), SerializeField]
    private StaticLayersGenerator _staticLayersGenerator;
    [Space(15),Header("Generator of the interactable layers (not-generated in edit mode)."), SerializeField]
    private InteractorsLayersGenerator _interactorsLayersGenerator;
    
    private static string assetsParentName = "LayersAssets_Container";
    #endregion

    #region Properties
    [field: SerializeField, HideInInspector]
    public LayersPoolsManager PoolsManager { get; set; }
    public InteractorsLayersGenerator InteractorsLayersGenerator => _interactorsLayersGenerator;
    #endregion
    
    #region Methods
    public void GenerateBlock(bool NeedToUpdateSystem = false)
    {
        if (PoolsManager == null)
        {
            Debug.LogError("PoolsManager is null!");
            return;
        }

        if (_blockData == null && TryGetComponent<LevelBlockDataContainer>(out LevelBlockDataContainer blockDataContainer))
        {
            _blockData = blockDataContainer;
        }

        if (_blockData == null)
        {
            Debug.LogError("LevelBlockDataContainer is null!");
            return;
        }
        
        UpdateParentContainer();
        ClearGeneratorsObjects();
        
        if(NeedToUpdateSystem)
            UpdateGenerators();
        
        GenerateLayers();
    }

    public void UpdateGenerators()
    {
        if (_staticLayersGenerator != null)
        {
            _staticLayersGenerator.PoolsManager = this.PoolsManager;
            _staticLayersGenerator.ObjectsTransformParent = _assetsParent;
            _staticLayersGenerator.GetLayers();
        }
        
        if (_interactorsLayersGenerator != null)
        {
            _interactorsLayersGenerator.PoolsManager = this.PoolsManager;
            _interactorsLayersGenerator.ObjectsTransformParent = _assetsParent;
            _interactorsLayersGenerator.HorizontalSize = _blockData.Size.x;
            _interactorsLayersGenerator.GetLayers();
        }
    }
    
    private void GenerateLayers()
    {
        if(_staticLayersGenerator != null)
            _staticLayersGenerator.GenerateLayers();

        if (_interactorsLayersGenerator != null)
            _interactorsLayersGenerator.GenerateLayers();
    }
    
    private void UpdateParentContainer()
    {
        if (_assetsParent == null)
        {
            int childsCount = transform.childCount;
            bool containerFound = false;
        
            for (int i = 0; i < childsCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                if (child.name == assetsParentName)
                {
                    containerFound = true;
                    break;
                }
            }

            if (containerFound == false)
            {
                GameObject newAssetsParent = new GameObject();
                newAssetsParent.name = assetsParentName;
                newAssetsParent.transform.SetParent(transform);
                newAssetsParent.transform.localPosition = Vector3.zero;
                newAssetsParent.transform.localRotation = Quaternion.identity;
                _assetsParent = newAssetsParent.transform;
            }
        }
        else
        {
            _assetsParent.localPosition = Vector3.zero;
            _assetsParent.localRotation = Quaternion.identity;
        }
    }

    public void  ClearGeneratorsObjects()
    {
        if(_staticLayersGenerator != null)
            _staticLayersGenerator.ClearObjects();

        if (_interactorsLayersGenerator != null)
            _interactorsLayersGenerator.ClearObjects();
    }

    private void OnDestroy() => ClearGeneratorsObjects();
    #endregion
}