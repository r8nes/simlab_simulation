using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Epochs
{
    [CreateAssetMenu(menuName = "Godland/Epoch Config", fileName = "EpochConfig")]
    public sealed class EpochConfig : ScriptableObject
    {
        [SerializeField, AssetsOnly, InfoBox("По порядку. Первая — стартовая эпоха забега.")]
        private List<EpochDefinition> _epochs = new();

        [field: SerializeField, MinValue(0.1f), SuffixLabel("s")]
        public float BannerDuration { get; private set; } = 3f;

        public IReadOnlyList<EpochDefinition> Epochs => _epochs;
    }
}
