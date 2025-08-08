using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SpritesBoundsCalculator
{
    #region  Fields
    private List<Renderer> _renderers = new List<Renderer>();
    private Type[] _renderersTypes = new Type[2] { typeof(SpriteRenderer), typeof(TilemapRenderer)};
    private Bounds _totalVisibleBounds = new Bounds();
    #endregion

    #region Properties
    public Vector3 BoundsSize { get; private set; } = Vector3.zero;
    #endregion

    #region Constructors
    public SpritesBoundsCalculator(GameObject SpritesRenderersParentHolder)
    {
        FillRenderers(SpritesRenderersParentHolder);
        CalculateBounds();
    }
    #endregion
    
    #region Methods
    private void FillRenderers(GameObject ParentObject)
    {
        Component[] rendererComponents = ComponentsSearcher.GetComponentsOfTypesFromObjectAndAllChildren(ParentObject, _renderersTypes);
        Debug.Log("Renderers count: " + rendererComponents.Length);
        
        if (rendererComponents.Length > 0)
        {
            _renderers.Clear();
            
            foreach (Component rendererComponent in rendererComponents)
            {
                _renderers.Add(rendererComponent as Renderer);
            }
        }
    }

    private void CalculateBounds()
    {
        Transform trans = _renderers[0].gameObject.transform;
        Tilemap tilemap = trans.GetComponent<Tilemap>();
        SpriteRenderer tilemapRenderer = _renderers[0] as SpriteRenderer;
        BoundsSize = tilemapRenderer.bounds.size;
    }
    #endregion
}
