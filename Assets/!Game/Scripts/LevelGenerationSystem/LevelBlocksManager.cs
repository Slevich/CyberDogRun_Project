using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class LevelBlocksManager : MonoBehaviour
{
    #region Fields
    [Header("Block sample prefab."),
     Tooltip("Block sample inside which generating objects."),
     SerializeField]
    private LevelBlockDataContainer _blockSample;
    
    [Header("Number of prepared blocks."),
     Tooltip("The number of blocks prepared at the start of the game."),
     SerializeField,
     Range(2,5)]
    private int _preparedBlocksAmount = 2;
    
    [Header("Regenerate blocks on awake?"),
     Tooltip("Checkbox to set that blocks will be regenerated on awake."),
     SerializeField]
    private bool _regenerateOnAwake = false;
    
    [Header("Parent for the blocks."),
     Tooltip("Parent transform for the blocks."),
     SerializeField]
    private Transform _parentTransform;
    
    [Header("List of level blocks."), 
     Tooltip("List of level blocks that are managed by the manager. DO NOT TOUCH!"), 
     SerializeField]
    private List<LevelBlockDataContainer> _blocks = new List<LevelBlockDataContainer>();
    
    [SerializeField, HideInInspector]
    private LevelBlocksDependenciesManager _dependenciesManager;
    
    private static LevelBlocksManager _instance;
    private static readonly string blocksParentName = "BlocksContainer";
    private static readonly string levelBlockVariantName = "LevelBlockVariant";
    private static readonly float blocksLayering = 0.1f;
    #endregion
    
    #region Properties
    public static LevelBlocksManager Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindFirstObjectByType<LevelBlocksManager>();
            }
            
            return _instance;
        }
    }
    
    public List<LevelBlockDataContainer> Blocks => _blocks;
    public int BlocksCount => _blocks.Count;

    private LevelBlocksDependenciesManager DependenciesManager
    {
        get
        {
            if (_dependenciesManager == null && TryGetComponent<LevelBlocksDependenciesManager>(out LevelBlocksDependenciesManager dependenciesManager))
            {
                _dependenciesManager = dependenciesManager;
            }
            else if (_dependenciesManager == null)
            {
                _dependenciesManager = gameObject.AddComponent<LevelBlocksDependenciesManager>();
            }
            
            return _dependenciesManager;
        }
    }
    #endregion

    #region Methods
    private void Awake()
    {
        if(Instance != this)
            Destroy(this.gameObject);
        

    }

    private void Start()
    {
        if (_regenerateOnAwake)
        {
            GenerateBlocks();
        }
    }

    public void StartExecution()
    {
        _dependenciesManager.SendStartData(_blocks);
    }

    public void StopExecution()
    {
        _dependenciesManager.SendStop();
    }
    
    [ExecuteInEditMode]
    public void UpdateSystem(int Amount)
    {
        if(Amount < 0)
            return;
        
        ClearBlocks();
        
        if(_blockSample == null)
            return;

        if (_parentTransform == null)
        {
            GameObject blocksParent = new GameObject();
            blocksParent.name = blocksParentName;
            blocksParent.transform.parent = transform;
            blocksParent.transform.localPosition = Vector3.zero;
            _parentTransform = blocksParent.transform;
        }
        else
        {
            _parentTransform.name = blocksParentName;
        }

        GeneratorsManager generatorsManager = null;
        if (_parentTransform.gameObject.TryGetComponent(out GeneratorsManager manager))
        {
            generatorsManager = manager;
        }
        else
            generatorsManager = _parentTransform.AddComponent<GeneratorsManager>();
        
        for (int i = 1; i <= Amount; i++)
        {
            LevelBlockDataContainer newBlock = Instantiate(_blockSample, _parentTransform);
            newBlock.transform.localPosition = Vector3.zero;
            newBlock.transform.localRotation = Quaternion.identity;
            newBlock.name = levelBlockVariantName + "_" + i;
            _blocks.Add(newBlock);
            
        }

        generatorsManager.BlockGenerationManagers = _blocks.Select(block => block.GenerationManager).ToList();
        UpdateBlocksLayout();
        GenerateBlocks();
    }

    [ExecuteInEditMode]
    private void UpdateBlocksLayout()
    {
        if(_blocks.Count == 0)
            return;

        Vector3 parentPosition = _parentTransform.position;
        
        for (int i = 0; i < _blocks.Count; i++)
        {
            float layering = blocksLayering;
            
            if (i == 0)
                layering = 0;
            
            LevelBlockDataContainer blockDataContainer = _blocks[i];
            float horizontalPosition = parentPosition.x + (blockDataContainer.Size.x * i) - layering;
            Vector3 newPosition = new Vector3(horizontalPosition, parentPosition.y, 0);
            blockDataContainer.ObjectTransform.position = newPosition;
        }
    }

    public void BlockToTheLayoutEnd()
    {
        if(_blocks.Count == 0)
            return;

        IOrderedEnumerable<LevelBlockDataContainer> blocksOnXAxisOrdered = _blocks.OrderBy(block => block.ObjectTransform.position.x);
        LevelBlockDataContainer mostLeftBlock = blocksOnXAxisOrdered.First();
        LevelBlockDataContainer mostRightBlock = blocksOnXAxisOrdered.Last();
        Vector3 parentPosition = _parentTransform.position;
        float horizontalPosition = mostRightBlock.ObjectTransform.position.x + (mostLeftBlock.Size.x) - blocksLayering;
        Vector3 newPosition = new Vector3(horizontalPosition, parentPosition.y, 0);
        mostLeftBlock.ObjectTransform.position = newPosition;
        mostLeftBlock.GenerationManager.GenerateBlock();
    }

    [ExecuteInEditMode]
    public void ClearBlocks()
    {
        if(_blocks.Count == 0)
            return;
        
        for (int i = _blocks.Count - 1; i >= 0; i--)
        {
            _blocks[i].GenerationManager.ClearGeneratorsObjects();
            GameObject removedBlock = _blocks[i].gameObject;
            _blocks.RemoveAt(i);
            DestroyImmediate(removedBlock);
        }
    }

    [ExecuteInEditMode]
    public void GenerateBlocks()
    {
        if(_blocks.Count == 0)
            return;

        LayersPoolsManager poolsManager = DependenciesManager.PoolsManager;
        if (poolsManager == null)
        {
            Component poolsManagerComponent =
                ComponentsSearcher.GetSingleComponentOfTypeFromObjectAndChildren(gameObject, typeof(LayersPoolsManager));
            
            if (poolsManagerComponent == null)
                return;
            
            poolsManager = (LayersPoolsManager)poolsManagerComponent;
        }
#if UNITY_EDITOR
        poolsManager.UpdateLayers();
#endif  
        foreach (LevelBlockDataContainer block in _blocks)
        {
            block.GenerationManager.PoolsManager = poolsManager;
            block.GenerationManager.GenerateBlock(true);
        }
    }
    #endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(LevelBlocksManager))]
public class LevelBlocksManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        serializedObject.Update();

        EditorGUILayout.Space(25);
        bool updateButtonPressed = GUILayout.Button("Update system!");

        if (updateButtonPressed)
        {
            LevelBlocksManager blocksManager = (LevelBlocksManager)target;
            int preparedBlocksValue = serializedObject.FindProperty("_preparedBlocksAmount").intValue;
            blocksManager.UpdateSystem(preparedBlocksValue);
        }
        
        EditorGUILayout.Space(25);
        bool regenerateButtonPressed = GUILayout.Button("Only regenerate blocks!");
        if (regenerateButtonPressed)
        {
            LevelBlocksManager blocksManager = (LevelBlocksManager)target;
            blocksManager.GenerateBlocks();
        }
        
        EditorGUILayout.Space(25);
        bool clearButtonPressed = GUILayout.Button("Clear blocks...");
        if (clearButtonPressed)
        {
            LevelBlocksManager blocksManager = (LevelBlocksManager)target;
            blocksManager.ClearBlocks();
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}
#endif