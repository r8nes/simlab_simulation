using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core;
using Godland.Core.Flow;
using Godland.Core.Powers.Gods;
using Godland.Core.World.Grid;
using Godland.Core.Life.Minds;
using Godland.Core.Life.People;
using Godland.Game.Fate.Catastrophes;

namespace Godland.Game.Powers.Pantheons
{
    public enum PantheonBond
    {
        Neutral,
        War,
        Alliance
    }

    public sealed class PantheonHouse
    {
        private readonly List<God> _gods = new();

        public PantheonHouse(PantheonDefinition definition, God head, int arrivedYear, Mind mind)
        {
            Definition = definition;
            Head = head;
            Faith = head.Religion;
            ArrivedYear = arrivedYear;
            Mind = mind;
            _gods.Add(head);
        }

        public PantheonDefinition Definition { get; }
        public Religion Faith { get; }
        public int ArrivedYear { get; }
        public Mind Mind { get; }
        public God Head { get; internal set; }
        public bool IsAsleep { get; internal set; }
        public int SleepYear { get; internal set; }
        public string LastDeed { get; internal set; }
        public IReadOnlyList<God> Gods => _gods;
        public Dictionary<PantheonHouse, int> Relations { get; } = new();
        public Dictionary<PantheonHouse, PantheonBond> Bonds { get; } = new();
        public HashSet<string> Facts { get; } = new();
        public HashSet<string> Fulfilled { get; } = new();
        public Dictionary<God, (GodScheme Scheme, List<GodAction> Plan)> Schemes { get; } = new();

        internal void Add(God god) => _gods.Add(god);

        internal void Remove(God god) => _gods.Remove(god);

        public int RelationWith(PantheonHouse other) => Relations.GetValueOrDefault(other);

        public PantheonBond BondWith(PantheonHouse other) => Bonds.GetValueOrDefault(other);
    }

    public sealed class PantheonRegistry
    {
        private readonly List<PantheonHouse> _houses = new();

        public IReadOnlyList<PantheonHouse> All => _houses;
        public IEnumerable<PantheonHouse> Active => _houses.Where(house => !house.IsAsleep);

        public PantheonHouse Find(string id) => _houses.FirstOrDefault(house => house.Definition.Id == id && !house.IsAsleep);

        internal void Add(PantheonHouse house) => _houses.Add(house);

        internal void Remove(PantheonHouse house) => _houses.Remove(house);
    }

    public sealed class PantheonKeeper : IWorldStep, IDisposable
    {
        private readonly PantheonConfig _config;
        private readonly PantheonRegistry _registry;
        private readonly CatastropheContext _context;
        private readonly IMindRules _mindRules;
        private readonly FlowBias _flow;
        private readonly Godland.Game.Fate.Sagas.SagaKeeper _sagas;
        private int _year;

        public PantheonKeeper(PantheonConfig config, PantheonRegistry registry, CatastropheContext context, IMindRules mindRules, FlowBias flow,
            Godland.Game.Fate.Sagas.SagaKeeper sagas)
        {
            _sagas = sagas;
            _config = config;
            _registry = registry;
            _context = context;
            _mindRules = mindRules;
            _flow = flow;

            Shrines.IdolRevealed += OnIdolRevealed;
            Gods.Fallen += OnGodLost;
            Gods.Forgotten += OnGodLost;
            Gods.Died += OnGodLost;
        }

        public event Action<PantheonHouse, string> Arrived;
        public event Action<PantheonHouse, God> GodBorn;
        public event Action<PantheonHouse, PantheonDeed, GridCoord, string> Acted;
        public event Action<PantheonHouse, PantheonHouse, PantheonBond> BondChanged;
        public event Action<PantheonHouse, PantheonHouse, GridCoord> Battled;
        public event Action<PantheonHouse, God> Succeeded;
        public event Action<PantheonHouse, string> Faded;
        public event Action<PantheonHouse, PantheonHouse> Absorbed;
        public event Action<PantheonHouse, God, GodAction, GridCoord> Schemed;
        public event Action<PantheonHouse, God, GodScheme> SchemeFulfilled;

        public PantheonRegistry Registry => _registry;

        private Pantheon Gods => _context.World.Pantheon;
        private Shrines Shrines => _context.World.Shrines;
        private Settlements Settlements => _context.World.Settlements;

        public void Dispose()
        {
            Shrines.IdolRevealed -= OnIdolRevealed;
            Gods.Fallen -= OnGodLost;
            Gods.Forgotten -= OnGodLost;
            Gods.Died -= OnGodLost;
        }

