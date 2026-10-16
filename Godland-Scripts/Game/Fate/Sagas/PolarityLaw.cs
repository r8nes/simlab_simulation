using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Game.Fate.Catastrophes;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Sagas
{
    [Serializable]
    public sealed class PolarityPair
    {
        [field: SerializeField, Required, AssetsOnly] public TileDefinition From { get; private set; }
        [field: SerializeField, Required, AssetsOnly] public TileDefinition To { get; private set; }
    }

    [Serializable]
    public sealed class PolarityLaw : IAgeLaw
    {
        [SerializeField, InfoBox("Словарь полярности: всё, что есть и появляется, переводится на язык нового мира.")] private List<PolarityPair> _pairs = new();
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _period = 10;
        [SerializeField, MinValue(1), InfoBox("Сколько старых тайлов переводится за период.")] private int _cells = 3;
        [SerializeField, InfoBox("Каким узором наступает новый мир.")] private Godland.Core.World.Patterns.PatternStyle _pattern = Godland.Core.World.Patterns.PatternStyle.Spots;
        [SerializeField, InfoBox("Всё новое сразу переводится (поставленное, эволюционировавшее).")] private bool _translateNew = true;

        [NonSerialized] private CatastropheContext _context;
        [NonSerialized] private string _cause;
        [NonSerialized] private bool _translating;

        public void Begin(CatastropheContext context, string cause)
        {
            _context = context;
            _cause = cause;
            if (_translateNew) context.World.Tiles.Changed += OnChanged;
        }

        public void Year(CatastropheContext context, int age, string cause)
        {
            if (age % _period != 0) return;

            var world = context.World;
            var cells = world.Tiles.All
                .Where(pair => context.IsShaped(pair.Key) && !world.Guards.IsProtected(pair.Key) && Translate(pair.Value) != null)
                .Select(pair => pair.Key);
            var seeds = world.Tiles.All.Where(pair => _pairs.Any(pair2 => pair2.To.Id == pair.Value)).Select(pair => pair.Key);
            foreach (var cell in context.Patterns.Rank("polarity", _pattern, seeds, cells).Take(_cells).ToList())
                if (world.Tiles.TryGet(cell, out var id)) Place(cell, Translate(id));
        }

        public void End(CatastropheContext context)
        {
            context.World.Tiles.Changed -= OnChanged;
            _context = null;
        }

        private string Translate(string tileId) => _pairs.FirstOrDefault(pair => pair.From.Id == tileId)?.To.Id;

        private void OnChanged(GridCoord cell, string tileId, string cause)
        {
            if (_translating || tileId == null || _context == null || cell == _context.World.Grid.Center) return;
            if (Translate(tileId) is { } translated) Place(cell, translated);
        }

        private void Place(GridCoord cell, string tileId)
        {
            _translating = true;
            _context.World.Tiles.Place(cell, tileId, _cause);
            _translating = false;
        }
    }
}
