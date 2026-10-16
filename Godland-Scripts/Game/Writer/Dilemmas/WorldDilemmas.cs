using System;
using System.Linq;
using Godland.Core;
using Godland.Core.World.Creation;
using Godland.Core.World.Grid;
using Godland.Core.Life.Kingdoms;
using Godland.Core.Life.People;
using Godland.Game.Fate.Catastrophes;
using Godland.Game.Powers.Gods;
using Godland.Game.Life.Kingdoms;
using Godland.Game.Powers.Pantheons;
using Godland.Game.Powers.Sovereigns;
using Godland.Game.Powers.Titans;

namespace Godland.Game.Writer.Dilemmas
{
    public sealed class WorldDilemmas : IWorldStep, IDisposable
    {
        private readonly DilemmaConfig _config;
        private readonly DilemmaQueue _queue;
        private readonly CatastropheContext _context;
        private readonly Dynasties _dynasties;
        private readonly PantheonKeeper _pantheons;
        private readonly CatastropheKeeper _catastrophes;
        private readonly SovereignKeeper _sovereigns;
        private readonly TitanConfig _titans;
        private readonly GodsConfig _gods;
        private readonly Feuds _feuds;
        private readonly CreationPoints _creation;
        private readonly FaithPoints _faith;

        public WorldDilemmas(
            DilemmaConfig config, DilemmaQueue queue, CatastropheContext context, Dynasties dynasties, PantheonKeeper pantheons,
            CatastropheKeeper catastrophes, SovereignKeeper sovereigns, TitanConfig titans, GodsConfig gods, Feuds feuds,
            CreationPoints creation, FaithPoints faith)
        {
            _config = config;
            _queue = queue;
            _context = context;
            _dynasties = dynasties;
            _pantheons = pantheons;
            _catastrophes = catastrophes;
            _sovereigns = sovereigns;
            _titans = titans;
            _gods = gods;
            _feuds = feuds;
            _creation = creation;
            _faith = faith;

            _dynasties.CivilWar += OnCivilWar;
            _catastrophes.Foretold += OnForetold;
            _pantheons.BondChanged += OnBond;
        }

        private Godland.Game.Fate.Emergence.EmergenceContext World => _context.World;

        public void Dispose()
        {
            _dynasties.CivilWar -= OnCivilWar;
            _catastrophes.Foretold -= OnForetold;
            _pantheons.BondChanged -= OnBond;
        }

        public void Advance(int year)
        {
            if (!_config.DealsEnabled || year % _config.DealPeriod != 0 || World.Epoch.Index < _config.DealFromEpoch) return;
            if (World.Random.Next(100) >= _config.DealChancePercent) return;

            OfferDeal();
        }

        private void Remember(GridCoord? place, string key) => World.Memory.Add(place ?? World.Grid.Center, key);

        private void OnCivilWar(Kingdom kingdom, GridCoord rebel, string cause)
        {
            if (!_config.CrossroadsEnabled || !World.Shrines.TryGet(rebel, out var rebels)) return;

            var crown = kingdom.Religion;
            _queue.Offer(new Dilemma($"Мятеж в царстве {kingdom.Name}", $"Город восстал: {cause}. Чью сторону примет писатель?", rebel,
                new DilemmaOption("Не вмешиваться", 0, 0, () => Remember(rebel, "crossroad-ignored")),
                new DilemmaOption("Поддержать корону", _config.BackSideCreation, 0, () =>
                {
                    foreach (var cell in World.Shrines.FollowersOf(rebels).ToList()) World.Shrines.Impose(cell, crown, BeliefChange.Conquest);
                    Remember(kingdom.Capital, "crown-backed");
                }),
                new DilemmaOption("Поддержать мятеж", _config.BackSideCreation, 0, () =>
                {
                    rebels.Kindle(30);
                    World.Settlements.Bless(rebel, 3);
                    Remember(rebel, "rebellion-backed");
                }),
                new DilemmaOption("Примирить стороны", 0, _config.ReconcileFaith, () =>
                {
                    _feuds.Calm(World.Shrines.FollowersOf(rebels).Concat(kingdom.Members));
                    Remember(rebel, "truce");
                })));
        }

        private void OnForetold(CatastropheDefinition catastrophe, int strikeYear)
        {
            if (!_config.CrossroadsEnabled) return;

            _queue.Offer(new Dilemma($"Предвестие: {catastrophe.Title}", $"{catastrophe.Omen}\nУдар в {strikeYear} году.", null,
                new DilemmaOption("Принять судьбу", 0, 0, () => Remember(null, "fate-accepted")),
                _catastrophes.WasDelayed ? null : new DilemmaOption("Отсрочить на век", _config.DelayCreation, 0, () =>
                {
                    _catastrophes.Delay(100);
                    Remember(null, "doom-delayed");
                }),
                new DilemmaOption("Подготовить людей", 0, _config.PrepareFaith, () =>
                {
                    foreach (var cell in World.Settlements.All.Keys.ToList()) World.Settlements.Bless(cell, 2);
                    Remember(null, "prepared");
                })));
        }

