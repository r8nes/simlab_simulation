using System;
using System.Collections.Generic;
using Godland.Core.World.Grid;
using Godland.Core.World.Timing;

namespace Godland.Core.World.Tiles
{
    public sealed class TileHistory
    {
        private const int MaxRecords = 12;

        private readonly WorldClock _clock;
        private readonly Dictionary<GridCoord, List<TileRecord>> _records = new();

        public TileHistory(WorldTiles tiles, WorldClock clock)
        {
            _clock = clock;
            tiles.Changed += OnChanged;
        }

        public IReadOnlyList<TileRecord> Of(GridCoord coord) =>
            _records.TryGetValue(coord, out var records) ? records : Array.Empty<TileRecord>();

        public void Note(GridCoord coord, string text) => Add(coord, new TileRecord(_clock.Year, null, text));

        private void OnChanged(GridCoord coord, string tileId, string cause) =>
            Add(coord, new TileRecord(_clock.Year, tileId ?? string.Empty, cause));

        private void Add(GridCoord coord, TileRecord record)
        {
            if (!_records.TryGetValue(coord, out var records)) _records[coord] = records = new List<TileRecord>();
            records.Add(record);
            if (records.Count > MaxRecords) records.RemoveAt(0);
        }
    }

    public readonly struct TileRecord
    {
        public TileRecord(int year, string tileId, string text)
        {
            Year = year;
            TileId = tileId;
            Text = text;
        }

        public int Year { get; }
        public string TileId { get; }
        public string Text { get; }
    }
}
