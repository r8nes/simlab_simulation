using System;
using System.Collections.Generic;
using Godland.Core.World.Grid;
using Godland.Core.World.Tiles;

namespace Godland.Core.Life.Features
{
    public sealed class TileFeatures
    {
        private readonly WorldTiles _tiles;
        private readonly Dictionary<GridCoord, string> _features = new();
        private readonly HashSet<GridCoord> _wild = new();

        public TileFeatures(WorldTiles tiles)
        {
            _tiles = tiles;
            _tiles.Cleared += Remove;
        }

        public event Action<GridCoord, string> Added;
        public event Action<GridCoord, string> Removed;

        public IReadOnlyDictionary<GridCoord, string> All => _features;

        public bool TryGet(GridCoord coord, out string featureId) => _features.TryGetValue(coord, out featureId);

        public bool IsWild(GridCoord coord) => _wild.Contains(coord);

        public bool CanAdd(GridCoord coord) => _tiles.TryGet(coord, out _) && !_features.ContainsKey(coord);

        public void Add(GridCoord coord, string featureId)
        {
            if (!CanAdd(coord)) throw new InvalidOperationException($"Cell {coord} can't take a feature.");

            _features[coord] = featureId;
            Added?.Invoke(coord, featureId);
        }

        public void AddWild(GridCoord coord, string featureId)
        {
            if (!CanAdd(coord)) throw new InvalidOperationException($"Cell {coord} can't take a feature.");

            _wild.Add(coord);
            _features[coord] = featureId;
            Added?.Invoke(coord, featureId);
        }

        public void Replace(GridCoord coord, string featureId, bool wild = false)
        {
            Remove(coord);
            if (wild) AddWild(coord, featureId);
            else Add(coord, featureId);
        }

        public void Remove(GridCoord coord)
        {
            _wild.Remove(coord);
            if (_features.Remove(coord, out var featureId))
                Removed?.Invoke(coord, featureId);
        }
    }
}
