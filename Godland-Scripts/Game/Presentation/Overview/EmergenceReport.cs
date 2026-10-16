using System.Collections.Generic;
using System.Linq;
using Godland.Game.Fate.Emergence;

namespace Godland.Game.Presentation.Overview
{
    public sealed class EmergenceReport : IWorldReport
    {
        private const int ShowFromPercent = 40;

        private readonly EmergenceConfig _config;
        private readonly EmergenceKeeper _keeper;
        private readonly EmergenceContext _context;

        public EmergenceReport(EmergenceConfig config, EmergenceKeeper keeper, EmergenceContext context)
        {
            _config = config;
            _keeper = keeper;
            _context = context;
        }

        public string Title => "Возникновения";

        public IEnumerable<WorldReportLine> Lines()
        {
            var near = _config.Emergences
                .Where(emergence => _keeper.TimesOf(emergence) < emergence.MaxPerRun && emergence.Condition is IProgressCondition)
                .Select(emergence => (Emergence: emergence, Progress: ((IProgressCondition)emergence.Condition).Progress(_context)))
                .Where(entry => entry.Progress.Value * 100 >= entry.Progress.Threshold * ShowFromPercent)
                .OrderByDescending(entry => entry.Progress.Value * 100 / entry.Progress.Threshold)
                .ToList();

            if (near.Count == 0)
                yield return new WorldReportLine("Ничего не назревает", "Мир копит память: охоты, войны, извержения, чудеса. Когда память дойдёт до порога, родится новое.");

            foreach (var (emergence, (value, threshold)) in near)
            {
                var percent = System.Math.Min(100, value * 100 / threshold);
                var epoch = _context.Epoch.Index < emergence.MinEpoch ? $" Не раньше эпохи {emergence.MinEpoch}." : string.Empty;
                yield return new WorldReportLine(
                    $"{emergence.Title}: {percent}%",
                    $"Память {value} из {threshold}. Шанс {emergence.ChancePercent}% за проверку, когда наберётся.{epoch}");
            }

            var happened = _config.Emergences.Where(emergence => _keeper.TimesOf(emergence) > 0).Select(emergence => emergence.Title).ToList();
            if (happened.Count > 0) yield return new WorldReportLine("Уже возникло", string.Join(", ", happened));
        }
    }
}
