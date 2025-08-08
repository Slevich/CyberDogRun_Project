using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Zenject;

public class ScoreCounter : MonoBehaviour
{
    #region Fields
    [Header("Current player score."), SerializeField, ReadOnly]
    private int _currentScore = 0;
    
    private PlayerCollisionIdentifier _collisionIdentifier;
    private List<CollisionTagContainer> _scoredObstacles = new List<CollisionTagContainer>(); 
    private static ScoreCounter _instance;
    #endregion

    #region Properties

    public static ScoreCounter Instance
    {
        get
        {
            if(_instance == null)
                _instance = FindObjectOfType<ScoreCounter>();
            
            return _instance;
        }
    }
    
    public Action<int> ScoreChanged { get; set; }
    public int CurrentScore => _currentScore;
    #endregion
    
    #region Methods
    private void Start()
    {
        if(_instance != null && _instance != this)
            Destroy(gameObject);
        
        ScoreChanged?.Invoke(_currentScore);
    }

    [Inject]
    public void Construct(PlayerCollisionIdentifier CollisionIdentifier)
    {
        _collisionIdentifier = CollisionIdentifier;
        
        if(_collisionIdentifier !=  null)
            Initialize();
    }

    private void Initialize()
    {
        _collisionIdentifier.RaysCollisionsDetected
            .SkipWhile(tags => tags.Any(tag => tag.Tag != CollisionTags.Obstacle))
            .Subscribe(tags => ScoreDetection(tags))
            .AddTo(this);
    }

    private void ScoreDetection(CollisionTagContainer[] tags)
    {
        IEnumerable<CollisionTagContainer> obstaclesTagsContainers = tags.Where(tag => tag.Tag == CollisionTags.Obstacle);
        
        if(obstaclesTagsContainers.Count() == 0)
            return;

        Vector3 casterPosition = _collisionIdentifier.CasterPosition;
        float casterXAxisPosition = casterPosition.x;
        
        foreach (CollisionTagContainer tagContainer in obstaclesTagsContainers)
        {
            float tagContainerXAxisPosition = tagContainer.transform.position.x;

            if (tagContainerXAxisPosition < casterXAxisPosition)
            {
                if(_scoredObstacles.Contains(tagContainer))
                    continue;
                
                _currentScore++;
                _scoredObstacles.Add(tagContainer);
                ScoreChanged?.Invoke(_currentScore);
            }
        }
        
        if(_scoredObstacles.Count == 0)
            return;
        
        int scoredObstaclesCount = _scoredObstacles.Count;

        for (int i = scoredObstaclesCount - 1; i >= 0; i--)
        {
            CollisionTagContainer scoredTagContainer = _scoredObstacles[i];
            
            if (!obstaclesTagsContainers.Contains(scoredTagContainer))
            {
                _scoredObstacles.RemoveAt(i);
            }
        }
    }
    #endregion
}