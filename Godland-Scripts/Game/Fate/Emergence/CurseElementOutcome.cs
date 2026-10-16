using System;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Game.Powers.Gods;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Emergence
{
    [Serializable]
    public sealed class CurseElementOutcome : IEmergenceOutcome
    {
        [SerializeField, Required, AssetsOnly] private GodsConfig _gods;
        [SerializeField, Required, AssetsOnly, InfoBox("Во что медленно гниют тайлы проклятой стихии.")] private TileDefinition _rot;
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _period = 40;

        public void Apply(EmergenceContext context, GridCoord site, string title)
        {
            var mark = context.Memory.KeysAt(context.Grid.Center, "god-fell:").OrderByDescending(entry => entry.Value).FirstOrDefault();
            if (mark.Key == null) return;

            var ruleId = mark.Key.Substring("god-fell:".Length);
            context.Pantheon.Curse(ruleId);
            context.Memory.Forget(context.Grid.Center, mark.Key, 0);
            var rule = _gods.Gods.First(candidate => candidate.Id == ruleId);
            context.Spreads.Add(rule.DomainTileIds.ToList(), _rot.Id, _period, 1, title, rule.DomainTileIds.Append(_rot.Id).ToList());
        }
    }
}
