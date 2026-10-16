using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Prophecies
{
    [CreateAssetMenu(menuName = "Godland/Prophecy Config", fileName = "ProphecyConfig")]
    public sealed class ProphecyConfig : ScriptableObject
    {
        [SerializeField, AssetsOnly, InfoBox("Каждое пророчество — развилка: сбылось или предотвращено, и мир получает одну из двух вех (новое правило до конца забега).")]
        private List<ProphecyDefinition> _prophecies = new();

        public IReadOnlyList<ProphecyDefinition> Prophecies => _prophecies;
    }
}
