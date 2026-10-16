using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.WorldEvents
{
    [CreateAssetMenu(menuName = "Godland/World Events Config", fileName = "WorldEventsConfig")]
    public sealed class WorldEventsConfig : ScriptableObject
    {
        [field: SerializeField, MinValue(1), SuffixLabel("years"), InfoBox("Как часто мир бросает кубик события.")]
        public int Period { get; private set; } = 30;

        [field: SerializeField, Range(0, 100), SuffixLabel("%")]
        public int ChancePercent { get; private set; } = 70;

        [SerializeField, AssetsOnly]
        private List<WorldEventDefinition> _events = new();

        public IReadOnlyList<WorldEventDefinition> Events => _events;
    }
}
