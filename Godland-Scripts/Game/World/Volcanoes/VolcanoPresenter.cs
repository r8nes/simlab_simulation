using System;
using System.Collections.Generic;
using Godland.Core.World.Grid;
using Godland.Core.World.Volcanoes;
using Godland.Game.Presentation.Feedback;
using VContainer.Unity;
using Godland.Game.Presentation.Journal;

namespace Godland.Game.World.Volcanoes
{
    public sealed class VolcanoPresenter : IStartable, IDisposable
    {
        private readonly VolcanoActivity _volcanoes;
        private readonly FloatingTextSpawner _spawner;
        private readonly EventJournal _journal;

        public VolcanoPresenter(VolcanoActivity volcanoes, FloatingTextSpawner spawner, EventJournal journal)
        {
            _journal = journal;
            _volcanoes = volcanoes;
            _spawner = spawner;
        }

        public void Start()
        {
            _volcanoes.Awakened += OnAwakened;
            _volcanoes.Erupted += OnErupted;
            _volcanoes.Melted += OnMelted;
        }

        public void Dispose()
        {
            _volcanoes.Awakened -= OnAwakened;
            _volcanoes.Erupted -= OnErupted;
            _volcanoes.Melted -= OnMelted;
        }

        private void OnAwakened(GridCoord volcano, int years)
        {
            _spawner.Spawn(volcano, $"Вулкан пробуждается: извержение через {years} лет", FloatingTextStyle.Threat);
            _journal.Write($"Вулкан пробуждается: извержение через {years} лет", JournalTone.Threat);
        }

        private void OnErupted(GridCoord volcano, IReadOnlyList<GridCoord> burned)
        {
            var text = burned.Count > 0 ? $"Извержение! Пепел на {burned.Count} тайлах" : "Извержение!";
            _spawner.Spawn(volcano, text, FloatingTextStyle.Threat);
            _journal.Write(text, JournalTone.Threat);
        }

        private void OnMelted(GridCoord volcano, GridCoord target) =>
            _spawner.Spawn(target, "Снег растаял", FloatingTextStyle.Fertility);
    }
}
