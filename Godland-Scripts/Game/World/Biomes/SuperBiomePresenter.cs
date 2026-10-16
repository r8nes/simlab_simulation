using System;
using Godland.Core.World.Biomes;
using Godland.Core.World.Grid;
using Godland.Game.Fate.Epochs;
using Godland.Game.Presentation.Journal;
using VContainer.Unity;

namespace Godland.Game.World.Biomes
{
    public sealed class SuperBiomePresenter : IStartable, IDisposable
    {
        private const float BannerSeconds = 4f;

        private readonly SuperBiomes _biomes;
        private readonly SuperBiomeConfig _config;
        private readonly EventJournal _journal;
        private readonly EpochView _banner;

        public SuperBiomePresenter(SuperBiomes biomes, SuperBiomeConfig config, EventJournal journal, EpochView banner)
        {
            _biomes = biomes;
            _config = config;
            _journal = journal;
            _banner = banner;
        }

        public void Start()
        {
            _biomes.Entered += OnEntered;
            _biomes.Left += OnLeft;
            _biomes.Transformed += OnTransformed;
        }

        public void Dispose()
        {
            _biomes.Entered -= OnEntered;
            _biomes.Left -= OnLeft;
            _biomes.Transformed -= OnTransformed;
        }

        private void OnEntered(SuperBiomeRule biome)
        {
            var definition = _config.DefinitionOf(biome);
            _banner.Announce("Мир стал супербиомом", definition.DisplayName, BannerSeconds);
            _journal.Write($"Мир стал супербиомом «{definition.DisplayName}»: {definition.Description}", JournalTone.Good);
        }

        private void OnLeft(SuperBiomeRule biome) =>
            _journal.Write($"Супербиом «{_config.DefinitionOf(biome).DisplayName}» распался", JournalTone.Info);

        private void OnTransformed(SuperBiomeRule biome, GridCoord _) =>
            _journal.Write($"{_config.DefinitionOf(biome).DisplayName} поглощает ещё одну землю", JournalTone.Info);
    }
}
