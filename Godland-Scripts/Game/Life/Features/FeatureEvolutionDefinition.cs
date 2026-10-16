using System;
using Godland.Core.Life.Features;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Life.Features
{
    [Serializable]
    public sealed class FeatureEvolutionDefinition
    {
        [SerializeField, Required, AssetsOnly] private FeatureDefinition _result;
        [SerializeField, AssetsOnly, InfoBox("Нужен рядом такой тайл. Пусто — не важно.")] private TileDefinition _neighborTile;
        [SerializeField, AssetsOnly, InfoBox("Нужен рядом такой обитатель. Пусто — не важно.")] private FeatureDefinition _neighborFeature;
        [SerializeField, Range(1, 100), SuffixLabel("%"), InfoBox("Скрытый шанс одного броска. Кубик бросается раз в период эволюции.")] private int _chance = 20;

        public FeatureEvolutionRule ToCore() =>
            new(_result.Id, _neighborTile != null ? _neighborTile.Id : null, _neighborFeature != null ? _neighborFeature.Id : null, _chance / 100f);
    }
}
