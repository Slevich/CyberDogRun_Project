using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class BlocksReturn : MonoBehaviour
{
    #region Fields
    private Transform _anchoredTransform;
    private Transform[] _blocksTransforms = new Transform [] {};
    private CancellationTokenSource _cancellationTokenSource;
    private bool _alreadyStarted = false;
    #endregion

    #region Properties
    public bool HasBlocks => _blocksTransforms.Length > 0;
    #endregion

    #region Methods
    public void SetBlocksTransforms(Transform[] BlocksTransforms)
    {
        if(_alreadyStarted)
            return;
        
        if (BlocksTransforms == null || BlocksTransforms.Length == 0)
        {
            _blocksTransforms = Array.Empty<Transform>();
            return;
        }
            
        _blocksTransforms = BlocksTransforms;
        
        CameraBordersDetectionSystem.Instance.BoundedObjectsChanged
            .Where(blocks => blocks.Any(block => !block.InBorders))
            .Subscribe(blocks => ReturnBlocksToStart(blocks))
            .AddTo(this);
    }

    public void SetAnchoredTransform(Transform AnchoredTransform)
    {
        if(AnchoredTransform != null)
            _anchoredTransform = AnchoredTransform;
    }
    
    private void ReturnBlocksToStart(IBounded[] boundedBlocks)
    {
        if(boundedBlocks.Length == 0)
            return;
        
        if(_blocksTransforms.Length == 0)
            return;
        
        if(_anchoredTransform == null)
            return;

        foreach (IBounded boundedBlock in boundedBlocks)
        {
            Transform boundedTransform = boundedBlock.ObjectTransform;
            
            if(!boundedBlock.InBorders)
                continue;
            
            IEnumerable<Transform> boundedTransforms = _blocksTransforms.Where(blockTransform => blockTransform == boundedTransform);
        
            if (boundedTransforms.Count() == 0)
                return;
        
            LevelBlocksManager.Instance.BlockToTheLayoutEnd();
        }
    }
    #endregion
}