using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.Life.Cities;
using Godland.Core.World.Corruption;
using Godland.Core.Fate.Epochs;
using Godland.Core.Life.Features;
using Godland.Core.Powers.Gods;
using Godland.Core.World.Grid;
using Godland.Core.Life.Kingdoms;
using Godland.Core.Life.People;
using Godland.Core.Randomness;
using Godland.Core.World.Tiles;

namespace Godland.Core.Life.Heroes
{
    public sealed class HeroSaga
    {
        private readonly WorldGrid _grid;
        private readonly WorldTiles _tiles;
        private readonly TileFeatures _features;
        private readonly Monsters _monsters;
        private readonly ICorruptionRules _corruption;
        private readonly Pantheon _pantheon;
        private readonly Settlements _settlements;
        private readonly Shrines _shrines;
        private readonly Urbanization _cities;
        private readonly Realms _realms;
        private readonly TileGuards _guards;
        private readonly WorldEpoch _epoch;
        private readonly IHeroRules _rules;
        private readonly Godland.Core.Powers.Titans.TitanHost _titans;
        private readonly SeededRandom _random;
        private readonly List<Hero> _heroes = new();
        private readonly HashSet<(Hero, Hero)> _met = new();
        private int? _nextBirth;
        private GridCoord? _cradle;
        private int _cradleBonus;
        private int _heroEpoch = -1;

        public HeroSaga(
            WorldGrid grid, WorldTiles tiles, TileFeatures features, Monsters monsters, ICorruptionRules corruption, Pantheon pantheon,
            Settlements settlements, Shrines shrines, Urbanization cities, Realms realms, TileGuards guards,
            WorldEpoch epoch, IHeroRules rules, SeededRandom random, Godland.Core.Powers.Titans.TitanHost titans)
        {
            _titans = titans;
            _grid = grid;
            _tiles = tiles;
            _features = features;
            _monsters = monsters;
            _corruption = corruption;
            _pantheon = pantheon;
            _settlements = settlements;
            _shrines = shrines;
            _cities = cities;
            _realms = realms;
            _guards = guards;
            _epoch = epoch;
            _rules = rules;
            _random = random;
        }

        public event Action<Hero> Born;
        public event Action<Hero> Moved;
        public event Action<Hero> Departed;
        public event Action<Hero> Triumphed;
        public event Action<Hero> Perished;
        public event Action<Hero> Fiasco;
        public event Action<Hero> Retired;
        public event Action<Hero> Ascended;
        public event Action<Hero> Faded;
        public event Action<Hero, Hero> Allied;
        public event Action<Hero, Hero> Dueled;
        public event Action<Hero, Hero, bool> Treaty;

        public IReadOnlyList<Hero> All => _heroes;

        public bool TryGetAt(GridCoord coord, out Hero hero)
        {
            hero = _heroes.FirstOrDefault(candidate => candidate.Position == coord);
            return hero != null;
        }

        public void Bless(Hero hero) => hero.Blessed = true;

        public bool CanGuide(Hero hero, GridCoord cell) => Quests(hero).Any(quest => quest.Cell == cell);

        public bool Guide(Hero hero, GridCoord cell)
        {
            var quest = Quests(hero).Where(candidate => candidate.Cell == cell).Cast<(GridCoord Cell, QuestKind Kind)?>().FirstOrDefault();
            if (quest is not { } chosen) return false;

            hero.Target = chosen.Cell;
            hero.Quest = chosen.Kind;
            Departed?.Invoke(hero);
            return true;
        }

        public void Recall(Hero hero)
        {
            if (!_heroes.Remove(hero)) return;

            foreach (var member in hero.Party) _settlements.Bless(member.Home, 1);
            Retired?.Invoke(hero);
        }

        public bool Introduce(Hero first, Hero second)
        {
            if (first == second || !_heroes.Contains(first) || !_heroes.Contains(second)) return false;

            _met.Add(Order(first, second));
            Encounter(first, second);
            return true;
        }

