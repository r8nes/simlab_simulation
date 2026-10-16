using System;
using System.Linq;
using Godland.Core.Life.People;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Sovereigns
{
    [Serializable]
    public sealed class SlayGodsEffect : ISovereignEffect
    {
        [SerializeField, Required, AssetsOnly, InfoBox("Во что превращаются престолы убитых богов.")] private TileDefinition _seatBecomes;

        public void OnRise(SovereignContext context, Religion claimant)
        {
            foreach (var god in context.Pantheon.Gods.ToList())
            {
                context.Tiles.Place(god.Seat, _seatBecomes.Id, "бойня правителя мира");
                context.Pantheon.Slay(god);
            }
        }

        public void OnReign(SovereignContext context, int year) { }

        public void OnFall(SovereignContext context) { }
    }
}
