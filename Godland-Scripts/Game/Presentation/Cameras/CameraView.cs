using UnityEngine;

namespace Godland.Game.Presentation.Cameras
{
    public sealed class CameraView : MonoBehaviour
    {
        [SerializeField] private Camera _camera;

        public void Apply(CameraPose pose) => transform.SetPositionAndRotation(pose.Position, pose.Rotation);

        public Quaternion Rotation => transform.rotation;

        public Ray ScreenPointToRay(Vector2 screenPoint) => _camera.ScreenPointToRay(screenPoint);
    }
}
