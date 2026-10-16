using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.Powers.Gods;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Core.Randomness;
using Godland.Core.World.Tiles;

namespace Godland.Core.Powers.Titans
{
    public sealed class TitanHost
    {
        private const int SleeperReach = 2;
        private const int SleeperRage = 100;
        private const int MaxAwake = 2;
        private const int AwakenGapYears = 200;

        private readonly WorldGrid _grid;
        private readonly WorldTiles _tiles;
        private readonly Settlements _settlements;
        private readonly Pantheon _pantheon;
        private readonly Temples _temples;
        private readonly Plague _plague;
        private readonly Shrines _shrines;
        private readonly TileGuards _guards;
        private readonly WorldModifiers _modifiers;
        private readonly SeededRandom _random;
        private readonly List<Titan> _titans = new();
        private readonly List<(GridCoord Cell, string From, string To, int Year)> _regrowth = new();
        private int _year;
        private int _lastAwakened = int.MinValue / 2;

        public TitanHost(
            WorldGrid grid, WorldTiles tiles, Settlements settlements, Pantheon pantheon, Temples temples, Plague plague, TileGuards guards,
            WorldModifiers modifiers, SeededRandom random, Shrines shrines)
        {
            _shrines = shrines;
            _grid = grid;
            _tiles = tiles;
            _settlements = settlements;
            _pantheon = pantheon;
            _temples = temples;
            _plague = plague;
            _guards = guards;
            _modifiers = modifiers;
            _random = random;
        }

        public event Action<Titan> Awoke;
        public event Action<Titan> Moved;
        public event Action<Titan, God> Devoured;
        public event Action<Titan, God, bool> Clashed;
        public event Action<Titan, God> Pacted;
        public event Action<Titan> Roused;
        public event Action<Titan> Tamed;
        public event Action<Titan> Died;
        public event Action<Titan, string> Slain;

        public IReadOnlyList<Titan> All => _titans;

        public bool TryGetAt(GridCoord cell, out Titan titan)
        {
            titan = _titans.FirstOrDefault(candidate => candidate.Position == cell);
            return titan != null;
        }

        public bool CanAwaken => _titans.Count(titan => titan.IsAwake) < MaxAwake && _year - _lastAwakened >= AwakenGapYears;

        public Titan Awaken(TitanRule rule, GridCoord cell, string origin = null)
        {
            if (!CanAwaken) return null;

            _lastAwakened = _year;
            var titan = new Titan(rule, cell, _year) { Origin = origin };
            _titans.Add(titan);
            Awoke?.Invoke(titan);
            return titan;
        }

        public void Slay(Titan titan, string by = "Герои")
        {
            if (!_titans.Remove(titan)) return;

            Slain?.Invoke(titan, by);
            Perish(titan);
        }

        public void Tame(Titan titan, int guardRadius)
        {
            if (titan.IsTamed) return;

            titan.IsTamed = true;
            var home = titan.Position;
            _guards.Add(cell => _titans.Contains(titan) && Math.Max(Math.Abs(cell.X - home.X), Math.Abs(cell.Y - home.Y)) <= guardRadius);
            Tamed?.Invoke(titan);
        }

        public void Disturb(GridCoord cell)
        {
            if (_titans.Count(titan => titan.IsAwake) >= MaxAwake) return;

            foreach (var sleeper in _titans.Where(titan => !titan.IsAwake &&
                         Math.Max(Math.Abs(titan.Position.X - cell.X), Math.Abs(titan.Position.Y - cell.Y)) <= SleeperReach).ToList())
            {
                sleeper.IsAwake = true;
                sleeper.BornYear = _year - Math.Max(0, sleeper.Rule.Lifespan - SleeperRage);
                Roused?.Invoke(sleeper);
            }
        }

        public void Direct(Titan titan, GridCoord cell) => titan.Destination = cell;

        public void Lull(Titan titan, int years)
        {
            if (!titan.IsAwake || titan.IsTamed) return;

            titan.IsAwake = false;
            titan.LulledYear = _year;
            titan.WakeYear = _year + years;
        }

        public void Rouse(Titan titan)
        {
            if (titan.IsAwake) return;

            if (titan.LulledYear > 0) titan.BornYear += _year - titan.LulledYear;
            titan.LulledYear = 0;
            titan.WakeYear = 0;
            titan.IsAwake = true;
            Roused?.Invoke(titan);
        }

        public void Update(int year)
        {
            _year = year;
            Regrow();
            foreach (var sleeper in _titans.Where(titan => !titan.IsAwake && titan.WakeYear > 0 && year >= titan.WakeYear).ToList())
                Rouse(sleeper);
            foreach (var titan in _titans.ToList())
            {
                if (!titan.IsAwake || titan.IsTamed) continue;
                if (year - titan.BornYear >= titan.Rule.Lifespan)
                {
                    _titans.Remove(titan);
                    Died?.Invoke(titan);
                    Perish(titan);
                    continue;
                }

                if ((year - titan.BornYear) % titan.Rule.StepPeriod == 0) Step(titan);
            }
        }

