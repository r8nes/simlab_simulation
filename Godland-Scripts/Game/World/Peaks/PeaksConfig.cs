using Godland.Core.World.Peaks;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.World.Peaks
{
    [CreateAssetMenu(menuName = "Godland/Peaks Config", fileName = "PeaksConfig")]
    public sealed class PeaksConfig : ScriptableObject, IPeakRules
    {
        [field: SerializeField, Required, AssetsOnly, InfoBox("Супергора: игрок не может её менять. Если её изменит мир, она становится вулканом и сразу извергается.")]
        public TileDefinition Peak { get; private set; }

        [field: SerializeField, Required, AssetsOnly] public TileDefinition Volcano { get; private set; }

        public string PeakTileId => Peak.Id;
        public string VolcanoTileId => Volcano.Id;
    }
}
