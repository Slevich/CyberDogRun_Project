using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LevelBlocksManager))]
public class LevelBlocksDependenciesManager : MonoBehaviour
{
    #region Fields
    [SerializeField, HideInInspector]
    private GameObject _parentHolder;
    [SerializeField, HideInInspector] 
    public LayersPoolsManager _poolsManager;
    [SerializeField, HideInInspector] 
    public AxisMovementSystem _movementSystem;
    [SerializeField, HideInInspector] 
    public BlocksReturn _returnSystem;
    #endregion
    
    #region Properties

    public LayersPoolsManager PoolsManager
    {
        get
        {
            if(_poolsManager == null)
                FindReferences();
            
            return _poolsManager;
        }
    }

    public AxisMovementSystem MovementSystem
    {
        get
        {
            if (_movementSystem == null)
                FindReferences();
            
            return _movementSystem;
        }
    }

    public BlocksReturn ReturnSystem
    {
        get
        {
            if (_returnSystem == null)
                FindReferences();
            
            return _returnSystem;
        }
    }
    #endregion
    
    #region Methods
    private void FindReferences()
    {
        Component poolsSystemComponent = FindChildComponent<LayersPoolsManager>(gameObject);
        if (poolsSystemComponent != null)
            _poolsManager = (LayersPoolsManager)poolsSystemComponent;
        
        Component axisMovementComponent = FindChildComponent<AxisMovementSystem>(gameObject);
        if(axisMovementComponent != null)
            _movementSystem = (AxisMovementSystem)axisMovementComponent;
        
        Component blocksReturnComponent = FindChildComponent<BlocksReturn>(gameObject);
        if(blocksReturnComponent != null)
            _returnSystem = (BlocksReturn)blocksReturnComponent;
    }
    
    public void SendStartData(List<LevelBlockDataContainer> Blocks)
    {
        if(Blocks.Count == 0)
            return;

        GameObject[] blocksObjects = Blocks.Select(block => block.gameObject).ToArray();
        Transform blocksParentTransform = blocksObjects.FirstOrDefault().transform.parent;
        
        CameraBordersDetectionSystem.Instance.SetBoundedObjects(blocksObjects);
        ParallaxEffectSystem.Instance.FindParallaxEffectors(blocksParentTransform.gameObject);
        
        if (MovementSystem != null)
        {
            MovementSystem.SetMovementData(blocksObjects);
        }
        
        if (ReturnSystem  != null)
        {
            ReturnSystem.SetBlocksTransforms(Blocks.Select(block => block.transform).ToArray());
            ReturnSystem.SetAnchoredTransform(blocksParentTransform);
        }
    }

    public void SendStop()
    {
        CameraBordersDetectionSystem.Instance.StopDetection();
        ParallaxEffectSystem.Instance.StopExecution();
        
        if (MovementSystem != null)
        {
            MovementSystem.StopMovement();
        }
    }
    
    [ExecuteInEditMode]
    private Component FindChildComponent<T>(GameObject obj) where T : Component
    {
        return ComponentsSearcher.GetSingleComponentOfTypeFromObjectAndChildren(obj, typeof(T));
    }
    #endregion
}
