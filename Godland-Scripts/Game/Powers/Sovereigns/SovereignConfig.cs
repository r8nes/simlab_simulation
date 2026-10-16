using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Sovereigns
{
    [CreateAssetMenu(menuName = "Godland/Sovereign Config", fileName = "SovereignConfig")]
    public sealed class SovereignConfig : ScriptableObject
    {
        [field: SerializeField, MinValue(1), SuffixLabel("years"), InfoBox("Как часто проверяются восхождение и свержение.")]
        public int CheckPeriod { get; private set; } = 10;

        [field: SerializeField, MinValue(0), SuffixLabel("years"), InfoBox("Минимальное правление до возможного свержения.")]
        public int MinReignYears { get; private set; } = 50;

        [field: SerializeField, MinValue(1), SuffixLabel("years"), InfoBox("Раз в период правитель выбирает деяние.")]
        public int DeedPeriod { get; private set; } = 20;

        [SerializeField, AssetsOnly, InfoBox("Силы, которые могут захватить мир, если сложатся условия. Одновременно правит только одна.")]
        private List<SovereignDefinition> _sovereigns = new();

        public IReadOnlyList<SovereignDefinition> Sovereigns => _sovereigns;
    }
}
