using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.Fate.Epochs;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Core.Randomness;
using Godland.Core.World.Tiles;
using Godland.Core.World.Volcanoes;

namespace Godland.Core.Powers.Gods
{
    public sealed class Pantheon
    {
        private readonly WorldGrid _grid;
        private readonly WorldTiles _tiles;
        private readonly Settlements _settlements;
        private readonly Shrines _shrines;
        private readonly VolcanoActivity _volcanoes;
        private readonly WorldEpoch _epoch;
        private readonly IGodRules _rules;
        private readonly SeededRandom _random;
        private readonly List<God> _gods = new();
        private readonly HashSet<Religion> _resisting = new();
        private Func<God, int> _power = _ => 0;
        private readonly Dictionary<string, int> _rebirthYear = new();
        private int _year;
        private int? _lifespan;

        public Pantheon(
            WorldGrid grid, WorldTiles tiles, Settlements settlements, Shrines shrines, VolcanoActivity volcanoes,
            WorldEpoch epoch, IGodRules rules, SeededRandom random, TileGuards guards)
        {
            _grid = grid;
            _tiles = tiles;
            _settlements = settlements;
            _shrines = shrines;
            _volcanoes = volcanoes;
            _epoch = epoch;
            _rules = rules;
            _random = random;

            _tiles.Changed += OnSeatChanged;
            guards.Add(cell => TryGetAt(cell, out _));
        }

        public event Action<God> Born;
        public event Action<God> Fallen;
        public event Action<God, GridCoord> Moved;
        public event Action<God> Forgotten;
        public event Action<God> Died;
        public event Action<God, IReadOnlyList<GridCoord>> Acted;
        public event Action<Religion> IdolFaded;
        public event Action<Religion> IdolResisted;

        public IReadOnlyList<God> Gods => _gods;

        public bool TryGetAt(GridCoord seat, out God god)
        {
            god = _gods.FirstOrDefault(candidate => candidate.Seat == seat);
            return god != null;
        }

        public void SetPower(Func<God, int> power) => _power = power;

        public bool AcceptsHeroes { get; private set; }

        private int _suppressions;

        public bool IsSuppressed => _suppressions > 0;

        public void Suppress(bool suppressed) => _suppressions = System.Math.Max(0, _suppressions + (suppressed ? 1 : -1));

        public void MakeMortal(int lifespan) => _lifespan = lifespan;

        public void OpenToHeroes() => AcceptsHeroes = true;

        private readonly HashSet<string> _cursed = new();

        public bool IsCursed(string ruleId) => _cursed.Contains(ruleId);

        public void Curse(string ruleId) => _cursed.Add(ruleId);

        public God Manifest(GridCoord seat, string templateId, string title)
        {
            var template = RuleOf(templateId);
            if (template == null || TryGetAt(seat, out _)) return null;

            var god = new God(template, seat, _shrines.Ordain(seat, template.Dogma, template.Id), _year, title);
            _gods.Add(god);
            _shrines.Impose(seat, god.Religion, BeliefChange.Calling);
            Born?.Invoke(god);
            return god;
        }

        public God Ascend(GridCoord seat, string heroName)
        {
            if (!AcceptsHeroes || IsSuppressed || TryGetAt(seat, out _) || !_rules.Gods.Any(rule => rule.IsElemental)) return null;

            var elemental = _rules.Gods.Where(rule => rule.IsElemental).ToList();
            var template = elemental[_random.Next(elemental.Count)];
            var god = new God(template, seat, _shrines.Ordain(seat, template.Dogma, template.Id), _year, heroName);
            _gods.Add(god);
            _shrines.Impose(seat, god.Religion, BeliefChange.Calling);
            Born?.Invoke(god);
            return god;
        }

        public void Forget(God god)
        {
            if (!_gods.Remove(god)) return;

            _rebirthYear[god.Rule.Id] = _year + _rules.RebirthCooldown;
            Forgotten?.Invoke(god);
        }

        public bool IsGodFaith(Religion religion) => _gods.Any(god => god.Religion == religion);

