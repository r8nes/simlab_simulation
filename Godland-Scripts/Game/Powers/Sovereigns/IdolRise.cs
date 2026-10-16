using System;
using System.Linq;
using Godland.Core.Life.People;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Sovereigns
{
    [Serializable]
    public sealed class IdolRise : ISovereignRise
    {
        [SerializeField, Required, InfoBox("Id идола (IdolDefinition.Id), чья вера может захватить мир.")] private string _idolId;
        [SerializeField, MinValue(0)] private int _minFervor = 50;
        [SerializeField, MinValue(1)] private int _minFollowers = 3;
        [SerializeField, MinValue(0), InfoBox("Боги должны быть в мире, но слабы: храмов у всех вместе не больше этого.")] private int _maxGodTemples = 3;
        [SerializeField, MinValue(0)] private int _minGods = 1;
        [SerializeField, Range(0, 100), SuffixLabel("%"), InfoBox("Шанс за проверку, если условия сошлись: восхождение не гарантировано.")] private int _chancePercent = 50;

        public bool TryFindClaimant(SovereignContext context, out Religion claimant)
        {
            var gods = context.Pantheon.Gods;
            claimant = null;
            if (gods.Count < _minGods || gods.Sum(context.Temples.CountOf) > _maxGodTemples) return false;

            claimant = context.Shrines.Religions.FirstOrDefault(religion =>
                religion.IdolId == _idolId && religion.Fervor >= _minFervor && context.Shrines.FollowersOf(religion).Count >= _minFollowers);
            return claimant != null && context.Random.Next(100) < _chancePercent;
        }
    }
}
