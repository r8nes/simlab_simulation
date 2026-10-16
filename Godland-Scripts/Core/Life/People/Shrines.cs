using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Cells;
using Godland.Core.World.Grid;
using Godland.Core.Randomness;
using Godland.Core.World.Tiles;

namespace Godland.Core.Life.People
{
    public sealed class Shrines
    {
        private readonly WorldGrid _grid;
        private readonly WorldTiles _tiles;
        private readonly Settlements _settlements;
        private readonly ISettlementRules _settlementRules;
        private readonly IShrineRules _rules;
        private readonly SeededRandom _random;
        private readonly FaithPoints _faith;
        private readonly CellFertility _fertility;
        private readonly IdolOracle _oracle;
        private readonly WorldModifiers _modifiers;
        private int _year;
        private readonly Dictionary<GridCoord, Religion> _believers = new();
        private Religion _unity;
        private Func<GridCoord, bool> _faithBlocked = _ => false;

        public Shrines(
            WorldGrid grid, WorldTiles tiles, Settlements settlements, ISettlementRules settlementRules,
            IShrineRules rules, SeededRandom random, FaithPoints faith, CellFertility fertility, IdolOracle oracle,
            WorldModifiers modifiers)
        {
            _grid = grid;
            _tiles = tiles;
            _settlements = settlements;
            _settlementRules = settlementRules;
            _rules = rules;
            _random = random;
            _faith = faith;
            _fertility = fertility;
            _oracle = oracle;
            _modifiers = modifiers;

            _tiles.Cleared += Desecrate;
            _settlements.Abandoned += Forsake;
            _settlements.Migrated += CarryFaith;
            _settlements.SetHuntTaboo(IsHuntTaboo);
            _settlements.HuntFinished += (settlement, _) => RecordHunt(settlement);
        }

        public event Action<GridCoord, Religion, Religion, BeliefChange> BeliefChanged;
        public event Action<Religion, GridCoord> IdolRevealed;

        public int Count => _believers.Values.Select(religion => religion.Shrine).Distinct().Count();

        public int BelieversOf(GridCoord shrine) => _believers.Values.Count(religion => religion.Shrine == shrine);

        public bool IsShrine(GridCoord coord) => _believers.Values.Any(religion => religion.Shrine == coord);

        public bool TryGet(GridCoord settlement, out Religion religion) => _believers.TryGetValue(settlement, out religion);

        public void Update(int year)
        {
            _year = year;
            if (year % _settlementRules.FaithPeriod == 0)
            {
                _faith.Add(_believers.Values.Sum(religion => _rules.ShrineFaith + religion.Dogma.FaithBonus));
                Kindle();
            }
            if (year % _settlementRules.GrowthPeriod == 0) Bless();
            if (year % _rules.ShrinePeriod == 0) DeifyAll();
            if (year % _rules.ConversionPeriod == 0) SpreadFaith();
            if (year % _rules.SchismPeriod == 0) Schisms();
        }

        public bool CanDeify(GridCoord settlement) =>
            _settlements.TryGet(settlement, out _) && !_believers.ContainsKey(settlement) && SacredNeighbors(settlement).Count > 0;

        public void Deify(GridCoord settlement)
        {
            var candidates = SacredNeighbors(settlement);
            if (candidates.Count == 0) return;

            var shrine = candidates[_random.Next(candidates.Count)];
            var religion = _believers.Values.FirstOrDefault(existing => existing.Shrine == shrine && !existing.IsHeresy)
                ?? new Religion(shrine, DogmaOf(shrine), false);
            Believe(settlement, religion, BeliefChange.Revelation);
        }

        public void SetFaithBlock(Func<GridCoord, bool> blocked) => _faithBlocked = blocked;

        public void Renounce(GridCoord settlement)
        {
            Lose(settlement, BeliefChange.Renounced);
        }

        public void DesecrateShrine(GridCoord shrine) => Desecrate(shrine);

        public void Impose(GridCoord settlement, Religion religion, BeliefChange reason = BeliefChange.Conquest)
        {
            if (_settlements.TryGet(settlement, out _)) Believe(settlement, religion, reason);
        }

        public Religion Ordain(GridCoord seat, Dogma dogma, string godId) => new(seat, dogma, false, godId);

