using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Pantheons
{
    [CreateAssetMenu(menuName = "Godland/Scheme Book", fileName = "SchemeBook")]
    public sealed class SchemeBook : ScriptableObject
    {
        [SerializeField, InfoBox("Замыслы: имя действующего лица (бога, титана, правителя) или пусто — для всех этого рода.")] private List<GodScheme> _schemes = new();
        [SerializeField] private List<GodAction> _actions = new();

        public IReadOnlyList<GodScheme> Schemes => _schemes;
        public IReadOnlyList<GodAction> Actions => _actions;
    }
}
