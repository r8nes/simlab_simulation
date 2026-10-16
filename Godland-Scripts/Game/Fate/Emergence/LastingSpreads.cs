using System.Collections.Generic;
using System.Linq;
using Godland.Core;
using Godland.Core.World.Grid;
using Godland.Core.Randomness;
using Godland.Core.World.Tiles;

namespace Godland.Game.Fate.Emergence
{
    public sealed class LastingSpreads : IWorldStep
    {
        private readonly WorldGrid _grid;
        private readonly WorldTiles _tiles;
        private readonly TileGuards _guards;
        private readonly SeededRandom _random;
        private readonly List<SpreadRule> _rules = new();

        public LastingSpreads(WorldGrid grid, WorldTiles tiles, TileGuards guards, SeededRandom random)
        {
            _grid = grid;
            _tiles = tiles;
            _guards = guards;
            _random = random;
        }

        public void Add(IReadOnlyCollection<string> from, string to, int period, int count, string cause, IReadOnlyCollection<string> sources = null) =>
            _rules.Add(new SpreadRule(from, to, period, count, cause, sources ?? new[] { to }));

        public void Advance(int year)
        {
            foreach (var rule in _rules.Where(rule => year % rule.Period == 0))
            {
                var targets = _tiles.All
                    .Where(pair => rule.From.Contains(pair.Value) && pair.Key != _grid.Center && !_guards.IsProtected(pair.Key)
                        && _grid.Neighbors(pair.Key).Any(cell => _tiles.TryGet(cell, out var id) && rule.Sources.Contains(id)))
                    .Select(pair => pair.Key)
                    .OrderBy(_ => _random.Next(10000))
                    .Take(rule.Count)
                    .ToList();
                foreach (var cell in targets) _tiles.Place(cell, rule.To, rule.Cause);
            }
        }

        private sealed class SpreadRule
        {
            public SpreadRule(IReadOnlyCollection<string> from, string to, int period, int count, string cause, IReadOnlyCollection<string> sources)
            {
                From = from;
                To = to;
                Period = period;
                Count = count;
                Cause = cause;
                Sources = sources;
            }

            public IReadOnlyCollection<string> From { get; }
            public string To { get; }
            public int Period { get; }
            public int Count { get; }
            public string Cause { get; }
            public IReadOnlyCollection<string> Sources { get; }
        }
    }
}
