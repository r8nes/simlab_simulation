using System.Linq;
using System;
using Godland.Core;
using UnityEngine;

namespace Godland.Game.Writer.Gifts
{
    [Serializable]
    public sealed class WorldModifierGift : IGiftEffect
    {
        [SerializeField] private WorldModifier _modifier;
        [SerializeField] private float _value = 1f;

        public bool IsRelevant(GiftContext context) => _modifier switch
        {
            WorldModifier.IreThreshold => context.Settlements.All.Count > 0,
            WorldModifier.PlagueSpread => context.Settlements.All.Count > 1,
            WorldModifier.CorruptionChance => context.Tiles.All.Keys.Any(context.Corruption.IsTainted),
            _ => true
        };

        public string Describe() => _modifier switch
        {
            WorldModifier.IreThreshold => $"жители терпят на {_value} чудес больше, прежде чем восстать",
            WorldModifier.PlagueSpread => "мор распространяется реже",
            WorldModifier.SuperBiomePeriod => "супербиомы разрастаются медленнее",
            WorldModifier.CorruptionChance => "порча ползёт медленнее",
            _ => string.Empty
        };

        public void Apply(GiftContext context) => context.World.Apply(_modifier, _value);
    }
}
