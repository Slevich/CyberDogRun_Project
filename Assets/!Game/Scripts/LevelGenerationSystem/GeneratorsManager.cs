using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GeneratorsManager : MonoBehaviour
{
    [field: SerializeField, HideInInspector]
    public List<LevelBlockGenerationManager> BlockGenerationManagers { get; set; } = new List<LevelBlockGenerationManager>();

    private LevelBlockGenerationManager _bonusHolder = null;
    
    private void Awake()
    {
           LeaveJustOneBonusLayer();
    }

    private void LeaveJustOneBonusLayer()
    {
        if(BlockGenerationManagers.Count == 0)
            return;

        bool alreadyHasBonusLayer = false;
        
        for (int i = 0; i < BlockGenerationManagers.Count; i++)
        {
            if (alreadyHasBonusLayer)
            {
                InteractorsGeneratorLayerData[] data = BlockGenerationManagers[i].InteractorsLayersGenerator.LayersData;
                List<InteractorsGeneratorLayerData> dataList = data.ToList();

                for (int j = dataList.Count - 1; j >= 0; j--)
                {
                    if (dataList[j].Layer == BlockLayers.Bonuses)
                    {
                        dataList.Remove(dataList[j]);
                    }
                }
                
                BlockGenerationManagers[i].InteractorsLayersGenerator.LayersData = dataList.ToArray();
            }
            else
            {
                alreadyHasBonusLayer = BlockGenerationManagers[i].InteractorsLayersGenerator.LayersData.Any(data => data.Layer == BlockLayers.Bonuses);
            }
        }
    }
}
