using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core;
using Godland.Core.World.Grid;
using Godland.Core.Life.Minds;
using Godland.Core.World.Tiles;
using Godland.Game.Fate.Catastrophes;
using Godland.Game.Writer.Run;

namespace Godland.Game.Fate.Sagas
{
    public sealed class SagaKeeper : IWorldStep
    {
        private readonly Godland.Core.Flow.FlowBias _flow;
        private const int FullPower = 100;
        private const int ContestRadius = 2;

        private readonly SagaConfig _config;
        private readonly CatastropheContext _world;
        private readonly IMindRules _mindRules;
        private readonly RunUnlocks _unlocks;
        private readonly List<SagaFaction> _factions = new();
        private readonly HashSet<SagaDefinition> _told = new();
        private readonly Dictionary<GridCoord, int> _sides = new();
        private readonly HashSet<GridCoord> _gap = new();
        private readonly HashSet<GridCoord> _heaven = new();
        private readonly HashSet<GridCoord> _heavenGate = new();
        private int _actBegan;
        private int _nextBridge;
        private int _bridgeUntil;

        public SagaKeeper(SagaConfig config, CatastropheContext world, IMindRules mindRules, RunUnlocks unlocks, PlayerLocks locks, Godland.Core.Flow.FlowBias flow)
        {
            _flow = flow;
            _config = config;
            _world = world;
            _mindRules = mindRules;
            _unlocks = unlocks;
            locks.Add(cell => _sides.ContainsKey(cell) || _gap.Contains(cell) || _heaven.Contains(cell) || _heavenGate.Contains(cell));
            world.World.Settlements.Abandoned += OnDeath;
            world.World.Guards.Add(cell => _gap.Contains(cell) || _heaven.Contains(cell) || _heavenGate.Contains(cell) || (Bridge is { } bridge && bridge == cell));
        }

        public event Action<SagaDefinition> Began;
        public event Action<SagaActDefinition> ActBegan;
        public event Action<SagaTransition> Turned;
        public event Action<SagaDefinition, SagaTransition> Ended;
        public event Action<SagaFaction, SagaFaction, GridCoord> Battled;
        public event Action<SagaFaction, string> Overthrown;
        public event Action<SagaFaction, SagaFaction, GridCoord> Betrayed;
        public event Action<SagaFaction> ArtifactFound;
        public event Action<GridCoord> BridgeOpened;
        public event Action BridgeClosed;
        public event Action<SagaFaction, string, GridCoord> Acted;

        public CatastropheContext World => _world;
        public SagaDefinition Current { get; private set; }
        public SagaActDefinition Act { get; private set; }
        public IReadOnlyList<SagaFaction> Factions => _factions;
        public int ActAge => _world.World.Clock.Year - _actBegan;
        public bool IsSplit => _gap.Count > 0;
        public GridCoord? Bridge { get; private set; }
        public bool BridgeOpen => Bridge != null;
        public int NextBridgeYear => _nextBridge;
        public IReadOnlyCollection<SagaDefinition> Told => _told;

        public int PowerOf(SagaFactionDefinition definition) => FactionOf(definition)?.Power ?? 0;

        public bool HasArtifact(SagaFactionDefinition definition) => FactionOf(definition)?.HasArtifact ?? false;

        public SagaFaction FactionOf(SagaFactionDefinition definition) => _factions.FirstOrDefault(faction => faction.Definition == definition);

        public SagaFaction OwnerOf(GridCoord cell) => _factions.FirstOrDefault(faction => faction.Lands.Contains(cell));

        public void Empower(SagaFaction faction, int amount) => faction.Bonus += amount;

        public IEnumerable<SagaTransition> GoalsOf(SagaFaction faction) =>
            Act == null ? Enumerable.Empty<SagaTransition>() : Act.Transitions.Where(transition => transition.Condition?.Favors(this) == faction);

        public int ThresholdOf(SagaFaction faction) =>
            GoalsOf(faction).Select(transition => transition.Condition switch
            {
                FactionPowerCondition power => power.Threshold,
                SidePowerCondition side => side.Threshold,
                _ => 0
            }).Where(threshold => threshold > 0).DefaultIfEmpty(100).Min();

        public string GoalText(SagaFaction faction)
        {
            var goal = GoalsOf(faction).FirstOrDefault();
            return goal == null ? "их победа не меняет акт" : goal.EndsSaga ? $"их победа: {goal.Text} (сага кончится)" : $"их победа: {goal.Text}";
        }