        private void Step(Titan titan)
        {
            var next = NextCell(titan);
            if (next == titan.Position) return;

            titan.Position = next;
            Moved?.Invoke(titan);
            if (!Trample(titan)) return;

            if (titan.Rule.Target == TitanTarget.GodSeats && _pantheon.TryGetAt(next, out var prey) && !titan.Allies.Contains(prey))
            {
                _pantheon.Slay(prey);
                Devoured?.Invoke(titan, prey);
                return;
            }

            FightGods(titan);
        }

        private void FightGods(Titan titan)
        {
            foreach (var god in _pantheon.Gods.Where(god => !titan.Fought.Contains(god) &&
                         Math.Max(Math.Abs(god.Seat.X - titan.Position.X), Math.Abs(god.Seat.Y - titan.Position.Y)) <= 1).ToList())
            {
                titan.Fought.Add(god);
                if (_random.Next(100) < Math.Min(80, 30 + _temples.CountOf(god) * 10))
                {
                    titan.Allies.Add(god);
                    Pacted?.Invoke(titan, god);
                    continue;
                }

                var titanWins = _random.Next(100) < Math.Clamp(60 - _temples.CountOf(god) * 10, 10, 90);
                Clashed?.Invoke(titan, god, titanWins);
                if (!titanWins)
                {
                    Slay(titan, null);
                    return;
                }

                _pantheon.Slay(god);
            }
        }

        private GridCoord NextCell(Titan titan)
        {
            var goal = Goal(titan);
            if (goal is { } target && target != titan.Position)
            {
                var dx = Math.Sign(target.X - titan.Position.X);
                var dy = Math.Sign(target.Y - titan.Position.Y);
                return Math.Abs(target.X - titan.Position.X) >= Math.Abs(target.Y - titan.Position.Y)
                    ? new GridCoord(titan.Position.X + dx, titan.Position.Y)
                    : new GridCoord(titan.Position.X, titan.Position.Y + dy);
            }

            var options = _grid.Neighbors(titan.Position).Where(_tiles.All.ContainsKey).ToList();
            return options.Count == 0 ? titan.Position : options[_random.Next(options.Count)];
        }

        private GridCoord? Goal(Titan titan)
        {
            if (titan.Destination is { } destination)
            {
                if (destination != titan.Position) return destination;
                titan.Destination = null;
            }

            IEnumerable<GridCoord> goals = titan.Rule.Target switch
            {
                TitanTarget.GodSeats => _pantheon.Gods.Where(god => !titan.Allies.Contains(god)).Select(god => god.Seat),
                TitanTarget.Settlements => _settlements.All.Keys.Where(cell => cell != titan.Position),
                TitanTarget.Edges => _tiles.All
                    .Where(pair => !titan.Rule.SpareTileIds.Contains(pair.Value) && pair.Key != titan.Position
                        && _grid.Neighbors(pair.Key).Count(_tiles.All.ContainsKey) < 4)
                    .Select(pair => pair.Key),
                _ => Enumerable.Empty<GridCoord>()
            };
            return goals.OrderBy(cell => Math.Abs(cell.X - titan.Position.X) + Math.Abs(cell.Y - titan.Position.Y))
                .Cast<GridCoord?>().FirstOrDefault();
        }

        private bool Trample(Titan titan)
        {
            var cell = titan.Position;
            if (titan.Rule.Infects && _plague.IsForeverImmune(cell))
            {
                Slay(titan, "Неуязвимые к мору");
                return false;
            }

            foreach (var victim in _grid.Neighbors(cell).Append(cell).Where(target => _settlements.TryGet(target, out _) && !IsSpared(titan, target)).ToList())
            {
                _settlements.Weaken(victim, titan.Rule.Harm);
                if (titan.Rule.Infects) _plague.Strike(victim);
            }

            if (!_tiles.TryGet(cell, out var current) || cell == _grid.Center || _guards.IsProtected(cell)) return true;
            if (titan.Rule.SpareTileIds.Contains(current)) return true;

            var result = _settlements.TryGet(cell, out _) && titan.Rule.SettlementTileId != null ? titan.Rule.SettlementTileId : TrailOf(titan);
            if (result == null || result == current) return true;

            _tiles.Place(cell, result, "след титана");
            if (titan.Rule.RegrowTileId != null) _regrowth.Add((cell, result, titan.Rule.RegrowTileId, _year + titan.Rule.RegrowYears));
            return true;
        }

        private bool IsSpared(Titan titan, GridCoord settlement) =>
            _shrines.TryGet(settlement, out var faith) && titan.Allies.Any(god => god.Religion == faith);

        private string TrailOf(Titan titan)
        {
            var forms = titan.Rule.Forms;
            return forms.Count == 0 ? titan.Rule.TrailTileId : forms[(_year - titan.BornYear) / titan.Rule.ShiftPeriod % forms.Count];
        }

        private void Regrow()
        {
            foreach (var entry in _regrowth.Where(entry => entry.Year <= _year).ToList())
            {
                _regrowth.Remove(entry);
                if (_tiles.TryGet(entry.Cell, out var id) && id == entry.From) _tiles.Place(entry.Cell, entry.To, "земля возродилась после титана");
            }
        }

        private void Perish(Titan titan)
        {
            if (titan.Rule.DeathNightYears > 0) _modifiers.BeginNight(_year + titan.Rule.DeathNightYears);
        }
    }
}
