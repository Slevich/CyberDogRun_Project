using System;
using System.Linq;
using UnityEngine;

public static class WorldBlocksContainer
{
    #region Fields
    private static LevelBlockDataContainer[] blocks = new LevelBlockDataContainer[] { };
    #endregion

    #region Methods
    public static void SetBlocks(LevelBlockDataContainer[] NewBlocks) => blocks = NewBlocks;
    public static LevelBlockDataContainer[] GetBlocks() => blocks;
    public static void ClearBlocks() => blocks = new LevelBlockDataContainer[] { };

    public static Transform[] GetBlocksTransforms()
    {
        if(blocks.Length == 0)
            return Array.Empty<Transform>();
        
        return blocks.Select(block => block.transform).ToArray();
    }
    
    public static GameObject[] GetBlocksGameObjects()
    {
        if(blocks.Length == 0)
            return Array.Empty<GameObject>();
        
        return blocks.Select(block => block.gameObject).ToArray();
    }
    #endregion

}