        public void GiveArtifact(SagaFaction faction)
        {
            if (faction.HasArtifact) return;

            faction.HasArtifact = true;
            ArtifactFound?.Invoke(faction);
        }

        public bool CanOpenBridge => IsSplit && !BridgeOpen;

        public void OpenBridge(int year, int years)
        {
            var center = _world.World.Grid.Center;
            var cell = _gap.OrderBy(gap => Math.Abs(gap.Y - center.Y)).ThenBy(gap => gap.Y).Cast<GridCoord?>().FirstOrDefault();
            if (cell is not { } bridge) return;

            Bridge = bridge;
            _bridgeUntil = year + years;
            _world.World.Tiles.Place(bridge, _config.BridgeTile.Id, "между мирами открылся мост");
            BridgeOpened?.Invoke(bridge);
        }

        public void Advance(int year)
        {
            if (Current == null)
            {
                if (year % _config.CheckPeriod == 0) TryBegin();
                return;
            }

            if (ActAge > 0)
                foreach (var law in Act.Laws) law.Year(_world, ActAge, Act.Title.ToLowerInvariant());

            RefreshLands();
            if (year % _config.SpreadPeriod == 0) Spread(year);
            if (year % _config.BattlePeriod == 0) Battle();
            if (year % _config.DeedPeriod == 0) Deeds();
            if (Act.Intrigues && year % _config.IntriguePeriod == 0) Intrigue();
            if (IsSplit) TendBridge(year);
            RefreshPower();
            if (year % _config.CheckPeriod == 0) TryTurn();
        }

        private void OnDeath(GridCoord cell)
        {
            foreach (var faction in _factions.Where(faction => faction.Definition.PowerPerDeath > 0)) faction.Bonus += faction.Definition.PowerPerDeath;
        }

        private void TryBegin()
        {
            foreach (var saga in _config.Sagas)
            {
                if (saga.OnlyFromSaga || _told.Contains(saga) || _world.World.Epoch.Index < saga.MinEpoch) continue;
                if (saga.Start == null || !saga.Start.IsReady(_world) || _world.World.Random.Next(100) >= _flow.Chance($"saga:{saga.name}", saga.ChancePercent)) continue;

                _flow.Note($"saga:{saga.name}");
                Begin(saga);
                return;
            }
        }

        public bool TryCall(SagaDefinition saga)
        {
            if (Current != null || saga == null) return false;

            _flow.Note($"saga:{saga.name}");
            Begin(saga);
            return true;
        }

        private void Begin(SagaDefinition saga)
        {
            Current = saga;
            _told.Add(saga);
            _world.World.Memory.Add(_world.World.Grid.Center, "sagas");
            Began?.Invoke(saga);
            BeginAct(saga.FirstAct);
        }

        private void BeginAct(SagaActDefinition act)
        {
            var previous = Act;
            if (previous != null)
                foreach (var law in previous.Laws) law.End(_world);

            Act = act;
            _actBegan = _world.World.Clock.Year;
            ArrangeFactions(act);
            if (act.Split && !IsSplit) SplitWorld();
            else if (!act.Split && IsSplit) HealWorld();
            if (act.OutsideCells > 0 && _factions.Count > 0) RaiseOutside(act.OutsideCells, _factions[0]);
            foreach (var action in act.PlayerActions) _unlocks.UnlockAction(action);
            foreach (var law in act.Laws) law.Begin(_world, act.Title.ToLowerInvariant());
            _world.World.Memory.Add(_world.World.Grid.Center, $"saga-act:{act.name}");
            RefreshLands();
            RefreshPower();
            ActBegan?.Invoke(act);
        }

        private void ArrangeFactions(SagaActDefinition act)
        {
            var wanted = act.Factions.ToList();
            if (act.FirstFactionVariants.Count > 0)
            {
                var kept = _factions.FirstOrDefault(faction => act.FirstFactionVariants.Contains(faction.Definition));
                var first = kept?.Definition ?? act.FirstFactionVariants[_world.World.Random.Next(act.FirstFactionVariants.Count)];
                wanted.Insert(0, first);
            }

            var next = new List<SagaFaction>();
            for (var side = 0; side < wanted.Count; side++)
            {
                var definition = wanted[side];
                var existing = _factions.FirstOrDefault(faction => faction.Definition == definition);
                var faction = new SagaFaction(definition, existing?.Mind ?? new Mind(_mindRules), side)
                {
                    LeaderIndex = existing?.LeaderIndex ?? 0,
                    Bonus = existing?.Bonus ?? 0,
                    HasArtifact = false,
                    LastDeed = existing?.LastDeed
                };
                next.Add(faction);
            }

            _factions.Clear();
            _factions.AddRange(next);
        }

