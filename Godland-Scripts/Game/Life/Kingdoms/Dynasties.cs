using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core;
using Godland.Core.World.Grid;
using Godland.Core.Life.Kingdoms;
using Godland.Core.Life.People;
using Godland.Game.Fate.Catastrophes;
using Godland.Game.Fate.Emergence;
using Godland.Game.Powers.Prophecies;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Life.Kingdoms
{
    public enum RulerTrait
    {
        Conqueror,
        Builder,
        Pious,
        Tyrant,
        Wise
    }

    public enum CitySpecialty
    {
        Market,
        Port,
        Temple,
        Fortress,
        Granary
    }

    public sealed class Ruler
    {
        public Ruler(string name, string dynasty, RulerTrait trait, int crownedYear, int lifespan, int number)
        {
            Name = name;
            Dynasty = dynasty;
            Trait = trait;
            CrownedYear = crownedYear;
            Lifespan = lifespan;
            Number = number;
        }

        public string Name { get; }
        public string Dynasty { get; }
        public RulerTrait Trait { get; }
        public int CrownedYear { get; }
        public int Lifespan { get; }
        public int Number { get; }
        public string Title => Number > 1 ? $"{Name} {Roman(Number)} из дома {Dynasty}" : $"{Name} из дома {Dynasty}";

        private static string Roman(int number) => number switch { 2 => "II", 3 => "III", 4 => "IV", 5 => "V", 6 => "VI", 7 => "VII", _ => number.ToString() };
    }

    [Serializable]
    public sealed class WonderDefinition
    {
        [SerializeField, Required] private string _name;
        [SerializeField, TextArea] private string _text;
        [SerializeField, AssetsOnly, InfoBox("Тайл, которым чудо встаёт рядом со столицей (пусто — не меняет землю).")] private TileDefinition _tile;
        [SerializeReference] private List<IEmergenceOutcome> _outcomes = new();
        [SerializeReference, InfoBox("Вечный закон, который чудо приносит миру.")] private List<IAgeLaw> _laws = new();

        public string Name => _name;
        public string Text => _text;
        public TileDefinition Tile => _tile;
        public IReadOnlyList<IEmergenceOutcome> Outcomes => _outcomes;
        public IReadOnlyList<IAgeLaw> Laws => _laws;
    }

    public sealed class Dynasties : IWorldStep, IDisposable
    {
        private readonly DynastyConfig _config;
        private readonly CatastropheContext _context;
        private readonly MilestoneLawKeeper _laws;
        private readonly Dictionary<Kingdom, Ruler> _rulers = new();
        private readonly Dictionary<Kingdom, List<WonderDefinition>> _wonders = new();
        private readonly HashSet<WonderDefinition> _built = new();
        private readonly Dictionary<GridCoord, CitySpecialty> _specialties = new();
        private int _year;

        public Dynasties(DynastyConfig config, CatastropheContext context, MilestoneLawKeeper laws)
        {
            _config = config;
            _context = context;
            _laws = laws;
            Realms.Founded += OnFounded;
            Realms.Dissolved += OnDissolved;
            _context.Cities.Risen += OnCityRisen;
            _context.Cities.Fallen += OnCityFallen;
            bool IsFortress(GridCoord cell) => _specialties.TryGetValue(cell, out var specialty) && specialty == CitySpecialty.Fortress;
            _context.World.Guards.Add(IsFortress);
            _context.World.Wards.Add(cell => IsFortress(cell) || _context.World.Grid.Neighbors(cell).Any(IsFortress));
        }

        public event Action<Kingdom, Ruler> Crowned;
        public event Action<Kingdom, Ruler, string> Died;
        public event Action<Kingdom, Ruler, Ruler> Abdicated;
        public event Action<Kingdom, Ruler, Ruler> Usurped;
        public event Action<Kingdom, GridCoord, string> CivilWar;
        public event Action<Kingdom, WonderDefinition> WonderBuilt;
        public event Action<Kingdom, Ruler, string> Ruled;
        public event Action<GridCoord, CitySpecialty> Specialized;

        private Realms Realms => _context.World.Realms;

        public Ruler RulerOf(Kingdom kingdom) => _rulers.GetValueOrDefault(kingdom);

        public IReadOnlyList<WonderDefinition> WondersOf(Kingdom kingdom) => _wonders.TryGetValue(kingdom, out var list) ? list : Array.Empty<WonderDefinition>();

        public bool TryGetSpecialty(GridCoord city, out CitySpecialty specialty) => _specialties.TryGetValue(city, out specialty);

        public static string NameOf(RulerTrait trait) => trait switch
        {
            RulerTrait.Conqueror => "Завоеватель",
            RulerTrait.Builder => "Строитель",
            RulerTrait.Pious => "Благочестивый",
            RulerTrait.Tyrant => "Тиран",
            _ => "Мудрый"
        };

        public static string NameOf(CitySpecialty specialty) => specialty switch
        {
            CitySpecialty.Port => "порт",
            CitySpecialty.Temple => "храмовый град",
            CitySpecialty.Fortress => "крепость",
            CitySpecialty.Granary => "житница",
            _ => "торговый город"
        };

        public void Dispose()
        {
            Realms.Founded -= OnFounded;
            Realms.Dissolved -= OnDissolved;
            _context.Cities.Risen -= OnCityRisen;
            _context.Cities.Fallen -= OnCityFallen;
        }

        public void Advance(int year)
        {
            _year = year;
            if (year % _config.RulePeriod == 0)
                foreach (var kingdom in Realms.All.ToList())
                    if (Realms.All.Contains(kingdom)) Rule(kingdom);
            if (year % _config.SpecialtyPeriod == 0) Specialties();
        }

        private void OnFounded(Kingdom kingdom) => Crown(kingdom, null);

        private void OnDissolved(Kingdom kingdom)
        {
            _rulers.Remove(kingdom);
            _wonders.Remove(kingdom);
        }

        private Ruler Crown(Kingdom kingdom, Ruler predecessor)
        {
            var random = _context.World.Random;
            var names = _config.Names;
            var name = names.Count == 0 ? "Безымянный" : names[random.Next(names.Count)];
            var dynasty = predecessor?.Dynasty ?? (_config.DynastyNames.Count == 0 ? kingdom.Name : _config.DynastyNames[random.Next(_config.DynastyNames.Count)]);
            var number = 1 + _rulers.Values.Count(ruler => ruler.Name == name && ruler.Dynasty == dynasty) + (predecessor != null && predecessor.Name == name ? predecessor.Number : 0);
            var traits = (RulerTrait[])Enum.GetValues(typeof(RulerTrait));
            var lifespan = _config.MinLifespan + random.Next(Math.Max(1, _config.MaxLifespan - _config.MinLifespan + 1));
            var ruler = new Ruler(name, dynasty, traits[random.Next(traits.Length)], _year, lifespan, Math.Max(1, number));
            _rulers[kingdom] = ruler;
            _context.World.History.Note(kingdom.Capital, $"Коронован {ruler.Title}, {NameOf(ruler.Trait)}");
            Crowned?.Invoke(kingdom, ruler);
            return ruler;
        }

        private void Rule(Kingdom kingdom)
        {
            if (!_rulers.TryGetValue(kingdom, out var ruler)) Crown(kingdom, null);
            ruler = _rulers[kingdom];
            var reign = _year - ruler.CrownedYear;
            if (reign >= ruler.Lifespan)
            {
                Died?.Invoke(kingdom, ruler, "умер своей смертью");
                PassCrown(kingdom, ruler, false);
                return;
            }

            if (reign * 100 >= ruler.Lifespan * _config.OldAgePercent && ruler.Trait != RulerTrait.Tyrant
                && _context.World.Random.Next(100) < _config.AbdicatePercent + (ruler.Trait == RulerTrait.Conqueror ? 0 : _config.CalmAbdicateBonus))
            {
                PassCrown(kingdom, ruler, true);
                return;
            }

            var deed = ruler.Trait switch
            {
                RulerTrait.Conqueror => Conquer(kingdom),
                RulerTrait.Builder => Build(kingdom),
                RulerTrait.Pious => Pray(kingdom),
                RulerTrait.Tyrant => Oppress(kingdom),
                _ => Govern(kingdom)
            };
            if (deed != null) Ruled?.Invoke(kingdom, ruler, deed);
        }

        private void PassCrown(Kingdom kingdom, Ruler ruler, bool abdication)
        {
            var heir = Crown(kingdom, ruler);
            if (abdication) Abdicated?.Invoke(kingdom, ruler, heir);

            var random = _context.World.Random;
            var dispute = _config.CrisisPercent
                + (ruler.Trait == RulerTrait.Tyrant || heir.Trait == RulerTrait.Tyrant ? _config.TyrantCrisisBonus : 0)
                - (ruler.Trait == RulerTrait.Wise ? _config.WiseCalm : 0)
                - (abdication ? _config.AbdicationCalm : 0);
            if (random.Next(100) >= dispute) return;

            if (random.Next(100) < _config.UsurpPercent)
            {
                var usurper = Crown(kingdom, null);
                _context.World.Memory.Add(kingdom.Capital, "usurpation");
                Usurped?.Invoke(kingdom, heir, usurper);
                return;
            }

            if (TrySecede(kingdom, out var rebel))
                CivilWar?.Invoke(kingdom, rebel, abdication ? $"не признали отречения в пользу {heir.Title}" : $"не признали наследника {heir.Title}");
        }

        private bool TrySecede(Kingdom kingdom, out GridCoord rebel)
        {
            var world = _context.World;
            rebel = default;
            var cities = kingdom.Members.Where(cell => cell != kingdom.Capital && _context.Cities.IsCity(cell)).ToList();
            var candidates = cities.Count > 0 ? cities : kingdom.Members.Where(cell => cell != kingdom.Capital).ToList();
            if (candidates.Count == 0) return false;

            rebel = candidates.OrderByDescending(cell => Distance(cell, kingdom.Capital)).ThenBy(cell => cell.X).ThenBy(cell => cell.Y).First();
            var faith = new Religion(rebel, kingdom.Religion.Dogma, true);
            var rebelCity = rebel;
            foreach (var member in kingdom.Members.Where(cell => Distance(cell, rebelCity) < Distance(cell, kingdom.Capital)).ToList())
                world.Shrines.Impose(member, faith, BeliefChange.Conquest);
            world.Shrines.Impose(rebel, faith, BeliefChange.Conquest);
            world.Memory.Add(rebel, "civil-war");
            world.Memory.Add(rebel, "war");
            world.History.Note(rebel, $"Город восстал против царства {kingdom.Name}");
            return true;
        }

        private string Conquer(Kingdom kingdom)
        {
            if (Realms.IsAtWar(kingdom)) return null;

            Realms.MakeAggressor(kingdom);
            var target = Realms.All.Where(other => other != kingdom && !Realms.IsAtWar(other))
                .OrderBy(other => other.Members.Count).FirstOrDefault();
            return target != null && Realms.TryDeclareWar(kingdom, target) ? $"объявил войну царству {target.Name}" : null;
        }

        private string Build(Kingdom kingdom)
        {
            var world = _context.World;
            if (!world.Settlements.TryGet(kingdom.Capital, out var population) || population < _config.WonderCapitalPopulation) return null;
            if (world.Random.Next(100) >= _config.WonderPercent) return null;

            var wonders = _config.Wonders.Where(wonder => !_built.Contains(wonder)).ToList();
            if (wonders.Count == 0) return null;

            var wonder = wonders[world.Random.Next(wonders.Count)];
            _built.Add(wonder);
            if (!_wonders.TryGetValue(kingdom, out var list)) _wonders[kingdom] = list = new List<WonderDefinition>();
            list.Add(wonder);
            if (wonder.Tile != null)
            {
                var site = world.Grid.Neighbors(kingdom.Capital)
                    .Where(cell => world.Tiles.All.ContainsKey(cell) && !world.Settlements.TryGet(cell, out _) && !world.Guards.IsProtected(cell))
                    .OrderBy(cell => cell.X).ThenBy(cell => cell.Y).Cast<GridCoord?>().FirstOrDefault();
                if (site is { } cell) world.Tiles.Place(cell, wonder.Tile.Id, $"чудо света: {wonder.Name}");
            }

            foreach (var outcome in wonder.Outcomes) outcome.Apply(world, kingdom.Capital, wonder.Name);
            foreach (var law in wonder.Laws) _laws.Enact(law, wonder.Name.ToLowerInvariant());
            world.Memory.Add(kingdom.Capital, "wonder");
            world.Memory.Add(world.Grid.Center, $"wonder:{wonder.Name}");
            world.History.Note(kingdom.Capital, $"Возведено чудо света: {wonder.Name}");
            WonderBuilt?.Invoke(kingdom, wonder);
            return $"возвёл чудо света: {wonder.Name}";
        }

        private string Pray(Kingdom kingdom)
        {
            kingdom.Religion.Kindle(10);
            return "велел молиться: вера царства горит ярче";
        }

        private string Oppress(Kingdom kingdom)
        {
            var world = _context.World;
            if (world.Random.Next(100) >= _config.TyrantRevoltPercent) return null;

            var victims = kingdom.Members.Where(cell => cell != kingdom.Capital).OrderBy(cell => cell.X).ThenBy(cell => cell.Y).ToList();
            if (victims.Count == 0) return null;

            var victim = victims[world.Random.Next(victims.Count)];
            world.Settlements.Weaken(victim, 1);
            if (world.Settlements.TryGet(victim, out _)) world.Shrines.Renounce(victim);
            world.Memory.Add(victim, "tyranny");
            return "задавил непокорных: поселение отреклось от веры царства";
        }

        private string Govern(Kingdom kingdom)
        {
            var settlements = _context.World.Settlements;
            foreach (var member in kingdom.Members.ToList())
                if (settlements.TryGet(member, out var population) && population < settlements.CapacityOf(member)) settlements.Bless(member, 1);
            return "правит мудро: поселения царства растут";
        }

        private void OnCityRisen(GridCoord city)
        {
            var world = _context.World;
            bool Near(params string[] ids) => world.Grid.Neighbors(city).Any(cell => world.Tiles.TryGet(cell, out var id) && ids.Contains(id));
            var specialty = Near("water", "sea", "ocean", "river") ? CitySpecialty.Port
                : world.Shrines.TryGet(city, out var faith) && faith.Shrine is { } shrine && Distance(shrine, city) <= 1 ? CitySpecialty.Temple
                : Near("mountain", "volcano") ? CitySpecialty.Fortress
                : Near("field", "granary", "irrigated_field") ? CitySpecialty.Granary
                : CitySpecialty.Market;
            _specialties[city] = specialty;
            Specialized?.Invoke(city, specialty);
        }

        private void OnCityFallen(GridCoord city) => _specialties.Remove(city);

        private void Specialties()
        {
            var world = _context.World;
            foreach (var (city, specialty) in _specialties.ToList())
            {
                if (!_context.Cities.IsCity(city)) continue;

                switch (specialty)
                {
                    case CitySpecialty.Port:
                        foreach (var neighbor in world.Grid.Neighbors(city))
                            if (world.Settlements.TryGet(neighbor, out var population) && population < world.Settlements.CapacityOf(neighbor)) world.Settlements.Bless(neighbor, 1);
                        world.Memory.Add(city, "trade");
                        break;
                    case CitySpecialty.Temple:
                        if (world.Shrines.TryGet(city, out var faith)) faith.Kindle(5);
                        break;
                    case CitySpecialty.Granary:
                        foreach (var neighbor in world.Grid.Neighbors(city)) _context.Fertility.Add(neighbor, 1);
                        break;
                    default:
                        world.Memory.Add(city, "trade");
                        if (world.Settlements.TryGet(city, out var size) && size < world.Settlements.CapacityOf(city)) world.Settlements.Bless(city, 1);
                        break;
                }
            }
        }

        private static int Distance(GridCoord a, GridCoord b) => Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));
    }
}
