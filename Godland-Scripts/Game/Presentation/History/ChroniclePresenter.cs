using System;
using Godland.Game.Presentation.Cameras;
using Godland.Game.World.Grid;
using Godland.Game.World.Tiles;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace Godland.Game.Presentation.History
{
    public sealed class ChroniclePresenter : IStartable, IDisposable
    {
        private readonly Chronicle _chronicle;
        private readonly ChronicleView _view;
        private readonly CameraController _camera;
        private readonly GridGeometry _geometry;
        private readonly TileSelection _selection;
        private readonly InputActionMap _map = new("Chronicle");

        public ChroniclePresenter(Chronicle chronicle, ChronicleView view, CameraController camera, GridGeometry geometry, TileSelection selection)
        {
            _chronicle = chronicle;
            _view = view;
            _camera = camera;
            _geometry = geometry;
            _selection = selection;
        }

        public void Start()
        {
            _view.Show(false);
            foreach (var entry in _chronicle.Entries) Add(entry);
            _chronicle.Added += Add;
            _map.AddAction("Chronicle", InputActionType.Button, "<Keyboard>/h").performed += _ => _view.Show(!_view.IsShown);
            _map.Enable();
        }

        public void Dispose()
        {
            _chronicle.Added -= Add;
            _map.Dispose();
        }

        private void Add(ChronicleEntry entry)
        {
            Action focus = entry.Place is { } place ? () => Focus(place) : null;
            _view.Add($"<color=#ffd27a>{entry.Year}</color>  {entry.Text}", focus);
        }

        private void Focus(Godland.Core.World.Grid.GridCoord place)
        {
            _camera.FocusOn(_geometry.ToWorld(place));
            _selection.Select(place);
        }
    }
}
