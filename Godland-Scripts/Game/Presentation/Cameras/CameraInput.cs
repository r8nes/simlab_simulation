using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace Godland.Game.Presentation.Cameras
{
    public sealed class CameraInput : IInitializable, IDisposable
    {
        private readonly InputActionMap _map = new("Camera");
        private InputAction _keyPan;
        private InputAction _keyRotation;
        private InputAction _pointerDelta;
        private InputAction _leftHold;
        private InputAction _rightHold;
        private InputAction _middleHold;
        private InputAction _zoom;
        private readonly Godland.Game.Presentation.Inputs.PointerInput _pointer;

        public CameraInput(Godland.Game.Presentation.Inputs.PointerInput pointer) => _pointer = pointer;

        public void Initialize()
        {
            _keyPan = _map.AddAction("KeyPan", InputActionType.Value);
            _keyPan.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            _keyPan.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");

            _keyRotation = _map.AddAction("KeyRotation", InputActionType.Value);
            _keyRotation.AddCompositeBinding("1DAxis")
                .With("Negative", "<Keyboard>/q")
                .With("Positive", "<Keyboard>/e");

            _pointerDelta = _map.AddAction("PointerDelta", InputActionType.PassThrough, "<Pointer>/delta");
            _leftHold = _map.AddAction("LeftHold", InputActionType.Button, "<Mouse>/leftButton");
            _rightHold = _map.AddAction("RightHold", InputActionType.Button, "<Mouse>/rightButton");
            _middleHold = _map.AddAction("MiddleHold", InputActionType.Button, "<Mouse>/middleButton");
            _zoom = _map.AddAction("Zoom", InputActionType.PassThrough, "<Mouse>/scroll/y");

            _map.Enable();
        }

        public CameraInputFrame Read()
        {
            var delta = _pointerDelta.ReadValue<Vector2>();
            var scroll = _pointer.IsOverUi ? 0f : _zoom.ReadValue<float>();
            var right = _rightHold.IsPressed();
            var rotating = (right && _leftHold.IsPressed()) || _middleHold.IsPressed();
            var panning = right && !rotating;
            return new CameraInputFrame(
                _keyPan.ReadValue<Vector2>(),
                panning ? delta : Vector2.zero,
                _keyRotation.ReadValue<float>(),
                rotating ? delta : Vector2.zero,
                scroll == 0f ? 0f : Mathf.Sign(scroll));
        }

        public void Dispose() => _map.Dispose();
    }
}
