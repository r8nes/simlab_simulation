using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.Life.Features;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Core.Randomness;
using Godland.Core.World.Tiles;
using Godland.Core.World.Volcanoes;

namespace Godland.Core.World.Peaks
{
    public sealed class SuperPeaks
    {
        private readonly WorldGrid _grid;
        private readonly WorldTiles _tiles;
        private readonly Settlements _settlements;
        private readonly TileFeatures _features;
        private readonly VolcanoActivity _volcanoes;
        private readonly IPeakRules _rules;
        private readonly SeededRandom _random;
        private readonly HashSet<GridCoord> _peaks = new();
        private bool _raised;

        public SuperPeaks(
            WorldGrid grid, WorldTiles tiles, Settlements settlements, TileFeatures features, VolcanoActivity volcanoes, IPeakRules rules,
            SeededRandom random)
        {
            _grid = grid;
            _tiles = tiles;
            _settlements = settlements;
            _features = features;
            _volcanoes = volcanoes;
            _rules = rules;
            _random = random;
            _tiles.Changed += OnChanged;
            _tiles.SetUncounted(IsPeak);
        }

        public event Action<IReadOnlyList<GridCoord>> Raised;
        public event Action<GridCoord, GridCoord> Displaced;
        public event Action<GridCoord, string> Withstood;

        public IReadOnlyCollection<GridCoord> All => _peaks;

        public bool IsPeak(GridCoord cell) => _peaks.Contains(cell);

        public bool HasRisen => _raised;

        public IReadOnlyList<GridCoord> RaiseRidge(GridCoord start, int size, string cause)
        {
            if (_raised) return Array.Empty<GridCoord>();

            var ridge = Shape(start, size);
            _raised = ridge.Count > 0;
            foreach (var cell in ridge)
            {
                Displace(cell, cause);
                _grid.Unlock(cell);
                _peaks.Add(cell);
                if (_features.TryGet(cell, out _)) _features.Remove(cell);
                _tiles.Place(cell, _rules.PeakTileId, cause);
            }

            if (ridge.Count > 0) Raised?.Invoke(ridge);
            return ridge;
        }

        private List<GridCoord> Shape(GridCoord start, int size)
        {
            var ridge = new List<GridCoord>();
            if (!CanRise(start)) start = _grid.Coords.Where(CanRise).OrderBy(cell => Distance(cell, start)).ThenBy(cell => cell.X).ThenBy(cell => cell.Y)
                .Cast<GridCoord?>().FirstOrDefault() ?? start;
            if (!CanRise(start)) return ridge;

            ridge.Add(start);
            while (ridge.Count < size)
            {
                var options = ridge.SelectMany(_grid.Neighbors).Distinct().Where(cell => !ridge.Contains(cell) && CanRise(cell))
                    .OrderBy(cell => cell.X).ThenBy(cell => cell.Y).ToList();
                if (options.Count == 0) break;

                ridge.Add(options[_random.Next(options.Count)]);
            }

            return ridge;
        }

        private bool CanRise(GridCoord cell) =>
            _grid.Contains(cell) && !_peaks.Contains(cell) && Distance(cell, _grid.Center) > 1;

        private void Displace(GridCoord cell, string cause)
        {
            var outside = _grid.Outside.OrderBy(candidate => Distance(candidate, cell)).ThenBy(candidate => candidate.X).ThenBy(candidate => candidate.Y)
                .Cast<GridCoord?>().FirstOrDefault();
            if (outside is not { } target) return;

            _grid.Grant(target);
            if (!_tiles.TryGet(cell, out var tileId)) return;

            _grid.Unlock(target);
            _tiles.Place(target, tileId, $"вытеснено: {cause}");
            if (_features.TryGet(cell, out var feature)) _features.Add(target, feature);
            if (_settlements.TryGet(cell, out var population))
            {
                _settlements.Leave(cell);
                _settlements.TrySettle(target, population);
            }

            Displaced?.Invoke(cell, target);
        }

        private void OnChanged(GridCoord cell, string tileId, string cause)
        {
            if (!_peaks.Contains(cell) || tileId == _rules.PeakTileId) return;

            _tiles.Place(cell, _rules.PeakTileId, "супергору не сдвинуть");
            Withstood?.Invoke(cell, cause);
        }

        private static int Distance(GridCoord a, GridCoord b) => Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
    }
}
