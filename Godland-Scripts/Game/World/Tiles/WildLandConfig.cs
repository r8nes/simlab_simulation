using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.World.Tiles
{
    [Serializable]
    public sealed class WildLandRule
    {
        [field: SerializeField, Required, AssetsOnly] public TileDefinition Result { get; private set; }
        [field: SerializeField, InfoBox("Подходит, если причина изменения содержит это слово (пусто — не важно).")] public string CauseContains { get; private set; }
        [field: SerializeField, AssetsOnly] public List<TileDefinition> Neighbors { get; private set; } = new();
        [field: SerializeField, MinValue(0)] public int MinNeighbors { get; private set; } = 1;
        [field: SerializeField] public bool NeedsSettlement { get; private set; }
    }

    [CreateAssetMenu(menuName = "Godland/Wild Land Config", fileName = "WildLandConfig")]
    public sealed class WildLandConfig : ScriptableObject
    {
        [field: SerializeField, Required, AssetsOnly, InfoBox("Тайл для застройки: его ставит только писатель. Когда мир сам должен поставить его, он выбирает тайл по обстановке.")]
        public TileDefinition BuildTile { get; private set; }

        [field: SerializeField, Required, AssetsOnly] public TileDefinition Fallback { get; private set; }

        [field: SerializeField, ListDrawerSettings(ShowFoldout = true), InfoBox("Правила проверяются по порядку, первое подходящее выигрывает.")]
        public List<WildLandRule> Rules { get; private set; } = new();
    }
}
