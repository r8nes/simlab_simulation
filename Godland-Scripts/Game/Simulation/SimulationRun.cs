using System.Collections.Generic;
using Godland.Core.Powers.Babel;
using Godland.Game.Powers.Sovereigns;
using Godland.Game.Powers.Prophecies;
using Godland.Core.Life.Heroes;
using System.Linq;
using Godland.Core.World.Biomes;
using Godland.Core.Life.Cities;
using Godland.Core.Life.Kingdoms;
using Godland.Core.Fate.Cataclysms;
using Godland.Core.World.Climate;
using Godland.Core.World.Corruption;
using Godland.Core.World.Creation;
using Godland.Core.Fate.Epochs;
using Godland.Core.Life.Features;
using Godland.Core.Powers.Gods;
using Godland.Core.Life.People;
using Godland.Core.Randomness;
using Godland.Core.World.Tiles;
using Godland.Core.World.Timing;
using Godland.Core.World.Volcanoes;
using Godland.Game.Writer.Gifts;
using Godland.Game.Writer.Run;
using Godland.Game.Presentation.Settings;
using Godland.Game.Fate.WorldEvents;

namespace Godland.Game.Simulation
{
    public static class SimulationRun
    {
        public const int SnapshotPeriod = 300;

        public static RunStats Play(GameSettings settings, ISimulationBot bot, int seed, int years,
            (Dictionary<string, int> Percents, int Unseen)? flow = null)
        {
            var session = new SimulationSession(settings, bot, seed, years, flow);
            while (!session.IsDone) session.Step();
            return session.Finish();
        }

        public static Dictionary<string, int> Snapshot(SimulatedWorld world) => SimulationSession.Snapshot(world);
    }

    public sealed class SimulationSession
    {
        private readonly SimulatedWorld _world;
        private readonly RunStats _stats;
        private readonly WorldClock _clock;
        private readonly GiftOffers _offers;
        private readonly ISimulationBot _bot;
        private readonly SeededRandom _botRandom;
        private readonly int _interval;
        private readonly int _years;
        private int _year;

