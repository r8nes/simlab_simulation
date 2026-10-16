using System.Collections.Generic;
using System.Linq;
using Godland.Game.Powers.Gods;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Titans
{
    [CreateAssetMenu(menuName = "Godland/Titan Config", fileName = "TitanConfig")]
    public sealed class TitanConfig : ScriptableObject
    {
        [field: SerializeField, Required, AssetsOnly]
        public GodAvatarView AvatarPrefab { get; private set; }

        [SerializeField, AssetsOnly]
        private List<TitanDefinition> _titans = new();

        public System.Collections.Generic.IReadOnlyList<TitanDefinition> All => _titans;

        public TitanDefinition DefinitionOf(string id) => _titans.First(titan => titan.Id == id);
    }
}
