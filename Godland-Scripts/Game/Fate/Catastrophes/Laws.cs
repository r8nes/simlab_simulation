using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Catastrophes
{
    [Serializable]
    public sealed class RiseLandLaw : IAgeLaw
    {
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _period = 10;
        [SerializeField, MinValue(1)] private int _cells = 2;
        [SerializeField, Required, AssetsOnly, InfoBox("Какая земля поднимается (случайно из списка).")] private List<TileDefinition> _tiles = new();
        [SerializeField, Range(0, 100), SuffixLabel("%"), InfoBox("Шанс, что на новой земле сразу селятся выжившие.")] private int _settleChancePercent = 30;
        [SerializeField, MinValue(1)] private int _population = 2;
        [SerializeField, InfoBox("Поднимать землю только до прежнего размера мира.")] private bool _capAtPeak = true;

        public void Begin(CatastropheContext context, string cause) { }

        public void Year(CatastropheContext context, int age, string cause)
        {
            if (age % _period != 0 || _tiles.Count == 0) return;

            var world = context.World;
            var room = _capAtPeak ? Math.Max(0, context.PeakTiles - world.Tiles.CountedCount) : _cells;
            foreach (var cell in Candidates(context).Take(Math.Min(_cells, room)))
            {
                if (!world.Grid.IsUnlocked(cell)) world.Grid.Unlock(cell);
                world.Tiles.Place(cell, _tiles[world.Random.Next(_tiles.Count)].Id, cause);
                if (world.Random.Next(100) < _settleChancePercent) world.Settlements.TrySettle(cell, _population);
            }
        }

        public void End(CatastropheContext context) { }

        private IEnumerable<GridCoord> Candidates(CatastropheContext context)
        {
            var world = context.World;
            var voids = world.Grid.Coords.Where(cell => world.Grid.IsUnlocked(cell) && !world.Tiles.All.ContainsKey(cell));
            var shores = world.Grid.Coords.Where(cell => !world.Grid.IsUnlocked(cell)
                && world.Grid.Neighbors(cell).Any(world.Tiles.All.ContainsKey));
            return context.Shuffled(voids).Concat(context.Shuffled(shores));
        }
    }

    [Serializable]
    public sealed class TransformOverTimeLaw : IAgeLaw
    {
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _period = 10;
        [SerializeField, MinValue(1)] private int _cells = 2;
        [SerializeField, Required, AssetsOnly] private List<TileDefinition> _from = new();
        [SerializeField, Required, AssetsOnly] private TileDefinition _to;

        public void Begin(CatastropheContext context, string cause) { }

        public void Year(CatastropheContext context, int age, string cause)
        {
            if (age % _period != 0) return;

            var world = context.World;
            var cells = world.Tiles.All
                .Where(pair => context.IsShaped(pair.Key) && _from.Any(tile => tile.Id == pair.Value) && !world.Guards.IsProtected(pair.Key))
                .Select(pair => pair.Key);
            foreach (var cell in context.Shuffled(cells).Take(_cells)) world.Tiles.Place(cell, _to.Id, cause);
        }

        public void End(CatastropheContext context) { }
    }

    [Serializable]
    public sealed class FertilityLaw : IAgeLaw
    {
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _period = 10;
        [SerializeField, MinValue(1)] private int _amount = 2;

        public void Begin(CatastropheContext context, string cause) { }

        public void Year(CatastropheContext context, int age, string cause)
        {
            if (age % _period != 0) return;

            foreach (var cell in context.World.Tiles.All.Keys.Where(context.Fertility.CanHold).ToList())
                context.Fertility.Add(cell, _amount);
        }

        public void End(CatastropheContext context) { }
    }

    [Serializable]
    public sealed class EmergenceLaw : IAgeLaw
    {
        [SerializeField, MinValue(100), SuffixLabel("%")] private int _chancePercent = 200;

        public void Begin(CatastropheContext context, string cause) => context.World.Modifiers.SetEmergenceChance(_chancePercent);

        public void Year(CatastropheContext context, int age, string cause) { }

        public void End(CatastropheContext context) => context.World.Modifiers.SetEmergenceChance(100);
    }

    [Serializable]
    public sealed class GrowthLaw : IAgeLaw
    {
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _period = 20;
        [SerializeField, MinValue(1), InfoBox("Выжившие быстро плодятся: прибавка каждому поселению.")] private int _amount = 1;

        public void Begin(CatastropheContext context, string cause) { }

        public void Year(CatastropheContext context, int age, string cause)
        {
            if (age % _period != 0) return;

            var settlements = context.World.Settlements;
            foreach (var (cell, population) in settlements.All.ToList())
            {
                var room = settlements.CapacityOf(cell) - population;
                if (room > 0) settlements.Bless(cell, Math.Min(_amount, room));
            }
        }

        public void End(CatastropheContext context) { }
    }
}
