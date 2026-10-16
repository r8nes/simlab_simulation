using System;
using System.Linq;
using Godland.Game.Fate.Catastrophes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Prophecies
{
    [Serializable]
    public sealed class TriggerCondition : IProphecyCondition
    {
        [SerializeReference, Required] private ICatastropheTrigger _trigger;

        public bool IsMet(ProphecyContext context) => _trigger != null && _trigger.IsReady(context.World);
    }

    [Serializable]
    public sealed class MemoryRiseCondition : IProphecyCondition
    {
        [SerializeField, Required, InfoBox("Ключ памяти или его начало (hunt: — все охоты хищников).")] private string _key;
        [SerializeField, MinValue(1), InfoBox("На сколько память всего мира должна вырасти со дня пророчества.")] private int _rise = 5;

        public bool IsMet(ProphecyContext context)
        {
            var world = context.World.World;
            var total = world.Grid.Coords.Sum(cell => world.Memory.KeysAt(cell, _key).Sum(mark => mark.Value));
            return context.Record.Rise(_key, total) >= _rise;
        }
    }

    [Serializable]
    public sealed class RecordCountCondition : IProphecyCondition
    {
        [SerializeField, Required, InfoBox("abandoned — брошенные поселения.")] private string _key = "abandoned";
        [SerializeField, MinValue(1)] private int _count = 5;

        public bool IsMet(ProphecyContext context) => context.Record.CountOf(_key) >= _count;
    }

    [Serializable]
    public sealed class StarvingCondition : IProphecyCondition
    {
        [SerializeField, MinValue(1), InfoBox("Сколько поселений одновременно живут в долг (голодают).")] private int _count = 4;

        public bool IsMet(ProphecyContext context) =>
            context.Settlements.All.Keys.Count(cell => context.Settlements.EnergyOf(cell) < 0) >= _count;
    }

    [Serializable]
    public sealed class CityPopulationCondition : IProphecyCondition
    {
        [SerializeField, MinValue(1)] private int _population = 14;

        public bool IsMet(ProphecyContext context) =>
            context.World.Cities.All.Any(city => context.Settlements.TryGet(city, out var population) && population >= _population);
    }

    [Serializable]
    public sealed class WarStreakCondition : IProphecyCondition
    {
        [SerializeField, MinValue(1), SuffixLabel("years"), InfoBox("Сколько лет (всего, не обязательно подряд) в мире шла война царств со дня пророчества.")] private int _years = 40;

        public bool IsMet(ProphecyContext context)
        {
            var world = context.World.World;
            var year = world.Clock.Year;
            if (world.Realms.Wars.Count > 0 && context.Record.CountOf("war-year") != year)
            {
                context.Record.Set("war-year", year);
                context.Record.Count("war-years");
            }

            return context.Record.CountOf("war-years") >= _years;
        }
    }

    [Serializable]
    public sealed class RevertedTilesCondition : IProphecyCondition
    {
        [SerializeField, MinValue(1), InfoBox("Сколько клеток после пророчества вернулись к виду, который уже был у них раньше.")] private int _count = 4;

        public bool IsMet(ProphecyContext context)
        {
            var world = context.World.World;
            return world.Tiles.All.Keys.Count(cell =>
            {
                var records = world.History.Of(cell).Where(record => !string.IsNullOrEmpty(record.TileId)).ToList();
                if (records.Count < 3 || records[^1].Year < context.Record.AnnouncedYear) return false;

                return records.Take(records.Count - 2).Any(record => record.TileId == records[^1].TileId);
            }) >= _count;
        }
    }

    [Serializable]
    public sealed class TitanAgeCondition : IProphecyCondition
    {
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _years = 150;

        public bool IsMet(ProphecyContext context)
        {
            var world = context.World.World;
            return world.Titans.All.Any(titan => titan.IsAwake && world.Clock.Year - titan.BornYear >= _years);
        }
    }

    [Serializable]
    public sealed class FaithShareBelowCondition : IProphecyCondition
    {
        [SerializeField, Range(1, 100), SuffixLabel("%")] private int _sharePercent = 40;
        [SerializeField, MinValue(1)] private int _minSettlements = 10;

        public bool IsMet(ProphecyContext context)
        {
            var settlements = context.Settlements.All.Count;
            return settlements >= _minSettlements && context.Shrines.BelieverCount * 100 < settlements * _sharePercent;
        }
    }
}
