using System;
using Godland.Core.World.Grid;
using Godland.Game.Writer.Actions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Sagas
{
    public static class SagaSide
    {
        public static string Describe(SagaKeeper sagas, GridCoord target, int power)
        {
            if (sagas.OwnerOf(target) is not { } faction) return "здесь нет земель саги";

            var sign = power > 0 ? "+" : string.Empty;
            return $"«{faction.Definition.DisplayName}» {sign}{power} силы (сейчас {faction.Power} из {sagas.ThresholdOf(faction)}); {sagas.GoalText(faction)}";
        }
    }

    [Serializable]
    public sealed class EmpowerSideEffect : ITileEffect, ITargetedDescription
    {
        [SerializeField, MinValue(1), InfoBox("Писатель толкает сторону, которой принадлежит эта земля: её сила растёт.")] private int _power = 15;

        public string Describe() => $"сторона этой земли сильнее на {_power}";

        public bool CanAffect(GridCoord target, TileEffectContext context) => context.Sagas.OwnerOf(target) != null;

        public string DescribeAt(GridCoord target, TileEffectContext context) => SagaSide.Describe(context.Sagas, target, _power);

        public void Apply(GridCoord target, TileEffectContext context) => context.Sagas.Empower(context.Sagas.OwnerOf(target), _power);
    }

    [Serializable]
    public sealed class WeakenSideEffect : ITileEffect, ITargetedDescription
    {
        [SerializeField, MinValue(1)] private int _power = 15;

        public string Describe() => $"сторона этой земли слабее на {_power}";

        public string DescribeAt(GridCoord target, TileEffectContext context) => SagaSide.Describe(context.Sagas, target, -_power);

        public bool CanAffect(GridCoord target, TileEffectContext context) => context.Sagas.OwnerOf(target) != null;

        public void Apply(GridCoord target, TileEffectContext context) => context.Sagas.Empower(context.Sagas.OwnerOf(target), -_power);
    }

    [Serializable]
    public sealed class OpenBridgeEffect : ITileEffect
    {
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _years = 20;

        public string Describe() => $"открыть мост между мирами на {_years} лет";

        public bool CanAffect(GridCoord target, TileEffectContext context) => context.Sagas.CanOpenBridge && context.Sagas.OwnerOf(target) != null;

        public void Apply(GridCoord target, TileEffectContext context) => context.Sagas.OpenBridge(context.Sagas.World.World.Clock.Year, _years);
    }

    [Serializable]
    public sealed class PlantArtifactEffect : ITileEffect
    {
        public string Describe() => "подбросить артефакт стороне этой земли";

        public bool CanAffect(GridCoord target, TileEffectContext context) =>
            context.Sagas.Act is { ArtifactHunt: true } && context.Sagas.OwnerOf(target) is { HasArtifact: false };

        public void Apply(GridCoord target, TileEffectContext context) => context.Sagas.GiveArtifact(context.Sagas.OwnerOf(target));
    }
}
