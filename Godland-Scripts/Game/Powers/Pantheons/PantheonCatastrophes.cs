using System;
using System.Linq;
using Godland.Game.Fate.Catastrophes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Pantheons
{
    [Serializable]
    public sealed class PantheonReadyTrigger : ICatastropheTrigger
    {
        [SerializeField, Required, InfoBox("Id пантеона: norse, egypt, slavic, aztec, greek, sumer.")] private string _pantheonId;
        [SerializeField, MinValue(1)] private int _minGods = 3;
        [SerializeField, MinValue(0), SuffixLabel("years")] private int _minAge = 250;
        [SerializeField, Range(0, 100), SuffixLabel("%"), InfoBox("Доля поселений мира, верящих в пантеон.")] private int _minSharePercent = 30;

        public bool IsReady(CatastropheContext context)
        {
            var house = context.Pantheons.Find(_pantheonId);
            if (house == null || house.Gods.Count < _minGods || context.World.Clock.Year - house.ArrivedYear < _minAge) return false;

            var settlements = context.World.Settlements.All.Count;
            return settlements > 0 && context.World.Shrines.FollowersOf(house.Faith).Count * 100 >= settlements * _minSharePercent;
        }
    }

    [Serializable]
    public sealed class TwilightOfHouseStrike : ICatastropheStrike
    {
        [SerializeField, Required] private string _pantheonId;

        public void Strike(CatastropheContext context, string cause)
        {
            var house = context.Pantheons.Find(_pantheonId);
            if (house == null) return;

            foreach (var god in house.Gods.ToList()) context.World.Pantheon.Slay(god);
            context.World.Memory.Add(context.World.Grid.Center, "gods-fallen", house.Gods.Count + 1);
        }
    }

    [Serializable]
    public sealed class NightStrike : ICatastropheStrike
    {
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _years = 100;

        public void Strike(CatastropheContext context, string cause) => context.World.Modifiers.BeginNight(context.World.Clock.Year + _years);
    }
}
