using System;
using Godland.Core.World.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Emergence
{
    [Serializable]
    public sealed class HeroCradleOutcome : IEmergenceOutcome
    {
        [SerializeField, Range(0, 100), SuffixLabel("%")] private int _bonus = 20;

        public void Apply(EmergenceContext context, GridCoord site, string title) => context.Heroes.SetCradle(site, _bonus);
    }
}
