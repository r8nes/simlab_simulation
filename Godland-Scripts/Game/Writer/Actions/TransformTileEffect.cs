using System;
using Godland.Core.World.Grid;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Writer.Actions
{
    [Serializable]
    public sealed class TransformTileEffect : ITileEffect
    {
        [SerializeField, Required, AssetsOnly] private TileDefinition _result;

        public string Describe() => $"превращает в «{_result.DisplayName}»";

        public bool CanAffect(GridCoord target, TileEffectContext context) => true;

        public void Apply(GridCoord target, TileEffectContext context) => context.Tiles.Place(target, _result.Id, "чудо");
    }
}