        private void SplitWorld()
        {
            var world = _world.World;
            var center = world.Grid.Center;
            foreach (var cell in world.Grid.Coords.Where(cell => cell.X == center.X && cell != center).ToList())
            {
                _gap.Add(cell);
                if (world.Settlements.TryGet(cell, out _)) world.Settlements.Leave(cell);
                if (world.Features.TryGet(cell, out _)) world.Features.Remove(cell);
                if (!world.Grid.IsUnlocked(cell)) world.Grid.Unlock(cell);
                world.Tiles.Place(cell, _config.GapTile.Id, "мир раскололся надвое");
            }

            foreach (var cell in world.Grid.Coords.Where(cell => cell.X != center.X)) _sides[cell] = cell.X < center.X ? 0 : 1;
            _nextBridge = world.Clock.Year + Act.BridgeMinGap;
            if (Act.BridgeAlwaysOpen) OpenBridge(world.Clock.Year, int.MaxValue / 2);
        }

        private void HealWorld()
        {
            CloseBridge();
            foreach (var cell in _gap.ToList()) _world.World.Tiles.Place(cell, _config.HealedGapTile.Id, "мир снова един");
            _gap.Clear();
            _sides.Clear();
        }

        private void RaiseOutside(int count, SagaFaction faction)
        {
            var world = _world.World;
            var grid = world.Grid;
            var sides = new[] { new GridCoord(-1, 0), new GridCoord(1, 0), new GridCoord(0, -1), new GridCoord(0, 1) };
            foreach (var direction in _world.Shuffled(sides))
            {
                var edge = new GridCoord(
                    direction.X < 0 ? 0 : direction.X > 0 ? grid.Width - 1 : grid.Center.X,
                    direction.Y < 0 ? 0 : direction.Y > 0 ? grid.Height - 1 : grid.Center.Y);
                var gate = new GridCoord(edge.X - direction.X, edge.Y - direction.Y);
                var along = direction.X != 0 ? new GridCoord(0, 1) : new GridCoord(1, 0);
                var island = Enumerable.Range(0, count)
                    .Select(i => (i + 1) / 2 * (i % 2 == 0 ? 1 : -1))
                    .Select(offset => new GridCoord(edge.X + along.X * offset, edge.Y + along.Y * offset))
                    .Where(cell => grid.InBounds(cell) && !grid.Contains(cell)
                        && grid.Neighbors(cell).All(neighbor => !grid.Contains(neighbor) || neighbor == gate))
                    .ToList();
                if (island.Count == 0 || !grid.InBounds(gate)) continue;

                grid.Grant(gate);
                grid.Unlock(gate);
                world.Tiles.Place(gate, _config.BridgeTile.Id, "мост к небесному пределу");
                _heavenGate.Add(gate);
                foreach (var cell in island)
                {
                    grid.Grant(cell);
                    grid.Unlock(cell);
                    world.Tiles.Place(cell, faction.Definition.Tile.Id, "небесный предел");
                    _heaven.Add(cell);
                }

                return;
            }
        }

        private void RefreshLands()
        {
            var world = _world.World;
            foreach (var faction in _factions)
            {
                faction.Lands.Clear();
                if (IsSplit)
                {
                    foreach (var (cell, side) in _sides)
                        if (side == faction.Side && world.Tiles.All.ContainsKey(cell)) faction.Lands.Add(cell);
                }
                else if (faction.Definition.HoldsSettlements)
                {
                    var enemyLands = world.Tiles.All.Keys.Where(cell => _factions.Any(other => other != faction && !other.Definition.HoldsSettlements && IsTileOf(other, cell))).ToList();
                    foreach (var cell in world.Settlements.All.Keys)
                    {
                        if (enemyLands.Contains(cell)) continue;
                        if (enemyLands.Count == 0 || enemyLands.Any(enemy => Math.Max(Math.Abs(enemy.X - cell.X), Math.Abs(enemy.Y - cell.Y)) <= ContestRadius)) faction.Lands.Add(cell);
                    }
                }
                else
                {
                    foreach (var cell in world.Tiles.All.Keys)
                        if (IsTileOf(faction, cell)) faction.Lands.Add(cell);
                }

                if (faction.Side == 0)
                    foreach (var cell in _heaven) faction.Lands.Add(cell);
            }
        }

        private bool IsTileOf(SagaFaction faction, GridCoord cell) => _world.World.Tiles.TryGet(cell, out var id) && id == faction.Definition.Tile.Id;