        public void Update(int year)
        {
            _year = year;
            if (_epoch.Index < _rules.GodsFromEpoch) return;

            if (_lifespan is { } lifespan) Age(lifespan);
            if (year % _rules.BirthPeriod == 0 && !AcceptsHeroes && !IsSuppressed) GiveBirth();
            if (year % _rules.CallPeriod == 0) Call();
            if (year % _rules.FadePeriod == 0 && _gods.Count > 0) FadeIdols();

            foreach (var god in _gods.ToList())
                if (_gods.Contains(god) && god.SilentUntil <= year && god.Tick())
                    Act(god);
        }

        public bool CanBirth(string ruleId) => FindSeat(RuleOf(ruleId)) != null;

        public bool TryBirth(string ruleId)
        {
            var rule = RuleOf(ruleId);
            if (FindSeat(rule) is not { } seat) return false;

            Bear(rule, seat);
            return true;
        }

        private GodRule RuleOf(string ruleId) => _rules.Gods.FirstOrDefault(rule => rule.Id == ruleId);

        public God BirthAs(string ruleId, string title, string pantheonId, Religion faith = null)
        {
            var rule = RuleOf(ruleId);
            if (rule == null || FindSeat(rule, false) is not { } seat) return null;

            var god = new God(rule, seat, faith ?? _shrines.Ordain(seat, rule.Dogma, rule.Id), _year, title, pantheonId);
            _gods.Add(god);
            _shrines.Impose(seat, god.Religion, BeliefChange.Calling);
            Born?.Invoke(god);
            return god;
        }

        public bool CanSeat(string ruleId) => RuleOf(ruleId) is { } rule && FindSeat(rule, false) != null;

        private GridCoord? FindSeat(GodRule rule, bool unique = true)
        {
            if (rule == null || (unique && _gods.Any(god => god.Rule.Id == rule.Id))) return null;

            var domain = _tiles.All.Where(pair => rule.DomainTileIds.Contains(pair.Value)).Select(pair => pair.Key).ToList();
            if (domain.Count == 0) return null;

            var best = domain.Max(cell => DomainNeighbors(cell, rule));
            var seats = domain.Where(cell => DomainNeighbors(cell, rule) == best && !_gods.Any(god => god.Seat == cell)).ToList();
            return seats.Count == 0 ? null : seats[_random.Next(seats.Count)];
        }

        private void Bear(GodRule rule, GridCoord seat)
        {
            var god = new God(rule, seat, _shrines.Ordain(seat, rule.Dogma, rule.Id), _year);
            _gods.Add(god);
            Born?.Invoke(god);
        }

        public int PressureOf(GodRule rule)
        {
            var domain = _tiles.All.Where(pair => rule.DomainTileIds.Contains(pair.Value)).Select(pair => pair.Key).ToHashSet();
            var people = _settlements.All.Keys.Count(cell => domain.Contains(cell) || _grid.Neighbors(cell).Any(domain.Contains));
            return domain.Count + people * _rules.PressurePerSettlement;
        }

        private void GiveBirth()
        {
            var candidate = _rules.Gods
                .Where(rule => rule.IsElemental && !_cursed.Contains(rule.Id) && !_gods.Any(god => god.Rule.Id == rule.Id) && _year >= _rebirthYear.GetValueOrDefault(rule.Id))
                .Select(rule => (Rule: rule, Pressure: PressureOf(rule)))
                .Where(entry => entry.Pressure >= entry.Rule.MinDomainTiles && FindSeat(entry.Rule) != null)
                .OrderByDescending(entry => entry.Pressure)
                .FirstOrDefault();
            if (candidate.Rule == null) return;

            var elemental = _gods.Where(god => god.PantheonId == null).ToList();
            if (elemental.Count >= _rules.MaxGods)
            {
                var weakest = elemental.OrderBy(god => PressureOf(god.Rule)).First();
                if (candidate.Pressure * 100 < PressureOf(weakest.Rule) * _rules.DisplacePercent) return;

                Forget(weakest);
            }

            if (FindSeat(candidate.Rule) is { } seat) Bear(candidate.Rule, seat);
        }

        private void Age(int lifespan)
        {
            foreach (var god in _gods.Where(god => _year - god.BornYear >= lifespan).ToList())
            {
                _gods.Remove(god);
                foreach (var rule in _rules.Gods) _rebirthYear.Remove(rule.Id);
                _rebirthYear[god.Rule.Id] = _year + _rules.RebirthCooldown;
                Died?.Invoke(god);
            }
        }

