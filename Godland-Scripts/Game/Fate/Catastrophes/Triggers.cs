using System;
using System.Linq;
using Godland.Game.Powers.Titans;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Catastrophes
{
    [Serializable]
    public sealed class DevouredTrigger : ICatastropheTrigger
    {
        [SerializeField, Range(1, 100), SuffixLabel("%"), InfoBox("Пожиратель съел такую долю мира от его пика.")]
        private int _eatenPercent = 40;

        public bool IsReady(CatastropheContext context) =>
            context.Devourer.IsActive && context.PeakTiles > 0
            && (context.PeakTiles - context.World.Tiles.CountedCount) * 100 >= context.PeakTiles * _eatenPercent;
    }

    [Serializable]
    public sealed class TitansMeetTrigger : ICatastropheTrigger
    {
        [SerializeField, Required, AssetsOnly] private TitanDefinition _first;
        [SerializeField, Required, AssetsOnly] private TitanDefinition _second;
        [SerializeField, Range(0, 100), SuffixLabel("%"), InfoBox("Если жив только один, он зовёт другого: шанс пробудить его за проверку.")]
        private int _callChancePercent = 25;

        public bool IsReady(CatastropheContext context)
        {
            var titans = context.World.Titans;
            var first = titans.All.Any(titan => titan.Rule.Id == _first.Id && titan.IsAwake);
            var second = titans.All.Any(titan => titan.Rule.Id == _second.Id && titan.IsAwake);
            if (first && second) return true;
            if (!first && !second) return false;
            if (context.World.Random.Next(100) >= _callChancePercent) return false;

            var caller = titans.All.First(titan => titan.Rule.Id == (first ? _first.Id : _second.Id));
            var land = context.World.Tiles.All.Keys
                .Where(cell => context.IsShaped(cell) && Math.Abs(cell.X - caller.Position.X) + Math.Abs(cell.Y - caller.Position.Y) >= 3)
                .OrderBy(cell => cell.X).ThenBy(cell => cell.Y).ToList();
            if (land.Count == 0) return false;

            titans.Awaken((first ? _second : _first).Rule, land[context.World.Random.Next(land.Count)], $"его позвал {(first ? _first : _second).DisplayName}");
            return false;
        }
    }
}
