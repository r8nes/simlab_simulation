using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Core.Life.People;
using Godland.Core.World.Tiles;
using Godland.Core.World.Volcanoes;

namespace Godland.Core.Fate.Cataclysms
{
    public sealed class IdolCataclysms
    {
        private readonly WorldGrid _grid;
        private readonly WorldTiles _tiles;
        private readonly Settlements _settlements;
        private readonly Shrines _shrines;
        private readonly VolcanoActivity _volcanoes;
        private readonly ICataclysmRules _rules;
        private readonly TileGuards _guards;
        private Summoning _summoning;

        public IdolCataclysms(
            WorldGrid grid, WorldTiles tiles, Settlements settlements, Shrines shrines, VolcanoActivity volcanoes, ICataclysmRules rules,
            TileGuards guards)
        {
            _grid = grid;
            _tiles = tiles;
            _settlements = settlements;
            _shrines = shrines;
            _volcanoes = volcanoes;
            _rules = rules;
            _guards = guards;
        }

        public event Action<Summoning> Announced;
        public event Action<Summoning> Averted;
        public event Action<Summoning, IReadOnlyList<GridCoord>> Struck;

        public Summoning Current => _summoning;

        public int SharePercent(Religion religion) =>
            _shrines.BelieverCount == 0 ? 0 : _shrines.FollowersOf(religion).Count * 100 / _shrines.BelieverCount;

        public void Update()
        {
            if (_summoning == null)
            {
                TryAnnounce();
                return;
            }

            if (!CanSummon(_summoning.Religion, _rules.SummonFervor / 2))
            {
                var averted = _summoning;
                _summoning = null;
                Averted?.Invoke(averted);
                return;
            }

            if (!_summoning.Tick()) return;

            var struck = _summoning;
            _summoning = null;
            var cells = Strike(struck.Religion);
            struck.Religion.Cool(struck.Religion.Fervor);
            Struck?.Invoke(struck, cells);
        }

        private void TryAnnounce()
        {
            var religion = _shrines.Religions.FirstOrDefault(candidate => CanSummon(candidate, _rules.SummonFervor));
            if (religion == null) return;

            _summoning = new Summoning(religion, _rules.WarningYears);
            Announced?.Invoke(_summoning);
        }

        private bool CanSummon(Religion religion, int minFervor) =>
            religion.IdolId != null
            && _rules.CataclysmOf(religion.IdolId).Kind != CataclysmKind.None
            && _shrines.BelieverCount >= _rules.MinWorldBelievers
            && SharePercent(religion) >= _rules.DominancePercent
            && religion.Fervor >= minFervor;

        private IReadOnlyList<GridCoord> Strike(Religion religion)
        {
            var cataclysm = _rules.CataclysmOf(religion.IdolId);
            if (cataclysm.Kind == CataclysmKind.AwakenVolcanoes)
            {
                _volcanoes.AwakenAll();
                return Array.Empty<GridCoord>();
            }

            var area = cataclysm.Kind == CataclysmKind.TransformWorld || religion.Shrine is not { } shrine
                ? _tiles.All.Keys.ToList()
                : _tiles.All.Keys.Where(cell => Math.Max(Math.Abs(cell.X - shrine.X), Math.Abs(cell.Y - shrine.Y)) <= cataclysm.Radius).ToList();

            var changed = new List<GridCoord>();
            foreach (var cell in area)
            {
                if (cell == religion.Shrine || !_tiles.TryGet(cell, out var tileId) || tileId == cataclysm.ResultTileId || _rules.IsImmune(tileId) || _guards.IsProtected(cell)) continue;
                if (cataclysm.AffectedTileIds.Count > 0 && !cataclysm.AffectedTileIds.Contains(tileId)) continue;

                _tiles.Place(cell, cataclysm.ResultTileId, "катаклизм идола");
                _settlements.Weaken(cell, _rules.PopulationLoss);
                changed.Add(cell);
            }

            return changed;
        }
    }
}
