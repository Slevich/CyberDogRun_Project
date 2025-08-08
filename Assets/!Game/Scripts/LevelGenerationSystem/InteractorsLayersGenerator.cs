using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[Serializable]
public class InteractorsLayersGenerator : LayersGenerator
{
    [field: SerializeField, HideInInspector]
    public float HorizontalSize { get; set; } = 1f;

    [field: SerializeField]
    private InteractorsGeneratorLayerData[] _layersData =  new InteractorsGeneratorLayerData[0];
    private static readonly float minDistanceBetweenAssetsIntoLayer = 6.5f;
    private int _blockGenerationsCount = 0;
    
    public InteractorsGeneratorLayerData[] LayersData { get => _layersData; set => _layersData = value; }
    
    public override void GetLayers()
    {
        _layers = _layersData.Select(data => data.Layer).ToArray();
    }
    

    public override void GenerateLayers()
    {
        UpdateContainers();

        if (Application.isPlaying)
        {
            _blockGenerationsCount++;
        }
        else
        {
            return;
        }
        
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
                
        if (_layersData.Length == 0)
        {
            Debug.LogError("No Layers data Found!");
            return;
        }
        
        List<InteractorsGeneratorLayerData> conflictedLayers = new List<InteractorsGeneratorLayerData>();

        foreach (InteractorsGeneratorLayerData layerData in _layersData)
        {
            layerData.SkipThisGeneration = false;
            
            if(layerData.GenerationConflict)
                conflictedLayers.Add(layerData);
        }
        
        if (conflictedLayers.Count() > 1)
        {
            int conflictLayersCount = conflictedLayers.Count();
            int generatedLayerIndex = Random.Range(0, conflictLayersCount);

            for (int i = 0; i < conflictLayersCount; i++)
            {
                if(i == generatedLayerIndex)
                    continue;
                
                conflictedLayers[i].SkipThisGeneration = true;
            }
        }
        
        foreach (InteractorsGeneratorLayerData data in _layersData)
        {
            GetObjectsFromPool(data);
        }
    }

    private void GetObjectsFromPool(InteractorsGeneratorLayerData layerData)
    {   
        if(layerData == null)
            return;
        
        if(_blockGenerationsCount <= layerData.BlockGenerationsToSkip)
            return;
        
        layerData.CurrentGenerationsCount++;
        
        if(layerData.SkipThisGeneration)
            return;
        
        if((layerData.BlockGenerationsSpawnInterval > 0) && ( layerData.CurrentGenerationsCount % layerData.BlockGenerationsSpawnInterval != 0))
            return;

        int objectsPerBlock = 1;

        if (layerData.CurrentGenerationsCount > objectsPerBlock)
            objectsPerBlock = Random.Range(1, layerData.CurrentMaxAmountPerBlock + 1);
        
        for (int i = 1; i <= objectsPerBlock; i++)
        {
            LayerPoolObject poolObject = PoolsManager.GetObjectFromLayer(layerData.Layer);
            
            if(poolObject == null)
                return;

            string objectLayerName = poolObject.DataContainer.Data.Sort.ToString();
            Transform poolObjectParent = _blocksLayersTransformsParents.Where(parent => parent.transform.name.Contains(objectLayerName)).FirstOrDefault();
            ParallaxEffector effector = poolObjectParent.GetComponent<ParallaxEffector>();
            effector.ParallaxSpeed = poolObject.DataContainer.Data.ParallaxSpeed;
            bool assetPlaced = TryPlaceLayerAsset(poolObject, poolObjectParent, layerData.VerticalOffset);

            if (!assetPlaced)
            {
                Debug.Log("Не смог разместить объект!");
                poolObject.ReturnToPool();
                continue;
            }
            
            _layerAssets.Add(poolObject);
        }

        if ((layerData.CurrentMaxAmountPerBlockIncrementInterval > 0) && (layerData.CurrentMaxAmountPerBlock < layerData.MaxAmountPerBlock) && (layerData.CurrentGenerationsCount % layerData.CurrentMaxAmountPerBlockIncrementInterval == 0))
        {
            layerData.CurrentMaxAmountPerBlock++;
        }
    }
    
