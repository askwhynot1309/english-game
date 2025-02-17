using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json.Linq; // Install Newtonsoft JSON package from Package Manager

public class ImageManager : MonoBehaviour
{
    public string apiKey = "0jjEcn9CHDFAk5GvSM0nISvfbjmEv7FfLFndfzKdF4cozSyr9e8xw7hM";
    public string searchQuery = "apple";
    public RawImage displayImage;

    private const string PEXELS_API_URL = "https://api.pexels.com/v1/search?query=";

    void Start()
    {
        StartCoroutine(FetchImageFromPexels(searchQuery));
    }

    IEnumerator FetchImageFromPexels(string query)
    {
        string url = PEXELS_API_URL + query;
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", apiKey);

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

        string imageUrl = photos[0]["src"]["medium"].ToString(); // Get the first image

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
}