        public SimulationSession(GameSettings settings, ISimulationBot bot, int seed, int years,
            (Dictionary<string, int> Percents, int Unseen)? flow = null, System.Action<int, string> log = null)
        {
            var world = new SimulatedWorld(settings, seed);
            if (flow is { } bias) world.Get<Godland.Core.Flow.FlowBias>().Load(bias.Percents, bias.Unseen);
            var stats = new RunStats { Seed = seed };
            if (log != null) stats.Counted += log;
            var clock = world.Get<WorldClock>();
            int Now() => clock.Year;

            var settlements = world.Get<Settlements>();
            world.Get<CreatureGenes>().Speciated += (_, gene) => stats.Count($"Новый вид: {gene}", Now());
            world.Get<CreatureGenes>().Mauled += (_, _) => stats.Count("Звери напали на поселение", Now());
            world.Get<Shrines>().BeliefChanged += (cell, old, now, why) => { if (why == BeliefChange.Conversion && old != null) stats.Count("Волна обращения (сменили веру)", Now()); };
            world.Get<Godland.Game.Writer.Dilemmas.DilemmaQueue>().Resolved += (dilemma, option) => stats.Count(dilemma.Title.StartsWith("Сделка") ? "Сделка сил" : "Развилка", Now());
            world.Get<Godland.Game.Writer.Threads.WriterThreads>().Happened += (text, _) => stats.Count(text.StartsWith("Пророк") && text.Contains("умолк") ? "Пророк умолк" : text.StartsWith("Родился пророк") ? "Родился пророк" : text.StartsWith("Пророк") ? "Пророк говорит" : text.Split(':')[0], Now());
            world.Get<Godland.Game.Writer.Commandments.CommandmentRewrite>().Rewritten += (_, _) => stats.Count("Заповедь переписана", Now());
            settlements.Founded += _ => stats.Count("Основано поселение", Now());
            settlements.Abandoned += _ => stats.Count("Поселение опустело", Now());
            settlements.Rescued += (_, _) => stats.Count("Соседи спасли поселение", Now());
            var plague = world.Get<Plague>();
            plague.Outbreak += (_, _) => stats.Count("Вспышка мора", Now());
            plague.Spread += (_, _) => stats.Count("Мор перекинулся", Now());
            world.Get<Feuds>().Raided += (_, _, conquered) => stats.Count(conquered ? "Набег с обращением" : "Набег", Now());
            world.Get<VolcanoActivity>().Erupted += (_, _) => stats.Count("Извержение", Now());
            var cataclysms = world.Get<IdolCataclysms>();
            cataclysms.Announced += _ => stats.Count("Призыв объявлен", Now());
            cataclysms.Averted += _ => stats.Count("Призыв сорван", Now());
            cataclysms.Struck += (summoning, _) => stats.Count($"Катаклизм: {summoning.Religion.IdolId}", Now());
            var pantheon = world.Get<Pantheon>();
            pantheon.Born += god => stats.Count($"Родился бог: {god.Rule.Id}", Now());
            pantheon.Fallen += god => stats.Count($"Пал бог: {god.Rule.Id}", Now());
            pantheon.IdolFaded += _ => stats.Count("Идол угас", Now());
            pantheon.IdolResisted += _ => stats.Count("Идол устоял", Now());
            world.Get<Godfighters>().Rebelled += _ => stats.Count("Бунт богоборцев", Now());
            world.Get<SuperBiomes>().Entered += biome => stats.Count($"Супербиом: {biome.Id}", Now());
            world.Get<CorruptionSpread>().Corrupted += _ => stats.Count("Порча расползлась", Now());
            var shrines = world.Get<Shrines>();
            shrines.IdolRevealed += (religion, _) => stats.Count($"Идол: {religion.IdolId}", Now());
            shrines.BeliefChanged += (_, _, _, reason) =>
            {
                if (reason == BeliefChange.Schism) stats.Count("Раскол (поселений)", Now());
            };
            world.Get<WorldEpoch>().Advanced += index => stats.Count($"Эпоха {index}: {world.Get<Godland.Game.Fate.Epochs.EpochProgression>().Current.DisplayName}", Now());
            world.Get<Monsters>().Attacked += (_, _, _) => stats.Count("Нападение чудовищ", Now());
            var climate = world.Get<WorldClimate>();
            climate.Desertified += _ => stats.Count("Опустынивание", Now());
            climate.Frozen += (_, _) => stats.Count("Ледник сполз", Now());
            climate.ForestGrew += _ => stats.Count("Лес разросся", Now());
            world.Get<WorldEventRoller>().Happened += worldEvent => stats.Count($"Событие: {worldEvent.Title}", Now());
            var offers = world.Get<GiftOffers>();
            offers.Chosen += gift => stats.Count($"Дар: {gift.DisplayName}", Now());

            var cities = world.Get<Urbanization>();
            cities.Risen += _ => stats.Count("Город возник", Now());
            cities.Fallen += _ => stats.Count("Город пал", Now());
            cities.Built += (_, _, _) => stats.Count("Город застроил тайл", Now());
            var realms = world.Get<Realms>();
            realms.Founded += _ => stats.Count("Царство основано", Now());
            realms.Dissolved += _ => stats.Count("Царство распалось", Now());
            realms.WarDeclared += _ => stats.Count("Война", Now());
            realms.Conquered += (_, _, _) => stats.Count("Захват в войне", Now());
            var temples = world.Get<Temples>();
            temples.Built += (_, _) => stats.Count("Храм построен", Now());
            temples.Lost += (_, _) => stats.Count("Храм заброшен", Now());
            pantheon.Forgotten += god => stats.Count($"Забыт бог: {god.Rule.Id}", Now());
            var heroes = world.Get<HeroSaga>();
            heroes.Born += _ => stats.Count("Родился герой", Now());
            heroes.Triumphed += hero => stats.Count($"Легенда: {hero.Quest}", Now());
            heroes.Perished += hero => stats.Count($"Трагедия: {hero.Quest}", Now());
            heroes.Retired += _ => stats.Count("Герой ушёл на покой", Now());
            var pantheons = world.Get<Godland.Game.Powers.Pantheons.PantheonKeeper>();
            pantheons.Arrived += (house, _) => stats.Count($"Пантеон пришёл: {house.Definition.DisplayName}", Now());
            pantheons.GodBorn += (house, god) => stats.Count($"Бог пантеона: {god.HeroName}", Now());
            pantheons.BondChanged += (_, _, bond) => stats.Count($"Пантеоны: {bond}", Now());
            pantheons.Succeeded += (house, _) => stats.Count($"Пантеон сменил главу: {house.Definition.DisplayName}", Now());
            pantheons.Faded += (house, _) => stats.Count($"Пантеон угас: {house.Definition.DisplayName}", Now());
            pantheons.Absorbed += (strong, weak) => stats.Count($"Пантеон растворился: {weak.Definition.DisplayName}", Now());
            pantheons.Schemed += (house, god, action, _) => stats.Count($"Замысел-шаг: {action.Name}", Now());
            var dynasties = world.Get<Godland.Game.Life.Kingdoms.Dynasties>();
            dynasties.Crowned += (_, ruler) => stats.Count($"Коронован: {Godland.Game.Life.Kingdoms.Dynasties.NameOf(ruler.Trait)}", Now());
            dynasties.CivilWar += (_, _, _) => stats.Count("Гражданская война", Now());
            dynasties.Abdicated += (_, _, _) => stats.Count("Отречение", Now());
            dynasties.Usurped += (_, _, _) => stats.Count("Узурпация", Now());
            dynasties.WonderBuilt += (_, wonder) => stats.Count($"Чудо света: {wonder.Name}", Now());
            dynasties.Specialized += (_, specialty) => stats.Count($"Город: {Godland.Game.Life.Kingdoms.Dynasties.NameOf(specialty)}", Now());
            var schemes = world.Get<Godland.Game.Powers.Pantheons.WorldSchemes>();
            schemes.Schemed += (actor, action, _) => stats.Count($"Замысел мира: {actor.Split(',')[0]} — {action.Name}", Now());
            schemes.Fulfilled += (actor, scheme) => stats.Count($"Замысел мира исполнен: {actor.Split(',')[0]} — {scheme.Goal}", Now());
            var gods = world.Get<Godland.Core.Powers.Gods.Pantheon>();
            var tiles = world.Get<Godland.Core.World.Tiles.WorldTiles>();
            var lastCause = "";
            tiles.Changed += (_, _, cause) => lastCause = cause;
            gods.Fallen += god => { if (god.PantheonId != null) stats.Count($"Бог пантеона пал: {lastCause}", Now()); };
            gods.Forgotten += god => { if (god.PantheonId != null) stats.Count("Бог пантеона забыт", Now()); };
            gods.Died += god => { if (god.PantheonId != null) stats.Count("Бог пантеона умер от старости", Now()); };
            pantheons.SchemeFulfilled += (house, god, scheme) => stats.Count($"Замысел исполнен: {scheme.Goal}", Now());
            world.Get<Godland.Game.Writer.Commands.EntityCommands>().Issued += (command, _) => stats.Count($"Приказ: {command.Definition.Title}", Now());
            heroes.Faded += _ => stats.Count("Герой ушёл с эпохой", Now());
            heroes.Born += hero => stats.Count($"Герой: {hero.Archetype?.Title}", Now());
            heroes.Fiasco += hero => stats.Count($"Грандиозный провал: {hero.Quest}", Now());
            heroes.Allied += (_, _) => stats.Count("Союз героев", Now());
            heroes.Dueled += (_, _) => stats.Count("Дуэль героев", Now());
            heroes.Treaty += (_, _, peace) => stats.Count(peace ? "Договор героев с миром" : "Договор героев", Now());

            world.Get<Godland.Core.World.Cells.FertilityOverflow>().Transformed += (_, tile) => stats.Count($"Избыток: стал {tile}", Now());
            var anomalies = world.Get<Godland.Game.Fate.Anomalies.AnomalyKeeper>();
            anomalies.Began += anomaly => stats.Count($"Явление: {anomaly.Title}", Now());
            world.Get<Godland.Core.Fate.Anomalies.Devourer>().Ate += _ => stats.Count("Пожиратель съел тайл", Now());
            world.Get<Godland.Core.Fate.Anomalies.WorldHeart>().Died += () => stats.Count("Сердце мира остановилось", Now());
            world.Get<Godland.Game.Fate.Emergence.EmergenceKeeper>().Emerged += (emergence, _) => stats.Count($"Возникло: {emergence.Title}", Now());
            var titans = world.Get<Godland.Core.Powers.Titans.TitanHost>();
            titans.Awoke += titan => stats.Count($"Титан пробудился: {titan.Rule.Id}", Now());
            titans.Slain += (titan, by) => stats.Count(by == null ? $"Титан повержен богом: {titan.Rule.Id}" : $"Титан повержен: {titan.Rule.Id}", Now());
            titans.Pacted += (_, _) => stats.Count("Договор бога с титаном", Now());
            titans.Died += titan => stats.Count($"Титан угас: {titan.Rule.Id}", Now());
            titans.Devoured += (_, _) => stats.Count("Титан пожрал бога", Now());
            titans.Clashed += (_, _, won) => stats.Count(won ? "Титан одолел бога" : "Бог поверг титана", Now());
            titans.Roused += titan => stats.Count($"Проснулся: {titan.Rule.Id}", Now());
            var races = world.Get<Godland.Game.Life.Races.RaceKeeper>();
            races.Born += (race, _) => stats.Count((race.GodName == null ? "Народ из обстоятельств: " : "Народ от бога: ") + race.Definition.DisplayName, Now());
            races.Diverged += (_, child) => stats.Count($"Раскол народа: {child.Name}", Now());
            races.Vanished += race => stats.Count($"Народ исчез: {race.Name}", Now());
            races.Acted += (race, action, _, reason) => stats.Count($"{race.Definition.DisplayName}: {action.Name} ({reason})", Now());
            var wonders = world.Get<Godland.Game.Life.Races.WonderAge>();
            wonders.Began += () => stats.Count("Эпоха чудес началась", Now());
            wonders.Ended += dominant => stats.Count(dominant == null ? "Эпоха чудес угасла" : $"Эпоха чудес: победил народ {dominant.Definition.DisplayName}", Now());
            wonders.Clashed += (_, _, _) => stats.Count("Война народов", Now());
            wonders.Befriended += (_, _, _) => stats.Count("Дружба народов", Now());
            wonders.Miracle += _ => stats.Count("Чудо эпохи", Now());
            var catastrophes = world.Get<Godland.Game.Fate.Catastrophes.CatastropheKeeper>();
            catastrophes.Foretold += (catastrophe, _) => stats.Count($"Предвестие: {catastrophe.Title}", Now());
            catastrophes.Struck += catastrophe => stats.Count($"Катастрофа: {catastrophe.Title}", Now());
            catastrophes.Averted += catastrophe => stats.Count($"Катастрофа миновала: {catastrophe.Title}", Now());
            catastrophes.AgeEnded += age => stats.Count($"Окончена эпоха: {age.DisplayName}", Now());
            var reclaim = world.Get<Godland.Game.Life.Reclaim.ReclaimKeeper>();
            reclaim.Overgrown += (_, _) => stats.Count("Брошенная земля заросла", Now());
            reclaim.Inhabited += (_, _) => stats.Count("Брошенную землю заняли звери", Now());
            reclaim.Returned += (_, race) => stats.Count(race == null ? "Люди вернулись на брошенную землю" : $"Брошенную землю заняли: {race.Definition.DisplayName}", Now());
            var commandments = world.Get<Godland.Game.Writer.Commandments.CommandmentKeeper>();
            var draft = world.Get<Godland.Game.Writer.Commandments.CommandmentDraft>();
            draft.Offer();
            commandments.Proclaim(draft.PickRandom());
            stats.Count($"Заповедь: {commandments.Current.Text}", Now());
            commandments.Fired += (commandment, _) => stats.Count($"Заповедь сработала: {commandment.Then.Text}", Now());
            var peaks = world.Get<Godland.Core.World.Peaks.SuperPeaks>();
            peaks.Raised += _ => stats.Count("Встал хребет супергор", Now());
            peaks.Withstood += (_, _) => stats.Count("Супергора устояла", Now());
            var sagas = world.Get<Godland.Game.Fate.Sagas.SagaKeeper>();
            sagas.Began += saga => stats.Count($"Сага: {saga.Title}", Now());
            sagas.ActBegan += act => stats.Count($"Акт саги: {act.Title}", Now());
            sagas.Ended += (saga, transition) => stats.Count($"Конец саги {saga.Title}: {transition.Text}", Now());
            sagas.Battled += (_, _, _) => stats.Count("Битва саги", Now());
            sagas.Overthrown += (_, _) => stats.Count("Заговор в саге", Now());
            sagas.ArtifactFound += faction => stats.Count($"Артефакт: {faction.Definition.DisplayName}", Now());
            world.Get<Godland.Core.Life.Heroes.HeroSaga>().Ascended += hero => stats.Count("Герой стал полубогом", Now());
            var babel = world.Get<TowerOfBabel>();
            babel.Founded += _ => stats.Count("Башня заложена", Now());
            babel.Completed += blessed => stats.Count(blessed ? "Башня: благословлена" : "Башня: вызов небесам", Now());
            babel.Toppled += () => stats.Count("Башня рухнула", Now());
            var sovereigns = world.Get<SovereignKeeper>();
            sovereigns.Rose += sovereign => stats.Count($"Восхождение: {sovereign.DisplayName}", Now());
            sovereigns.Fell += sovereign => stats.Count($"Свергнут: {sovereign.DisplayName}", Now());
            sovereigns.Acted += (sovereign, deed) => stats.Count($"Деяние правителя: {deed.Split('(')[0].Trim()}", Now());
            var keeper = world.Get<ProphecyKeeper>();
            keeper.Announced += (prophecy, _) => stats.Count($"Пророчество: {prophecy.Title}", Now());
            keeper.Resolved += (_, milestone, fulfilled) => stats.Count($"Веха ({(fulfilled ? "сбылось" : "предотвращено")}): {milestone.DisplayName}", Now());
            world.Get<Pantheon>().Died += god => stats.Count($"Бог умер от старости: {god.Rule.Id}", Now());
            world.Get<LivingLand>().Spread += (_, _, _) => stats.Count("Живая земля: тайл захватил соседа", Now());
            settlements.Exodus += (_, _) => stats.Count("Исход: открыта новая клетка", Now());

            _world = world;
            _stats = stats;
            _clock = clock;
            _offers = offers;
            _bot = bot;
            _botRandom = new SeededRandom(seed * 31 + 7);
            _interval = settings.Run.GiftIntervalYears;
            _years = years;
        }

