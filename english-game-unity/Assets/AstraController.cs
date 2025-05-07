using Astra;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;
using Assets;
using TMPro;


[System.Serializable]
public class NewBodyFrameEvent : UnityEvent<BodyStream, BodyFrame> { }


public class AstraController : MonoBehaviour
{
    public bool AutoRequestAndroidUsbPermission = true;
    public TextMeshProUGUI message;

    private Astra.StreamSet _streamSet;
    private Astra.StreamReader _readerDepth;
    private Astra.StreamReader _readerBody;

    private DepthStream _depthStream;
    private BodyStream _bodyStream;

    bool _isDepthOn = false;
    bool _isBodyOn = false;

    private long _lastBodyFrameIndex = -1;
    private long _lastDepthFrameIndex = -1;

    private int _frameCount = 0;
    private bool _areStreamsInitialized = false;

    private TimerHistory updateFramesTime = new TimerHistory();
    private TimerHistory astraUpdateTime = new TimerHistory();
    private TimerHistory totalFrameTime = new TimerHistory();

    public Text TimeText = null;
    public Toggle ToggleDebugText = null;
    private bool debugTextEnabled = false;

    public NewBodyFrameEvent NewBodyFrameEvent = new NewBodyFrameEvent();

    public Toggle ToggleDepth = null;
    public Toggle ToggleColor = null;
    public Toggle ToggleNV21Color = null;
    public Toggle ToggleBody = null;
    public Toggle ToggleMaskedColor = null;
    public Toggle ToggleColorizedBody = null;

    private void Awake()
    {
        Debug.Log("AstraUnityContext.Awake");
        AstraUnityContext.Instance.Initializing += OnAstraInitializing;
        AstraUnityContext.Instance.Terminating += OnAstraTerminating;
        AstraUnityContext.Instance.Initialize();
    }

    void Start()
    {
        if (TimeText != null)
        {
            TimeText.text = "";
        }

        if (ToggleDebugText != null)
        {
            debugTextEnabled = ToggleDebugText.isOn;
        }
    }

    private void OnAstraInitializing(object sender, AstraInitializingEventArgs e)
    {
        Debug.Log("AstraController is initializing");
        InitializeStreams();
    }

    public void InitializeStreams()
    {
        try
        {
            AstraUnityContext.Instance.WaitForUpdate(AstraBackgroundUpdater.WaitIndefinitely);

            _streamSet = Astra.StreamSet.Open();

            _readerDepth = _streamSet.CreateReader();
            _readerBody = _streamSet.CreateReader();

            _depthStream = _readerDepth.GetStream<DepthStream>();

            var depthModes = _depthStream.AvailableModes;
            ImageMode selectedDepthMode = depthModes[0];

            int targetDepthWidth, targetDepthHeight, targetDepthFps;
            if (_depthStream.usbInfo.Pid == 0x60b ||
                _depthStream.usbInfo.Pid == 0x60e ||
                _depthStream.usbInfo.Pid == 0x608 ||
                _depthStream.usbInfo.Pid == 0x617)
            {
                targetDepthWidth = 640;
                targetDepthHeight = 400;
                targetDepthFps = 30;
            }
            else
            {
                targetDepthWidth = 320;
                targetDepthHeight = 240;
                targetDepthFps = 30;
            }
            foreach (var m in depthModes)
            {
                if (m.Width == targetDepthWidth &&
                    m.Height == targetDepthHeight &&
                    m.FramesPerSecond == targetDepthFps)
                {
                    selectedDepthMode = m;
                    break;
                }
            }

            _depthStream.SetMode(selectedDepthMode);

            _bodyStream = _readerBody.GetStream<BodyStream>();
            _areStreamsInitialized = true;
        }
        catch (AstraException e)
        {
            //Debug.Log("AstraController: Couldn't initialize streams: " + e.ToString());
            message.text = "Chưa kết nối thiết bị camera.";
            UninitializeStreams();
        }
    }

    private void OnAstraTerminating(object sender, AstraTerminatingEventArgs e)
    {
        //Debug.Log("AstraController is tearing down");
        UninitializeStreams();
    }

    private void UninitializeStreams()
    {
        AstraUnityContext.Instance.WaitForUpdate(AstraBackgroundUpdater.WaitIndefinitely);

        //Debug.Log("AstraController: Uninitializing streams");
        if (_readerDepth != null)
        {
            _readerDepth.Dispose();
            _readerBody.Dispose();
            _readerDepth = null;
            _readerBody = null;
        }

        if (_streamSet != null)
        {
            _streamSet.Dispose();
            _streamSet = null;
        }
    }

