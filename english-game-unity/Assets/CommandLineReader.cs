using System;
using UnityEngine;

public class CommandLineReader : MonoBehaviour
{
    public static string UserId { get; private set; }
    public static string FloorId { get; private set; }
    public static string GameId { get; private set; }
    public static string AccessToken { get; private set; }

    void Awake()
    {
        string[] args = Environment.GetCommandLineArgs();

        foreach (var arg in args)
        {
            if (arg.StartsWith("--userId="))
                UserId = arg.Substring("--userId=".Length);

            if (arg.StartsWith("--floorId="))
                FloorId = arg.Substring("--floorId=".Length);

            if (arg.StartsWith("--gameId="))
                GameId = arg.Substring("--gameId=".Length);

            if (arg.StartsWith("--accessToken="))
                AccessToken = arg.Substring("--accessToken=".Length);
        }
        #if UNITY_EDITOR
                if (string.IsNullOrEmpty(UserId)) UserId = "84aee218-f13c-4596-ad45-4d843a96e101";
                if (string.IsNullOrEmpty(FloorId)) FloorId = "779dd5d3-ddf3-49c6-8e9f-9c90003f9148";
                if (string.IsNullOrEmpty(GameId)) GameId = "29b4ba7e1d174bec999f867f2fd930b3    ";
                if (string.IsNullOrEmpty(AccessToken)) AccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJDdXN0b21lciIsInVzZXJJZCI6Ijg0YWVlMjE4LWYxM2MtNDU5Ni1hZDQ1LTRkODQzYTk2ZTEwMSIsImZ1bGxuYW1lIjoiVGVzdGVyIHPhu5EgbeG7mXQiLCJlbWFpbCI6ImJhaG9zaW4yODdAYW5jZXdhLmNvbSIsInJvbGUiOiJDdXN0b21lciIsImF2YXRhclVybCI6IiIsImV4cCI6MTc0NjI2Nzc5MCwiaXNzIjoiSW50ZXJhY3RpdmVGbG9vciIsImF1ZCI6IkludGVyYWN0aXZlRmxvb3IifQ.hyXhvk0PpteFK1nO-HySWTXnDP_2WjURJyacTh0kOOs";
        #endif

        Debug.Log($"[Startup Args] userId: {UserId}, floorId: {FloorId}, gameId: {GameId}, accessToken: {AccessToken}");
    }
}
