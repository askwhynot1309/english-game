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
                if (string.IsNullOrEmpty(UserId)) UserId = "3cd94c6a-7bbe-4b08-bd4e-961ec9e41930";
                if (string.IsNullOrEmpty(FloorId)) FloorId = "1";
                if (string.IsNullOrEmpty(GameId)) GameId = "b834ad5204834367a06be32cb4baf818    ";
                if (string.IsNullOrEmpty(AccessToken)) AccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJDdXN0b21lciIsInVzZXJJZCI6IjNjZDk0YzZhLTdiYmUtNGIwOC1iZDRlLTk2MWVjOWU0MTkzMCIsImZ1bGxuYW1lIjoicXV5ZW4iLCJlbWFpbCI6InVzZXJAZ21haWwuY29tIiwicm9sZSI6IkN1c3RvbWVyIiwiYXZhdGFyVXJsIjoiaHR0cHM6Ly9ncmF0aXNvZ3JhcGh5LmNvbS93cC1jb250ZW50L3VwbG9hZHMvMjAyNS8wMS9ncmF0aXNvZ3JhcGh5LWRvZy12YWNhdGlvbi04MDB4NTI1LmpwZyIsImV4cCI6MTc0NTY1NDEwOCwiaXNzIjoiSW50ZXJhY3RpdmVGbG9vciIsImF1ZCI6IkludGVyYWN0aXZlRmxvb3IifQ.7C_HOxGnjLW2en3sg8Cc0ymyi0G2SLpzvh-aSxHleUo";
        #endif

        Debug.Log($"[Startup Args] userId: {UserId}, floorId: {FloorId}, gameId: {GameId}, accessToken: {AccessToken}");
    }
}
