using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Catastrophes
{
    [Serializable]
    public sealed class TransformShareStrike : ICatastropheStrike
    {
        [SerializeField, AssetsOnly, InfoBox("Какие тайлы задевает (пусто — любые).")] private List<TileDefinition> _from = new();
        [SerializeField, Required, AssetsOnly] private TileDefinition _to;
        [SerializeField, Range(1, 100), SuffixLabel("%")] private int _percent = 50;
        [SerializeField, MinValue(0)] private int _settlementLoss = 2;

        public void Strike(CatastropheContext context, string cause)
        {
            var world = context.World;
            var cells = context.Shuffled(world.Tiles.All
                .Where(pair => context.IsShaped(pair.Key) && pair.Value != _to.Id && (_from.Count == 0 || _from.Any(tile => tile.Id == pair.Value)))
                .Select(pair => pair.Key));
            foreach (var cell in cells.Take((cells.Length * _percent + 99) / 100))
            {
                if (world.Features.TryGet(cell, out _)) world.Features.Remove(cell);
                world.Tiles.Place(cell, _to.Id, cause);
                if (_settlementLoss > 0 && world.Settlements.TryGet(cell, out _)) world.Settlements.Weaken(cell, _settlementLoss);
            }
        }
    }

    [Serializable]
    public sealed class DecimateStrike : ICatastropheStrike
    {
        [SerializeField, Range(1, 100), SuffixLabel("%"), InfoBox("Сколько жителей гибнет в каждом поселении.")] private int _lossPercent = 60;
        [SerializeField, AssetsOnly, InfoBox("Во что превращаются вымершие поселения (пусто — не меняются).")] private TileDefinition _ruin;

        public void Strike(CatastropheContext context, string cause)
        {
            var world = context.World;
            foreach (var (cell, population) in world.Settlements.All.ToList())
            {
                world.Settlements.Weaken(cell, Math.Max(1, population * _lossPercent / 100));
                if (_ruin != null && !world.Settlements.TryGet(cell, out _) && context.IsShaped(cell)) world.Tiles.Place(cell, _ruin.Id, cause);
            }
        }
    }

    [Serializable]
    public sealed class RaceConquestStrike : ICatastropheStrike
    {
        public void Strike(CatastropheContext context, string cause) => context.Races.Conquer(cause);
    }

    [Serializable]
    public sealed class PlagueStrike : ICatastropheStrike
    {
        [SerializeField, Range(1, 100), SuffixLabel("%")] private int _percent = 50;

        public void Strike(CatastropheContext context, string cause)
        {
            var settlements = context.Shuffled(context.World.Settlements.All.Keys);
            foreach (var cell in settlements.Take((settlements.Length * _percent + 99) / 100)) context.World.Plague.Strike(cell);
        }
    }
}
