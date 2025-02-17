using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using System.Linq;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public RawImage displayImage;
    public TextMeshProUGUI answer1Text;
    public TextMeshProUGUI answer2Text;
    public Button answer1Button;
    public Button answer2Button;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;

    private List<WordData> words = new List<WordData>
    {
        new WordData("apple", "banana"),
        new WordData("car", "bike"),
        new WordData("dog", "cat"),
        new WordData("chair", "table"),
        new WordData("ball", "hat"),
        new WordData("computer", "phone"),
        new WordData("house", "building"),
        new WordData("fish", "bird"),
        new WordData("tree", "bush"),
        new WordData("pencil", "pen"),
    };
    private int score = 0;
    private float timeRemaining = 30f;
    private bool gameActive = true;

    private string correctAnswer;
    private const string API_KEY = "0jjEcn9CHDFAk5GvSM0nISvfbjmEv7FfLFndfzKdF4cozSyr9e8xw7hM";
    private const string PEXELS_API_URL = "https://api.pexels.com/v1/search?query=";

    void Start()
    {
        StartNewRound();
        answer1Button.onClick.AddListener(() => CheckAnswer(answer1Text.text));
        answer2Button.onClick.AddListener(() => CheckAnswer(answer2Text.text));
    }

    void Update()
    {
        if (gameActive)
        {
            timeRemaining -= Time.deltaTime;
            timerText.text = "Time: " + Mathf.Ceil(timeRemaining);

            if (timeRemaining <= 0)
            {
                gameActive = false;
                timerText.text = "Game Over!";
            }
        }
    }

    void StartNewRound()
    {
        if (!gameActive) return;

        WordData word = words[Random.Range(0, words.Count)];
        correctAnswer = word.correctWord;
        string wrongAnswer = word.wrongWord;

        // Randomly assign positions to answers
        if (Random.value > 0.5f)
        {
            answer1Text.text = correctAnswer;
            answer2Text.text = wrongAnswer;
        }
        else
        {
            answer1Text.text = wrongAnswer;
            answer2Text.text = correctAnswer;
        }

        StartCoroutine(FetchImageFromPexels(correctAnswer));
    }

    IEnumerator FetchImageFromPexels(string query)
    {
        string url = PEXELS_API_URL + query;
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", API_KEY);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error fetching image: " + request.error);
            yield break;
        }

        JObject jsonResponse = JObject.Parse(request.downloadHandler.text);
        JArray photos = (JArray)jsonResponse["photos"];

        if (photos.Count == 0)
        {
            Debug.LogError("No images found for query: " + query);
            yield break;
        }

        string imageUrl = photos[0]["src"]["medium"].ToString();
        StartCoroutine(DownloadAndDisplayImage(imageUrl));
    }

    IEnumerator DownloadAndDisplayImage(string imageUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error downloading image: " + request.error);
            yield break;
        }

        Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
        displayImage.texture = texture;
    }

    void CheckAnswer(string chosenAnswer)
    {
        if (!gameActive) return;

        if (chosenAnswer == correctAnswer)
        {
            score += 100;
            scoreText.text = "Score: " + score;
        }

        StartNewRound();
    }
}
