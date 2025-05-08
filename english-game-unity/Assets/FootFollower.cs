using TMPro;
using UnityEngine;

public class FootFollower : MonoBehaviour
{
    public AstraInputController inputController;

    private float gameMinX = -8f;
    private float gameMaxX = 8f;
    private float gameMinY = -4.5f;
    private float gameMaxY = 4.5f;

    private float realMinX;
    private float realMaxX;
    private float realMinY;
    private float realMaxY;

    private Vector3 velocity = Vector3.zero;
    private Vector3? _latestFootPos = null;

    private Vector3 previousTarget = Vector3.zero;

    private const float smoothTime = 0.1f;

    private bool hasNewData = false;

    private void Start()
    {
        var floorWidth = CommandLineReader.FloorWidth.ToString();
        floorWidth = floorWidth.Replace(",", ".");
        var floorLength = CommandLineReader.FloorLength.ToString();
        floorLength = floorLength.Replace(",", ".");
        var distance = CommandLineReader.CameraToFloor.ToString();
        distance = distance.Replace(",", ".");


        realMinX = floorWidth != null ? -float.Parse(floorWidth) / 2 : -0.8f;
        realMaxX = floorWidth != null ? float.Parse(floorWidth) / 2 : 0.8f;
        if (distance != null || floorLength != null)
        {
            realMinY = float.Parse(distance) + float.Parse(floorLength);
        }
        else
        {
            realMinY = 3f;
        }

        realMaxY = distance != null ? float.Parse(distance) : 1.8f;

        transform.position = Vector3.zero;
        previousTarget = transform.position;

        if (inputController != null)
        {
            inputController.onDetectBody += OnFootDetected;
        }
    }

    private void OnDestroy()
    {
        inputController.onDetectBody -= OnFootDetected;
    }

    private void OnFootDetected(bool isDetected, Vector3 footPosition)
    {
        if (!isDetected)
        {
            _latestFootPos = null;
        }
        else
        {
            _latestFootPos = footPosition;
            hasNewData = true;
        }
    }

    private void Update()
    {
        if (_latestFootPos.HasValue)
        {
            Vector3 handPosition = _latestFootPos.Value;

            float mappedX = MapValue(handPosition.x, realMinX, realMaxX, gameMinX, gameMaxX);
            float mappedY = MapValue(handPosition.z, realMinY, realMaxY, gameMinY, gameMaxY);

            Vector3 target = new Vector3(mappedX, mappedY, transform.position.z);

            transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, smoothTime);
            previousTarget = target;
            hasNewData = false;
        }
    }

    private float MapValue(float value, float inMin, float inMax, float outMin, float outMax)
    {
        float t = Mathf.InverseLerp(inMin, inMax, value);
        return Mathf.Lerp(outMin, outMax, t);
    }
}
