using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Timing;
using Godland.Core.World.Volcanoes;
using Godland.Game.Fate.Anomalies;
using Godland.Game.Fate.Catastrophes;
using Godland.Core.World.Grid;
using Godland.Game.Writer.Commands;

namespace Godland.Game.Presentation.Overview
{
    public sealed class ThreatsReport : IWorldReport
    {
        private const string CorruptedId = "corrupted";
        private const string SourceId = "source";

        private readonly CatastropheKeeper _catastrophes;
        private readonly CatastropheConfig _config;
        private readonly CatastropheContext _context;
        private readonly AnomalyKeeper _anomalies;
        private readonly VolcanoActivity _volcanoes;
        private readonly WorldClock _clock;

        private readonly EntityCommands _commands;

        public ThreatsReport(
            CatastropheKeeper catastrophes, CatastropheConfig config, CatastropheContext context, AnomalyKeeper anomalies,
            VolcanoActivity volcanoes, WorldClock clock, EntityCommands commands)
        {
            _commands = commands;
            _catastrophes = catastrophes;
            _config = config;
            _context = context;
            _anomalies = anomalies;
            _volcanoes = volcanoes;
            _clock = clock;
        }

        public string Title => "Угрозы";

        public IEnumerable<WorldReportLine> Lines()
        {
            var world = _context.World;
            if (_catastrophes.Coming is { } coming)
                yield return new WorldReportLine($"Грядёт: {coming.Title}", $"{coming.Omen}\nУдар в {_catastrophes.StrikeYear} году ({_catastrophes.StrikeYear - _clock.Year} лет).", null, new[]
                {
                    _catastrophes.WasDelayed ? null : _commands.Make("catastrophe-delay", "catastrophe",
                        () => _catastrophes.Delay(_commands.Config.CatastropheDelayYears) ? world.Grid.Center : null)
                });

            foreach (var catastrophe in _config.Catastrophes.Where(catastrophe => catastrophe != _catastrophes.Coming && catastrophe.Trigger.IsReady(_context)))
                yield return new WorldReportLine(
                    $"Назревает: {catastrophe.Title}",
                    $"Условие выполнено. Шанс {catastrophe.ChancePercent}% каждые {_config.CheckPeriod} лет" +
                    (catastrophe.MinEpoch > world.Epoch.Index ? $", но не раньше эпохи {catastrophe.MinEpoch}." : "."));

            if (_anomalies.Coming is { } anomaly)
                yield return new WorldReportLine($"Знамение: {anomaly.Omen}", $"Явление «{anomaly.Title}» придёт в {_anomalies.ComingYear} году.");

            if (_context.Devourer.IsActive)
                yield return new WorldReportLine("Пожиратель ест мир", $"Накормить верой ещё {_context.Devourer.FeedingsLeft} раз. Съел {_context.PeakTiles - world.Tiles.CountedCount} из {_context.PeakTiles} земель.", _context.Devourer.Maw);

            var sick = world.Settlements.All.Keys.Where(world.Plague.IsSick).ToList();
            if (sick.Count > 0)
                yield return new WorldReportLine($"Мор: болеют {sick.Count} поселений", "Мор переходит к соседям, у которых тоже есть причины болеть (теснота, голод, болота).", sick[0]);

            var corrupted = world.Tiles.All.Where(pair => pair.Value == CorruptedId).Select(pair => pair.Key).ToList();
            var sources = world.Tiles.All.Where(pair => pair.Value == SourceId).Select(pair => pair.Key).ToList();
            if (corrupted.Count > 0 || sources.Count > 0)
                yield return new WorldReportLine(
                    $"Порча: {corrupted.Count} земель" + (sources.Count > 0 ? $", Источников: {sources.Count}" : string.Empty),
                    sources.Count > 0 ? "Источник распространяет порчу. Без Источника она отступает." : "Источника нет: порча отступает.",
                    sources.Count > 0 ? sources[0] : corrupted[0]);

            foreach (var (volcano, years) in _volcanoes.Awakenings)
                yield return new WorldReportLine("Вулкан просыпается", $"Извержение через {years} лет: соседние земли сгорят в пепел.", volcano);
        }
    }
}