        public int BelieverCount => _believers.Count;

        public IReadOnlyList<Religion> Religions => _believers.Values.Distinct().ToList();

        public void Witness(GridCoord settlement)
        {
            if (_believers.TryGetValue(settlement, out var current)) current.Cool(_rules.MiracleFervorLoss);
        }

        public Religion MergeHeresies(int fervor)
        {
            var heresies = Religions.Where(religion => religion.IsHeresy).ToList();
            if (heresies.Count == 0) return null;

            var largest = heresies.OrderByDescending(religion => FollowersOf(religion).Count).First();
            var church = new Religion(largest.Shrine, largest.Dogma, false);
            church.Kindle(fervor);
            foreach (var heresy in heresies)
                foreach (var follower in FollowersOf(heresy))
                    Believe(follower, church, BeliefChange.Conversion);
            return church;
        }

        public Religion Unity => _unity;

        public void Unify(Religion religion)
        {
            _unity = religion;
            foreach (var (settlement, faith) in _believers.ToList())
                if (Absorbs(faith))
                    Believe(settlement, religion, BeliefChange.Conversion);
        }

        private bool Absorbs(Religion faith) => _unity != null && faith != _unity && !faith.IsHeresy;

        private void Believe(GridCoord settlement, Religion religion, BeliefChange reason)
        {
            if (_faithBlocked(settlement)) return;
            if (Absorbs(religion)) religion = _unity;

            _believers.TryGetValue(settlement, out var previous);
            _believers[settlement] = religion;
            if (previous != religion && (reason == BeliefChange.Conversion || reason == BeliefChange.Calling)) _convertedYear[settlement] = _year;
            BeliefChanged?.Invoke(settlement, previous, religion, reason);
        }

        private void Lose(GridCoord settlement, BeliefChange reason)
        {
            if (_believers.Remove(settlement, out var previous))
                BeliefChanged?.Invoke(settlement, previous, null, reason);
        }

        private Dogma DogmaOf(GridCoord shrine) => _tiles.TryGet(shrine, out var tileId) ? _rules.DogmaFor(tileId) : _rules.DefaultDogma;

        private void DeifyAll()
        {
            foreach (var settlement in _settlements.All.Keys.ToList())
                if (CanDeify(settlement) && _random.Next(100) < _rules.ShrineChancePercent)
                    Deify(settlement);
        }

        private List<GridCoord> SacredNeighbors(GridCoord settlement)
        {
            var sacred = _settlements.CultureOf(settlement)?.SacredTileIds;
            return sacred == null
                ? new List<GridCoord>()
                : _grid.Neighbors(settlement).Where(cell => _tiles.TryGet(cell, out var id) && sacred.Contains(id)).ToList();
        }

        private void Bless()
        {
            foreach (var (settlement, religion) in _believers.ToList())
            {
                if (religion.Dogma.NeighborFertility == 0) continue;

                foreach (var neighbor in _grid.Neighbors(settlement))
                    _fertility.Add(neighbor, religion.Dogma.NeighborFertility);
            }
        }

        private const int ZealYears = 40;
        private const int SteadfastYears = 120;
        private readonly Dictionary<GridCoord, int> _convertedYear = new();

        public bool IsZealous(GridCoord settlement) => _convertedYear.TryGetValue(settlement, out var year) && _year - year < ZealYears;

        public bool IsSteadfast(GridCoord settlement) =>
            _convertedYear.TryGetValue(settlement, out var year) && _year - year >= ZealYears && _year - year < ZealYears + SteadfastYears;

        private void SpreadZeal()
        {
            foreach (var preacher in _convertedYear.Keys.Where(IsZealous).OrderBy(cell => cell.X).ThenBy(cell => cell.Y).ToList())
            {
                if (!_believers.TryGetValue(preacher, out var religion)) continue;

                foreach (var neighbor in _grid.Neighbors(preacher).ToList())
                {
                    if (!_settlements.All.ContainsKey(neighbor) || IsZealous(neighbor) || IsSteadfast(neighbor)) continue;
                    if (_believers.TryGetValue(neighbor, out var own) && own == religion) continue;
                    if (_random.Next(100) < (_rules.ConversionChancePercent + religion.Dogma.ConversionBonusPercent) / 4)
                        Believe(neighbor, religion, BeliefChange.Conversion);
                }
            }
        }

