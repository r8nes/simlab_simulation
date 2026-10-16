using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Writer.Gifts
{
    [Serializable]
    public sealed class SecondSunGift : IGiftEffect
    {
        [SerializeField, AssetsOnly] private List<TileDefinition> _melting = new();
        [SerializeField, Required, AssetsOnly] private TileDefinition _melted;
        [SerializeField, AssetsOnly] private List<TileDefinition> _drying = new();
        [SerializeField, Required, AssetsOnly] private TileDefinition _desert;
        [SerializeField] private int _fertility = -3;

        public bool IsRelevant(GiftContext context) => context.Any(_melting.Concat(_drying).Select(tile => tile.Id).ToList());

        public string Describe() => "Снега тают, мелкая вода пересыхает в пустыни, земли выжжены";

        public void Apply(GiftContext context)
        {
            foreach (var cell in context.TilesOf(_melting.Select(tile => tile.Id).ToList())) context.Transform(cell, _melted.Id, 0);
            foreach (var cell in context.TilesOf(_drying.Select(tile => tile.Id).ToList())) context.Transform(cell, _desert.Id, 1);
            foreach (var cell in context.Tiles.All.Keys.ToList()) context.Fertility.Add(cell, _fertility);
        }
    }
}
