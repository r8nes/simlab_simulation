using System;
using Godland.Game.Life.People;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Anomalies
{
    [Serializable]
    public sealed class SkyCityAnomaly : IAnomalyEffect
    {
        [SerializeField, Required, AssetsOnly] private TileDefinition _city;
        [SerializeField, Required, AssetsOnly] private DogmaDefinition _dogma;
        [SerializeField, MinValue(1)] private int _population = 10;
        [SerializeField, MinValue(1), SuffixLabel("years"), InfoBox("Раз в период небесный народ обращает соседнее поселение.")] private int _period = 20;

        public bool CanBegin(AnomalyContext context) => !context.SkyFolk.HasLanded;

        public void Begin(AnomalyContext context, int year) => context.SkyFolk.TryLand(_city.Id, _dogma.ToCore(), _population, _period);
    }
}
