using UnityEngine;
using VContainer.Unity;

namespace Godland.Game.Presentation.Cameras
{
    public sealed class CameraController : IStartable, ITickable
    {
        private readonly CameraConfig _config;
        private readonly CameraInput _input;
        private readonly CameraView _view;
        private readonly ICameraBounds _bounds;

        private CameraPose _target;
        private CameraPose _current;
        private float? _minDistance;

        public CameraController(CameraConfig config, CameraInput input, CameraView view, ICameraBounds bounds)
        {
            _config = config;
            _input = input;
            _view = view;
            _bounds = bounds;
        }

        public void Start()
        {
            _target = Clamp(new CameraPose(_config.StartFocus, _config.StartYaw, _config.StartPitch, _config.StartDistance));
            _current = _target;
            _view.Apply(_current);
        }

        public CameraPose Target => _target;

        public void FocusOn(Vector3 point) => FocusOn(point, _target.Distance);

        public void FocusOn(Vector3 point, float distance) =>
            _target = Clamp(new CameraPose(new Vector3(point.x, _target.Focus.y, point.z), _target.Yaw, _target.Pitch, distance));

        public void MoveTo(CameraPose pose) => _target = Clamp(pose);

        public void AllowCloseUp(float minDistance) => _minDistance = minDistance;

        public void ResetCloseUp() => _minDistance = null;

        public void Tick()
        {
            var deltaTime = Time.unscaledDeltaTime;
            var input = _input.Read();

            _target = Clamp(Zoom(Rotate(Pan(_target, input, deltaTime), input, deltaTime), input));
            _current = CameraPose.Lerp(_current, _target, 1f - Mathf.Exp(-_config.Sharpness * deltaTime));
            _view.Apply(_current);
        }

        private CameraPose Pan(CameraPose pose, CameraInputFrame input, float deltaTime)
        {
            var planar = input.KeyPan * (_config.KeyPanSpeed * deltaTime) - input.DragPan * _config.DragPanSensitivity;
            var offset = pose.Heading * new Vector3(planar.x, 0f, planar.y) * pose.Distance;
            return new CameraPose(pose.Focus + offset, pose.Yaw, pose.Pitch, pose.Distance);
        }

        private CameraPose Rotate(CameraPose pose, CameraInputFrame input, float deltaTime)
        {
            var yaw = pose.Yaw
                      + input.KeyRotation * _config.KeyRotationSpeed * deltaTime
                      + input.DragRotation.x * _config.DragRotationSensitivity;
            var pitch = pose.Pitch - input.DragRotation.y * _config.DragRotationSensitivity;
            return new CameraPose(pose.Focus, yaw, pitch, pose.Distance);
        }

        private CameraPose Zoom(CameraPose pose, CameraInputFrame input)
        {
            var distance = pose.Distance * (1f - input.Zoom * _config.ZoomStep);
            return new CameraPose(pose.Focus, pose.Yaw, pose.Pitch, distance);
        }

        private CameraPose Clamp(CameraPose pose)
        {
            var bounds = _bounds.FocusBounds;
            var focus = new Vector3(
                Mathf.Clamp(pose.Focus.x, bounds.xMin, bounds.xMax),
                pose.Focus.y,
                Mathf.Clamp(pose.Focus.z, bounds.yMin, bounds.yMax));
            var pitch = Mathf.Clamp(pose.Pitch, _config.PitchRange.x, _config.PitchRange.y);
            var distance = Mathf.Clamp(pose.Distance, _minDistance ?? _config.DistanceRange.x, _config.DistanceRange.y);
            return new CameraPose(focus, pose.Yaw, pitch, distance);
        }
    }
}
