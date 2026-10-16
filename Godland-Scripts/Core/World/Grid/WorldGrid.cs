using System;
using System.Collections.Generic;
using System.Linq;

namespace Godland.Core.World.Grid
{
    public sealed class WorldGrid
    {
        private static readonly GridCoord[] NeighborOffsets = { new(1, 0), new(-1, 0), new(0, 1), new(0, -1) };

        private readonly HashSet<GridCoord> _unlocked = new();
        private readonly HashSet<GridCoord> _world = new();

        public WorldGrid(int width, int height, int initialUnlockedSize, int worldSize = 0)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            if (initialUnlockedSize < 0 || initialUnlockedSize > Math.Min(width, height))
                throw new ArgumentOutOfRangeException(nameof(initialUnlockedSize));

            Width = width;
            Height = height;
            var size = worldSize <= 0 ? Math.Max(width, height) : worldSize;
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
                if (Math.Abs(x - width / 2) <= size / 2 && Math.Abs(y - height / 2) <= size / 2) _world.Add(new GridCoord(x, y));
            UnlockCenter(initialUnlockedSize);
        }

        public event Action<GridCoord> Unlocked;
        public event Action<GridCoord> Granted;

        public int Width { get; }
        public int Height { get; }
        public GridCoord Center => new(Width / 2, Height / 2);

        public bool Contains(GridCoord coord) => _world.Contains(coord);

        public bool InBounds(GridCoord coord) =>
            coord.X >= 0 && coord.X < Width && coord.Y >= 0 && coord.Y < Height;

        public IEnumerable<GridCoord> Outside =>
            Bounds.Where(coord => !_world.Contains(coord) && NeighborOffsets.Any(offset => _world.Contains(new GridCoord(coord.X + offset.X, coord.Y + offset.Y))));

        public void Grant(GridCoord coord)
        {
            if (!InBounds(coord)) throw new ArgumentOutOfRangeException(nameof(coord));
            if (_world.Add(coord)) Granted?.Invoke(coord);
        }

        public bool IsUnlocked(GridCoord coord) => _unlocked.Contains(coord);

        public IEnumerable<GridCoord> Coords => Bounds.Where(_world.Contains);

        private IEnumerable<GridCoord> Bounds
        {
            get
            {
                for (var y = 0; y < Height; y++)
                for (var x = 0; x < Width; x++)
                    yield return new GridCoord(x, y);
            }
        }

        public IEnumerable<GridCoord> Neighbors(GridCoord coord)
        {
            foreach (var offset in NeighborOffsets)
            {
                var neighbor = new GridCoord(coord.X + offset.X, coord.Y + offset.Y);
                if (Contains(neighbor)) yield return neighbor;
            }
        }

        public void Unlock(GridCoord coord)
        {
            if (!Contains(coord)) throw new ArgumentOutOfRangeException(nameof(coord));
            if (_unlocked.Add(coord)) Unlocked?.Invoke(coord);
        }

        public bool IsPurchasable(GridCoord coord) =>
            Contains(coord) && !IsUnlocked(coord) && Neighbors(coord).Any(IsUnlocked);

        public IReadOnlyList<GridCoord> UnlockRing()
        {
            var ring = Coords.Where(coord => !IsUnlocked(coord) && TouchesUnlocked(coord)).ToList();
            foreach (var coord in ring) Unlock(coord);
            return ring;
        }

        private bool TouchesUnlocked(GridCoord coord)
        {
            for (var dy = -1; dy <= 1; dy++)
            for (var dx = -1; dx <= 1; dx++)
                if ((dx != 0 || dy != 0) && IsUnlocked(new GridCoord(coord.X + dx, coord.Y + dy)))
                    return true;
            return false;
        }

        public IReadOnlyList<GridCoord> Directions => NeighborOffsets;

        public bool CanExpand(GridCoord direction) => Contains(EdgeStart(direction, 1));

        public void UnlockSide(GridCoord direction, int cells)
        {
            var start = EdgeStart(direction, cells);
            for (var i = 0; i < cells; i++)
                TryUnlock(direction.X != 0
                    ? new GridCoord(start.X, start.Y + i)
                    : new GridCoord(start.X + i, start.Y));
        }

        private GridCoord EdgeStart(GridCoord direction, int cells)
        {
            var unlocked = _unlocked.ToList();
            var minX = unlocked.Min(coord => coord.X);
            var maxX = unlocked.Max(coord => coord.X);
            var minY = unlocked.Min(coord => coord.Y);
            var maxY = unlocked.Max(coord => coord.Y);
            var offset = (cells - 1) / 2;

            return direction.X != 0
                ? new GridCoord(direction.X > 0 ? maxX + 1 : minX - 1, (minY + maxY) / 2 - offset)
                : new GridCoord((minX + maxX) / 2 - offset, direction.Y > 0 ? maxY + 1 : minY - 1);
        }

        private void TryUnlock(GridCoord coord)
        {
            if (Contains(coord)) Unlock(coord);
        }

        private void UnlockCenter(int size)
        {
            var minX = (Width - size) / 2;
            var minY = (Height - size) / 2;
            for (var y = minY; y < minY + size; y++)
            for (var x = minX; x < minX + size; x++)
                Unlock(new GridCoord(x, y));
        }
    }
}
