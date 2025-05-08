using System;
using UnityEngine;

public class CommandLineReader : MonoBehaviour
{
    public static string UserId { get; private set; }
    public static string FloorId { get; private set; }
    public static string GameId { get; private set; }
    public static string AccessToken { get; private set; }
    public static float FloorWidth { get; set; }
    public static float FloorLength { get; set; }
    public static float CameraToFloor { get; set; }


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

            if (arg.StartsWith("--floorWidth=") && float.TryParse(arg.Substring("--floorWidth=".Length), out float fw))
                FloorWidth = fw;

            if (arg.StartsWith("--floorLength=") && float.TryParse(arg.Substring("--floorLength=".Length), out float fl))
                FloorLength = fl;

            if (arg.StartsWith("--cameraToFloor=") && float.TryParse(arg.Substring("--cameraToFloor=".Length), out float cf))
                CameraToFloor = cf;

        }
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(UserId)) UserId = "84aee218-f13c-4596-ad45-4d843a96e101";
                if (string.IsNullOrEmpty(FloorId)) FloorId = "779dd5d3-ddf3-49c6-8e9f-9c90003f9148";
                if (string.IsNullOrEmpty(GameId)) GameId = "29b4ba7e1d174bec999f867f2fd930b3    ";
                if (string.IsNullOrEmpty(AccessToken)) AccessToken = "";
                if (FloorWidth == 0f) FloorWidth = 3.0f;
                if (FloorLength == 0f) FloorLength = 1.0f;
                if (CameraToFloor == 0f) CameraToFloor = 2.0f;
#endif

        Debug.Log($"[Startup Args] userId: {UserId}, floorId: {FloorId}, gameId: {GameId}, accessToken: {AccessToken}");
    }
}
