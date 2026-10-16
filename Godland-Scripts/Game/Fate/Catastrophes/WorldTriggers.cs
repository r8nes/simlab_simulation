using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Catastrophes
{
    [Serializable]
    public sealed class TileShareTrigger : ICatastropheTrigger
    {
        [SerializeField, Required, AssetsOnly] private List<TileDefinition> _tiles = new();
        [SerializeField, Range(1, 100), SuffixLabel("%")] private int _sharePercent = 30;
        [SerializeField, MinValue(1)] private int _minWorldTiles = 12;

        public bool IsReady(CatastropheContext context)
        {
            var tiles = context.World.Tiles;
            if (tiles.CountedCount < _minWorldTiles) return false;

            var matching = tiles.CountedIds.Count(id => _tiles.Any(tile => tile.Id == id));
            return matching * 100 >= tiles.CountedCount * _sharePercent;
        }
    }

    [Serializable]
    public sealed class TileCountTrigger : ICatastropheTrigger
    {
        [SerializeField, Required, AssetsOnly] private List<TileDefinition> _tiles = new();
        [SerializeField, MinValue(1)] private int _count = 5;

        public bool IsReady(CatastropheContext context) =>
            context.World.Tiles.CountedIds.Count(id => _tiles.Any(tile => tile.Id == id)) >= _count;
    }

    [Serializable]
    public sealed class PlagueTrigger : ICatastropheTrigger
    {
        [SerializeField, Range(1, 100), SuffixLabel("%"), InfoBox("Доля больных поселений.")] private int _sickPercent = 30;
        [SerializeField, MinValue(1)] private int _minSick = 3;

        public bool IsReady(CatastropheContext context)
        {
            var sick = context.World.Plague.SickCount;
            var all = context.World.Settlements.All.Count;
            return sick >= _minSick && all > 0 && sick * 100 >= all * _sickPercent;
        }
    }

    [Serializable]
    public sealed class NightTrigger : ICatastropheTrigger
    {
        [SerializeField, MinValue(1), SuffixLabel("years"), InfoBox("Ночь продлится ещё столько лет или дольше.")] private int _minYearsLeft = 30;

        public bool IsReady(CatastropheContext context) =>
            context.World.Modifiers.NightUntilYear - context.World.Clock.Year >= _minYearsLeft;
    }

    [Serializable]
    public sealed class GlobalMemoryTrigger : ICatastropheTrigger
    {
        [SerializeField, Required] private string _key;
        [SerializeField, MinValue(1)] private int _threshold = 10;
        [SerializeField, InfoBox("Суммировать память всех клеток мира, а не только общую.")] private bool _anywhere;

        public bool IsReady(CatastropheContext context)
        {
            var memory = context.World.Memory;
            var total = _anywhere ? memory.CellsWith(_key).Sum(cell => memory.Get(cell, _key)) : memory.Get(context.World.Grid.Center, _key);
            return total >= _threshold;
        }
    }

    [Serializable]
    public sealed class RaceWarTrigger : ICatastropheTrigger
    {
        [SerializeField, MinValue(1), InfoBox("Сколько войн народов мир должен запомнить.")] private int _wars = 8;
        [SerializeField, MinValue(2)] private int _minRaces = 2;

        public bool IsReady(CatastropheContext context) =>
            context.Races.Living.Count() >= _minRaces && context.World.Memory.Get(context.World.Grid.Center, "race-war") >= _wars;
    }

    [Serializable]
    public sealed class TitanAwakeTrigger : ICatastropheTrigger
    {
        [SerializeField, MinValue(1)] private int _count = 1;

        public bool IsReady(CatastropheContext context) => context.World.Titans.All.Count(titan => titan.IsAwake) >= _count;
    }

    [Serializable]
    public sealed class GodlessTrigger : ICatastropheTrigger
    {
        [SerializeField, MinValue(1), InfoBox("Сколько богов должно было пасть, пока сейчас ни одного нет.")] private int _fallen = 2;

        public bool IsReady(CatastropheContext context) =>
            context.World.Pantheon.Gods.Count == 0 && context.World.Memory.Get(context.World.Grid.Center, "gods-fallen") >= _fallen;
    }

    [Serializable]
    public sealed class OneFaithTrigger : ICatastropheTrigger
    {
        [SerializeField, Range(1, 100), SuffixLabel("%"), InfoBox("Доля поселений, которые верят в одну веру.")] private int _sharePercent = 80;
        [SerializeField, MinValue(1)] private int _minBelievers = 8;

        public bool IsReady(CatastropheContext context)
        {
            var shrines = context.World.Shrines;
            var settlements = context.World.Settlements.All.Count;
            if (shrines.BelieverCount < _minBelievers || settlements == 0) return false;

            var largest = shrines.Religions.Max(religion => shrines.FollowersOf(religion).Count);
            return largest * 100 >= settlements * _sharePercent;
        }
    }

    [Serializable]
    public sealed class LivingRacesTrigger : ICatastropheTrigger
    {
        [SerializeField, MinValue(1)] private int _count = 2;

        public bool IsReady(CatastropheContext context) => context.Races.Living.Count() >= _count;
    }

    [Serializable]
    public sealed class FewSettlementsTrigger : ICatastropheTrigger
    {
        [SerializeField, MinValue(0), InfoBox("Поселений не больше этого числа.")] private int _max = 8;

        public bool IsReady(CatastropheContext context) => context.World.Settlements.All.Count <= _max;
    }

    [Serializable]
    public sealed class GodAliveTrigger : ICatastropheTrigger
    {
        [SerializeField, Required, InfoBox("Id бога: talas, urgar, veila, ifrit.")] private string _godId;
        [SerializeField, MinValue(0)] private int _minTemples;

        public bool IsReady(CatastropheContext context) =>
            context.World.Pantheon.Gods.Any(god => god.Rule.Id == _godId && god.HeroName == null && context.Temples.CountOf(god) >= _minTemples);
    }

    [Serializable]
    public sealed class AllTrigger : ICatastropheTrigger
    {
        [SerializeReference] private List<ICatastropheTrigger> _all = new();

        public bool IsReady(CatastropheContext context) => _all.All(trigger => trigger.IsReady(context));
    }

    [Serializable]
    public sealed class HeldTrigger : ICatastropheTrigger
    {
        [SerializeReference, Required] private ICatastropheTrigger _condition;
        [SerializeField, MinValue(1), SuffixLabel("years"), InfoBox("Условие должно держаться столько лет подряд.")] private int _years = 200;

        [NonSerialized] private int _since = -1;
        [NonSerialized] private int _lastYear = -1;

        public bool IsReady(CatastropheContext context)
        {
            var year = context.World.Clock.Year;
            if (year < _lastYear) _since = -1;
            _lastYear = year;
            if (!_condition.IsReady(context))
            {
                _since = -1;
                return false;
            }

            if (_since < 0) _since = year;
            return year - _since >= _years;
        }
    }

    [Serializable]
    public sealed class AnyTrigger : ICatastropheTrigger
    {
        [SerializeReference] private List<ICatastropheTrigger> _any = new();

        public bool IsReady(CatastropheContext context) => _any.Any(trigger => trigger.IsReady(context));
    }
}
