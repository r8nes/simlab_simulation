using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Godland.Game.World.Rules
{
    [CreateAssetMenu(menuName = "Godland/World Rules Config", fileName = "WorldRulesConfig")]
    public sealed class WorldRulesConfig : ScriptableObject
    {
        [field: SerializeField, BoxGroup("Fertility"), MinValue(1)]
        public int MaxFertility { get; private set; } = 5;

        [field: SerializeField, BoxGroup("Fertility"), MinValue(1), SuffixLabel("years")]
        public int FertilityGrowthPeriod { get; private set; } = 10;

        [field: SerializeField, FormerlySerializedAs("<StartingEnergy>k__BackingField"), BoxGroup("Creation"), MinValue(0)]
        public int StartingCreationPoints { get; private set; } = 6;

        [field: SerializeField, BoxGroup("Creation"), MinValue(0)]
        public int MaxFertilityReward { get; private set; } = 4;

        [field: SerializeField, BoxGroup("Cells"), MinValue(0), InfoBox("Цена первой купленной клетки на краю мира.")]
        public int CellBaseCost { get; private set; } = 50;

        [field: SerializeField, BoxGroup("Cells"), MinValue(0), InfoBox("На сколько дорожает каждая следующая клетка.")]
        public int CellCostStep { get; private set; } = 3;
    }
}
