using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.Life.Cities;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Core.Randomness;

namespace Godland.Core.Life.Kingdoms
{
    public sealed class Realms
    {
        private readonly WorldGrid _grid;
        private readonly Settlements _settlements;
        private readonly Shrines _shrines;
        private readonly Urbanization _cities;
        private readonly IKingdomRules _rules;
        private readonly SeededRandom _random;
        private readonly List<Kingdom> _kingdoms = new();
        private readonly List<War> _wars = new();
        private int _nameIndex;
        private int _year;
        private readonly HashSet<Kingdom> _aggressors = new();

        public Realms(
            WorldGrid grid, Settlements settlements, Shrines shrines, Urbanization cities, Feuds feuds,
            IKingdomRules rules, SeededRandom random)
        {
            _grid = grid;
            _settlements = settlements;
            _shrines = shrines;
            _cities = cities;
            _rules = rules;
            _random = random;

            feuds.SetTruce((a, b) => TryGetAt(a, out _) && TryGetAt(b, out _));
        }

        public event Action<Kingdom> Founded;
        public event Action<Kingdom> Dissolved;
        public event Action<Kingdom, GridCoord> CapitalMoved;
        public event Action<War> WarDeclared;
        public event Action<War> PeaceMade;
        public event Action<Kingdom, Kingdom, GridCoord> Conquered;

        public IReadOnlyList<Kingdom> All => _kingdoms;

        public IReadOnlyList<War> Wars => _wars;

        public bool TryGetAt(GridCoord coord, out Kingdom kingdom)
        {
            kingdom = _kingdoms.FirstOrDefault(candidate => candidate.Members.Contains(coord));
            return kingdom != null;
        }

        public bool IsAtWar(Kingdom kingdom) => _wars.Any(war => war.Involves(kingdom));

        public bool IsAggressor(Kingdom kingdom) => _aggressors.Contains(kingdom);

        public void MakeAggressor(Kingdom kingdom) => _aggressors.Add(kingdom);

        public bool AreWarsEternal { get; private set; }
        public bool AreWarsForbidden { get; private set; }

        public void MakeWarsEternal() => AreWarsEternal = true;

        public void ForbidWars()
        {
            AreWarsForbidden = true;
            foreach (var war in _wars.ToList())
            {
                _wars.Remove(war);
                PeaceMade?.Invoke(war);
            }
        }

        public void Update(int year)
        {
            _year = year;
            if (year % _rules.FormPeriod == 0)
            {
                Refresh();
                Form();
            }

            if (year % _rules.BattlePeriod == 0) Fight();
            if (year % _rules.WarCheckPeriod == 0) DeclareWars(year);
            EndWars(year);
        }

        private void Refresh()
        {
            foreach (var kingdom in _kingdoms.ToList())
            {
                var intact = IsSeat(kingdom, kingdom.Capital) || MoveCapital(kingdom);
                var members = intact ? Realm(kingdom.Capital, kingdom.Religion, kingdom) : new List<GridCoord>();
                if (!intact || members.Count < _rules.SurviveMembers)
                {
                    kingdom.Strikes++;
                    if (kingdom.Strikes >= _rules.DissolveStrikes + (_year - kingdom.Founded) / Math.Max(1, _rules.StabilityYears)) Dissolve(kingdom);
                    continue;
                }

                kingdom.Strikes = 0;
                kingdom.SetMembers(members);
            }
        }

        private void Form()
        {
            foreach (var city in _cities.All.ToList())
            {
                if (TryGetAt(city, out _) || !_shrines.TryGet(city, out var faith)) continue;

                var members = Realm(city, faith, null);
                if (members.Count < _rules.MinMembers) continue;

                var name = NextName();
                var kingdom = new Kingdom(_nameIndex - 1, name, city, faith) { Founded = _year };
                kingdom.SetMembers(members);
                _kingdoms.Add(kingdom);
                Founded?.Invoke(kingdom);
            }
        }

        private List<GridCoord> Realm(GridCoord capital, Religion faith, Kingdom owner)
        {
            var realm = new List<GridCoord> { capital };
            var frontier = new Queue<GridCoord>(realm);
            var seen = new HashSet<GridCoord>(realm);
            while (frontier.Count > 0)
            {
                foreach (var neighbor in _grid.Neighbors(frontier.Dequeue()))
                {
                    if (!seen.Add(neighbor) || !_settlements.TryGet(neighbor, out _)) continue;
                    if (!_shrines.TryGet(neighbor, out var other) || other != faith) continue;
                    if (TryGetAt(neighbor, out var holder) && holder != owner) continue;

                    realm.Add(neighbor);
                    frontier.Enqueue(neighbor);
                }
            }

            return realm;
        }

