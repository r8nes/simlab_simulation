using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core;
using Godland.Core.Powers.Gods;
using Godland.Core.World.Grid;
using Godland.Core.Life.Minds;
using Godland.Game.Fate.Emergence;
using Godland.Game.Powers.Gods;

namespace Godland.Game.Life.Races
{
    public sealed class RaceKeeper : IWorldStep
    {
        private readonly RacesConfig _config;
        private readonly EmergenceContext _context;
        private readonly Temples _temples;
        private readonly GodsConfig _gods;
        private readonly IMindRules _mindRules;
        private readonly List<Race> _races = new();
        private readonly Dictionary<Race, string> _suggestions = new();

        public RaceKeeper(RacesConfig config, EmergenceContext context, Temples temples, GodsConfig gods, IMindRules mindRules)
        {
            _mindRules = mindRules;
            _config = config;
            _context = context;
            _temples = temples;
            _gods = gods;
            _context.Settlements.Migrated += OnMigrated;
        }

        public event Action<Race, GridCoord> Born;
        public event Action<Race> Vanished;
        public event Action<Race, Race> Diverged;
        public event Action<Race, RaceAction, GridCoord, string> Acted;

        public IReadOnlyList<Race> All => _races;
        public IEnumerable<Race> Living => _races.Where(race => race.IsAlive);

        public Race RaceAt(GridCoord cell) => _races.FirstOrDefault(race => race.Cells.Contains(cell));

        public void Advance(int year)
        {
            Prune();
            if (year % _config.CheckPeriod == 0) TryBirth(year);
            if (year % _config.DeedPeriod == 0)
                foreach (var race in Living.ToList())
                {
                    race.Genome.Mutate(_context.Random, _config.MutationStep);
                    Act(race);
                    TryDiverge(race, year);
                }
        }

        private void TryBirth(int year)
        {
            foreach (var definition in _config.Races.Where(definition => _races.All(race => race.Definition != definition)).ToList())
                if (!TryByGod(definition, year)) TryByChance(definition, year);
        }

        private bool TryByGod(RaceDefinition definition, int year)
        {
            var god = _context.Pantheon.Gods.FirstOrDefault(candidate => Fits(definition, candidate, year));
            if (god == null || HomeOf(god) is not { } cell) return false;

            var name = _gods.NameOf(god);
            if (!Found(definition, cell, year, name, $"бог {name} создал народ: {definition.DisplayName}")) return false;

            _context.Shrines.Impose(cell, god.Religion, Godland.Core.Life.People.BeliefChange.Calling);
            return true;
        }

        private void TryByChance(RaceDefinition definition, int year)
        {
            foreach (var origin in definition.Origins.Where(origin => _context.Epoch.Index >= origin.MinEpoch))
            {
                var sites = origin.Condition.Sites(_context).Where(cell => cell != _context.Grid.Center && RaceAt(cell) == null).ToList();
                if (sites.Count == 0 || _context.Random.Next(100) >= origin.ChancePercent) continue;

                var site = sites[_context.Random.Next(sites.Count)];
                origin.Condition.Consume(_context, site);
                if (Found(definition, site, year, null, origin.Text)) return;
            }
        }

        public bool Summon(GridCoord cell, string cause)
        {
            var unborn = _config.Races.Where(definition => _races.All(race => race.Definition != definition)).ToList();
            if (unborn.Count == 0) return false;

            return Found(unborn[_context.Random.Next(unborn.Count)], cell, _context.Clock.Year, null, cause);
        }

        private bool Found(RaceDefinition definition, GridCoord cell, int year, string godName, string cause)
        {
            if (_context.Features.TryGet(cell, out _)) _context.Features.Remove(cell);
            _context.Tiles.Place(cell, definition.Home.Id, cause);
            if (!_context.Settlements.TryGet(cell, out _)) _context.Settlements.TrySettle(cell, definition.Population);
            if (!_context.Settlements.TryGet(cell, out _)) return false;

            var race = new Race(definition, godName, cause, year, new Mind(_mindRules), Mutated(definition.Genome.Copy(), 3));
            race.Cells.Add(cell);
            _races.Add(race);
            _context.History.Note(cell, cause);
            _context.Memory.Add(_context.Grid.Center, "races");
            Born?.Invoke(race, cell);
            return true;
        }

