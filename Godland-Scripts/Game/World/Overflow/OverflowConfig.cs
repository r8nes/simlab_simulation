using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Cells;
using Godland.Game.Writer.Run;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.World.Overflow
{
    [CreateAssetMenu(menuName = "Godland/Overflow Config", fileName = "OverflowConfig")]
    public sealed class OverflowConfig : ScriptableObject, IOverflowRules
    {
        [field: SerializeField, InfoBox("Покупаемая механика прокачки (пока включена всегда).")]
        public ProgressionFeature RequiredFeature { get; private set; } = ProgressionFeature.FertilityOverflow;

        [field: SerializeField, MinValue(1), InfoBox("Сколько избыточного плодородия сверх максимума должно накопиться, чтобы тайл сам перешёл в следующий.")]
        public int OverflowThreshold { get; private set; } = 20;

        [SerializeField, InfoBox("Что во что превращается от избытка.")]
        private List<OverflowStep> _steps = new();

        public string OverflowResult(string tileId, int epoch) => Step(tileId, epoch)?.To.Id;

        public int ThresholdFor(string tileId, int epoch) => Step(tileId, epoch)?.Threshold ?? OverflowThreshold;

        private OverflowStep Step(string tileId, int epoch) =>
            _steps.FirstOrDefault(step => step.From != null && step.From.Id == tileId && epoch >= step.FromEpoch);
    }

    [System.Serializable]
    public sealed class OverflowStep
    {
        [field: SerializeField, Required, AssetsOnly, HorizontalGroup]
        public TileDefinition From { get; private set; }

        [field: SerializeField, Required, AssetsOnly, HorizontalGroup]
        public TileDefinition To { get; private set; }

        [field: SerializeField, MinValue(1), HorizontalGroup(120), LabelText("Порог")]
        public int Threshold { get; private set; } = 20;

        [field: SerializeField, MinValue(0), HorizontalGroup(120), LabelText("С эпохи")]
        public int FromEpoch { get; private set; }
    }
}
