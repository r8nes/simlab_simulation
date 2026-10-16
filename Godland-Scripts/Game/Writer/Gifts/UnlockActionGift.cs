using System.Linq;
using System;
using Godland.Game.Writer.Actions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Writer.Gifts
{
    [Serializable]
    public sealed class UnlockActionGift : IGiftEffect
    {
        [SerializeField, Required, AssetsOnly] private TileActionDefinition _action;

        public bool IsRelevant(GiftContext context) => context.Actions.HasTarget(_action);

        public string Describe() => $"новое действие «{_action.DisplayName}» ({_action.CostText(_action.FaithCost)}): {_action.Describe()}";

        public void Apply(GiftContext context) => context.Unlocks.UnlockAction(_action);
    }
}
