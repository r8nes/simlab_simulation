using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core;
using Godland.Core.World.Grid;
using Godland.Game.Fate.Emergence;
using Godland.Game.Life.Features;
using Godland.Game.Life.Races;

namespace Godland.Game.Life.Reclaim
{
    public sealed class ReclaimKeeper : IWorldStep
    {
        private readonly ReclaimConfig _config;
        private readonly EmergenceContext _world;
        private readonly FeatureCatalog _features;
        private readonly RaceKeeper _races;
        private readonly Dictionary<GridCoord, int> _since = new();

        public ReclaimKeeper(ReclaimConfig config, EmergenceContext world, FeatureCatalog features, RaceKeeper races)
        {
            _config = config;
            _world = world;
            _features = features;
            _races = races;
        }

        public event Action<GridCoord, string> Overgrown;
        public event Action<GridCoord, string> Inhabited;
        public event Action<GridCoord, Race> Returned;

        public void Advance(int year)
        {
            if (year % _config.Period != 0) return;

            var settlements = _world.Settlements;
            foreach (var (cell, until) in settlements.Deserted.OrderBy(pair => pair.Key.X).ThenBy(pair => pair.Key.Y).ToList())
            {
                if (until <= year)
                {
                    if (_since.Remove(cell)) Return(cell);
                    continue;
                }

                if (!_since.ContainsKey(cell)) _since[cell] = year;
                Rewild(cell, year - _since[cell]);
            }
        }

        private void Rewild(GridCoord cell, int age)
        {
            _world.Memory.Add(cell, "deserted", _config.Period);
            if (!_world.Tiles.TryGet(cell, out var tile)) return;

            var step = _config.Steps.FirstOrDefault(candidate => candidate.From.Id == tile && age >= candidate.AfterYears);
            if (step != null && !_world.Guards.IsProtected(cell))
            {
                _world.Tiles.Place(cell, step.To.Id, "брошенная земля заросла");
                Overgrown?.Invoke(cell, step.To.Id);
                tile = step.To.Id;
            }

            if (_world.Features.TryGet(cell, out _) || _world.Random.Next(100) >= _config.WildChancePercent) return;

            var haunted = _config.HauntedTiles.Any(candidate => candidate.Id == tile) && _world.Random.Next(100) < _config.MonsterChancePercent;
            var options = _features.All
                .Where(feature => feature.IsMonster == haunted && _features.CanLiveOn(feature.Id, tile))
                .OrderBy(feature => feature.Id)
                .ToList();
            if (options.Count == 0) return;

            var chosen = options[_world.Random.Next(options.Count)];
            _world.Features.AddWild(cell, chosen.Id);
            Inhabited?.Invoke(cell, chosen.Id);
        }

        private void Return(GridCoord cell)
        {
            if (_world.Random.Next(100) >= _config.ReturnChancePercent) return;
            if (!_world.Settlements.TrySettle(cell, _config.ReturnPopulation)) return;

            var race = _world.Grid.Neighbors(cell).Select(_races.RaceAt).FirstOrDefault(candidate => candidate != null);
            race?.Cells.Add(cell);
            _world.History.Note(cell, race == null ? "люди вернулись на брошенную землю" : $"{race.Name} заняли брошенную землю");
            Returned?.Invoke(cell, race);
        }
    }
}