        public bool Summon(GridCoord home) => TryGiveBirth(home);

        public Hero SummonDemigod(GridCoord home, string name)
        {
            if (!_settlements.TryGet(home, out _)) return null;

            var traits = (HeroTrait[])Enum.GetValues(typeof(HeroTrait));
            var archetype = _rules.Archetypes.FirstOrDefault(candidate => candidate.Epoch == _epoch.Index);
            var hero = new Hero(name, home, traits[_random.Next(traits.Length)], archetype) { IsDemigod = true };
            _heroes.Add(hero);
            Born?.Invoke(hero);
            Ascended?.Invoke(hero);
            return hero;
        }

        public GridCoord? Cradle => _cradle;

        public void SetCradle(GridCoord city, int bonusPercent)
        {
            _cradle = city;
            _cradleBonus = bonusPercent;
        }

        public void Update(int year)
        {
            if (_epoch.Index != _heroEpoch)
            {
                _heroEpoch = _epoch.Index;
                FadeOldHeroes();
                _nextBirth = null;
            }

            if (_epoch.Index < _rules.HeroesFromEpoch) return;

            _nextBirth ??= year + NextBirthDelay();
            if (year >= _nextBirth)
            {
                TryGiveBirth();
                _nextBirth = year + NextBirthDelay();
            }

            if (year % _rules.MoveYears != 0) return;

            foreach (var hero in _heroes.ToList())
                if (_heroes.Contains(hero))
                    Advance(hero);
            Meet();
        }

        private int NextBirthDelay() => _rules.BirthMinYears + _random.Next(Math.Max(1, _rules.BirthMaxYears - _rules.BirthMinYears + 1));

        private void FadeOldHeroes()
        {
            foreach (var hero in _heroes.Where(hero => hero.Archetype != null && hero.Archetype.Epoch != _epoch.Index).ToList())
            {
                _heroes.Remove(hero);
                Faded?.Invoke(hero);
            }
        }

        private void TryGiveBirth() => TryGiveBirth(null);

        private bool TryGiveBirth(GridCoord? forced)
        {
            var archetypes = _rules.Archetypes.Where(archetype => archetype.Epoch == _epoch.Index).ToList();
            if (archetypes.Count == 0) return false;

            var archetype = archetypes[_random.Next(archetypes.Count)];
            var minPopulation = Math.Max(archetype.MinPopulation, 1);
            var homes = archetype.NeedsCity ? _cities.All : _settlements.All.Keys;
            GridCoord? home = forced ?? (_cradle is { } cradle && _cities.IsCity(cradle) ? (GridCoord?)cradle : null) ?? homes
                .Where(cell => _settlements.TryGet(cell, out var population) && population >= minPopulation)
                .OrderByDescending(cell => _settlements.All[cell]).ThenBy(cell => cell.X).ThenBy(cell => cell.Y)
                .Cast<GridCoord?>().FirstOrDefault();
            if (home is not { } site) return false;
            if (!_settlements.TryGet(site, out _) || _heroes.SelectMany(hero => hero.Party).Any(hero => hero.Home == site)) return false;

            var taken = _heroes.SelectMany(living => living.Party).Select(living => living.Name).ToHashSet();
            var names = archetype.Names.Select(candidate => $"{archetype.Title} {candidate}").Where(candidate => !taken.Contains(candidate)).ToList();
            if (names.Count == 0) return false;

            var name = names[_random.Next(names.Count)];
            var traits = (HeroTrait[])Enum.GetValues(typeof(HeroTrait));
            var hero = new Hero(name, site, traits[_random.Next(traits.Length)], archetype);
            _heroes.Add(hero);
            Born?.Invoke(hero);
            return true;
        }

