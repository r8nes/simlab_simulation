using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Game.World.Tiles;
using Godland.Game.Powers.Titans;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Catastrophes
{
    [Serializable]
    public sealed class SplitElementsStrike : ICatastropheStrike
    {
        [SerializeField, Required, AssetsOnly, InfoBox("Половина мира сгорает в это.")] private TileDefinition _burnt;
        [SerializeField, Required, AssetsOnly, InfoBox("Другая половина уходит под это.")] private TileDefinition _flooded;
        [SerializeField, MinValue(0)] private int _burnLoss = 3;
        [SerializeField, MinValue(0)] private int _floodLoss = 2;

        public void Strike(CatastropheContext context, string cause)
        {
            var world = context.World;
            var cells = context.Shuffled(world.Tiles.All.Keys.Where(context.IsShaped));
            for (var i = 0; i < cells.Length; i++)
            {
                var cell = cells[i];
                var burnt = i % 2 == 0;
                if (world.Features.TryGet(cell, out _)) world.Features.Remove(cell);
                world.Tiles.Place(cell, burnt ? _burnt.Id : _flooded.Id, cause);
                if (world.Settlements.TryGet(cell, out _)) world.Settlements.Weaken(cell, burnt ? _burnLoss : _floodLoss);
            }
        }
    }

    [Serializable]
    public sealed class SlayTitansStrike : ICatastropheStrike
    {
        [SerializeField, AssetsOnly] private List<TitanDefinition> _titans = new();

        public void Strike(CatastropheContext context, string cause)
        {
            foreach (var titan in context.World.Titans.All.Where(titan => _titans.Any(definition => definition.Id == titan.Rule.Id)).ToList())
                context.World.Titans.Slay(titan, cause);
        }
    }

    [Serializable]
    public sealed class DevourerDepartsStrike : ICatastropheStrike
    {
        [SerializeField, Required, AssetsOnly, InfoBox("Что остаётся там, где была пасть.")] private TileDefinition _scar;
        [SerializeField, MinValue(0)] private int _radius = 1;

        public void Strike(CatastropheContext context, string cause)
        {
            var maw = context.Devourer.Maw;
            context.Devourer.Depart(cause);
            foreach (var cell in context.World.Grid.Coords.Where(cell => context.World.Grid.IsUnlocked(cell)
                         && Math.Max(Math.Abs(cell.X - maw.X), Math.Abs(cell.Y - maw.Y)) <= _radius && cell != context.World.Grid.Center))
                context.World.Tiles.Place(cell, _scar.Id, cause);
        }
    }

    [Serializable]
    public sealed class SurvivorsStrike : ICatastropheStrike
    {
        [SerializeField, MinValue(1), InfoBox("Если людей не осталось, столько выживших селятся на уцелевшей земле.")] private int _groups = 2;
        [SerializeField, MinValue(1)] private int _population = 3;

        public void Strike(CatastropheContext context, string cause)
        {
            var world = context.World;
            if (world.Settlements.All.Count >= _groups) return;

            foreach (var cell in context.Shuffled(world.Tiles.All.Keys.Where(context.IsShaped)).Take(_groups))
                world.Settlements.TrySettle(cell, _population);
        }
    }
}
