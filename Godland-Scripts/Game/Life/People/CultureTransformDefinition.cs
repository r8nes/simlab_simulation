using System;
using Godland.Core.Life.People;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Life.People
{
    [Serializable]
    public sealed class CultureTransformDefinition
    {
        [SerializeField, Required, AssetsOnly, HorizontalGroup, LabelWidth(30)] private TileDefinition _from;
        [SerializeField, Required, AssetsOnly, HorizontalGroup, LabelWidth(20)] private TileDefinition _to;
        [SerializeField, MinValue(1), HorizontalGroup(110), LabelWidth(50), SuffixLabel("лет")] private int _period = 40;

        public TileDefinition From => _from;
        public TileDefinition To => _to;
        public int PeriodYears => _period;

        public CultureTransform ToCore() => new(_from.Id, _to.Id, _period);
    }
}
