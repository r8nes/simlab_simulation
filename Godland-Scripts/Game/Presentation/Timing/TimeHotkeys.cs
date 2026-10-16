using System;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace Godland.Game.Presentation.Timing
{
    public sealed class TimeHotkeys : IInitializable, IDisposable
    {
        private static readonly string[] SpeedKeys = { "<Keyboard>/1", "<Keyboard>/2", "<Keyboard>/3" };

        private readonly TimeFlow _flow;
        private readonly InputActionMap _map = new("Time");

        public TimeHotkeys(TimeFlow flow)
        {
            _flow = flow;
        }

        public void Initialize()
        {
            _map.AddAction("Pause", InputActionType.Button, "<Keyboard>/space").performed += _ => _flow.TogglePause();

            for (var i = 0; i < SpeedKeys.Length; i++)
            {
                var speedIndex = i;
                _map.AddAction($"Speed{i + 1}", InputActionType.Button, SpeedKeys[i]).performed += _ => _flow.SetSpeed(speedIndex);
            }

            _map.Enable();
        }

        public void Dispose() => _map.Dispose();
    }
}
