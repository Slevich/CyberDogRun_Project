using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class StaticLayersGenerator : LayersGenerator
{
    public override void GetLayers()
    {
        _layers = new BlockLayers[]
        {
            BlockLayers.Background,
            BlockLayers.BackgroundProps,
            BlockLayers.Middleground,
            BlockLayers.MiddlegroundProps,
            BlockLayers.Foreground,
        };
    }
}