using System.Collections.Generic;
using System.Linq;
using Godland.Game.Presentation.Journal;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.WorldEvents
{
    [CreateAssetMenu(menuName = "Godland/World Event", fileName = "WorldEvent")]
    public sealed class WorldEventDefinition : ScriptableObject
    {
        [field: SerializeField, Required]
        public string Title { get; private set; }

        [field: SerializeField, TextArea, InfoBox("Запись в журнале.")]
        public string Text { get; private set; }

        [field: SerializeField]
        public JournalTone Tone { get; private set; } = JournalTone.Info;

        [field: SerializeField, InfoBox("Выключено — событие случается не больше раза за забег (глобальные потрясения).")]
        public bool Repeatable { get; private set; } = true;

        [field: SerializeField, Range(1, 100), SuffixLabel("%"), InfoBox("Шанс, что событие вообще может выпасть в этот бросок: редкие потрясения ниже 100.")]
        public int ChancePercent { get; private set; } = 100;

        [field: SerializeField, MinValue(0), InfoBox("Вес при броске.")]
        public int Weight { get; private set; } = 1;

        [field: SerializeField, MinValue(0), InfoBox("С какой эпохи (0 — природа, 1 — Каменный век).")]
        public int FromEpoch { get; private set; }

        [field: SerializeField, MinValue(-1), InfoBox("До какой эпохи включительно. −1 — без конца.")]
        public int ToEpoch { get; private set; } = -1;

        [SerializeReference, Required]
        private List<IWorldEventEffect> _effects = new();

        public bool FitsEpoch(int epoch) => epoch >= FromEpoch && (ToEpoch < 0 || epoch <= ToEpoch);

        public bool CanHappen(WorldEventContext context) => _effects.Count > 0 && _effects.All(effect => effect.CanApply(context));

        public void Apply(WorldEventContext context)
        {
            foreach (var effect in _effects)
                effect.Apply(context);
        }
    }
}