        private void Advance(Hero hero)
        {
            hero.Age += _rules.MoveYears;
            if (hero.Target is not { } target || !IsStillQuest(hero.Quest, target, hero))
            {
                if (!ChooseQuest(hero))
                {
                    if (hero.Age >= _rules.MaxAgeYears && !hero.IsDemigod)
                    {
                        _heroes.Remove(hero);
                        Retired?.Invoke(hero);
                        return;
                    }

                    Wander(hero);
                    return;
                }

                Departed?.Invoke(hero);
                target = hero.Target.Value;
            }

            if (hero.Position != target)
            {
                hero.Position = Step(hero.Position, target);
                Moved?.Invoke(hero);
            }

            if (hero.Position == target) Resolve(hero, target);
        }

        private void Wander(Hero hero)
        {
            var other = _heroes
                .Where(candidate => candidate != hero && !_met.Contains(Order(hero, candidate)))
                .OrderBy(candidate => Distance(hero.Position, candidate.Position))
                .FirstOrDefault();
            if (other == null || Distance(hero.Position, other.Position) <= _rules.MeetDistance) return;

            hero.Position = Step(hero.Position, other.Position);
            Moved?.Invoke(hero);
        }

        private bool ChooseQuest(Hero hero)
        {
            var quests = Quests(hero).ToList();
            if (quests.Count == 0)
            {
                hero.Target = null;
                hero.Quest = QuestKind.None;
                return false;
            }

            var favored = hero.Archetype?.FavoredQuest ?? QuestKind.None;
            var (cell, kind) = hero.Trait == HeroTrait.Reckless
                ? quests.OrderByDescending(quest => quest.Kind == favored).ThenByDescending(quest => quest.Kind).ThenBy(quest => Distance(hero.Position, quest.Cell)).First()
                : quests.OrderByDescending(quest => quest.Kind == favored).ThenBy(quest => Distance(hero.Position, quest.Cell)).First();
            hero.Target = cell;
            hero.Quest = kind;
            return true;
        }

        private IEnumerable<(GridCoord Cell, QuestKind Kind)> Quests(Hero hero)
        {
            foreach (var cell in _features.All.Keys.Where(_monsters.IsMonster))
                yield return (cell, QuestKind.Monster);
            foreach (var (cell, tileId) in _tiles.All)
                if (_corruption.IsSource(tileId))
                    yield return (cell, QuestKind.Corruption);
            foreach (var god in _pantheon.Gods.Where(_ => CanHuntGods(hero)))
                if (IsForeign(god, hero))
                    yield return (god.Seat, QuestKind.God);
            foreach (var cell in EnemyFront(hero))
                yield return (cell, QuestKind.War);
            foreach (var titan in _titans.All.Where(titan => titan.IsAwake && !titan.IsTamed))
                yield return (titan.Position, QuestKind.Titan);
        }

        private IEnumerable<GridCoord> EnemyFront(Hero hero)
        {
            var kingdom = KingdomOf(hero);
            if (kingdom == null) return Enumerable.Empty<GridCoord>();

            return _realms.Wars.Where(war => war.Involves(kingdom))
                .SelectMany(war => war.EnemyOf(kingdom).Members)
                .Where(cell => _settlements.TryGet(cell, out _));
        }

        private static bool CanHuntGods(Hero hero) => hero.Blessed || hero.Archetype is { HuntsGods: true };

        private bool IsForeign(God god, Hero hero) => FaithOf(hero) != god.Religion;

        private Religion FaithOf(Hero hero) => _shrines.TryGet(hero.Home, out var faith) ? faith : null;

        private Kingdom KingdomOf(Hero hero) => _realms.TryGetAt(hero.Home, out var kingdom) ? kingdom : null;

        private bool IsStillQuest(QuestKind kind, GridCoord cell, Hero hero) => kind switch
        {
            QuestKind.Monster => _monsters.IsMonster(cell),
            QuestKind.Corruption => _tiles.TryGet(cell, out var id) && _corruption.IsSource(id),
            QuestKind.God => CanHuntGods(hero) && _pantheon.TryGetAt(cell, out var god) && IsForeign(god, hero),
            QuestKind.War => EnemyFront(hero).Contains(cell),
            QuestKind.Titan => _titans.TryGetAt(cell, out var titan) && titan.IsAwake && !titan.IsTamed,
            _ => false
        };

