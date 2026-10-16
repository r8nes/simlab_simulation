using System.Linq;
using System;
using Godland.Game.Life.Features;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Writer.Gifts
{
    [Serializable]
    public sealed class UnlockUpgradeGift : IGiftEffect
    {
        [SerializeField, Required, AssetsOnly] private FeatureDefinition _upgrade;

        public bool IsRelevant(GiftContext context) => true;

        public string Describe() => $"новое улучшение «{_upgrade.DisplayName}» ({_upgrade.Cost}): {_upgrade.Describe()}";

        public void Apply(GiftContext context) => context.Unlocks.UnlockUpgrade(_upgrade);
    }
}