        private int DomainNeighbors(GridCoord cell, GodRule rule) =>
            _grid.Neighbors(cell).Count(neighbor => _tiles.TryGet(neighbor, out var id) && rule.DomainTileIds.Contains(id));

        private void Call()
        {
            foreach (var god in _gods.Where(god => god.SilentUntil <= _year))
            {
                foreach (var settlement in _settlements.All.Keys.ToList())
                {
                    if (Distance(settlement, god.Seat) > _rules.CallRadius) continue;
                    if (_shrines.TryGet(settlement, out var faith) && (faith == god.Religion || IsGodFaith(faith) || faith.Fervor >= _rules.ResistFervor)) continue;
                    if (_random.Next(100) < _rules.CallChancePercent) _shrines.Impose(settlement, god.Religion, BeliefChange.Calling);
                }
            }
        }

        private void FadeIdols()
        {
            foreach (var religion in _shrines.Religions.Where(religion => !IsGodFaith(religion)).ToList())
            {
                if (religion.Fervor >= _rules.ResistFervor)
                {
                    if (_resisting.Add(religion)) IdolResisted?.Invoke(religion);
                    continue;
                }

                religion.Cool(_rules.FadeFervor);
                var followers = _shrines.FollowersOf(religion);
                foreach (var follower in followers)
                    if (_random.Next(100) < _rules.FadeChancePercent)
                        _shrines.Renounce(follower);

                if (_shrines.FollowersOf(religion).Count == 0) IdolFaded?.Invoke(religion);
            }
        }

        public void Silence(God god, int years) => god.SilentUntil = _year + years;

        public bool Command(God god, GridCoord center)
        {
            if (!_gods.Contains(god)) return false;

            Act(god, center);
            return true;
        }

        private void Act(God god) => Act(god, god.Seat);

        private void Act(God god, GridCoord center)
        {
            var rule = god.Rule;
            var changed = new List<GridCoord>();
            var power = _power(god);
            switch (rule.Action)
            {
                case GodActionKind.TransformAround:
                    var targets = _tiles.All
                        .Where(pair => rule.FromTileIds.Contains(pair.Value) && Distance(pair.Key, center) <= rule.Radius + power / 2 && !TryGetAt(pair.Key, out _))
                        .Select(pair => pair.Key)
                        .OrderBy(_ => _random.Next(1000))
                        .Take(rule.Count + power)
                        .ToList();
                    foreach (var cell in targets)
                    {
                        _tiles.Place(cell, rule.ToTileId, "деяние бога");
                        changed.Add(cell);
                    }
                    break;
                case GodActionKind.AwakenVolcanoes:
                    foreach (var cell in _tiles.All.Keys.Where(cell => Distance(cell, center) <= rule.Radius + power / 2 && _volcanoes.CanAwaken(cell)).ToList())
                    {
                        _volcanoes.Awaken(cell);
                        changed.Add(cell);
                    }
                    break;
            }

            Acted?.Invoke(god, changed);
        }

        public void Slay(God god) => Topple(god.Seat);

        public void Unleash(God god)
        {
            if (!_gods.Contains(god)) return;

            Act(god);
            Act(god);
        }

        private void OnSeatChanged(GridCoord coord, string tileId, string cause)
        {
            var god = _gods.FirstOrDefault(candidate => candidate.Seat == coord);
            if (god == null || (tileId != null && god.Rule.DomainTileIds.Contains(tileId))) return;

            if (god.PantheonId != null && FindSeat(god.Rule, false) is { } refuge && refuge != coord)
            {
                god.Seat = refuge;
                Moved?.Invoke(god, coord);
                return;
            }

            Topple(coord);
        }

        private void Topple(GridCoord coord)
        {
            var fallen = _gods.FirstOrDefault(god => god.Seat == coord);
            if (fallen == null) return;

            _gods.Remove(fallen);
            _rebirthYear[fallen.Rule.Id] = _year + _rules.RebirthCooldown;
            Fallen?.Invoke(fallen);
        }

        private static int Distance(GridCoord a, GridCoord b) => Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
    }
}
