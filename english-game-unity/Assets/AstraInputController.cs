using UnityEngine.Events;
using UnityEngine;
using Astra;
using TMPro;
using System.Collections;

public class AstraInputController : MonoBehaviour
{
    public System.Action<bool, Vector3> onDetectBody;
    public TextMeshProUGUI message;
    public UnityEvent OnClickEvent = new UnityEvent();

    private Astra.Body[] _bodies = new Astra.Body[Astra.BodyFrame.MaxBodies];
    private Vector3 currentFootPos;

    private float minMoveThreshold = 0.1f;
    private float maxMoveThreshold = 0.6f;

    private Vector3 lastFootPos = Vector3.zero;

    private Vector3 smoothedFootPos = Vector3.zero;
    private bool isFirstSmooth = true;
    private Coroutine messageCoroutine = null;

    private float lastClickTime = 0f;
    private float clickCooldown = 0.5f;

    public void OnNewFrame(Astra.BodyStream bodyStream, Astra.BodyFrame frame)
    {
        if (frame.Width == 0 || frame.Height == 0) return;

        frame.CopyBodyData(ref _bodies);
        int trackedCount = 0;
        foreach (var body in _bodies)
        {
            if (body != null && body.Joints != null)
            {
                var rightFoot = body.Joints[(int)JointType.RightFoot];
                var rightKnee = body.Joints[(int)JointType.RightKnee];
                var rightHip = body.Joints[(int)JointType.RightHip];

                if (rightFoot.Status == JointStatus.Tracked &&
                    rightKnee.Status == JointStatus.Tracked &&
                    rightHip.Status == JointStatus.Tracked)
                {
                    trackedCount++;
                }
            }
        }

        if (trackedCount == 0)
        {
            //isFirstSmooth = true;
            Debug.Log("Không tìm thấy người chơi");
            ShowTemporaryMessage("Không tìm thấy người chơi.");
        }


        if (trackedCount > 1)
        {
            ShowTemporaryMessage("Có nhiều hơn 1 người chơi trong sàn.");
            return;
        }

        if (_bodies != null && _bodies.Length > 0 && _bodies[0] != null && _bodies[0].Joints != null)
        {
            var jointFoot = _bodies[0].Joints[(int)JointType.RightFoot];
            var jointHip = _bodies[0].Joints[(int)JointType.RightHip];
            var jointKnee = _bodies[0].Joints[(int)JointType.RightKnee];

            if (jointFoot.Status != JointStatus.Tracked &&
                jointHip.Status != JointStatus.Tracked &&
                jointKnee.Status != JointStatus.Tracked)
            {
                ShowTemporaryMessage("Mất theo dõi chuyển động.");
            }
            else
            {
                ShowTemporaryMessage("Đọc chuyển động thành công.");
            }

            Vector3 posFoot = GetJointWorldPos(jointFoot);
            Vector3 posKnee = GetJointWorldPos(jointKnee);
            Vector3 posHip = GetJointWorldPos(jointHip);

            Vector3 footWorldPos = 0.7f * posFoot + 0.2f * posKnee + 0.1f * posHip;

            float alpha = 0.5f;

            if (isFirstSmooth)
            {
                smoothedFootPos = footWorldPos;
                lastFootPos = footWorldPos;
                isFirstSmooth = false;
                ShowTemporaryMessage("Tìm thấy người chơi. Trò chơi bắt đầu.");
                return;
            }

            Vector3 delta = footWorldPos - lastFootPos;
            if (delta.magnitude > maxMoveThreshold)
            {
                Debug.Log("too far");
                return;
            }

            smoothedFootPos = alpha * footWorldPos + (1f - alpha) * smoothedFootPos;


            if (delta.magnitude > minMoveThreshold)
            {
                lastFootPos = footWorldPos;
                currentFootPos = smoothedFootPos;
                onDetectBody?.Invoke(true, currentFootPos);
                Debug.Log($"foot pos: {footWorldPos}");


                if (Time.time - lastClickTime >= clickCooldown)
                {
                    lastClickTime = Time.time;
                    OnClickEvent.Invoke();
                }
            }
        }
        else
        {
            return;
        }
    }


    private Vector3 GetJointWorldPos(Astra.Joint joint)
    {
        return new Vector3(
            joint.WorldPosition.X / 1000f,
            joint.WorldPosition.Y / 1000f,
            joint.WorldPosition.Z / 1000f
        );
    }

    private void ShowTemporaryMessage(string text, float duration = 2f)
    {
        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);

        messageCoroutine = StartCoroutine(ShowMessageCoroutine(text, duration));
    }

    private IEnumerator ShowMessageCoroutine(string text, float duration)
    {
        message.gameObject.SetActive(true);
        message.text = text;
        yield return new WaitForSeconds(duration);
        message.gameObject.SetActive(false);
        messageCoroutine = null;
    }
}