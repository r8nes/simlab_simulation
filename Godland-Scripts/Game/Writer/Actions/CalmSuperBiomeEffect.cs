using System;
using Godland.Core.World.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Writer.Actions
{
    [Serializable]
    public sealed class CalmSuperBiomeEffect : ITileEffect
    {
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _years = 100;

        public string Describe() => $"супербиом перестаёт расти на {_years} лет";

        public bool CanAffect(GridCoord target, TileEffectContext context) => context.SuperBiomes.CanCalm;

        public void Apply(GridCoord target, TileEffectContext context) => context.SuperBiomes.Calm(_years);
    }
}
