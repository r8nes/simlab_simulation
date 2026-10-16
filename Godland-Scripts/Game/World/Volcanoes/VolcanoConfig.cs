using System.Collections.Generic;
using System.Linq;
using Godland.Core.World.Volcanoes;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.World.Volcanoes
{
    [CreateAssetMenu(menuName = "Godland/Volcano Config", fileName = "VolcanoConfig")]
    public sealed class VolcanoConfig : ScriptableObject, IVolcanoRules
    {
        [SerializeField, BoxGroup("Tiles"), AssetsOnly, InfoBox("Какие тайлы — вулканы.")]
        private List<TileDefinition> _volcanoes = new();

        [SerializeField, BoxGroup("Tiles"), AssetsOnly, InfoBox("Соседние тайлы этих типов при извержении превращаются в пепел.")]
        private List<TileDefinition> _burnable = new();

        [SerializeField, BoxGroup("Tiles"), Required, AssetsOnly]
        private TileDefinition _ash;

        [SerializeField, BoxGroup("Tiles"), AssetsOnly, InfoBox("Что вулкан растапливает рядом.")]
        private List<TileDefinition> _meltable = new();

        [SerializeField, BoxGroup("Tiles"), Required, AssetsOnly, InfoBox("Во что превращается растаявший тайл.")]
        private TileDefinition _melted;

        [field: SerializeField, BoxGroup("Eruption"), MinValue(1), SuffixLabel("years"), InfoBox("Как часто бросается скрытый кубик извержения.")]
        public int CheckPeriod { get; private set; } = 40;

        [field: SerializeField, BoxGroup("Eruption"), Range(0, 100), SuffixLabel("%")]
        public int EruptionChancePercent { get; private set; } = 10;

        [field: SerializeField, BoxGroup("Eruption"), Range(0, 100), SuffixLabel("%"), InfoBox("Прибавка к шансу за каждое поселение, поклоняющееся этому вулкану: их вера будит его.")]
        public int ChancePerBelieverPercent { get; private set; } = 5;

        [field: SerializeField, BoxGroup("Eruption"), MinValue(1), SuffixLabel("years"), InfoBox("За сколько лет вулкан предупреждает об извержении.")]
        public int WarningYears { get; private set; } = 20;

        [field: SerializeField, BoxGroup("Eruption"), MinValue(0), InfoBox("Сколько жителей теряют соседние поселения.")]
        public int PopulationLoss { get; private set; } = 2;

        [field: SerializeField, BoxGroup("Melting"), MinValue(1), SuffixLabel("years"), InfoBox("Раз в период вулкан растапливает один соседний снег.")]
        public int MeltPeriod { get; private set; } = 30;

        public string AshTileId => _ash.Id;

        public string MeltedTileId => _melted.Id;

        public bool IsVolcano(string tileId) => _volcanoes.Any(tile => tile.Id == tileId);

        public bool BurnsToAsh(string tileId) => _burnable.Any(tile => tile.Id == tileId);

        public bool Melts(string tileId) => _meltable.Any(tile => tile.Id == tileId);
    }
}