        private void Resolve(Hero hero, GridCoord target)
        {
            _heroes.Remove(hero);
            var party = hero.Party.ToList();
            var chance = _rules.SuccessChancePercent
                + (hero.Blessed ? _rules.BlessedBonusPercent : 0)
                + party.Count(member => member.Trait == HeroTrait.Brave) * _rules.BraveBonusPercent
                + hero.Glory
                + hero.Companions.Count * _rules.AllyBonusPercent
                + (hero.Home == _cradle ? _cradleBonus : 0)
                + (hero.IsDemigod ? _rules.DemigodBonusPercent : 0)
                + (hero.Archetype != null && hero.Quest == hero.Archetype.FavoredQuest ? hero.Archetype.FavoredBonusPercent : 0)
                - (hero.Quest == QuestKind.God && _pantheon.TryGetAt(target, out var guarded) && guarded.PantheonId != null ? _rules.PantheonGuardPercent : 0);
            if (_random.Next(100) >= Math.Min(95, chance))
            {
                Fail(hero, target, party);
                return;
            }

            switch (hero.Quest)
            {
                case QuestKind.Monster:
                    _features.Remove(target);
                    break;
                case QuestKind.Corruption:
                    _tiles.Place(target, _corruption.HealedTileId, "подвиг героя");
                    break;
                case QuestKind.God when _pantheon.TryGetAt(target, out var god):
                    _pantheon.Slay(god);
                    break;
                case QuestKind.Titan when _titans.TryGetAt(target, out var titan):
                    _titans.Slay(titan, $"Герой {hero.Name}");
                    break;
                case QuestKind.War:
                    _settlements.Weaken(target, _rules.TragedyLoss);
                    if (FaithOf(hero) is { } faith) _shrines.Impose(target, faith);
                    break;
            }

            if (!hero.IsDemigod)
                foreach (var member in party)
                    _settlements.Bless(member.Home, _rules.LegendPopulation);
            else hero.Glory++;

            hero.Legends++;
            var ascends = !_pantheon.AcceptsHeroes && !hero.IsDemigod && _random.Next(100) < _rules.DemigodChancePercent + hero.Glory * 3;
            if (ascends) hero.IsDemigod = true;
            Triumphed?.Invoke(hero);
            if (hero.IsDemigod)
            {
                hero.Target = null;
                hero.Quest = QuestKind.None;
                _heroes.Add(hero);
                if (ascends) Ascended?.Invoke(hero);
                return;
            }

            if (_pantheon.AcceptsHeroes && _settlements.TryGet(hero.Home, out _)) _pantheon.Ascend(hero.Home, hero.Name);
        }

        private void Fail(Hero hero, GridCoord target, IReadOnlyList<Hero> party)
        {
            foreach (var member in party)
                _settlements.Weaken(member.Home, _rules.TragedyLoss);

            var fiasco = _rules.FiascoChancePercent
                + party.Count(member => member.Trait == HeroTrait.Reckless) * _rules.RecklessFiascoPercent
                - party.Count(member => member.Trait == HeroTrait.Wise) * _rules.WiseFiascoPercent;
            if (_random.Next(100) >= fiasco)
            {
                Perished?.Invoke(hero);
                return;
            }

            switch (hero.Quest)
            {
                case QuestKind.Monster:
                    if (_features.TryGet(target, out var monster))
                        foreach (var cell in Around(target).Where(_features.CanAdd).Take(_rules.FiascoSpread))
                            _features.AddWild(cell, monster);
                    break;
                case QuestKind.Corruption:
                    foreach (var cell in Around(target).Where(CanCorrupt).Take(_rules.FiascoSpread))
                        _tiles.Place(cell, _corruption.CorruptedTileId, "провал героя");
                    break;
                case QuestKind.God when _pantheon.TryGetAt(target, out var god):
                    _pantheon.Unleash(god);
                    foreach (var member in party)
                        _shrines.Impose(member.Home, god.Religion, BeliefChange.Calling);
                    break;
            }

            Fiasco?.Invoke(hero);
        }

