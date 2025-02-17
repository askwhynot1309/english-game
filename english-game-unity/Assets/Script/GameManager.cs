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
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI feedbackText;

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
        new WordData("green tree", "bush"),
        new WordData("pencil", "pen"),
        new WordData("bookshelf", "table"),
        new WordData("cat", "bear"),
        new WordData("shoes", "sandals"),
        new WordData("cup", "mug"),
        new WordData("crayon", "chalk"),
        new WordData("truck", "van"),
        new WordData("paper", "rock"),
        new WordData("tent", "factory"),
        new WordData("Formula 1", "NASCAR"),
        new WordData("Umbrella", "Triangle"),
        new WordData("Treasure map", "book"),
        new WordData("rose flower", "tree"),
        new WordData("chicken", "bird"),
        new WordData("school bag", "suitcase"),
        new WordData("hat", "pants"),
    };
    private int score = 0;
    private float timeRemaining = 30f;
    private bool gameActive = true;

    private string correctAnswer;
    private const string API_KEY = "0jjEcn9CHDFAk5GvSM0nISvfbjmEv7FfLFndfzKdF4cozSyr9e8xw7hM";
    private const string PEXELS_API_URL = "https://api.pexels.com/v1/search?query=";
    private int highScore = 0;

    void Start()
    {
        StartNewRound();
        gameOverPanel.SetActive(false);
        highScore = PlayerPrefs.GetInt("HighScore", 0);
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
                timerText.text = "Time's up!";
                GameOver();
            }
        }
    }

    void GameOver()
    {
        gameActive = false;
        gameOverPanel.SetActive(true);

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        finalScoreText.text = $"Final Score: {score}\nHigh Score: {highScore}";
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
            feedbackText.text = "Correct";
            feedbackText.color = Color.green;
        }
        else
        {
            feedbackText.text = "Incorrect!";
            feedbackText.color = Color.red;
        }

        feedbackText.alpha = 1;
        StartCoroutine(HideFeedbackAfterDelay());

        StartNewRound();
    }

    public void RestartGame()
    {
        score = 0;
        timeRemaining = 30f;
        gameActive = true;
        gameOverPanel.SetActive(false);
        scoreText.text = "Score: " + score;
        timerText.text = "Time: " + Mathf.Ceil(timeRemaining);
        Debug.Log("Restart Game");
        StartNewRound();
    }

    IEnumerator HideFeedbackAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        feedbackText.alpha = 0;
    }
}
