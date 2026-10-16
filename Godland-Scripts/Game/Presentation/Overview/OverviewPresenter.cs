using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Core.World.Timing;
using Godland.Game.Presentation.Cameras;
using Godland.Game.Writer.Commands;
using Godland.Game.World.Grid;
using Godland.Game.World.Tiles;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace Godland.Game.Presentation.Overview
{
    public sealed class OverviewPresenter : IStartable, IDisposable
    {
        private readonly IReadOnlyList<IWorldReport> _reports;
        private readonly OverviewView _view;
        private readonly WorldClock _clock;
        private readonly CameraController _camera;
        private readonly GridGeometry _geometry;
        private readonly TileSelection _selection;
        private readonly EntityCommands _commands;
        private readonly InputActionMap _map = new("Overview");
        private int _tab;
        private readonly HashSet<string> _expanded = new();
        private readonly List<History.ChronicleRowView> _tabs = new();

        public OverviewPresenter(
            IEnumerable<IWorldReport> reports, OverviewView view, WorldClock clock, CameraController camera, GridGeometry geometry,
            TileSelection selection, EntityCommands commands)
        {
            _commands = commands;
            _reports = reports.ToList();
            _view = view;
            _clock = clock;
            _camera = camera;
            _geometry = geometry;
            _selection = selection;
        }

        public void Start()
        {
            _view.Show(false);
            for (var i = 0; i < _reports.Count; i++)
            {
                var index = i;
                _tabs.Add(_view.AddTab(_reports[i].Title, () => Select(index)));
            }

            _clock.Ticked += OnTicked;
            _map.AddAction("Overview", InputActionType.Button, "<Keyboard>/o").performed += _ => Toggle();
            _map.Enable();
        }

        public void Dispose()
        {
            _clock.Ticked -= OnTicked;
            _map.Dispose();
        }

        private void Toggle()
        {
            _view.Show(!_view.IsShown);
            if (_view.IsShown) Refresh();
        }

        private void Select(int index)
        {
            _tab = index;
            Refresh();
        }

        private void OnTicked(int _)
        {
            if (_view.IsShown) Refresh();
        }

        private void Refresh()
        {
            _view.FitTabs();
            MarkTabs();
            var report = _reports[_tab];
            _view.ShowHeader($"{report.Title} · {_clock.Year} год");
            _view.ClearRows();
            foreach (var line in report.Lines())
            {
                Action focus = line.Place is { } place ? () => Focus(place) : null;
                _view.AddRow($"<b><color=#ffd27a>{line.Title}</color></b>\n<size=85%>{line.Text}</size>", focus);
                if (line.Commands.Count == 0) continue;

                var key = $"{report.Title}/{line.Title}";
                var open = _expanded.Contains(key);
                _view.AddCommand(open ? "<color=#ffd27a>−  Скрыть действия</color>" : $"<color=#ffd27a>+  Действия ({line.Commands.Count})</color>",
                    () => Expand(key));
                if (!open) continue;

                foreach (var command in line.Commands)
                {
                    var possible = _commands.CanIssue(command);
                    var label = _commands.Label(command);
                    _view.AddCommand(possible ? $"» {label}" : $"<color=#888888>» {label}</color>",
                        possible ? () => Issue(command) : null);
                }
            }
        }

        private void MarkTabs()
        {
            for (var i = 0; i < _reports.Count; i++)
            {
                var commands = _reports[i].Lines().SelectMany(line => line.Commands).ToList();
                var ready = commands.Count(_commands.CanIssue);
                var title = i == _tab ? $"<color=#ffd27a>{_reports[i].Title}</color>" : _reports[i].Title;
                var badge = ready > 0 ? $" <color=#7fe08a><b>({ready})</b></color>" : commands.Count > 0 ? $" <color=#8a8a8a>({commands.Count})</color>" : string.Empty;
                _tabs[i].SetText(title + badge);
            }
        }

        private void Expand(string key)
        {
            if (!_expanded.Remove(key)) _expanded.Add(key);
            Refresh();
        }

        private void Issue(EntityCommand command)
        {
            _commands.Issue(command);
            Refresh();
        }

        private void Focus(GridCoord place)
        {
            _camera.FocusOn(_geometry.ToWorld(place));
            _selection.Select(place);
        }
    }
}
