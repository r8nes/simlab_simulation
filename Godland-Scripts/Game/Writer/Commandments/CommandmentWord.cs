using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Writer.Commandments
{
    [CreateAssetMenu(menuName = "Godland/Commandment Word", fileName = "Word")]
    public sealed class CommandmentWord : ScriptableObject
    {
        [field: SerializeField, Required, InfoBox("Как фрагмент читается в заповеди.")] public string Text { get; private set; }
        [field: SerializeField] public CommandmentSlot Slot { get; private set; }
        [field: SerializeField] public CommandmentRarity Rarity { get; private set; }
        [field: SerializeField, InfoBox("Слово закрыто, пока его не откроет прокачка или открытие.")] public bool RequiresUnlock { get; private set; }

        [field: SerializeField, ShowIf(nameof(Slot), CommandmentSlot.When)] public CommandmentTrigger Trigger { get; private set; }

        [SerializeReference, ShowIf(nameof(Slot), CommandmentSlot.Then)] private ICommandmentEffect _effect;
        [field: SerializeField, ShowIf(nameof(Slot), CommandmentSlot.Then), MinValue(0), SuffixLabel("years"), InfoBox("Как часто последствие может случаться.")]
        public int CooldownYears { get; private set; } = 30;

        [SerializeReference, ShowIf(nameof(Slot), CommandmentSlot.Where)] private ICommandmentPlace _place;

        [field: SerializeField, ShowIf(nameof(Slot), CommandmentSlot.Amplifier)] public CommandmentAmplifier Amplifier { get; private set; }

        public ICommandmentEffect Effect => _effect;
        public ICommandmentPlace Place => _place;
    }
}
