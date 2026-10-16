using System;
using System.Linq;
using Godland.Core;
using Godland.Core.World.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Emergence
{
    [Serializable]
    public sealed class ManifestGodOutcome : IEmergenceOutcome
    {
        [SerializeField, Required, InfoBox("Id бога-образца из GodsConfig: его деяние и догмат.")] private string _templateId;
        [SerializeField, Required] private string _title = "Чумной владыка";
        [SerializeField, MinValue(1), InfoBox("Мор при нём разносится во столько раз быстрее.")] private float _plagueSpread = 2f;

        public void Apply(EmergenceContext context, GridCoord site, string title)
        {
            var seat = context.Settlements.All.Keys.Where(cell => !context.Guards.IsProtected(cell))
                .OrderBy(_ => context.Random.Next(10000)).Cast<GridCoord?>().FirstOrDefault();
            if (seat is not { } cell) return;

            context.Pantheon.Manifest(cell, _templateId, _title);
            if (_plagueSpread > 1f) context.Modifiers.Apply(WorldModifier.PlagueSpread, _plagueSpread);
        }
    }
}