        private bool CanCorrupt(GridCoord cell) =>
            _tiles.TryGet(cell, out var id) && !_corruption.IsSource(id) && id != _corruption.CorruptedTileId
            && !_corruption.ResistsCorruption(id) && !_guards.IsProtected(cell);

        private IEnumerable<GridCoord> Around(GridCoord center) =>
            _grid.Neighbors(center).Concat(_grid.Neighbors(center).SelectMany(_grid.Neighbors)).Distinct()
                .Where(cell => cell != center).OrderBy(_ => _random.Next(1000));

        private void Meet()
        {
            foreach (var first in _heroes.ToList())
            foreach (var second in _heroes.ToList())
            {
                if (first == second || !_heroes.Contains(first) || !_heroes.Contains(second)) continue;
                if (Distance(first.Position, second.Position) > _rules.MeetDistance || !_met.Add(Order(first, second))) continue;

                Encounter(first, second);
            }
        }

        private (Hero, Hero) Order(Hero a, Hero b) => _heroes.IndexOf(a) < _heroes.IndexOf(b) ? (a, b) : (b, a);

        private void Encounter(Hero first, Hero second)
        {
            var proud = first.Trait == HeroTrait.Proud || second.Trait == HeroTrait.Proud;
            var kingdom = KingdomOf(first);
            var friends = FaithOf(first) == FaithOf(second) || (kingdom != null && kingdom == KingdomOf(second));
            if (friends)
            {
                if (!proud) Ally(first, second);
                return;
            }

            if (!proud)
            {
                var treaty = _rules.TreatyChancePercent
                    + (first.Trait == HeroTrait.Wise || second.Trait == HeroTrait.Wise ? _rules.WiseTreatyPercent : 0)
                    + (first.Blessed || second.Blessed ? _rules.WiseTreatyPercent : 0);
                if (_random.Next(100) < treaty)
                {
                    MakeTreaty(first, second);
                    return;
                }

            }

            if (proud || _random.Next(100) < _rules.DuelChancePercent) Duel(first, second);
        }

        private void Ally(Hero leader, Hero follower)
        {
            _heroes.Remove(follower);
            leader.Join(follower);
            Allied?.Invoke(leader, follower);
        }

        private void MakeTreaty(Hero first, Hero second)
        {
            var a = KingdomOf(first);
            var b = KingdomOf(second);
            var peace = a != null && b != null && a != b && _realms.Wars.Any(war => war.Involves(a) && war.Involves(b)) && _realms.TryMakePeace(a);
            Treaty?.Invoke(first, second, peace);
        }

        private void Duel(Hero first, Hero second)
        {
            var edge = first.Glory - second.Glory
                + (first.Trait == HeroTrait.Brave ? _rules.BraveBonusPercent : 0)
                - (second.Trait == HeroTrait.Brave ? _rules.BraveBonusPercent : 0);
            var firstWins = _random.Next(100) < Math.Clamp(50 + edge, 10, 90);
            var winner = firstWins ? first : second;
            var loser = firstWins ? second : first;

            _heroes.Remove(loser);
            winner.Glory += _rules.GloryPerVictory;
            foreach (var member in loser.Party)
                _settlements.Weaken(member.Home, 1);
            Dueled?.Invoke(winner, loser);
        }

        private static GridCoord Step(GridCoord from, GridCoord to)
        {
            var dx = Math.Sign(to.X - from.X);
            var dy = Math.Sign(to.Y - from.Y);
            return Math.Abs(to.X - from.X) >= Math.Abs(to.Y - from.Y) ? new GridCoord(from.X + dx, from.Y) : new GridCoord(from.X, from.Y + dy);
        }

        private static int Distance(GridCoord a, GridCoord b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
    }
}
