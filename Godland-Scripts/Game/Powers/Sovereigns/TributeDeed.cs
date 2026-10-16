using System;
using Godland.Core.Life.People;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Sovereigns
{
    [Serializable]
    public sealed class TributeDeed : ISovereignDeed
    {
        [SerializeField, Range(1, 100), SuffixLabel("%")] private int _sharePercent = 25;

        public string Name => "требует дань: забирает часть веры мира";

        public int Score(SovereignContext context, Religion claimant, Temperament temperament) =>
            context.Faith.Amount > 50 ? temperament.Greed + Math.Min(40, context.Faith.Amount / 20) : 0;

        public string Perform(SovereignContext context, Religion claimant)
        {
            var tribute = context.Faith.Amount * _sharePercent / 100;
            context.Faith.TrySpend(tribute);
            claimant.Kindle(tribute / 10);
            return $"{Name} (−{tribute} веры)";
        }
    }
}
