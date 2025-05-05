using UnityEngine.Events;
using UnityEngine;
using Astra;

public class AstraInputController : MonoBehaviour
{
    public System.Action<bool, Vector3> onDetectBody;
    public UnityEvent OnClickEvent = new UnityEvent();

    private Astra.Body[] _bodies = new Astra.Body[Astra.BodyFrame.MaxBodies];
    private Vector3 currentFootPos;

    private float minMoveThreshold = 0.1f;
    private float maxMoveThreshold = 0.5f;

    private Vector3 lastFootPos = Vector3.zero;

    private Vector3 smoothedFootPos = Vector3.zero;
    private bool isFirstSmooth = true;

    private float lastClickTime = 0f;
    private float clickCooldown = 0.5f;

    public void OnNewFrame(Astra.BodyStream bodyStream, Astra.BodyFrame frame)
    {
        if (frame.Width == 0 || frame.Height == 0) return;

        frame.CopyBodyData(ref _bodies);

        if (_bodies != null && _bodies.Length > 0 && _bodies[0] != null && _bodies[0].Joints != null)
        {
            var jointFoot = _bodies[0].Joints[(int)JointType.RightFoot];
            var jointHip = _bodies[0].Joints[(int)JointType.RightHip];
            var jointKnee = _bodies[0].Joints[(int)JointType.RightKnee];

            Vector3 posFoot = GetJointWorldPos(jointFoot);
            Vector3 posKnee = GetJointWorldPos(jointKnee);
            Vector3 posHip = GetJointWorldPos(jointHip);

            Vector3 footWorldPos = 0.6f * posFoot + 0.25f * posKnee + 0.15f * posHip;

            float alpha = 0.5f;

            if (isFirstSmooth)
            {
                smoothedFootPos = footWorldPos;
                lastFootPos = footWorldPos;
                isFirstSmooth = false;
                return;
            }

            Vector3 delta = footWorldPos - lastFootPos;
            if (delta.magnitude > maxMoveThreshold)
            {
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
            onDetectBody?.Invoke(false, Vector3.zero);
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
}