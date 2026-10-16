using System;
using System.Collections.Generic;
using Godland.Game.Presentation.Cameras;
using Godland.Game.World.Grid;
using Godland.Game.World.Tiles;
using Godland.Game.Presentation.Timing;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace Godland.Game.Presentation.Inspection
{
    public sealed class InspectionPresenter : IStartable, IDisposable
    {
        private readonly TileInspection _inspection;
        private readonly TileSelection _selection;
        private readonly TimeFlow _time;
        private readonly CameraController _camera;
        private readonly GridGeometry _geometry;
        private readonly GridView _grid;
        private readonly TilesView _tiles;
        private readonly InspectToggleView _view;
        private readonly InputActionMap _map = new("Inspection");
        private readonly List<GameObject> _hidden = new();
        private readonly CameraConfig _cameraConfig;
        private CameraPose _returnPose;

        public InspectionPresenter(
            TileInspection inspection, TileSelection selection, TimeFlow time, CameraController camera,
            GridGeometry geometry, GridView grid, TilesView tiles, InspectToggleView view, CameraConfig cameraConfig)
        {
            _cameraConfig = cameraConfig;
            _inspection = inspection;
            _selection = selection;
            _time = time;
            _camera = camera;
            _geometry = geometry;
            _grid = grid;
            _tiles = tiles;
            _view = view;
        }

        public void Start()
        {
            _view.Bind(Toggle);
            _view.Show(false);
            _map.AddAction("Inspect", InputActionType.Button, "<Keyboard>/f").performed += _ => Toggle();
            _map.Enable();
            _inspection.Changed += OnChanged;
            _selection.Changed += OnSelectionChanged;
        }

        public void Dispose()
        {
            _inspection.Changed -= OnChanged;
            _selection.Changed -= OnSelectionChanged;
            _map.Dispose();
        }

        private void Toggle()
        {
            if (_inspection.IsActive) _inspection.Exit();
            else if (_selection.Selected is { } coord) _inspection.Enter(coord);
        }

        private void OnSelectionChanged()
        {
            if (_inspection.Target != _selection.Selected) _inspection.Exit();
        }

        private void OnChanged()
        {
            _view.Show(_inspection.IsActive);
            Reveal();

            if (_inspection.Target is not { } target)
            {
                _time.Unblock();
                _camera.ResetCloseUp();
                _camera.MoveTo(_returnPose);
                return;
            }

            _time.Block();
            _returnPose = _camera.Target;
            var center = _geometry.ToWorld(target);
            _camera.AllowCloseUp(_cameraConfig.InspectMinDistance);
            _camera.FocusOn(center, _cameraConfig.InspectDistance);
            Isolate(center);
        }

        private void Isolate(Vector3 center)
        {
            var radius = _geometry.CellSize * 0.5f;
            foreach (Transform child in _tiles.Root)
            {
                var offset = child.position - center;
                offset.y = 0f;
                if (!child.gameObject.activeSelf || offset.magnitude <= radius) continue;

                child.gameObject.SetActive(false);
                _hidden.Add(child.gameObject);
            }

            _grid.gameObject.SetActive(false);
        }

        private void Reveal()
        {
            foreach (var hidden in _hidden)
                if (hidden != null) hidden.SetActive(true);
            _hidden.Clear();
            _grid.gameObject.SetActive(true);
        }
    }
}
