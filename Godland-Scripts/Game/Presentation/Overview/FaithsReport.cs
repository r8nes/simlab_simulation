using System.Collections.Generic;
using System.Linq;
using Godland.Core.Fate.Cataclysms;
using Godland.Core.Life.People;
using Godland.Game.Life.People;
using Godland.Core.World.Grid;
using Godland.Game.Writer.Commands;

namespace Godland.Game.Presentation.Overview
{
    public sealed class FaithsReport : IWorldReport
    {
        private readonly Shrines _shrines;
        private readonly Settlements _settlements;
        private readonly IdolCataclysms _cataclysms;
        private readonly SettlementConfig _config;
        private readonly FaithNames _names;

        private readonly EntityCommands _commands;
        private readonly Feuds _feuds;

        public FaithsReport(Shrines shrines, Settlements settlements, IdolCataclysms cataclysms, SettlementConfig config, FaithNames names, EntityCommands commands, Feuds feuds)
        {
            _commands = commands;
            _feuds = feuds;
            _shrines = shrines;
            _settlements = settlements;
            _cataclysms = cataclysms;
            _config = config;
            _names = names;
        }

        public string Title => "Веры";

        public IEnumerable<WorldReportLine> Lines()
        {
            var faithless = _settlements.All.Count - _shrines.BelieverCount;
            yield return new WorldReportLine(
                $"Верующих поселений: {_shrines.BelieverCount} из {_settlements.All.Count}",
                faithless > 0 ? $"Без веры: {faithless}. Вера приходит от святынь рядом, от соседей и переселенцев." : "Весь мир во что-то верит.");

            if (_cataclysms.Current is { } summoning)
                yield return new WorldReportLine("Идёт призыв катаклизма", $"Осталось {summoning.YearsLeft} лет. Сорвётся, если доля веры или пыл упадут.");

            foreach (var religion in _shrines.Religions.OrderByDescending(religion => _shrines.FollowersOf(religion).Count))
            {
                var followers = _shrines.FollowersOf(religion);
                var parts = new List<string>
                {
                    $"Поселений: {followers.Count} ({_cataclysms.SharePercent(religion)}% верующих мира)",
                    $"Догмат: {_names.DogmaOf(religion)}",
                    religion.IdolId == null && !religion.IsGod ? $"Пыл: {religion.Fervor} из {_config.IdolRevealFervor} — потом откроется, какому идолу они молятся" : $"Пыл: {religion.Fervor}",
                    religion.Hunts > 0 || religion.Losses > 0 ? $"Помнит: охот {religion.Hunts}, потерь {religion.Losses}" : null
                };
                var place = religion.Shrine ?? followers.FirstOrDefault();
                var key = $"faith:{_names.Of(religion)}";
                yield return new WorldReportLine(_names.Of(religion), string.Join("\n", parts.Where(part => part != null)), place, new[]
                {
                    _commands.Make("faith-kindle", key, () =>
                    {
                        religion.Kindle(_commands.Config.FervorStep);
                        return place;
                    }),
                    _commands.Make("faith-cool", key, () =>
                    {
                        religion.Cool(_commands.Config.FervorStep);
                        return place;
                    }),
                    followers.Any(_feuds.IsFeuding) ? _commands.Make("feud-calm", key, () =>
                    {
                        _feuds.Calm(followers);
                        return place;
                    }) : null
                });
            }
        }
    }
}
