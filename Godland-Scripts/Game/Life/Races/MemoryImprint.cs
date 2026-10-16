using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Life.Races
{
    [Serializable]
    public sealed class MemoryImprint
    {
        [field: SerializeField] public string Key { get; private set; }
        [field: SerializeField, MinValue(1)] public int Amount { get; private set; } = 1;
        [field: SerializeField, InfoBox("Писать в центр мира (общая память), а не в клетку народа.")] public bool Global { get; private set; }
    }
}
