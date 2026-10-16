using System;
using System.Collections.Generic;
using Godland.Core.World.Grid;
using Godland.Core.Life.Kingdoms;
using Godland.Core.World.Timing;
using Godland.Game.World.Grid;
using UnityEngine;
using VContainer.Unity;

namespace Godland.Game.Life.Kingdoms
{
    public sealed class KingdomBordersPresenter : IStartable, IDisposable
    {
        private readonly Realms _realms;
        private readonly KingdomConfig _config;
        private readonly GridView _grid;
        private readonly WorldClock _clock;
        private readonly Dictionary<GridCoord, Color> _drawn = new();

        public KingdomBordersPresenter(Realms realms, KingdomConfig config, GridView grid, WorldClock clock)
        {
            _realms = realms;
            _config = config;
            _grid = grid;
            _clock = clock;
        }

        public void Start() => _clock.Ticked += OnTicked;

        public void Dispose() => _clock.Ticked -= OnTicked;

        private void OnTicked(int _)
        {
            var current = new Dictionary<GridCoord, Color>();
            foreach (var kingdom in _realms.All)
            foreach (var member in kingdom.Members)
                current[member] = _config.ColorOf(kingdom.Index);

            foreach (var (coord, _) in _drawn)
                if (!current.ContainsKey(coord))
                    _grid.SetRealm(coord, null);

            foreach (var (coord, color) in current)
                if (!_drawn.TryGetValue(coord, out var old) || old != color)
                    _grid.SetRealm(coord, color);

            _drawn.Clear();
            foreach (var pair in current) _drawn[pair.Key] = pair.Value;
        }
    }
}
