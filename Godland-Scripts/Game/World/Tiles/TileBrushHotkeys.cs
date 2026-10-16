using System;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace Godland.Game.World.Tiles
{
    public sealed class TileBrushHotkeys : IInitializable, IDisposable
    {
        private readonly TileBrush _brush;
        private readonly InputActionMap _map = new("TileBrush");

        public TileBrushHotkeys(TileBrush brush)
        {
            _brush = brush;
        }

        public void Initialize()
        {
            _map.AddAction("Next", InputActionType.Button, "<Keyboard>/tab").performed += _ => _brush.Next();
            _map.Enable();
        }

        public void Dispose() => _map.Dispose();
    }
}