    private void CheckDepthReader()
    {
        ReaderFrame frame;
        if (_readerDepth.TryOpenFrame(0, out frame))
        {
            using (frame)
            {
                DepthFrame depthFrame = frame.GetFrame<DepthFrame>();

                if (depthFrame != null)
                {
                    if (_lastDepthFrameIndex != depthFrame.FrameIndex)
                    {
                        _lastDepthFrameIndex = depthFrame.FrameIndex;
                    }
                }
            }
        }
    }

    private void CheckBodyReader()
    {
        ReaderFrame frame;
        if (_readerBody.TryOpenFrame(0, out frame))
        {
            using (frame)
            {
                BodyFrame bodyFrame = frame.GetFrame<BodyFrame>();

                if (bodyFrame != null)
                {
                    if (_lastBodyFrameIndex != bodyFrame.FrameIndex)
                    {
                        _lastBodyFrameIndex = bodyFrame.FrameIndex;

                        NewBodyFrameEvent.Invoke(_bodyStream, bodyFrame);
                    }
                }
            }
        }
    }

    private bool UpdateUntilDelegate()
    {
        return true;
    }

    private void CheckForNewFrames()
    {
        if (AstraUnityContext.Instance.WaitForUpdate(5) && AstraUnityContext.Instance.IsUpdateAsyncComplete)
        {
            updateFramesTime.Start();

            CheckDepthReader();
            CheckBodyReader();

            _frameCount++;

            updateFramesTime.Stop();
        }

        if (!AstraUnityContext.Instance.IsUpdateRequested)
        {
            UpdateStreamStartStop();
            AstraUnityContext.Instance.UpdateAsync(UpdateUntilDelegate);
        }
    }

    private void UpdateStreamStartStop()
    {
        _isDepthOn = ToggleDepth == null || ToggleDepth.isOn;
        _isBodyOn = ToggleBody == null || ToggleBody.isOn;

        if (_isDepthOn)
        {
            _depthStream.Start();
        }
        else
        {
            _depthStream.Stop();
        }

        if (_isBodyOn)
        {
            _bodyStream.Start();
        }
        else
        {
            _bodyStream.Stop();
        }
    }

    private void Update()
    {
        if (!_areStreamsInitialized)
        {
            InitializeStreams();
        }

        totalFrameTime.Stop();
        totalFrameTime.Start();

        if (_areStreamsInitialized)
        {
            CheckForNewFrames();
        }

        astraUpdateTime.Start();
        astraUpdateTime.Stop();

        if (ToggleDebugText != null)
        {
            bool newDebugTextEnabled = ToggleDebugText.isOn;

            if (debugTextEnabled && !newDebugTextEnabled)
            {
                TimeText.text = "";
            }

            debugTextEnabled = newDebugTextEnabled;
        }

        if (TimeText != null && debugTextEnabled)
        {
            BackgroundUpdaterTimings backgroundTimings = AstraUnityContext.Instance.BackgroundTimings;
            float totalFrameMs = totalFrameTime.AverageMilliseconds;
            float astraUpdateMs = backgroundTimings.updateAvgMillis;
            float lockWaitMs = backgroundTimings.lockWaitAvgMillis;
            float updateUntilMs = backgroundTimings.updateUntilAvgMillis;
            float updateFrameMs = updateFramesTime.AverageMilliseconds;
            TimeText.text = "Tot: " + totalFrameMs.ToString("0.0") + " ms\n" +
                            "AU: " + astraUpdateMs.ToString("0.0") + " ms\n" +
                            "LockWait: " + lockWaitMs.ToString("0.0") + " ms\n" +
                            "UpdateUntil: " + updateUntilMs.ToString("0.0") + " ms\n" +
                            "UpdateFr: " + updateFrameMs.ToString("0.0") + " ms\n";
        }
    }

    void OnDestroy()
    {
        Debug.Log("AstraController.OnDestroy");

        AstraUnityContext.Instance.WaitForUpdate(AstraBackgroundUpdater.WaitIndefinitely);

        if (_depthStream != null)
        {
            _depthStream.Stop();
        }

        if (_bodyStream != null)
        {
            _bodyStream.Stop();
        }

        UninitializeStreams();

        AstraUnityContext.Instance.Initializing -= OnAstraInitializing;
        AstraUnityContext.Instance.Terminating -= OnAstraTerminating;

        AstraUnityContext.Instance.Terminate();
    }

    private void OnApplicationQuit()
    {
        Debug.Log("AstraController handling OnApplicationQuit");
        AstraUnityContext.Instance.Terminate();
    }
}