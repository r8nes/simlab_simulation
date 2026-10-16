using Godland.Game.Powers.Sovereigns;
using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Game.Powers.Prophecies;
using Godland.Core.World.Biomes;
using Godland.Core.Fate.Epochs;
using Godland.Game.World.Biomes;
using Godland.Core.World.Timing;
using VContainer.Unity;
using Godland.Game.Presentation.Journal;

namespace Godland.Game.Fate.Epochs
{
    public sealed class EpochPresenter : IStartable, IDisposable
    {
        private readonly WorldEpoch _epoch;
        private readonly EpochProgression _progression;
        private readonly EpochConfig _config;
        private readonly EpochView _view;
        private readonly WorldClock _clock;
        private readonly EventJournal _journal;
        private readonly SuperBiomes _biomes;
        private readonly SuperBiomeConfig _biomeConfig;
        private readonly Godland.Core.WorldModifiers _modifiers;
        private readonly Godland.Game.Fate.Anomalies.AnomalyKeeper _anomalies;
        private readonly Godland.Game.Fate.Anomalies.AnomalyContext _anomalyContext;
        private readonly ProphecyKeeper _prophecies;
        private readonly SovereignKeeper _sovereigns;
        private readonly Godland.Core.Powers.Babel.TowerOfBabel _babel;
        private readonly Godland.Game.Life.Races.WonderAge _wonders;
        private readonly Godland.Game.Fate.Catastrophes.CatastropheKeeper _catastrophes;
        private readonly Godland.Game.Life.Races.RaceKeeper _races;
        private readonly Godland.Game.Fate.Sagas.SagaKeeper _sagas;

        public EpochPresenter(WorldEpoch epoch, EpochProgression progression, EpochConfig config, EpochView view, WorldClock clock, EventJournal journal,
            SuperBiomes biomes, SuperBiomeConfig biomeConfig, ProphecyKeeper prophecies,
            SovereignKeeper sovereigns, Godland.Core.Powers.Babel.TowerOfBabel babel, Godland.Core.WorldModifiers modifiers,
            Godland.Game.Fate.Anomalies.AnomalyKeeper anomalies, Godland.Game.Fate.Anomalies.AnomalyContext anomalyContext,
            Godland.Game.Life.Races.WonderAge wonders, Godland.Game.Life.Races.RaceKeeper races,
            Godland.Game.Fate.Catastrophes.CatastropheKeeper catastrophes, Godland.Game.Fate.Sagas.SagaKeeper sagas)
        {
            _sagas = sagas;
            _catastrophes = catastrophes;
            _wonders = wonders;
            _races = races;
            _anomalies = anomalies;
            _anomalyContext = anomalyContext;
            _modifiers = modifiers;
            _babel = babel;
            _sovereigns = sovereigns;
            _prophecies = prophecies;
            _biomes = biomes;
            _biomeConfig = biomeConfig;
            _journal = journal;
            _clock = clock;
            _epoch = epoch;
            _progression = progression;
            _config = config;
            _view = view;
        }

        public void Start()
        {
            ShowCurrent();
            _epoch.Advanced += OnAdvanced;
            _clock.Ticked += OnTicked;
        }

        public void Dispose()
        {
            _epoch.Advanced -= OnAdvanced;
            _clock.Ticked -= OnTicked;
        }

        private void OnTicked(int _) => ShowCurrent();

        private void ShowCurrent()
        {
            var leading = _biomes.Leading;
            var world = leading == null ? null
                : _biomes.Current != null ? $"Супербиом: {_biomeConfig.DefinitionOf(_biomes.Current).DisplayName} ({_biomes.SharePercent(_biomes.Current)}%)"
                    + (_biomes.CalmYearsLeft > 0 ? $", усмирён ещё {_biomes.CalmYearsLeft} лет" : string.Empty)
                : $"{_biomeConfig.DefinitionOf(leading).DisplayName}: {_biomes.SharePercent(leading)}% из {_biomeConfig.DominancePercent}%";
            if (_catastrophes.Age is { } age) _view.ShowCurrent($"{age.DisplayName} <size=60%><color=#e0b060>(последствие катастрофы)</color></size>", $"{age.Text} Ещё {_catastrophes.AgeYearsLeft(_clock.Year)} лет", Notes(world));
            else if (_wonders.IsActive) _view.ShowCurrent("Эпоха чудес", $"Народы правят миром ещё {_wonders.YearsLeft(_clock.Year)} лет", Notes(world));
            else _view.ShowCurrent(_progression.Current.DisplayName, _progression.GoalText, Notes(world));
        }

        private IEnumerable<string> Notes(string world)
        {
            if (_sagas.Current is { } saga)
                yield return $"<color=#c9a0ff>Сага «{saga.Title}», акт «{_sagas.Act.Title}»: "
                    + string.Join(" против ", _sagas.Factions.Select(faction => $"{faction.Definition.DisplayName} {faction.Power}/{_sagas.ThresholdOf(faction)}")) + "</color>";
            if (world != null) yield return world;
            if (_catastrophes.Coming is { } coming) yield return $"Грядёт: {coming.Title} — {_catastrophes.StrikeYear} год";
            if (_races.Living.Any()) yield return "Народы: " + string.Join("; ", _races.Living.Select(race => $"{race.Definition.DisplayName} ({race.Cells.Count}) — {race.Mind.Describe(name => name)}"));
            if (_anomalies.Coming != null) yield return $"Знамение: «{_anomalies.Coming.Omen}» — {_anomalies.ComingYear} год";
            if (_anomalyContext.Devourer.IsActive) yield return $"Пожиратель пожирает мир. Накормить верой: ещё {_anomalyContext.Devourer.FeedingsLeft}";
            if (_anomalyContext.Drift.IsActive) yield return "Земли дрейфуют";
            if (_anomalyContext.Heart.IsBeating) yield return "Бьётся сердце мира — не дай ему остановиться";
            if (_anomalyContext.Sleep.IsAsleep(_clock.Year)) yield return $"Мир спит до {_anomalyContext.Sleep.Until} года";
            if (_clock.Year < _modifiers.MadUntil) yield return $"Безумие: чудеса бьют мимо до {_modifiers.MadUntil} года";
            if (_babel.IsRising) yield return $"Вавилонская башня: {_babel.Floors}/{_babel.MaxFloors}" + (_babel.Blessed ? " (благословлена)" : string.Empty);
            if (_clock.Year < _modifiers.MiraclesBlockedUntil) yield return $"Люди отвергли чудеса: недоступны до {_modifiers.MiraclesBlockedUntil} года";
            if (_sovereigns.Ruler is { } ruler) yield return $"Правитель мира: {ruler.DisplayName} ({ruler.Temperament.Describe()}; {_sovereigns.MindOf(ruler).Describe(name => name)}). Свергнуть: {ruler.OverthrowText}";
            if (_prophecies.Active is { } prophecy) yield return $"Пророчество: «{prophecy.Title}» — до {_prophecies.Deadline} года";
            if (_prophecies.Milestones.Count > 0)
                yield return "Законы мира: " + string.Join(", ", _prophecies.Milestones.Select(milestone => milestone.DisplayName));
        }

        private void OnAdvanced(int _)
        {
            ShowCurrent();
            _view.Announce("Новая эпоха", _progression.Current.DisplayName, _config.BannerDuration);
            _journal.Write($"Наступила новая эпоха: {_progression.Current.DisplayName}" + (string.IsNullOrEmpty(_progression.Current.Text) ? string.Empty : $". {_progression.Current.Text}"), JournalTone.Good);
        }
    }
}
