using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Catastrophes
{
    [CreateAssetMenu(menuName = "Godland/Catastrophe", fileName = "Catastrophe")]
    public sealed class CatastropheDefinition : ScriptableObject
    {
        [field: SerializeField, Required] public string Title { get; private set; }
        [field: SerializeField, TextArea, InfoBox("Предвестие: что видит мир до удара.")] public string Omen { get; private set; }
        [field: SerializeField, TextArea] public string StrikeText { get; private set; }
        [field: SerializeField, MinValue(0), SuffixLabel("years")] public int WarningYears { get; private set; } = 20;
        [field: SerializeField, MinValue(0)] public int MinEpoch { get; private set; } = 1;
        [field: SerializeField, Range(1, 100), SuffixLabel("%"), InfoBox("Шанс удара за проверку, когда условие выполнено.")] public int ChancePercent { get; private set; } = 30;
        [field: SerializeField, InfoBox("Срабатывает и во время паузы и чужой эпохи последствий (для неотложных, как сытость Пожирателя).")] public bool IsUrgent { get; private set; }
        [field: SerializeField, MinValue(1)] public int MaxPerRun { get; private set; } = 1;
        [field: SerializeField, InfoBox("Удар отменяется, если к его году условие больше не держится.")] public bool CanBeAverted { get; private set; }

        [SerializeReference, Required] private ICatastropheTrigger _trigger;
        [SerializeReference] private List<ICatastropheStrike> _strikes = new();
        [field: SerializeField, Required, AssetsOnly, InfoBox("Эпоха последствий, которая сменяет текущую.")] public AgeDefinition Aftermath { get; private set; }

        public ICatastropheTrigger Trigger => _trigger;
        public IReadOnlyList<ICatastropheStrike> Strikes => _strikes;
    }
}
