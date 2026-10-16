using System.Collections.Generic;
using System.Linq;
using Godland.Core.Life.People;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Sovereigns
{
    [CreateAssetMenu(menuName = "Godland/Sovereign", fileName = "Sovereign")]
    public sealed class SovereignDefinition : ScriptableObject
    {
        [field: SerializeField, Required]
        public string DisplayName { get; private set; }

        [field: SerializeField, InfoBox("Название бойни (баннер при восхождении).")]
        public string Slaughter { get; private set; }

        [field: SerializeField, TextArea, InfoBox("Как меняется мир при его правлении — для игрока.")]
        public string Reign { get; private set; }

        [field: SerializeField]
        public Temperament Temperament { get; private set; } = new();

        [SerializeReference, Required]
        private ISovereignRise _rise;

        [SerializeReference, InfoBox("Что правитель может делать. Раз в период он выбирает деяние с лучшей для себя оценкой (по характеру и положению).")]
        private List<ISovereignDeed> _deeds = new();

        [SerializeReference]
        private List<ISovereignEffect> _effects = new();

        [SerializeReference, InfoBox("Правитель свергнут, если выполнено любое из условий.")]
        private List<ISovereignFall> _falls = new();

        public bool TryFindClaimant(SovereignContext context, out Religion claimant)
        {
            claimant = null;
            return _rise != null && _rise.TryFindClaimant(context, out claimant);
        }

        public IReadOnlyList<ISovereignEffect> Effects => _effects;

        public IReadOnlyList<ISovereignDeed> Deeds => _deeds;

        public bool IsOverthrown(SovereignContext context, Religion claimant) => _falls.Any(fall => fall.IsOverthrown(context, claimant));

        public string OverthrowText => string.Join(" или ", _falls.Select(fall => fall.Describe()));
    }
}
