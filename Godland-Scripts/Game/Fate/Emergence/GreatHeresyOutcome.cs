using System;
using Godland.Core.World.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Emergence
{
    [Serializable]
    public sealed class GreatHeresyOutcome : IEmergenceOutcome
    {
        [SerializeField, MinValue(0), InfoBox("Пыл новой церкви: с ним она быстро обретёт своего идола.")] private int _fervor = 40;

        public void Apply(EmergenceContext context, GridCoord site, string title) => context.Shrines.MergeHeresies(_fervor);
    }
}
