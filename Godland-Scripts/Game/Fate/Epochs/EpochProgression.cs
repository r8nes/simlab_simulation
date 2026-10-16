using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Game.Fate.Catastrophes;
using Godland.Core.Life.Cities;
using Godland.Core.Fate.Epochs;
using Godland.Core.World.Grid;
using Godland.Core.Life.Kingdoms;
using Godland.Core.Life.People;
using Godland.Core.Randomness;
using Godland.Core.World.Timing;
using Godland.Game.Life.People;
using Godland.Game.Writer.Run;

namespace Godland.Game.Fate.Epochs
{
    public sealed class EpochProgression : IDisposable
    {
        private readonly WorldEpoch _epoch;
        private readonly EpochConfig _config;
        private readonly WorldGrid _grid;
        private readonly Settlements _settlements;
        private readonly WorldClock _clock;
        private readonly SeededRandom _random;
        private readonly Shrines _shrines;
        private readonly SettlementConfig _settlementConfig;
        private readonly RunUnlocks _unlocks;
        private readonly Urbanization _cities;
        private readonly Realms _realms;
        private readonly CatastropheContext _world;
        private readonly Dictionary<int, EpochDefinition> _chosen = new();
        private int _epochStartYear;
        private int _hastened;

        public EpochProgression(
            WorldEpoch epoch, EpochConfig config, WorldGrid grid, Settlements settlements, WorldClock clock, SeededRandom random,
            Shrines shrines, SettlementConfig settlementConfig, RunUnlocks unlocks, Urbanization cities, Realms realms, CatastropheContext world)
        {
            _world = world;
            _epoch = epoch;
            _config = config;
            _grid = grid;
            _settlements = settlements;
            _clock = clock;
            _random = random;
            _shrines = shrines;
            _settlementConfig = settlementConfig;
            _unlocks = unlocks;
            _cities = cities;
            _realms = realms;

            _settlements.Founded += OnSettlementFounded;
            _clock.Ticked += OnTicked;
        }

        public EpochDefinition Current => _chosen.TryGetValue(_epoch.Index, out var chosen) ? chosen : _config.Epochs[_epoch.Index];

        public int StartYear => _epochStartYear;

        public bool IsBranch => _chosen.ContainsKey(_epoch.Index);

        public EpochDefinition Next => _epoch.Index + 1 < _config.Epochs.Count ? _config.Epochs[_epoch.Index + 1] : null;

        public void Dispose()
        {
            _settlements.Founded -= OnSettlementFounded;
            _clock.Ticked -= OnTicked;
        }

        private void OnSettlementFounded(GridCoord _) => TryAdvance(EpochTrigger.FirstSettlement);

        private int EarliestYear => Next == null ? 0 : _epochStartYear + Next.MinYearsAfterPrevious - _hastened;

        public void Hasten(int years) => _hastened += years;

        public string GoalText => Next == null ? null : _clock.Year < EarliestYear && Next.Trigger != EpochTrigger.Year
            ? $"{Condition}, не раньше {EarliestYear} года"
            : Condition;

        private string Condition => Next.Trigger switch
        {
            EpochTrigger.Year => $"{Next.DisplayName}: в {Next.TriggerYear - _hastened} году",
            EpochTrigger.FirstSettlement => $"{Next.DisplayName}: появятся первые жители",
            EpochTrigger.Villages => $"{Next.DisplayName}: деревень {Progress}/{Next.TriggerCount}",
            EpochTrigger.Shrines => $"{Next.DisplayName}: святынь {Progress}/{Next.TriggerCount}",
            EpochTrigger.Cities => $"{Next.DisplayName}: городов {Progress}/{Next.TriggerCount}",
            EpochTrigger.Kingdoms => $"{Next.DisplayName}: основать царство ({Progress}/{Next.TriggerCount})",
            _ => Next.DisplayName
        };

        private int Progress => Next?.Trigger switch
        {
            EpochTrigger.Year => _clock.Year,
            EpochTrigger.Villages => _settlements.All.Values.Count(population => population >= _settlementConfig.VillagePopulation),
            EpochTrigger.Shrines => _shrines.Count,
            EpochTrigger.Cities => _cities.All.Count,
            EpochTrigger.Kingdoms => _realms.All.Count,
            _ => 0
        };

        private void OnTicked(int _)
        {
            if (Next == null) return;

            var target = Next.Trigger == EpochTrigger.Year ? Next.TriggerYear - _hastened : Next.TriggerCount;
            if (Next.Trigger != EpochTrigger.FirstSettlement && Progress >= target) TryAdvance(Next.Trigger);
        }

        private void TryAdvance(EpochTrigger trigger)
        {
            if (Next == null || Next.Trigger != trigger || _clock.Year < EarliestYear) return;

            var directions = _grid.Directions.Where(_grid.CanExpand).ToList();
            if (directions.Count > 0)
                _grid.UnlockSide(directions[_random.Next(directions.Count)], Next.UnlockCells);

            var chosen = Next.Alternatives.FirstOrDefault(alternative => alternative.BranchCondition != null && alternative.BranchCondition.IsReady(_world));
            if (chosen != null) _chosen[_epoch.Index + 1] = chosen;
            foreach (var action in Next.UnlockedActions.Concat(chosen != null ? chosen.UnlockedActions : Enumerable.Empty<Godland.Game.Writer.Actions.TileActionDefinition>()))
                _unlocks.UnlockAction(action);

            _epochStartYear = _clock.Year;
            _epoch.Advance();
        }
    }
}
