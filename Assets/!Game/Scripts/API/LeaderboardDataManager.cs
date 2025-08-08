using System;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

public static class LeaderboardDataManager
{
    //Hosting links
    private static readonly string baseUrl = "https://cyber-dog-run.vercel.app";
    private static readonly string scoreSubmissionPath = "/api/submit-score";
    private static readonly string topScoresPath = "/api/top-scores";
    
    // Local test
    // private static readonly string baseUrl = "http://localhost:3000";
    // private static readonly string scoreSubmissionPath = "/submit-score";
    // private static readonly string topScoresPath = "/top-scores";
    
    public static async UniTask<UnityWebRequest.Result> PostScore(string PlayerName, int Score)
    {
        ScoreInfo data = new ScoreInfo { name = PlayerName, score = Score };
        string json = JsonUtility.ToJson(data);

        using (UnityWebRequest request = new UnityWebRequest(baseUrl + scoreSubmissionPath, "POST"))
        {
            byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Score submitted: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Submit failed: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
            }

            return request.result;
        }
    }
    
    public static async UniTask<ScoreInfo[]> GetTopScores(int ScoreCount = 5)
    {
        string url = $"{baseUrl}{topScoresPath}?limit={ScoreCount}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                
                if (string.IsNullOrEmpty(json) || json == "[]")
                    return new ScoreInfo[0];
                
                try
                {
                    ScoreInfo[] scores = JsonHelper.FromJson<ScoreInfo>(json);
                    return scores ?? new ScoreInfo[0];
                }
                catch (Exception e)
                {
                    Debug.LogError("Parsing JSON error: " + e.Message);
                    return new ScoreInfo[0];
                }
            }
            else
            {
                Debug.LogError("Fetch failed: " + request.error);
                return new ScoreInfo[0];
            }
        }
    }
}

[Serializable]
public class ScoreInfo
{
    public string name;
    public int score;
}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        var wrapper = JsonUtility.FromJson<Wrapper<T>>($"{{\"data\":{json}}}");
        return wrapper.data;
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] data;
    }
}
