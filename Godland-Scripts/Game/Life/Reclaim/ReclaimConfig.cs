using System;
using System.Collections.Generic;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Life.Reclaim
{
    [CreateAssetMenu(menuName = "Godland/Reclaim Config", fileName = "ReclaimConfig")]
    public sealed class ReclaimConfig : ScriptableObject
    {
        [field: SerializeField, MinValue(1), SuffixLabel("years"), InfoBox("Брошенная людьми земля не пустует: её занимают звери и природа, потом возвращаются люди.")]
        public int Period { get; private set; } = 10;

        [field: SerializeField, Range(0, 100), SuffixLabel("%"), InfoBox("Шанс за период, что на брошенной клетке поселится дикий обитатель.")]
        public int WildChancePercent { get; private set; } = 30;

        [field: SerializeField, Range(0, 100), SuffixLabel("%"), InfoBox("Шанс, что вместо зверя заведётся чудовище (на руинах и пустошах).")]
        public int MonsterChancePercent { get; private set; } = 15;

        [field: SerializeField, AssetsOnly] public List<TileDefinition> HauntedTiles { get; private set; } = new();

        [field: SerializeField, Range(0, 100), SuffixLabel("%"), InfoBox("Когда земля отдохнула, шанс, что сюда вернутся люди (соседний народ заберёт её себе).")]
        public int ReturnChancePercent { get; private set; } = 60;

        [field: SerializeField, MinValue(1)] public int ReturnPopulation { get; private set; } = 2;

        [SerializeField, InfoBox("Как брошенная земля зарастает со временем.")] private List<ReclaimStep> _steps = new();

        public IReadOnlyList<ReclaimStep> Steps => _steps;
    }

    [Serializable]
    public sealed class ReclaimStep
    {
        [field: SerializeField, Required, AssetsOnly] public TileDefinition From { get; private set; }
        [field: SerializeField, Required, AssetsOnly] public TileDefinition To { get; private set; }
        [field: SerializeField, MinValue(0), SuffixLabel("years"), InfoBox("Сколько лет земля должна пустовать.")] public int AfterYears { get; private set; } = 50;
    }
}
