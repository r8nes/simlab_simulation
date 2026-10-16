using System;
using Godland.Core.World.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Writer.Actions
{
    [Serializable]
    public sealed class BirthGodEffect : ITileEffect
    {
        [SerializeField, Required, InfoBox("Id бога из GodsConfig.")] private string _godId;
        [SerializeField] private string _godName;

        public string Describe() => $"рождается бог {_godName} на лучшем тайле своей стихии; он тебе не подчиняется";

        public bool CanAffect(GridCoord target, TileEffectContext context) => context.Pantheon.CanBirth(_godId);

        public void Apply(GridCoord target, TileEffectContext context) => context.Pantheon.TryBirth(_godId);
    }
}