        private bool Fits(RaceDefinition definition, God god, int year) =>
            definition.GodId != "-" && (string.IsNullOrEmpty(definition.GodId)
                ? god.HeroName == null
                : god.Rule.Id == definition.GodId && god.HeroName == null)
            && year - god.BornYear >= _config.MinGodAge
            && _temples.CountOf(god) >= _config.MinTemples;

        private GridCoord? HomeOf(God god) =>
            _context.Grid.Neighbors(god.Seat).Append(god.Seat)
                .Where(cell => cell != god.Seat && cell != _context.Grid.Center && _context.Tiles.All.ContainsKey(cell))
                .Where(cell => !_context.Guards.IsProtected(cell) && RaceAt(cell) == null)
                .OrderBy(cell => _context.Settlements.TryGet(cell, out _) ? 1 : 0)
                .ThenBy(_ => _context.Random.Next(1000))
                .Cast<GridCoord?>()
                .FirstOrDefault();

        private void Act(Race race)
        {
            foreach (var cell in race.Cells)
            foreach (var imprint in race.Definition.Imprints)
                _context.Memory.Add(imprint.Global ? _context.Grid.Center : cell, imprint.Key, imprint.Amount);

            if (race.IsVeteran)
                foreach (var cell in race.Cells.ToList()) _context.Settlements.Bless(cell, 1);
            race.Mind.Measure(Strength(race));
            var actions = race.Definition.Actions;
            var options = actions.Select(action => new MindOption(action.Name, SiteOf(race, action.Gate) == null ? 0 : race.Genome.Bias(action, action.Nature))).ToList();
            var models = Living.Select(other => other.Mind).ToList();
            var suggested = _suggestions.Remove(race, out var hint) ? actions.FirstOrDefault(action => action.Name == hint && SiteOf(race, action.Gate) != null) : null;
            var decision = suggested != null ? (Godland.Core.Life.Minds.MindDecision?)null : race.Mind.Decide(options, models, _context.Random);
            if (suggested == null && decision == null) return;

            var chosen = suggested ?? actions.First(action => action.Name == decision.Value.Option.Id);
            var reason = suggested != null ? "по подсказке свыше" : decision.Value.Reason;
            var site = SiteOf(race, chosen.Gate).Value;
            var before = _context.Settlements.All.Keys.ToHashSet();
            foreach (var outcome in chosen.Outcomes) outcome.Apply(_context, site, $"{race.Name}: {chosen.Name}");
            foreach (var cell in _context.Settlements.All.Keys.Where(cell => !before.Contains(cell) && RaceAt(cell) == null).ToList())
            {
                race.Cells.Add(cell);
                race.Genome.Mutate(_context.Random, _config.MutationStep);
            }
            _context.Memory.Add(site, $"race-act:{chosen.Name}");
            _context.History.Note(site, $"{race.Name}: {chosen.Name} ({reason})");
            race.LastDeed = $"{chosen.Name} ({reason}), {_context.Clock.Year} год";
            Acted?.Invoke(race, chosen, site, reason);
        }

        public void Suggest(Race race, string actionName) => _suggestions[race] = actionName;

        public bool Migrate(Race race, GridCoord target)
        {
            var source = _context.Grid.Neighbors(target)
                .Where(cell => race.Cells.Contains(cell) && _context.Settlements.TryGet(cell, out var population) && population >= 2)
                .OrderByDescending(cell => _context.Settlements.All[cell]).Cast<GridCoord?>().FirstOrDefault();
            if (source is not { } from) return false;

            var movers = _context.Settlements.All[from] / 2;
            if (!_context.Settlements.TrySettle(target, movers)) return false;

            _context.Settlements.Weaken(from, movers);
            race.Cells.Add(target);
            return true;
        }

