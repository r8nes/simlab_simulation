using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;

namespace Godland.Core.Fate.Emergence
{
    public sealed class WorldMemory
    {
        private readonly Dictionary<(GridCoord Cell, string Key), int> _marks = new();

        public void Add(GridCoord cell, string key, int amount = 1) =>
            _marks[(cell, key)] = _marks.GetValueOrDefault((cell, key)) + amount;

        public int Get(GridCoord cell, string key) => _marks.GetValueOrDefault((cell, key));

        public int Around(GridCoord center, string key, int radius) =>
            _marks.Where(pair => pair.Key.Key == key && Distance(pair.Key.Cell, center) <= radius).Sum(pair => pair.Value);

        public IEnumerable<GridCoord> CellsWith(string key) =>
            _marks.Where(pair => pair.Key.Key == key && pair.Value > 0).Select(pair => pair.Key.Cell);

        public IEnumerable<(string Key, int Value)> KeysAt(GridCoord cell, string prefix) =>
            _marks.Where(pair => pair.Key.Cell == cell && pair.Key.Key.StartsWith(prefix)).Select(pair => (pair.Key.Key, pair.Value));

        public void Forget(GridCoord center, string key, int radius)
        {
            foreach (var mark in _marks.Keys.Where(mark => mark.Key == key && Distance(mark.Cell, center) <= radius).ToList())
                _marks.Remove(mark);
        }

        public void ForgetAll(GridCoord center, int radius)
        {
            foreach (var mark in _marks.Keys.Where(mark => Distance(mark.Cell, center) <= radius).ToList())
                _marks.Remove(mark);
        }

        private static int Distance(GridCoord a, GridCoord b) => Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
    }
}