        private void DeclareWars(int year)
        {
            if (AreWarsForbidden) return;

            foreach (var kingdom in _kingdoms.ToList())
            {
                if (IsAtWar(kingdom)) continue;

                var aggressor = _aggressors.Contains(kingdom);
                var rival = _kingdoms.FirstOrDefault(other =>
                    other != kingdom && (aggressor || other.Religion != kingdom.Religion) && !IsAtWar(other) && Borders(kingdom, other));
                if (rival == null || (!aggressor && _random.Next(100) >= _rules.WarChancePercent)) continue;

                var length = _rules.WarMinYears + _random.Next(Math.Max(1, _rules.WarMaxYears - _rules.WarMinYears + 1));
                var war = new War(kingdom, rival, year + length);
                _wars.Add(war);
                WarDeclared?.Invoke(war);
            }
        }

        private bool Borders(Kingdom a, Kingdom b) =>
            a.Members.Any(cell => _grid.Neighbors(cell).Any(b.Members.Contains));

        private void Fight()
        {
            foreach (var war in _wars.ToList())
            {
                if (!_wars.Contains(war)) continue;

                var attacker = Strength(war.First) >= Strength(war.Second) ? war.First : war.Second;
                var defender = war.EnemyOf(attacker);
                var front = defender.Members.Where(cell => _grid.Neighbors(cell).Any(attacker.Members.Contains)).ToList();
                if (front.Count == 0) continue;

                var target = front[_random.Next(front.Count)];
                _settlements.Weaken(target, _rules.BattlePopulationLoss);
                _shrines.Impose(target, attacker.Religion);
                Conquered?.Invoke(attacker, defender, target);

                if (target == defender.Capital && !MoveCapital(defender)) Dissolve(defender);
            }
        }

        public bool TryDeclareWar(Kingdom first, Kingdom second)
        {
            if (AreWarsForbidden || first == second || !_kingdoms.Contains(first) || !_kingdoms.Contains(second) || IsAtWar(first) || IsAtWar(second)) return false;

            var length = _rules.WarMinYears + _random.Next(Math.Max(1, _rules.WarMaxYears - _rules.WarMinYears + 1));
            var war = new War(first, second, _year + length);
            _wars.Add(war);
            WarDeclared?.Invoke(war);
            return true;
        }

        public bool TryMakePeace(Kingdom kingdom)
        {
            var war = _wars.FirstOrDefault(candidate => candidate.Involves(kingdom));
            if (war == null || AreWarsEternal) return false;

            _wars.Remove(war);
            PeaceMade?.Invoke(war);
            return true;
        }

        private int Strength(Kingdom kingdom) =>
            kingdom.Members.Sum(cell => _settlements.TryGet(cell, out var population) ? population : 0);

        private void EndWars(int year)
        {
            if (AreWarsEternal) return;

            foreach (var war in _wars.Where(war => war.EndYear <= year).ToList())
            {
                _wars.Remove(war);
                PeaceMade?.Invoke(war);
            }
        }

        private bool IsSeat(Kingdom kingdom, GridCoord cell) =>
            _cities.IsCity(cell) && _shrines.TryGet(cell, out var faith) && faith == kingdom.Religion;

        private bool MoveCapital(Kingdom kingdom)
        {
            var seat = kingdom.Members
                .Where(cell => cell != kingdom.Capital && IsSeat(kingdom, cell))
                .OrderByDescending(cell => _settlements.All.TryGetValue(cell, out var population) ? population : 0).ThenBy(cell => cell.X).ThenBy(cell => cell.Y)
                .Cast<GridCoord?>().FirstOrDefault();
            if (seat is not { } capital) return false;

            kingdom.Capital = capital;
            CapitalMoved?.Invoke(kingdom, capital);
            return true;
        }

        private void Dissolve(Kingdom kingdom)
        {
            if (!_kingdoms.Remove(kingdom)) return;

            _wars.RemoveAll(war => war.Involves(kingdom));
            _aggressors.Remove(kingdom);
            Dissolved?.Invoke(kingdom);
        }

        private string NextName()
        {
            var names = _rules.Names;
            _nameIndex++;
            if (names.Count == 0) return $"Царство {_nameIndex}";

            var name = names[_random.Next(names.Count)];
            return _kingdoms.Any(kingdom => kingdom.Name == name) ? $"{name} {_nameIndex}" : name;
        }
    }
}
