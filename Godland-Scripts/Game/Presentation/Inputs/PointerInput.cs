using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace Godland.Game.Presentation.Inputs
{
    public sealed class PointerInput : IInitializable, IDisposable
    {
        private readonly InputActionMap _map = new("Pointer");
        private InputAction _position;
        private InputAction _primary;
        private InputAction _secondary;
        private InputAction _cancel;
        private bool _clickCancelled;

        public Vector2 Position => _position.ReadValue<Vector2>();

        public bool PrimaryClicked => _primary.WasReleasedThisFrame() && !_clickCancelled && !IsOverUi;

        public bool CancelPressed => _cancel.WasPressedThisFrame();

        public bool IsOverUi => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        public void Initialize()
        {
            _position = _map.AddAction("Position", InputActionType.PassThrough, "<Pointer>/position");
            _primary = _map.AddAction("Primary", InputActionType.Button, "<Mouse>/leftButton");
            _secondary = _map.AddAction("Secondary", InputActionType.Button, "<Mouse>/rightButton");
            _cancel = _map.AddAction("Cancel", InputActionType.Button, "<Keyboard>/escape");

            _primary.started += OnPrimaryStarted;
            _secondary.started += OnSecondaryStarted;
            _map.Enable();
        }

        public void Dispose()
        {
            _primary.started -= OnPrimaryStarted;
            _secondary.started -= OnSecondaryStarted;
            _map.Dispose();
        }

        private void OnPrimaryStarted(InputAction.CallbackContext _) => _clickCancelled = _secondary.IsPressed();

        private void OnSecondaryStarted(InputAction.CallbackContext _)
        {
            if (_primary.IsPressed()) _clickCancelled = true;
        }
    }
}