        public SimulatedWorld World => _world;
        public RunStats Stats => _stats;
        public int Year => _year;
        public bool IsDone => _year >= _years;

        public void Step()
        {
            if (IsDone) return;

            if (_year > 0 && _year % _interval == 0) _offers.Offer();
            if (_offers.IsOpen) _offers.Choose(_bot.ChooseGift(_offers.Current, _botRandom));

            _bot.Act(_year, _world, _botRandom);
            _world.Simulation.AdvanceYear();

            if (_clock.Year % SimulationRun.SnapshotPeriod == 0) _stats.Snapshots[_clock.Year] = Snapshot(_world);
            _year++;
        }

        public RunStats Finish()
        {
            _stats.FlowKeys.AddRange(_world.Get<Godland.Core.Flow.FlowBias>().Seen);
            return _stats;
        }

        public static System.Collections.Generic.Dictionary<string, int> Snapshot(SimulatedWorld world)
        {
            var settlements = world.Get<Settlements>();
            var shrines = world.Get<Shrines>();
            return new System.Collections.Generic.Dictionary<string, int>
            {
                ["Тайлов"] = world.Get<WorldTiles>().CountedCount,
                ["Супергор"] = world.Get<Godland.Core.World.Peaks.SuperPeaks>().All.Count,
                ["Поселений"] = settlements.All.Count,
                ["Жителей"] = settlements.All.Values.Sum(),
                ["Верующих поселений"] = shrines.BelieverCount,
                ["Религий"] = shrines.Religions.Count,
                ["Вера"] = world.Get<FaithPoints>().Amount,
                ["Созидание"] = world.Get<CreationPoints>().Amount,
                ["Эпоха"] = world.Get<WorldEpoch>().Index,
                ["Богов"] = world.Get<Pantheon>().Gods.Count,
                ["Богоборцев"] = settlements.All.Keys.Count(world.Get<Godfighters>().IsRebel),
                ["Больных мором"] = world.Get<Plague>().SickCount,
                ["Городов"] = world.Get<Urbanization>().All.Count,
                ["Царств"] = world.Get<Realms>().All.Count,
                ["Войн"] = world.Get<Realms>().Wars.Count,
                ["Храмов"] = world.Get<Pantheon>().Gods.Sum(world.Get<Temples>().CountOf)
            };
        }
    }
}
