using System;
using Godland.Core.Life.Features;
using Godland.Core.World.Grid;
using Godland.Game.Presentation.Feedback;
using VContainer.Unity;
using Godland.Game.Presentation.Journal;

namespace Godland.Game.Life.Features
{
    public sealed class ThreatPresenter : IStartable, IDisposable
    {
        private readonly FeatureInteractions _interactions;
        private readonly FeatureCatalog _catalog;
        private readonly FloatingTextSpawner _spawner;
        private readonly FeatureDepletion _depletion;
        private readonly TileFeatures _features;
        private readonly FeatureSpread _spread;
        private readonly FeatureEvolutions _evolutions;
        private readonly EventJournal _journal;
        private readonly Monsters _monsters;

        public ThreatPresenter(
            FeatureInteractions interactions, FeatureCatalog catalog, FloatingTextSpawner spawner, FeatureDepletion depletion, TileFeatures features,
            FeatureSpread spread, FeatureEvolutions evolutions, EventJournal journal,
            Monsters monsters)
        {
            _monsters = monsters;
            _journal = journal;
            _evolutions = evolutions;
            _spread = spread;
            _depletion = depletion;
            _features = features;
            _interactions = interactions;
            _catalog = catalog;
            _spawner = spawner;
        }

        public void Start()
        {
            _interactions.ThreatStarted += ShowThreat;
            _interactions.ThreatAdvanced += ShowThreat;
            _interactions.Resolved += ShowResolved;
            _depletion.Recovered += ShowRecovered;
            _spread.Spread += ShowSpread;
            _evolutions.Evolved += ShowEvolved;
            _monsters.Attacked += ShowAttack;
            _monsters.Mutated += ShowMutation;
        }

        public void Dispose()
        {
            _interactions.ThreatStarted -= ShowThreat;
            _interactions.ThreatAdvanced -= ShowThreat;
            _interactions.Resolved -= ShowResolved;
            _depletion.Recovered -= ShowRecovered;
            _spread.Spread -= ShowSpread;
            _evolutions.Evolved -= ShowEvolved;
            _monsters.Attacked -= ShowAttack;
            _monsters.Mutated -= ShowMutation;
        }

        private void ShowThreat(GridCoord prey, FeatureThreat threat)
        {
            var definition = _catalog.GetInteraction(threat.Interaction);
            _spawner.Spawn(prey, $"! {definition.Predator.DisplayName} {definition.ThreatText}: {threat.YearsLeft}", FloatingTextStyle.Threat);
        }

        private void ShowAttack(GridCoord monster, GridCoord settlement, string monsterId)
        {
            var text = $"Чудовища ({_catalog.Get(monsterId).DisplayName}) напали на поселение";
            _spawner.Spawn(settlement, text, FloatingTextStyle.Threat);
            _journal.Write(text, JournalTone.Threat);
        }

        private void ShowMutation(GridCoord coord, string from, string to)
        {
            var text = $"Мутация: {_catalog.Get(from).DisplayName} → {_catalog.Get(to).DisplayName}";
            _spawner.Spawn(coord, text, FloatingTextStyle.Threat);
            _journal.Write(text, JournalTone.Threat);
        }

        private void ShowEvolved(GridCoord coord, string from, string to)
        {
            var text = $"{_catalog.Get(from).DisplayName} эволюционировали: {_catalog.Get(to).DisplayName}!";
            _spawner.Spawn(coord, text, FloatingTextStyle.Evolution);
            _journal.Write(text, JournalTone.Good);
        }

        private void ShowSpread(GridCoord colony, GridCoord target, string featureId) =>
            _spawner.Spawn(target, $"Пришли дикие: {_catalog.Get(featureId).DisplayName}", FloatingTextStyle.Fertility);

        private void ShowRecovered(GridCoord coord)
        {
            if (_features.TryGet(coord, out var featureId))
                _spawner.Spawn(coord, $"{_catalog.Get(featureId).DisplayName}: восстановились", FloatingTextStyle.Fertility);
        }

        private void ShowResolved(GridCoord prey, FeatureInteraction interaction, bool countered)
        {
            var definition = _catalog.GetInteraction(interaction);
            var text = countered
                ? $"{definition.Prey.DisplayName} {definition.CounterText}"
                : $"{definition.Prey.DisplayName} {definition.PreyLostText} (восстановится через {_catalog.RecoveryYears} лет)";
            _spawner.Spawn(prey, text, countered ? FloatingTextStyle.Evolution : FloatingTextStyle.Threat);
        }
    }
}
