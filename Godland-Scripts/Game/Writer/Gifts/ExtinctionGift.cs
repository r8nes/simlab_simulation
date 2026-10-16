using System;
using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Grid;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Writer.Gifts
{
    [Serializable]
    public sealed class ExtinctionGift : IGiftEffect
    {
        [SerializeField, MinValue(1), SuffixLabel("years")] private int _returnAfter = 50;
        [SerializeField, MinValue(1)] private int _newSpecies = 4;
        [SerializeField, AssetsOnly, InfoBox("Кто может прийти после вымирания.")] private List<Godland.Game.Life.Features.FeatureDefinition> _species = new();

        public bool IsRelevant(GiftContext context) => context.Features.All.Count >= 3;

        public string Describe() => $"Все звери гибнут; через {_returnAfter} лет приходят новые виды";

        public void Apply(GiftContext context)
        {
            foreach (var cell in context.Features.All.Keys.ToList())
                context.Features.Remove(cell);

            context.Delayed.Schedule(_returnAfter, () => Repopulate(context));
        }

        private void Repopulate(GiftContext context)
        {
            for (var i = 0; i < _newSpecies; i++)
            {
                var options = (from pair in context.Tiles.All
                    where !context.Features.TryGet(pair.Key, out _)
                    from species in _species
                    where context.FeatureRules.CanLiveOn(species.Id, pair.Value)
                    select (pair.Key, species.Id)).ToList();
                if (options.Count == 0) return;

                var (cell, featureId) = context.Pick(options);
                context.Features.Add(cell, featureId);
            }
        }
    }
}