    private bool TryPlaceLayerAsset(LayerPoolObject asset, Transform assetParent, float verticalOffset, int maxAttempts = 50 )
    {
        // Получаем границы спавна
        Bounds spawnBounds = GetLayerBounds(asset.DataContainer.Data.Sort, assetParent.position);
    
        float targetY = spawnBounds.center.y + verticalOffset;
        float blockLeft = spawnBounds.center.x - (spawnBounds.size.x / 2f) + 1f;
        float blockRight = spawnBounds.center.x + (spawnBounds.size.x / 2f) - 1f;
    
        if (blockLeft >= blockRight)
        {
            Debug.LogWarning($"Object {asset.name} collider too big for the block.");
            return false;
        }
        
        Component assetColliderComponent = ComponentsSearcher.GetSingleComponentOfTypeFromObjectAndChildren(asset.gameObject, typeof(Collider2D));
        
        if(assetColliderComponent == null)
            return false;
            
        Collider2D assetCollider = (Collider2D)assetColliderComponent;
        
        float assetRadius = assetCollider != null
            ? Mathf.Max(assetCollider.bounds.extents.x, assetCollider.bounds.extents.y)
            : 0.5f;
    
        float checkRadius = assetRadius + minDistanceBetweenAssetsIntoLayer;
    
        for (int i = 0; i < maxAttempts; i++)
        {
            float randomXPosition = Random.Range(blockLeft, blockRight);
            Vector3 candidatePosition = new Vector3(randomXPosition, targetY, asset.transform.position.z);
            
            Physics2D.SyncTransforms();
    
            bool tooClose = false;
    
            Collider2D[] hits = Physics2D.OverlapCircleAll(candidatePosition, checkRadius);
            foreach (Collider2D collider in hits)
            {
                if (collider.gameObject == asset.gameObject)
                    continue;
                
                Component tagContainerComponent = ComponentsSearcher.GetSingleComponentOfTypeFromObjectAndChildren(collider.gameObject, typeof(CollisionTagContainer));
        
                if(tagContainerComponent == null)
                    return false;
            
                CollisionTagContainer tagContainer = (CollisionTagContainer)tagContainerComponent;
    
                if (tagContainer != null && tagContainer.Tag != CollisionTags.Ground)
                {
                    tooClose = true;
                    break;
                }
            }
            
            if (!tooClose)
            {
                asset.transform.SetParent(assetParent);
                asset.transform.rotation = Quaternion.identity;
                asset.transform.position = candidatePosition;
                return true;
            }
        }
    
        Debug.LogWarning($"Can't place object {asset.name} for {maxAttempts} attempts.");
        return false;
    }
        
        
        private Bounds GetLayerBounds(BlockLayers layer, Vector3 parentCenter)
        {
            switch (layer)
            {
                case BlockLayers.Bonuses:
                    return new Bounds(parentCenter, new Vector3(HorizontalSize, 1, 0));
                
                case BlockLayers.GroundObstacles:
                    return new Bounds(parentCenter, new Vector3(HorizontalSize, 1, 0));
                
                case BlockLayers.SkyObstacles:
                    Vector3 center = new Vector3(parentCenter.x + (HorizontalSize / 2), parentCenter.y, parentCenter.z);
                    return new Bounds(center, new Vector3(HorizontalSize, 1, 0));
                    
            }
            
            return new Bounds(parentCenter, new Vector3(HorizontalSize, 1, 0));
        }
}

[Serializable]
public class InteractorsGeneratorLayerData 
{
    [field: Space(10)]
    [field: Header("Layer to generate."), SerializeField]
    public BlockLayers Layer { get; set; } = BlockLayers.GroundObstacles;
    [field: Header("Vertical offset from block center."), SerializeField, Range(-10, 10)]
    public float VerticalOffset { get; set; } = -1f;
    [field: Header("How many block generation skipped from start?"), SerializeField] 
    public int BlockGenerationsToSkip { get; set; } = 3;
    [field: Header("Max amount per block"), SerializeField]
    public int MaxAmountPerBlock { get; set; } = 4;
    [field: Header("Current amount per block."), SerializeField]
    public int CurrentMaxAmountPerBlock { get; set; } = 1;
    [field: Header("Every how many block spawned new object."), SerializeField]
    public int BlockGenerationsSpawnInterval { get; set; } = 2;
    [field: Header("Every how many generations current amount per block increments."), SerializeField]
    public int CurrentMaxAmountPerBlockIncrementInterval { get; set; } = 1;
    [field: Header("Conflict layers can't generate in one time."), SerializeField]
    public bool GenerationConflict { get; set; } = false;
    
    public int CurrentGenerationsCount { get; set; } = 0;
    public bool SkipThisGeneration { get; set; } = false;

    public bool ReadyToGenerate(int BlockTotalGenerations)
    {
        return (BlockTotalGenerations > BlockGenerationsToSkip) &&
               (CurrentGenerationsCount % BlockGenerationsSpawnInterval == 0);
    }
}