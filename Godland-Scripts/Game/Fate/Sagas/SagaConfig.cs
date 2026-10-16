using System.Collections.Generic;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Sagas
{
    [CreateAssetMenu(menuName = "Godland/Saga Config", fileName = "SagaConfig")]
    public sealed class SagaConfig : ScriptableObject
    {
        [field: SerializeField, MinValue(1), SuffixLabel("years"), InfoBox("Саги — супер-переходы: Владыка переворачивает мир, и дальше идёт цепь актов со своим тоном, сторонами и концами.")]
        public int CheckPeriod { get; private set; } = 10;
        [field: SerializeField, MinValue(1), SuffixLabel("years")] public int SpreadPeriod { get; private set; } = 10;
        [field: SerializeField, MinValue(1), SuffixLabel("years")] public int BattlePeriod { get; private set; } = 10;
        [field: SerializeField, MinValue(1), SuffixLabel("years")] public int DeedPeriod { get; private set; } = 30;
        [field: SerializeField, MinValue(1), SuffixLabel("years")] public int IntriguePeriod { get; private set; } = 50;
        [field: SerializeField, Range(0, 100), SuffixLabel("%")] public int CoupChancePercent { get; private set; } = 20;
        [field: SerializeField, Range(0, 100), SuffixLabel("%")] public int BetrayalChancePercent { get; private set; } = 15;
        [field: SerializeField, Range(0, 100), SuffixLabel("%"), InfoBox("Шанс найти артефакт за период распространения.")] public int ArtifactChancePercent { get; private set; } = 3;
        [field: SerializeField, MinValue(0)] public int ArtifactPower { get; private set; } = 30;
        [field: SerializeField, MinValue(0), InfoBox("Сколько силы даёт победа в битве (и забирает у проигравшего).")] public int BattlePower { get; private set; } = 3;
        [field: SerializeField, Required, AssetsOnly, InfoBox("Бездна между половинами расщеплённого мира.")] public TileDefinition GapTile { get; private set; }
        [field: SerializeField, Required, AssetsOnly] public TileDefinition BridgeTile { get; private set; }
        [field: SerializeField, Required, AssetsOnly, InfoBox("Во что возвращается бездна, когда мир снова един.")] public TileDefinition HealedGapTile { get; private set; }

        [SerializeField, AssetsOnly] private List<SagaDefinition> _sagas = new();

        public IReadOnlyList<SagaDefinition> Sagas => _sagas;
    }
}