        public void Advance(int year)
        {
            _year = year;
            if (year % _config.CheckPeriod == 0) TryArrive();
            if (year % _config.BirthPeriod == 0) GiveBirths();
            if (year % _config.DeedPeriod == 0)
                foreach (var house in _registry.Active.ToList()) Act(house, null);
            if (year % _config.RelationPeriod == 0) Relate();
            if (year % _config.SchemePeriod == 0)
                foreach (var house in _registry.Active.ToList())
                foreach (var god in house.Gods.ToList())
                    if (!house.IsAsleep && house.Gods.Contains(god)) Scheme(house, god);
        }

        public static bool IsGod(God god, string name) => SchemePlanner.Fits(god.HeroName, name);

        private void Scheme(PantheonHouse house, God god)
        {
            if (!house.Schemes.TryGetValue(god, out var current) || current.Plan.Count == 0 || !CanDo(house, god, current.Plan[0]))
            {
                current = Plan(house, god);
                if (current.Scheme == null) return;
                house.Schemes[god] = current;
            }

            var action = current.Plan[0];
            current.Plan.RemoveAt(0);
            if (SiteOf(house, action.Site) is not { } site) return;

            Perform(house, god, action, site);
            if (!house.Facts.Contains(current.Scheme.Goal)) return;

            house.Fulfilled.Add(current.Scheme.Goal);
            house.Schemes.Remove(god);
            if (current.Scheme.Repeatable) house.Facts.Remove(current.Scheme.Goal);
            SchemeFulfilled?.Invoke(house, god, current.Scheme);
        }

        private (GodScheme Scheme, List<GodAction> Plan) Plan(PantheonHouse house, God god)
        {
            var schemes = house.Definition.Schemes.Where(scheme => IsGod(god, scheme.God));
            var actions = house.Definition.Actions.Where(action => IsGod(god, action.God) && WorldAllows(god, action)).ToList();
            return SchemePlanner.Choose(schemes, house.Facts, house.Fulfilled, actions, _context.World.Random, _config.PlanDepth);
        }

        private bool WorldAllows(God god, GodAction action) =>
            _year - god.BornYear >= action.MinGodAge && (action.When == null || action.When.IsReady(_context));

        private bool CanDo(PantheonHouse house, God god, GodAction action) =>
            WorldAllows(god, action) && action.Needs.All(house.Facts.Contains);

        private void Perform(PantheonHouse house, God god, GodAction action, GridCoord site)
        {
            var world = _context.World;
            var title = $"{god.HeroName}: {action.Name}";
            foreach (var outcome in action.Outcomes) outcome.Apply(world, site, title);
            foreach (var fact in action.Removes) house.Facts.Remove(fact);
            foreach (var fact in action.Adds) house.Facts.Add(fact);
            if (!string.IsNullOrEmpty(action.Demigod))
            {
                var home = Settlements.TryGet(site, out _) ? site : Shrines.FollowersOf(house.Faith).Cast<GridCoord?>().FirstOrDefault();
                if (home is { } cell) world.Heroes.SummonDemigod(cell, $"Полубог {action.Demigod}");
            }

            if (action.Saga != null) _sagas.TryCall(action.Saga);
            world.Memory.Add(site, $"god-act:{action.Name}");
            world.Memory.Add(world.Grid.Center, "god-acts");
            world.History.Note(site, title);
            Schemed?.Invoke(house, god, action, site);
        }

        public int StrengthOf(PantheonHouse house) => Shrines.FollowersOf(house.Faith).Count + house.Gods.Count * 3;

        public int SharePercent(PantheonHouse house)
        {
            var settlements = Settlements.All.Count;
            return settlements == 0 ? 0 : Shrines.FollowersOf(house.Faith).Count * 100 / settlements;
        }

        public void Shift(PantheonHouse first, PantheonHouse second, int amount)
        {
            SetRelation(first, second, Math.Clamp(first.RelationWith(second) + amount, -100, 100));
            UpdateBond(first, second);
        }

        public bool Invoke(PantheonHouse house) => Act(house, "по зову писателя");

        public bool Arrive(PantheonDefinition definition, string cause, Religion converts = null)
        {
            if (_registry.Find(definition.Id) != null) return false;

            var head = Gods.BirthAs(definition.Head.Body, definition.Head.FullName, definition.Id);
            if (head == null) return false;

            foreach (var old in _registry.All.Where(house => house.Definition == definition).ToList()) _registry.Remove(old);
            var house = new PantheonHouse(definition, head, _year, new Mind(_mindRules));
            _registry.Add(house);
            if (converts != null)
                foreach (var follower in Shrines.FollowersOf(converts).ToList())
                    Shrines.Impose(follower, house.Faith, BeliefChange.Calling);

            _flow.Note($"pantheon:{definition.Id}");
            _context.World.Memory.Add(_context.World.Grid.Center, $"pantheon:{definition.Id}");
            _context.World.History.Note(head.Seat, $"Пришёл пантеон «{definition.DisplayName}»: {cause}");
            Arrived?.Invoke(house, cause);
            return true;
        }

