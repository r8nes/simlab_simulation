using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Powers.Sovereigns
{
    [Serializable]
    public sealed class Temperament
    {
        [field: SerializeField, Range(0, 100), InfoBox("Характер правителя: от него зависит, какие деяния он выбирает.")]
        public int Cruelty { get; private set; } = 50;

        [field: SerializeField, Range(0, 100)]
        public int Greed { get; private set; } = 50;

        [field: SerializeField, Range(0, 100)]
        public int Pride { get; private set; } = 50;

        [field: SerializeField, Range(0, 100)]
        public int Fear { get; private set; } = 50;

        public string Describe()
        {
            var traits = new System.Collections.Generic.List<string>();
            if (Cruelty >= 60) traits.Add("жесток");
            if (Greed >= 60) traits.Add("жаден");
            if (Pride >= 60) traits.Add("горд");
            if (Fear >= 60) traits.Add("осторожен");
            return traits.Count == 0 ? "непредсказуем" : string.Join(", ", traits);
        }
    }
}
