using System.Linq;
using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Writer.Gifts
{
    [Serializable]
    public sealed class CounterChanceGift : IGiftEffect
    {
        [SerializeField, MinValue(1f)] private float _multiplier = 2f;

        public bool IsRelevant(GiftContext context) => context.Features.All.Count > 0;

        public string Describe() => "жертвы чаще дают отпор хищникам и мутируют";

        public void Apply(GiftContext context) => context.Interactions.MultiplyCounterChance(_multiplier);
    }
}
