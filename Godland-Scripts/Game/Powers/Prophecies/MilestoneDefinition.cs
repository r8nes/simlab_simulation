using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Prophecies
{
    [CreateAssetMenu(menuName = "Godland/Milestone", fileName = "Milestone")]
    public sealed class MilestoneDefinition : ScriptableObject
    {
        [field: SerializeField, Required]
        public string DisplayName { get; private set; }

        [field: SerializeField, TextArea, InfoBox("Новое правило мира — показывается игроку.")]
        public string Rule { get; private set; }

        [SerializeReference, Required]
        private List<IMilestoneEffect> _effects = new();

        public void Apply(ProphecyContext context)
        {
            foreach (var effect in _effects) effect.Apply(context);
        }
    }
}