        public Race Conquer(string cause)
        {
            var victor = Living.OrderByDescending(Strength).FirstOrDefault();
            if (victor == null) return null;

            foreach (var loser in Living.Where(race => race != victor).ToList())
            {
                victor.Mind.Inherit(loser.Mind);
                victor.Genome.Blend(loser.Genome, _config.BlendPercent);
                foreach (var cell in loser.Cells.ToList())
                {
                    if (_context.Tiles.All.ContainsKey(cell) && cell != _context.Grid.Center && !_context.Guards.IsProtected(cell))
                        _context.Tiles.Place(cell, victor.Definition.Home.Id, cause);
                    victor.Cells.Add(cell);
                }

                loser.Cells.Clear();
            }

            return victor;
        }

        public void Absorb(Race loser, Race winner) => loser.Genome.Blend(winner.Genome, _config.BlendPercent);

        private RaceGenome Mutated(RaceGenome genome, int times)
        {
            for (var i = 0; i < times; i++) genome.Mutate(_context.Random, _config.MutationStep);
            return genome;
        }

        private void TryDiverge(Race race, int year)
        {
            if (race.Cells.Count < _config.SplitMinCells || race.Genome.Distance(race.Definition.Genome) < _config.SplitDistance) return;

            var name = $"{race.Genome.Epithet()} {race.Definition.DisplayName.ToLowerInvariant()}";
            if (_races.Any(other => other.IsAlive && other.Name == name)) return;

            var origin = _context.Grid.Center;
            var leaving = race.Cells.OrderByDescending(cell => Math.Abs(cell.X - origin.X) + Math.Abs(cell.Y - origin.Y)).ThenBy(cell => cell.X)
                .Take(race.Cells.Count / 2).ToList();
            var mind = new Mind(_mindRules);
            mind.Inherit(race.Mind);
            var child = new Race(race.Definition, null, $"откололись от народа «{race.Name}»: их нрав ушёл слишком далеко", year, mind, race.Genome.Copy(), name) { Parent = race };
            foreach (var cell in leaving)
            {
                race.Cells.Remove(cell);
                child.Cells.Add(cell);
                _context.History.Note(cell, $"здесь народ «{race.Name}» раскололся: {name}");
            }

            race.Genome.Blend(race.Definition.Genome, 50);
            _races.Add(child);
            _context.Memory.Add(_context.Grid.Center, "race-split");
            Diverged?.Invoke(race, child);
        }

        public int Strength(Race race) =>
            race.Cells.Sum(cell => _context.Settlements.TryGet(cell, out var population) ? population : 0) + race.Cells.Count * 5;

        private GridCoord? SiteOf(Race race, RaceActionGate gate) =>
            race.Cells.Where(cell => Allows(race, cell, gate)).OrderBy(cell => cell.X).ThenBy(cell => cell.Y)
                .Cast<GridCoord?>().FirstOrDefault();

        private bool Allows(Race race, GridCoord cell, RaceActionGate gate) => gate switch
        {
            RaceActionGate.ForeignNeighbor => _context.Grid.Neighbors(cell).Any(other => _context.Settlements.TryGet(other, out _) && !race.Cells.Contains(other)),
            RaceActionGate.OpenLand => _context.Grid.Neighbors(cell).Any(other => _context.Tiles.All.ContainsKey(other) && !_context.Settlements.TryGet(other, out _)),
            RaceActionGate.Hungry => _context.Settlements.IsHungry(cell),
            RaceActionGate.Crowded => _context.Settlements.TryGet(cell, out var population) && population >= _context.Settlements.CapacityOf(cell),
            _ => true
        };

        private void OnMigrated(GridCoord from, GridCoord to)
        {
            var race = RaceAt(from);
            if (race != null && RaceAt(to) == null) race.Cells.Add(to);
        }

        private void Prune()
        {
            foreach (var race in Living.ToList())
            {
                race.Cells.RemoveWhere(cell => !_context.Settlements.TryGet(cell, out _));
                if (race.IsAlive) continue;

                _context.Memory.Add(_context.Grid.Center, "race-lost");
                Vanished?.Invoke(race);
            }
        }
    }
}
