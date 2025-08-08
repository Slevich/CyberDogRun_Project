using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LevelBlockGenerationManager))]
public class LevelBlockDataContainer : MonoBehaviour, IBounded
{
    #region Fields
    [Header("Block borders."), Tooltip("An array of layers to generate content into."),SerializeField]
    private SpriteRenderer _blockBordersRenderer;
    [SerializeField, ReadOnly]
    private LevelBlockGenerationManager levelBlockGenerationManager;
    
    private Vector3 _totalSpritesSize = Vector3.zero;
    #endregion

    #region Properties
    public bool InBorders { get; set; } = false;
    public Vector3 Center
    {
        get
        {
            if (_blockBordersRenderer != null)
            {
                Vector3 center = _blockBordersRenderer.bounds.center;
                return new Vector3(center.x, center.y, 0);
            }
            else
            {
                return Vector3.zero;
            }
        }
    }

    public Vector3 Size
    {
        get
        {
            if (_blockBordersRenderer != null)
            {
                Vector3 size = _blockBordersRenderer.bounds.size;
                return new Vector3(size.x, size.y, 0);
            }
            else
            {
                return Vector3.zero;
            }
        }
    }

    public LevelBlockGenerationManager GenerationManager
    {
        get
        {
            if (levelBlockGenerationManager == null)
            {
                Component generatorComponent = ComponentsSearcher.GetSingleComponentOfTypeFromObjectAndChildren(gameObject, typeof(LevelBlockGenerationManager));
                LevelBlockGenerationManager generationManager = null;

                if (generatorComponent == null)
                {
                    generationManager = gameObject.AddComponent<LevelBlockGenerationManager>();
                }
                else
                {
                    generationManager = (LevelBlockGenerationManager)generatorComponent;
                }
                
                levelBlockGenerationManager = generationManager;
            }
            
            return levelBlockGenerationManager;
        }
    }
    
    public Transform ObjectTransform => transform;
    #endregion

    #region Methods
    #if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying && _blockBordersRenderer != null)
        {
            Gizmos.color = Color.yellowNice;
            Gizmos.DrawWireCube(_blockBordersRenderer.transform.position, _totalSpritesSize);
        }
    }
    #endif
    #endregion
}