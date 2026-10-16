using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Emergence
{
    [CreateAssetMenu(menuName = "Godland/Emergence Config", fileName = "EmergenceConfig")]
    public sealed class EmergenceConfig : ScriptableObject
    {
        [field: SerializeField, MinValue(1), SuffixLabel("years"), InfoBox("Мир помнит, что на нём происходило; раз в период проверяет, не родилось ли из этой памяти что-то новое.")]
        public int CheckPeriod { get; private set; } = 25;

        [SerializeField, AssetsOnly]
        private List<EmergenceDefinition> _emergences = new();

        public IReadOnlyList<EmergenceDefinition> Emergences => _emergences;
    }
}
