using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;

namespace Godland.Core.World.Tiles
{
    public sealed class WorldTiles
    {
        private readonly WorldGrid _grid;
        private readonly Dictionary<GridCoord, string> _tiles = new();

        private Func<GridCoord, string, string, string> _substitute;

        public WorldTiles(WorldGrid grid)
        {
            _grid = grid;
        }

        public event Action<GridCoord> Cleared;
        public event Action<GridCoord, string> Placed;
        public event Action<GridCoord> Removed;
        public event Action<GridCoord, string, string> Changed;

        private Func<GridCoord, bool> _uncounted = _ => false;

        public IReadOnlyDictionary<GridCoord, string> All => _tiles;

        public IEnumerable<string> CountedIds => _tiles.Where(pair => !_uncounted(pair.Key)).Select(pair => pair.Value);

        public int CountedCount => _tiles.Keys.Count(cell => !_uncounted(cell));

        public void SetUncounted(Func<GridCoord, bool> uncounted) => _uncounted = uncounted;

        public bool PlayerActing { get; set; }

        public void SetSubstitute(Func<GridCoord, string, string, string> substitute) => _substitute = substitute;

        public bool TryGet(GridCoord coord, out string tileId) => _tiles.TryGetValue(coord, out tileId);

        public bool CanPlace(GridCoord coord) => _grid.IsUnlocked(coord) && !_tiles.ContainsKey(coord);

        public void Place(GridCoord coord, string tileId, string cause = null)
        {
            if (string.IsNullOrEmpty(tileId)) throw new ArgumentException("Tile id is empty.", nameof(tileId));
            if (!_grid.IsUnlocked(coord)) throw new InvalidOperationException($"Cell {coord} is locked.");

            if (!PlayerActing && _substitute != null) tileId = _substitute(coord, tileId, cause) ?? tileId;
            Cleared?.Invoke(coord);
            _tiles[coord] = tileId;
            Placed?.Invoke(coord, tileId);
            Changed?.Invoke(coord, tileId, cause);
        }

        public void Remove(GridCoord coord, string cause = null)
        {
            if (!_tiles.ContainsKey(coord)) return;

            Cleared?.Invoke(coord);
            _tiles.Remove(coord);
            Removed?.Invoke(coord);
            Changed?.Invoke(coord, null, cause);
        }
    }
}
