using System.Collections.Generic;
using Godland.Game.Writer.Run;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Writer.Progression
{
    [CreateAssetMenu(menuName = "Godland/Progression Config", fileName = "ProgressionConfig")]
    public sealed class ProgressionConfig : ScriptableObject
    {
        [field: SerializeField, BoxGroup("Поток"), InfoBox("Режиссёр потока ведёт статистику всех забегов и решает, что поднять, а что придержать.")]
        public bool FlowEnabled { get; private set; } = true;

        [field: SerializeField, BoxGroup("Поток"), Range(50, 99), SuffixLabel("%"), InfoBox("Сколько «свежей частоты» события остаётся после каждого забега: 85% — последние 6–7 забегов важнее остальных, но учитывается вся история.")]
        public int FlowDecayPercent { get; private set; } = 85;

        public Godland.Core.Flow.FlowRules FlowRules => new(FlowDecayPercent, FlowBoostPercent, FlowCutPercent);

        [field: SerializeField, BoxGroup("Поток"), Range(0, 200), SuffixLabel("%"), InfoBox("Насколько поднимается шанс того, чего давно не было.")]
        public int FlowBoostPercent { get; private set; } = 40;

        [field: SerializeField, BoxGroup("Поток"), Range(0, 95), SuffixLabel("%"), InfoBox("Насколько опускается шанс того, что было в каждом из последних забегов.")]
        public int FlowCutPercent { get; private set; } = 50;

        [field: SerializeField, MinValue(0), InfoBox("Сколько «Памяти миров» даёт первое открытие события. Повторное не даёт ничего.")]
        public int DiscoveryReward { get; private set; } = 10;

        [SerializeField, InfoBox("Слои мира, открытые с первого забега. Остальные открываются прокачкой и открытиями.")]
        private List<ProgressionFeature> _startingFeatures = new();

        [SerializeField, AssetsOnly] private List<DiscoveryDefinition> _discoveries = new();

        public IReadOnlyList<ProgressionFeature> StartingFeatures => _startingFeatures;
        public IReadOnlyList<DiscoveryDefinition> Discoveries => _discoveries;
    }
}
