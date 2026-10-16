using Godland.Core.Fate.Anomalies;
using Godland.Core.Randomness;
using Godland.Core.Powers.Babel;
using Godland.Core.World.Timing;
using Godland.Core;
using Godland.Core.Fate.Epochs;
using Godland.Core.Life.Heroes;
using Godland.Core.Life.Kingdoms;
using Godland.Core.Powers.Gods;
using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Biomes;
using Godland.Core.World.Cells;
using Godland.Core.Life.Features;
using Godland.Core.World.Volcanoes;
using Godland.Core.World.Creation;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Core.World.Tiles;
using Godland.Game.Writer.Run;

namespace Godland.Game.Writer.Actions
{
    public sealed class TileActions
    {
        private readonly RunUnlocks _unlocks;
        private readonly WorldTiles _tiles;
        private readonly Godland.Core.World.Peaks.SuperPeaks _peaks;
        private readonly PlayerLocks _locks;
        private readonly CreationPoints _creation;
        private readonly FaithPoints _faith;
        private readonly Shrines _shrines;
        private readonly Godfighters _godfighters;
        private readonly TileEffectContext _context;
        private readonly WorldEpoch _epoch;
        private readonly RunConfig _run;
        private readonly WorldModifiers _modifiers;
        private readonly WorldClock _clock;
        private readonly SeededRandom _random;
        private readonly HashSet<(string ActionId, GridCoord Coord)> _used = new();
        private readonly Dictionary<string, int> _readyYear = new();

        public event Action<GridCoord, TileActionDefinition> Applied;

        public TileActions(RunUnlocks unlocks, WorldGrid grid, WorldTiles tiles, CellFertility fertility, CreationPoints creation,
            FaithPoints faith, Settlements settlements, Shrines shrines,
            Godfighters godfighters, SuperBiomes superBiomes,
            TileFeatures features, Monsters monsters, VolcanoActivity volcanoes, Temples temples, Realms realms, HeroSaga heroes,
            WorldEpoch epoch, RunConfig run, WorldModifiers modifiers, WorldClock clock, TowerOfBabel babel,
            SeededRandom random, Devourer devourer, Pantheon pantheon, Godland.Game.Fate.Anomalies.AnomalyKeeper anomalies,
            Godland.Core.Powers.Titans.TitanHost titans, Godland.Core.World.Peaks.SuperPeaks peaks, PlayerLocks locks, Godland.Game.Fate.Sagas.SagaKeeper sagas)
        {
            _random = random;
            _clock = clock;
            _modifiers = modifiers;
            _epoch = epoch;
            _run = run;
            _unlocks = unlocks;
            _tiles = tiles;
            _peaks = peaks;
            _locks = locks;
            _creation = creation;
            _faith = faith;
            _shrines = shrines;
            _godfighters = godfighters;
            _context = new TileEffectContext(grid, tiles, fertility, settlements, shrines, creation, superBiomes, features, monsters, volcanoes, temples, realms, heroes, babel, devourer, random, pantheon, anomalies, titans, sagas);

            _tiles.Cleared += coord => _used.RemoveWhere(entry => entry.Coord == coord);
        }

        public IEnumerable<TileActionDefinition> AvailableFor(GridCoord coord) =>
            _tiles.TryGet(coord, out var tileId)
                ? _unlocks.Actions.Where(action => action.AppliesTo(tileId) && action.Effects.Any(effect => effect.CanAffect(coord, _context)))
                : Enumerable.Empty<TileActionDefinition>();

        public int FaithCostAt(TileActionDefinition action, GridCoord coord) =>
            action.FaithCost == 0
                ? 0
                : (int)Math.Ceiling(action.FaithCost * (100 + _run.MiracleCostPerEpochPercent * _epoch.Index) / 100f * _modifiers.MiracleCostPercent / 100f * _modifiers.MiracleTaxPercent / 100f);

        public bool HasTarget(TileActionDefinition action) =>
            _tiles.All.Any(pair => action.AppliesTo(pair.Value) && action.Effects.Any(effect => effect.CanAffect(pair.Key, _context)));

        public string DescribeAt(TileActionDefinition action, GridCoord coord) => action.DescribeAt(coord, _context);

        public bool IsUsed(TileActionDefinition action, GridCoord coord) => action.OncePerTile && _used.Contains((action.Id, coord));

        public bool CanApply(TileActionDefinition action, GridCoord coord) =>
            !_peaks.IsPeak(coord) && !(action.FaithCost == 0 && _locks.IsLocked(coord)) && _tiles.TryGet(coord, out var tileId) && action.AppliesTo(tileId) && !IsUsed(action, coord)
            && action.Effects.Any(effect => effect.CanAffect(coord, _context))
            && !(action.FaithCost > 0 && _godfighters.IsRebel(coord))
            && _creation.CanAfford(action.Cost) && _faith.CanAfford(FaithCostAt(action, coord))
            && (action.FaithCost == 0 || _clock.Year >= _modifiers.MiraclesBlockedUntil)
            && CooldownLeft(action) == 0;

        public int CooldownLeft(TileActionDefinition action) => Math.Max(0, _readyYear.GetValueOrDefault(action.Id) - _clock.Year);

        public bool TryApply(TileActionDefinition action, GridCoord coord)
        {
            if (!CanApply(action, coord)) return false;
            if (_clock.Year < _modifiers.MadUntil)
            {
                var stray = _tiles.All.Keys.Where(cell => CanApply(action, cell)).ToList();
                coord = stray[_random.Next(stray.Count)];
            }

            var faithCost = FaithCostAt(action, coord);
            _creation.TrySpend(action.Cost);
            _faith.TrySpend(faithCost);
            if (faithCost > 0 && _modifiers.MiracleRefundPercent > 0) _faith.Add(faithCost * _modifiers.MiracleRefundPercent / 100);

            if (action.OncePerTile) _used.Add((action.Id, coord));
            if (action.CooldownYears > 0) _readyYear[action.Id] = _clock.Year + action.CooldownYears;

            _tiles.PlayerActing = true;
            try
            {
                foreach (var effect in action.Effects)
                    effect.Apply(coord, _context);
            }
            finally
            {
                _tiles.PlayerActing = false;
            }

            if (faithCost > 0)
            {
                _shrines.Witness(coord);
                _godfighters.Witness(coord);
            }
            Applied?.Invoke(coord, action);
            return true;
        }
    }
}