        private void OnBond(PantheonHouse first, PantheonHouse second, PantheonBond bond)
        {
            if (!_config.CrossroadsEnabled || bond != PantheonBond.War) return;

            _queue.Offer(new Dilemma("Священная война", $"«{first.Definition.DisplayName}» и «{second.Definition.DisplayName}» пошли друг на друга.", first.Head?.Seat,
                new DilemmaOption("Пусть бьются", 0, 0, () => Remember(null, "crossroad-ignored")),
                new DilemmaOption($"Встать за «{first.Definition.DisplayName}»", 0, _config.TakeSideFaith, () => Back(first, second)),
                new DilemmaOption($"Встать за «{second.Definition.DisplayName}»", 0, _config.TakeSideFaith, () => Back(second, first)),
                new DilemmaOption("Примирить богов", _config.PeaceCreation, 0, () =>
                {
                    _pantheons.Shift(first, second, 60);
                    Remember(first.Head?.Seat, "peace");
                })));
        }

        private void Back(PantheonHouse favored, PantheonHouse other)
        {
            favored.Faith.Kindle(30);
            other.Faith.Cool(20);
            Remember(favored.Head?.Seat, "god-favored");
            Remember(other.Head?.Seat, "god-spurned");
        }

        private void OfferDeal()
        {
            var random = World.Random;
            var offers = new System.Collections.Generic.List<Func<Dilemma>>();
            foreach (var house in _pantheons.Registry.Active) offers.Add(() => PantheonDeal(house));
            foreach (var titan in World.Titans.All.Where(titan => titan.IsAwake && !titan.IsTamed)) offers.Add(() => TitanDeal(titan));
            if (_sovereigns.Ruler != null && _sovereigns.Claimant != null) offers.Add(SovereignDeal);
            if (offers.Count == 0) return;

            var dilemma = offers[random.Next(offers.Count)]();
            if (dilemma != null) _queue.Offer(dilemma);
        }

        private Dilemma PantheonDeal(PantheonHouse house)
        {
            var name = house.Definition.DisplayName;
            var head = house.Head == null ? name : _gods.NameOf(house.Head);
            if (World.Random.Next(2) == 0)
                return new Dilemma($"Сделка: {name}", $"{head}: «Дай нам веры — и мы поклянёмся сто лет не начинать войн».", house.Head?.Seat,
                    new DilemmaOption("Отказать", 0, 0, () => Remember(house.Head?.Seat, "pantheon-spurned")),
                    new DilemmaOption("Заключить клятву", 0, _config.OathFaith, () =>
                    {
                        foreach (var other in _pantheons.Registry.Active.Where(other => other != house).ToList()) _pantheons.Shift(house, other, 40);
                        Remember(house.Head?.Seat, "deal");
                    }));

            var city = World.Settlements.All.Keys
                .Where(cell => _context.Cities.IsCity(cell) && !(World.Shrines.TryGet(cell, out var faith) && faith == house.Faith))
                .OrderByDescending(cell => World.Settlements.All[cell]).ThenBy(cell => cell.X).ThenBy(cell => cell.Y)
                .Cast<GridCoord?>().FirstOrDefault();
            if (city is not { } target) return null;

            return new Dilemma($"Сделка: {name}", $"{head}: «Отдай нам этот город — и мы щедро заплатим созиданием».", target,
                new DilemmaOption("Отказать", 0, 0, () => Remember(house.Head?.Seat, "pantheon-spurned")),
                new DilemmaOption($"Отдать город (+{_config.CityPayment} созид.)", 0, 0, () =>
                {
                    World.Shrines.Impose(target, house.Faith, BeliefChange.Calling);
                    _creation.Add(_config.CityPayment);
                    Remember(target, "city-sold");
                    Remember(target, "deal");
                }));
        }

        private Dilemma TitanDeal(Godland.Core.Powers.Titans.Titan titan)
        {
            var name = _titans.DefinitionOf(titan.Rule.Id).DisplayName;
            return new Dilemma($"Сделка: {name}", $"{name}: «Накорми меня созиданием — и я усну на полтора века».", titan.Position,
                new DilemmaOption("Отказать", 0, 0, () =>
                {
                    var prey = World.Settlements.All.Keys
                        .OrderBy(cell => Math.Abs(cell.X - titan.Position.X) + Math.Abs(cell.Y - titan.Position.Y)).ThenBy(cell => cell.X).ThenBy(cell => cell.Y)
                        .Cast<GridCoord?>().FirstOrDefault();
                    if (prey is { } cell) World.Titans.Direct(titan, cell);
                    Remember(titan.Position, "titan-spurned");
                }),
                new DilemmaOption("Накормить титана", _config.FeedTitanCreation, 0, () =>
                {
                    World.Titans.Lull(titan, 150);
                    Remember(titan.Position, "titan-lulled");
                    Remember(titan.Position, "deal");
                }));
        }

        private Dilemma SovereignDeal()
        {
            var ruler = _sovereigns.Ruler;
            var claimant = _sovereigns.Claimant;
            var place = World.Shrines.FollowersOf(claimant).Cast<GridCoord?>().FirstOrDefault();
            return new Dilemma($"Сделка: {ruler.DisplayName}", $"{ruler.DisplayName}: «Признай мою власть над миром — и я отдам тебе десятину веры».", place,
                new DilemmaOption("Не признавать", 0, 0, () => Remember(place, "defiance")),
                new DilemmaOption($"Признать (+{_config.TitheFaith} веры)", 0, 0, () =>
                {
                    _faith.Add(_config.TitheFaith);
                    claimant.Kindle(30);
                    Remember(place, "deal");
                    Remember(place, "ruler-recognized");
                }));
        }
    }
}
