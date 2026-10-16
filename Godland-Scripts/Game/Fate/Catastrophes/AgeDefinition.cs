using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Catastrophes
{
    [CreateAssetMenu(menuName = "Godland/Age", fileName = "Age")]
    public sealed class AgeDefinition : ScriptableObject
    {
        [field: SerializeField, Required] public string DisplayName { get; private set; }
        [field: SerializeField, TextArea, InfoBox("Что происходит в эту эпоху — строка под названием.")] public string Text { get; private set; }
        [field: SerializeField, MinValue(1), SuffixLabel("years")] public int Years { get; private set; } = 300;

        [SerializeReference, InfoBox("Законы эпохи: как мир живёт и возрождается после удара.")]
        private List<IAgeLaw> _laws = new();

        public IReadOnlyList<IAgeLaw> Laws => _laws;
    }
}
