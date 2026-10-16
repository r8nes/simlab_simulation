using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Catastrophes
{
    [CreateAssetMenu(menuName = "Godland/Catastrophe Config", fileName = "CatastropheConfig")]
    public sealed class CatastropheConfig : ScriptableObject
    {
        [field: SerializeField, MinValue(1), SuffixLabel("years"), InfoBox("Катастрофы — этапы, а не концовки: удар ломает мир, эпоха последствий его перестраивает.")]
        public int CheckPeriod { get; private set; } = 10;

        [field: SerializeField, MinValue(0), SuffixLabel("years"), InfoBox("Пауза после конца эпохи последствий до следующей катастрофы.")]
        public int CooldownYears { get; private set; } = 200;

        [SerializeField, AssetsOnly] private List<CatastropheDefinition> _catastrophes = new();

        public IReadOnlyList<CatastropheDefinition> Catastrophes => _catastrophes;
    }
}
