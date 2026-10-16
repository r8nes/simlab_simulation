using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Presentation.Timing
{
    [CreateAssetMenu(menuName = "Godland/Time Config", fileName = "TimeConfig")]
    public sealed class TimeConfig : ScriptableObject
    {
        [field: SerializeField, MinValue(0.1f), SuffixLabel("s")]
        public float SecondsPerYear { get; private set; } = 2f;

        [field: SerializeField, MinValue(0.1f), ListDrawerSettings(ShowIndexLabels = true)]
        public float[] Speeds { get; private set; } = { 1f, 2f, 3f };
    }
}
