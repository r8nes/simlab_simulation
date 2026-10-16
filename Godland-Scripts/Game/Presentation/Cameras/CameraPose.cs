using UnityEngine;

namespace Godland.Game.Presentation.Cameras
{
    public readonly struct CameraPose
    {
        public readonly Vector3 Focus;
        public readonly float Yaw;
        public readonly float Pitch;
        public readonly float Distance;

        public CameraPose(Vector3 focus, float yaw, float pitch, float distance)
        {
            Focus = focus;
            Yaw = yaw;
            Pitch = pitch;
            Distance = distance;
        }

        public Quaternion Rotation => Quaternion.Euler(Pitch, Yaw, 0f);
        public Vector3 Position => Focus - Rotation * Vector3.forward * Distance;
        public Quaternion Heading => Quaternion.Euler(0f, Yaw, 0f);

        public static CameraPose Lerp(CameraPose from, CameraPose to, float t) =>
            new(
                Vector3.Lerp(from.Focus, to.Focus, t),
                Mathf.Lerp(from.Yaw, to.Yaw, t),
                Mathf.Lerp(from.Pitch, to.Pitch, t),
                Mathf.Lerp(from.Distance, to.Distance, t));
    }
}
