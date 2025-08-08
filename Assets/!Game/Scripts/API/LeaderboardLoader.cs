using System;
using UnityEngine;
using UnityEngine.Networking;

public class LeaderboardLoader : MonoBehaviour
{
    #region Fields
    [Header("Score info container prefab."), SerializeField]
    private ScoreNoteTextContainer _scoreTextContainerPrefab;
    #endregion

    #region Methods
    private void Start()
    {
        ClearChildren();
        FillLeaderboardData();
    }

    private void ClearChildren()
    {
        int childCount = transform.childCount;

        if (childCount > 0)
        {
            for (int i = childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }
    
    private async void FillLeaderboardData()
    {
        if (_scoreTextContainerPrefab == null)
        {
            Debug.LogError("No score text container prefab found!");
            return;
        }

        ScoreInfo[] scoresData =  await LeaderboardDataManager.GetTopScores(5);

        if (scoresData.Length > 0)
        {
            foreach (ScoreInfo data in scoresData)
            {
                GameObject newScoreNote = Instantiate(_scoreTextContainerPrefab.gameObject, transform);
                newScoreNote.transform.SetParent(transform);

                Component scoreNoteContainerComponent = ComponentsSearcher.GetSingleComponentOfTypeFromObjectAndChildren(newScoreNote, typeof(ScoreNoteTextContainer));

                if (scoreNoteContainerComponent != null)
                {
                    ScoreNoteTextContainer scoreTextContainer = (ScoreNoteTextContainer)scoreNoteContainerComponent;
                    scoreTextContainer.PlayerNameText.text = data.name;
                    scoreTextContainer.ScoreText.text = data.score.ToString();
                }
                else
                {
                    Destroy(newScoreNote);
                }
            }
        }
        else
            Debug.LogError("No scores found!");
    }
    #endregion

}