        private void RefreshPower()
        {
            var total = _factions.Sum(faction => faction.Lands.Count);
            var world = Math.Max(1, _world.World.Tiles.CountedCount);
            foreach (var faction in _factions)
            {
                var share = _factions.Count > 1 ? (total == 0 ? 50 : faction.Lands.Count * FullPower / total) : Math.Min(FullPower, faction.Lands.Count * FullPower * 2 / world);
                faction.Power = Math.Clamp(share + faction.Bonus + (faction.HasArtifact ? _config.ArtifactPower : 0), 0, FullPower);
                if (faction.Bonus > 0) faction.Bonus--;
                else if (faction.Bonus < 0) faction.Bonus++;
            }
        }

        private void Spread(int year)
        {
            var world = _world.World;
            foreach (var faction in _factions)
            {
                if (Act.ArtifactHunt && !faction.HasArtifact && world.Random.Next(100) < (faction.Definition.ArtifactChancePercent > 0 ? faction.Definition.ArtifactChancePercent : _config.ArtifactChancePercent)) GiveArtifact(faction);
                if (faction.Definition.HoldsSettlements) continue;

                for (var i = 0; i < faction.Definition.SpreadCells; i++)
                {
                    if (TargetFor(faction) is not { } cell) break;

                    if (world.Features.TryGet(cell, out _)) world.Features.Remove(cell);
                    if (world.Settlements.TryGet(cell, out _)) world.Settlements.Weaken(cell, 1);
                    world.Tiles.Place(cell, faction.Definition.Tile.Id, $"{Act.Title}: {faction.Definition.DisplayName}");
                    foreach (var other in _factions) other.Lands.Remove(cell);
                    faction.Lands.Add(cell);
                }
            }
        }

        private GridCoord? TargetFor(SagaFaction faction)
        {
            var world = _world.World;
            var over = faction.Definition.SpreadsOver;
            var candidates = world.Tiles.All
                .Where(pair => _world.IsShaped(pair.Key) && !world.Guards.IsProtected(pair.Key) && !faction.Lands.Contains(pair.Key) && (OwnerOf(pair.Key) == null || OwnerOf(pair.Key).Definition.HoldsSettlements))
                .Where(pair => !IsSplit || (_sides.TryGetValue(pair.Key, out var side) && side == faction.Side))
                .Where(pair => over.Count == 0 || over.Any(tile => tile.Id == pair.Value))
                .Select(pair => pair.Key)
                .ToList();
            var reach = faction.Side == 0 ? faction.Lands.Concat(_heavenGate).ToHashSet() : faction.Lands;
            var near = candidates.Where(cell => world.Grid.Neighbors(cell).Any(reach.Contains)).ToList();
            return _world.Patterns.Rank($"saga:{faction.Definition.name}", faction.Definition.Pattern, faction.Lands, near.Count > 0 ? near : candidates)
                .Cast<GridCoord?>().FirstOrDefault();
        }

        private void Battle()
        {
            if (_factions.Count < 2 || (IsSplit && !BridgeOpen)) return;

            var world = _world.World;
            var (first, second) = (_factions[0], _factions[1]);
            var firstRoll = first.Power + world.Random.Next(40);
            var secondRoll = second.Power + world.Random.Next(40);
            var (winner, loser) = firstRoll >= secondRoll ? (first, second) : (second, first);
            if (Frontier(loser, winner) is not { } cell) return;

            Conquer(cell, winner, $"{winner.Definition.DisplayName} взяли землю в битве");
            winner.Bonus += _config.BattlePower;
            loser.Bonus -= _config.BattlePower;
            world.Memory.Add(cell, "war");
            Battled?.Invoke(winner, loser, cell);
        }

        private GridCoord? Frontier(SagaFaction loser, SagaFaction winner)
        {
            var world = _world.World;
            var lands = loser.Lands.Where(cell => _world.IsShaped(cell) && !world.Guards.IsProtected(cell) && !_heaven.Contains(cell)).ToList();
            if (lands.Count == 0) return null;

            if (IsSplit && Bridge is { } bridge)
                return lands.OrderBy(cell => Math.Abs(cell.X - bridge.X) + Math.Abs(cell.Y - bridge.Y)).ThenBy(cell => cell.Y).First();

            var front = lands.Where(cell => world.Grid.Neighbors(cell).Any(winner.Lands.Contains)).ToList();
            return _world.Shuffled(front.Count > 0 ? front : lands).First();
        }

