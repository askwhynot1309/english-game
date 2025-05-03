using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class GameAPI : MonoBehaviour
{
    public static GameAPI Instance;

    private string userId;
    private string gameId;
    private string floorId;
    private string token;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            userId = CommandLineReader.UserId;
            gameId = CommandLineReader.GameId;
            token = CommandLineReader.AccessToken;
            floorId = CommandLineReader.FloorId;
            Debug.Log($"[GameAPI] Loaded fields: userId={userId}, gameId={gameId}, floorId={floorId}, token={(string.IsNullOrEmpty(token) ? "null" : "set")}");


        }
        else
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator GetHighScore(Action<int> onSuccess, Action<string> onError)
    {

        string url = $"https://ifle-api.fusdeploy.site/api/history/get-high-score?userId={userId}&gameId={gameId}";

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", $"Bearer {token}");
        //Debug.Log(token);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            HighScoreResponse data = JsonUtility.FromJson<HighScoreResponse>(json);
            Debug.Log(data);
            onSuccess?.Invoke(data.score);
        }
        else
        {
            onError?.Invoke(request.error);
        }
    }

    public IEnumerator PostPlayHistory(int finalScore, Action onSuccess, Action<string> onError)
    {
        PlayHistory history = new PlayHistory
        {
            gameId = gameId,
            floorId = floorId,
            userId = userId,
            score = finalScore,
            startAt = DateTime.Now,
        };

        string json = JsonUtility.ToJson(history);
        UnityWebRequest request = new UnityWebRequest("https://ifle-api.fusdeploy.site/api/history/new-play-history", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {token}");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            onSuccess?.Invoke();
        }
        else
        {
            onError?.Invoke(request.error);
        }
    }

    [Serializable]
    public class HighScoreResponse
    {
        public string id;
        public string gameId;
        public string floorId;
        public string startAt;
        public string userId;
        public int score;
    }

    [Serializable]
    public class PlayHistory
    {
        public string gameId;
        public string floorId;
        public DateTime startAt;
        public string userId;
        public int score;
    }
}
