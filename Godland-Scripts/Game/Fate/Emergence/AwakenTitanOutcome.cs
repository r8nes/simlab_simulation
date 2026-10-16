using System;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Game.Powers.Titans;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Emergence
{
    [Serializable]
    public sealed class AwakenTitanOutcome : IEmergenceOutcome
    {
        [SerializeField, Required, AssetsOnly] private TitanDefinition _titan;

        public void Apply(EmergenceContext context, GridCoord site, string title)
        {
            if (site == context.Grid.Center)
            {
                var land = context.Tiles.All.Keys.Where(cell => cell != context.Grid.Center && !context.Guards.IsProtected(cell)).ToList();
                if (land.Count > 0) site = land[context.Random.Next(land.Count)];
            }

            context.Titans.Awaken(_titan.Rule, site, $"возникло: {title}");
        }
    }
}
