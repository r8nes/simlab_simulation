using System.Collections.Generic;
using Godland.Game.Writer.Actions;
using Godland.Game.Fate.Catastrophes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Epochs
{
    [CreateAssetMenu(menuName = "Godland/Epoch", fileName = "Epoch")]
    public sealed class EpochDefinition : ScriptableObject
    {
        [field: SerializeField, Required]
        public string DisplayName { get; private set; }

        [field: SerializeField, InfoBox("Что должно случиться, чтобы наступила эта эпоха.")]
        public EpochTrigger Trigger { get; private set; }

        [field: SerializeField, MinValue(0), SuffixLabel("год"), ShowIf(nameof(Trigger), EpochTrigger.Year)]
        public int TriggerYear { get; private set; } = 250;

        [field: SerializeField, MinValue(1), ShowIf(nameof(HasCount)), InfoBox("Сколько деревень, святынь, городов или царств должно быть в мире одновременно.")]
        public int TriggerCount { get; private set; } = 2;

        private bool HasCount => Trigger is EpochTrigger.Villages or EpochTrigger.Shrines or EpochTrigger.Cities or EpochTrigger.Kingdoms;

        [field: SerializeField, MinValue(0), SuffixLabel("years"), InfoBox("Эпоха наступает не раньше, чем через столько лет после начала предыдущей, даже если условие уже выполнено.")]
        public int MinYearsAfterPrevious { get; private set; }

        [field: SerializeField, MinValue(0), InfoBox("Сколько новых клеток открывается с одной случайной стороны мира при наступлении эпохи.")]
        public int UnlockCells { get; private set; } = 3;

        [SerializeField, AssetsOnly, InfoBox("Действия (чудеса), которые становятся доступны с наступлением эпохи.")]
        private List<TileActionDefinition> _unlockedActions = new();

        public IReadOnlyList<TileActionDefinition> UnlockedActions => _unlockedActions;

        [field: SerializeField, BoxGroup("Branches"), TextArea, InfoBox("Что это за эпоха — для журнала и баннера.")]
        public string Text { get; private set; }

        [SerializeField, BoxGroup("Branches"), AssetsOnly, InfoBox("Другие эпохи, которые могут наступить вместо этой, если мир к ним пришёл. Проверяются по порядку.")]
        private List<EpochDefinition> _alternatives = new();

        [SerializeReference, BoxGroup("Branches"), InfoBox("Только для эпохи-ветки: условие, при котором она заменяет основную.")]
        private ICatastropheTrigger _branchCondition;

        [SerializeReference, BoxGroup("Branches"), InfoBox("Законы эпохи, пока она длится.")]
        private List<IAgeLaw> _laws = new();

        public IReadOnlyList<EpochDefinition> Alternatives => _alternatives;
        public ICatastropheTrigger BranchCondition => _branchCondition;
        public IReadOnlyList<IAgeLaw> Laws => _laws;
    }
}