        private void SpreadFaith()
        {
            SpreadZeal();
            foreach (var settlement in _settlements.All.Keys.ToList())
            {
                if (_believers.ContainsKey(settlement)) continue;

                var teachers = _grid.Neighbors(settlement).Where(_believers.ContainsKey).ToList();
                if (teachers.Count == 0) continue;

                var religion = _believers[teachers[_random.Next(teachers.Count)]];
                if (_random.Next(100) < _rules.ConversionChancePercent + religion.Dogma.ConversionBonusPercent)
                    Believe(settlement, religion, BeliefChange.Conversion);
            }
        }

        private void Schisms()
        {
            foreach (var religion in _believers.Values.Distinct().ToList())
            {
                var followers = _believers.Where(pair => pair.Value == religion).Select(pair => pair.Key).ToList();
                if (followers.Count < _rules.SchismMinFollowers || _random.Next(100) >= _rules.SchismChancePercent) continue;

                var dogmas = _rules.HeresyDogmas.Where(dogma => dogma.Id != religion.Dogma.Id).ToList();
                if (dogmas.Count == 0) continue;

                var heresy = new Religion(religion.Shrine, dogmas[_random.Next(dogmas.Count)], true);
                var origin = religion.Shrine ?? followers[0];
                foreach (var follower in followers.OrderByDescending(coord => Distance(coord, origin)).Take(followers.Count / 2))
                    Believe(follower, heresy, BeliefChange.Schism);
            }
        }

        public IReadOnlyList<GridCoord> FollowersOf(Religion religion) =>
            _believers.Where(pair => pair.Value == religion).Select(pair => pair.Key).ToList();

        private void Kindle()
        {
            foreach (var religion in _believers.Values.Distinct().ToList())
            {
                var followers = FollowersOf(religion);
                if (followers.Count == 0) continue;

                var fervor = followers.Count + religion.Dogma.FaithBonus;
                religion.Kindle(_modifiers.IsNight(_year) ? fervor * 2 : fervor);
                if (religion.IsGod || religion.IdolId != null || religion.Fervor < _rules.IdolRevealFervor) continue;

                var idol = _oracle.Choose(religion, followers);
                if (idol == null) continue;

                religion.Reveal(idol);
                IdolRevealed?.Invoke(religion, followers[0]);
            }
        }

        private void RecordHunt(GridCoord settlement)
        {
            if (_believers.TryGetValue(settlement, out var religion)) religion.RecordHunt();
        }

        private static int Distance(GridCoord a, GridCoord b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

        private bool IsHuntTaboo(GridCoord settlement) => _believers.TryGetValue(settlement, out var religion) && religion.Dogma.HuntTaboo;

        private void CarryFaith(GridCoord from, GridCoord to)
        {
            if (_believers.TryGetValue(from, out var religion) && !_believers.ContainsKey(to))
                Believe(to, religion, BeliefChange.Migration);
        }

        private void Desecrate(GridCoord coord)
        {
            if (_unity != null && _unity.Shrine == coord && TryMoveUnity()) return;

            foreach (var (settlement, religion) in _believers.Where(pair => pair.Value.Shrine == coord).ToList())
            {
                Lose(settlement, BeliefChange.Desecration);
                if (!religion.Dogma.ResistsDesecration)
                    _settlements.Weaken(settlement, _rules.DesecrationPopulationLoss);
            }
        }

        private bool TryMoveUnity()
        {
            var followers = FollowersOf(_unity);
            if (followers.Count == 0) return false;

            var seat = followers.OrderByDescending(cell => _settlements.All.TryGetValue(cell, out var population) ? population : 0).First();
            var church = new Religion(seat, _unity.Dogma, false);
            church.Kindle(_unity.Fervor);
            if (_unity.IdolId != null) church.Reveal(_unity.IdolId);
            var old = _unity;
            _unity = church;
            foreach (var follower in followers) Believe(follower, church, BeliefChange.Conversion);
            return old != church;
        }

        private void Forsake(GridCoord settlement)
        {
            if (_believers.TryGetValue(settlement, out var religion)) religion.RecordLoss();
            Lose(settlement, BeliefChange.Abandoned);
        }
    }
}
