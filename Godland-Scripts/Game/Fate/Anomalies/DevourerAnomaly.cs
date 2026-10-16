using System;
using System.Linq;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Anomalies
{
    [Serializable]
    public sealed class DevourerAnomaly : IAnomalyEffect
    {
        [SerializeField, Required, AssetsOnly] private TileDefinition _abyss;
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _eatPeriod = 10;
        [SerializeField, MinValue(1), InfoBox("Сколько раз нужно накормить Пожирателя верой, чтобы он закрылся.")] private int _feedings = 3;

        public bool CanBegin(AnomalyContext context) => !context.Devourer.IsActive && Sites(context).Any();

        public void Begin(AnomalyContext context, int year)
        {
            var sites = Sites(context).ToList();
            context.Devourer.Awaken(sites[context.Random.Next(sites.Count)], _abyss.Id, _eatPeriod, _feedings);
        }

        private static System.Collections.Generic.IEnumerable<Godland.Core.World.Grid.GridCoord> Sites(AnomalyContext context) =>
            context.Tiles.All.Keys.Where(cell => cell != context.Grid.Center && !context.Guards.IsProtected(cell)
                && Math.Abs(cell.X - context.Grid.Center.X) + Math.Abs(cell.Y - context.Grid.Center.Y) <= 2);
    }
}