        private void TryArrive()
        {
            var world = _context.World;
            if (_registry.Active.Count() >= _config.MaxPantheons) return;

            var candidates = _config.Pantheons
                .Where(definition => definition != null && world.Epoch.Index >= definition.MinEpoch && _registry.Find(definition.Id) == null
                                     && !_registry.All.Any(house => house.Definition == definition && house.IsAsleep && _year - house.SleepYear < _config.SleepYears)
                                     && definition.Arrival != null && definition.Arrival.IsReady(_context))
                .OrderBy(definition => definition.Id)
                .OrderBy(_ => world.Random.Next(1000))
                .ToList();
            foreach (var definition in candidates)
            {
                if (world.Random.Next(100) >= _flow.Chance($"pantheon:{definition.Id}", definition.ArrivalChancePercent)) continue;
                if (Arrive(definition, "земля позвала своих богов")) return;
            }
        }

        private void GiveBirths()
        {
            foreach (var house in _registry.Active.ToList())
            {
                if (house.Gods.Count >= _config.MaxGodsPerPantheon) continue;

                var child = house.Definition.Children.FirstOrDefault(candidate =>
                    house.Gods.All(god => god.HeroName != candidate.FullName)
                    && candidate.Birth != null && candidate.Birth.IsReady(_context) && Gods.CanSeat(candidate.Body));
                if (child == null) continue;

                var god = Gods.BirthAs(child.Body, child.FullName, house.Definition.Id, house.Faith);
                if (god == null) continue;

                house.Add(god);
                _context.World.History.Note(god.Seat, $"В пантеоне «{house.Definition.DisplayName}» родился {child.FullName}");
                GodBorn?.Invoke(house, god);
            }
        }

        private bool Act(PantheonHouse house, string forced)
        {
            var deeds = house.Definition.Deeds;
            if (deeds.Count == 0) return false;

            house.Mind.Measure(StrengthOf(house));
            var options = deeds.Select(deed => new MindOption(deed.Name, SiteOf(house, deed.Site) == null ? 0 : deed.Nature)).ToList();
            var models = _registry.Active.Where(other => other != house).Select(other => other.Mind).ToList();
            if (house.Mind.Decide(options, models, _context.World.Random) is not { } decision) return false;

            var chosen = deeds.First(deed => deed.Name == decision.Option.Id);
            if (SiteOf(house, chosen.Site) is not { } site) return false;

            var title = $"{house.Definition.DisplayName}: {chosen.Name}";
            foreach (var outcome in chosen.Outcomes) outcome.Apply(_context.World, site, title);
            var reason = forced ?? decision.Reason;
            house.LastDeed = $"{chosen.Name} ({reason}), {_year} год";
            _context.World.Memory.Add(site, $"pantheon-deed:{house.Definition.Id}");
            _context.World.History.Note(site, $"{title} ({reason})");
            Acted?.Invoke(house, chosen, site, reason);
            return true;
        }

        private GridCoord? SiteOf(PantheonHouse house, PantheonDeedSite site)
        {
            var random = _context.World.Random;
            switch (site)
            {
                case PantheonDeedSite.Seat:
                    return house.Head?.Seat;
                case PantheonDeedSite.Follower:
                    var followers = Shrines.FollowersOf(house.Faith);
                    return followers.Count == 0 ? house.Head?.Seat : followers[random.Next(followers.Count)];
                default:
                    var rival = _registry.Active.Where(other => other != house).OrderBy(house.RelationWith).FirstOrDefault();
                    if (rival == null || house.RelationWith(rival) >= 0) return null;

                    var targets = Shrines.FollowersOf(rival.Faith);
                    return targets.Count == 0 ? rival.Head?.Seat : targets[random.Next(targets.Count)];
            }
        }