        private void Conquer(GridCoord cell, SagaFaction winner, string cause)
        {
            var world = _world.World;
            if (world.Settlements.TryGet(cell, out _)) world.Settlements.Weaken(cell, 1);
            world.Tiles.Place(cell, winner.Definition.Tile.Id, cause);
            if (IsSplit) _sides[cell] = winner.Side;
            if (winner.Definition.HoldsSettlements && !world.Settlements.TryGet(cell, out _)) world.Settlements.TrySettle(cell, 2);
            foreach (var faction in _factions) faction.Lands.Remove(cell);
            winner.Lands.Add(cell);
        }

        private void Deeds()
        {
            var world = _world.World;
            var models = _factions.Select(faction => faction.Mind).ToList();
            foreach (var faction in _factions)
            {
                var deeds = faction.Definition.Deeds;
                var lands = faction.Lands.Where(_world.IsShaped).ToList();
                if (deeds.Count == 0 || lands.Count == 0) continue;

                faction.Mind.Measure(faction.Power);
                var options = deeds.Select(deed => new MindOption(deed.Name, deed.Nature)).ToList();
                if (faction.Mind.Decide(options, models, world.Random) is not { } decision) continue;

                var deed = deeds.First(candidate => candidate.Name == decision.Option.Id);
                var site = _world.Shuffled(lands).First();
                foreach (var outcome in deed.Outcomes) outcome.Apply(world, site, $"{faction.Leader}: {deed.Name}");
                faction.LastDeed = $"{deed.Name} ({decision.Reason}), {world.Clock.Year} год";
                world.History.Note(site, $"{faction.Leader}: {deed.Name}");
                Acted?.Invoke(faction, deed.Name, site);
            }
        }

        private void Intrigue()
        {
            var world = _world.World;
            foreach (var faction in _factions)
            {
                if (world.Random.Next(100) < _config.CoupChancePercent && faction.Definition.LeaderNames.Count > 1)
                {
                    var old = faction.Leader;
                    faction.LeaderIndex++;
                    faction.Mind.Forget();
                    faction.Bonus -= 5;
                    Overthrown?.Invoke(faction, old);
                }

                if (_factions.Count < 2 || world.Random.Next(100) >= _config.BetrayalChancePercent) continue;

                var other = _factions.First(candidate => candidate != faction);
                var turncoat = _world.Shuffled(faction.Lands.Where(cell => _world.IsShaped(cell) && !_heaven.Contains(cell) && !world.Guards.IsProtected(cell))).Cast<GridCoord?>().FirstOrDefault();
                if (turncoat is not { } cell) continue;

                Conquer(cell, other, $"перебежчики ушли к стороне «{other.Definition.DisplayName}»");
                Betrayed?.Invoke(faction, other, cell);
            }
        }

        private void TendBridge(int year)
        {
            if (Act.BridgeAlwaysOpen) return;
            if (BridgeOpen && year >= _bridgeUntil) CloseBridge();
            else if (!BridgeOpen && year >= _nextBridge) OpenBridge(year, Act.BridgeYears);
        }

        private void CloseBridge()
        {
            if (Bridge is not { } bridge) return;

            Bridge = null;
            if (_gap.Contains(bridge)) _world.World.Tiles.Place(bridge, _config.GapTile.Id, "мост между мирами закрылся");
            if (Act != null) _nextBridge = _world.World.Clock.Year + Act.BridgeMinGap + _world.World.Random.Next(Act.BridgeMaxGap - Act.BridgeMinGap + 1);
            BridgeClosed?.Invoke();
        }

        private void TryTurn()
        {
            foreach (var transition in Act.Transitions)
            {
                if (transition.Condition == null || !transition.Condition.Holds(this) || _world.World.Random.Next(100) >= transition.ChancePercent) continue;

                Turned?.Invoke(transition);
                if (transition.NextAct != null) BeginAct(transition.NextAct);
                else Finish(transition);
                return;
            }
        }

        private void Finish(SagaTransition transition)
        {
            var saga = Current;
            foreach (var law in Act.Laws) law.End(_world);
            if (IsSplit) HealWorld();
            if (transition.Restore != null)
                foreach (var faction in _factions.Where(faction => !faction.Definition.HoldsSettlements))
                foreach (var cell in faction.Lands.Where(cell => _world.IsShaped(cell) && !_heaven.Contains(cell)).ToList())
                    _world.World.Tiles.Place(cell, transition.Restore.Id, $"конец саги: {saga.Title}");

            Current = null;
            Act = null;
            _factions.Clear();
            Ended?.Invoke(saga, transition);
            if (transition.NextSaga != null) Begin(transition.NextSaga);
        }
    }
}