        private void Relate()
        {
            var random = _context.World.Random;
            var houses = _registry.Active.ToList();
            for (var i = 0; i < houses.Count; i++)
            for (var j = i + 1; j < houses.Count; j++)
            {
                var (first, second) = (houses[i], houses[j]);
                var friction = (first.Definition.Aggression + second.Definition.Aggression) / 20;
                var border = Border(first, second).Any() ? 6 : 0;
                var sharedEnemy = _registry.Active.Any(third => third != first && third != second
                    && first.BondWith(third) == PantheonBond.War && second.BondWith(third) == PantheonBond.War) ? 12 : 0;
                Shift(first, second, random.Next(21) - 10 + 8 - friction - border + sharedEnemy);
                if (first.BondWith(second) == PantheonBond.War) Battle(first, second);
                else if (first.BondWith(second) == PantheonBond.Alliance) TryAbsorb(first, second);
            }
        }

        private IEnumerable<(GridCoord Own, GridCoord Enemy)> Border(PantheonHouse first, PantheonHouse second)
        {
            var enemy = Shrines.FollowersOf(second.Faith).ToHashSet();
            return Shrines.FollowersOf(first.Faith)
                .SelectMany(cell => _context.World.Grid.Neighbors(cell).Where(enemy.Contains).Select(target => (cell, target)));
        }

        private void Battle(PantheonHouse first, PantheonHouse second)
        {
            var (winner, loser) = StrengthOf(first) >= StrengthOf(second) ? (first, second) : (second, first);
            var front = Border(winner, loser).OrderBy(pair => pair.Enemy.X).ThenBy(pair => pair.Enemy.Y).ToList();
            if (front.Count == 0) return;

            var target = front[_context.World.Random.Next(front.Count)].Enemy;
            Settlements.Weaken(target, _config.HolyWarLoss);
            if (Settlements.TryGet(target, out _) && _context.World.Random.Next(100) < _config.HolyWarConversionPercent)
                Shrines.Impose(target, winner.Faith, BeliefChange.Conquest);
            _context.World.Memory.Add(target, "holy-war");
            _context.World.Memory.Add(target, "war");
            Battled?.Invoke(winner, loser, target);
        }

        private void TryAbsorb(PantheonHouse first, PantheonHouse second)
        {
            var (strong, weak) = StrengthOf(first) >= StrengthOf(second) ? (first, second) : (second, first);
            if (StrengthOf(weak) * 100 > StrengthOf(strong) * _config.AbsorbPercent) return;

            foreach (var follower in Shrines.FollowersOf(weak.Faith).ToList()) Shrines.Impose(follower, strong.Faith, BeliefChange.Calling);
            foreach (var god in weak.Gods.ToList()) Gods.Forget(god);
            Sleep(weak);
            Absorbed?.Invoke(strong, weak);
        }

        private void SetRelation(PantheonHouse first, PantheonHouse second, int value)
        {
            first.Relations[second] = value;
            second.Relations[first] = value;
        }

        private void UpdateBond(PantheonHouse first, PantheonHouse second)
        {
            var relation = first.RelationWith(second);
            var bond = relation <= _config.WarThreshold ? PantheonBond.War : relation >= _config.AllianceThreshold ? PantheonBond.Alliance : PantheonBond.Neutral;
            if (first.BondWith(second) == bond) return;

            first.Bonds[second] = bond;
            second.Bonds[first] = bond;
            if (bond == PantheonBond.War) _context.World.Memory.Add(_context.World.Grid.Center, "holy-wars");
            BondChanged?.Invoke(first, second, bond);
        }

        private void OnIdolRevealed(Religion religion, GridCoord cell)
        {
            var definition = _config.Pantheons.FirstOrDefault(candidate =>
                candidate != null && candidate.Idols.Any(idol => idol != null && idol.Id == religion.IdolId) && _context.World.Epoch.Index >= candidate.MinEpoch);
            if (definition == null) return;

            var house = _registry.Find(definition.Id);
            if (house != null)
            {
                foreach (var follower in Shrines.FollowersOf(religion).ToList()) Shrines.Impose(follower, house.Faith, BeliefChange.Calling);
                return;
            }

            if (_registry.Active.Count() < _config.MaxPantheons) Arrive(definition, "его идол открылся верующим", religion);
        }

        private void OnGodLost(God god)
        {
            var house = _registry.Active.FirstOrDefault(candidate => candidate.Gods.Contains(god));
            if (house == null) return;

            house.Remove(god);
            if (house.Head != god) return;

            var heir = house.Gods.OrderBy(candidate => candidate.BornYear).FirstOrDefault();
            if (heir != null)
            {
                house.Head = heir;
                Succeeded?.Invoke(house, heir);
                return;
            }

            Sleep(house);
            Faded?.Invoke(house, "пал последний бог пантеона");
        }

        private void Sleep(PantheonHouse house)
        {
            house.IsAsleep = true;
            house.SleepYear = _year;
            house.Head = null;
        }
    }
}
