using System.Collections.Generic;
using Godland.Game.Life.Races;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Sagas
{
    [CreateAssetMenu(menuName = "Godland/Saga Faction", fileName = "Faction")]
    public sealed class SagaFactionDefinition : ScriptableObject
    {
        [field: SerializeField, Required] public string DisplayName { get; private set; }
        [field: SerializeField, InfoBox("Кем зовётся вождь стороны: Лич, Архангел, Вождь живых.")] public string LeaderTitle { get; private set; }
        [SerializeField, InfoBox("Имена вождей: при перевороте приходит следующий.")] private List<string> _leaderNames = new();
        [field: SerializeField, TextArea] public string Temperament { get; private set; }
        [field: SerializeField, Required, AssetsOnly, InfoBox("Тайл, которым сторона метит свои земли.")] public TileDefinition Tile { get; private set; }
        [field: SerializeField, InfoBox("Земли стороны — её поселения (живые, мятежники), а Tile — во что она превращает отбитые земли.")] public bool HoldsSettlements { get; private set; }
        [field: SerializeField, InfoBox("Каким узором растут земли стороны (реакция-диффузия): пятна, языки, лабиринт.")] public Godland.Core.World.Patterns.PatternStyle Pattern { get; private set; } = Godland.Core.World.Patterns.PatternStyle.Tongues;
        [field: SerializeField, Range(0, 100), SuffixLabel("%"), InfoBox("Свой шанс найти артефакт за период (0 — общий из SagaConfig).")] public int ArtifactChancePercent { get; private set; }
        [field: SerializeField, MinValue(0), InfoBox("Сколько силы сторона получает с каждой смертью поселения в мире (нежить кормится мёртвыми).")] public int PowerPerDeath { get; private set; }
        [field: SerializeField, MinValue(0), InfoBox("Сколько своих земель сторона поднимает за период распространения.")] public int SpreadCells { get; private set; } = 1;
        [SerializeField, AssetsOnly, InfoBox("Какие тайлы сторона может обратить в свои (пусто — любые).")] private List<TileDefinition> _spreadsOver = new();
        [SerializeField, InfoBox("Деяния вождя: его разум выбирает одно за период.")] private List<RaceAction> _deeds = new();

        public IReadOnlyList<string> LeaderNames => _leaderNames;
        public IReadOnlyList<TileDefinition> SpreadsOver => _spreadsOver;
        public IReadOnlyList<RaceAction> Deeds => _deeds;
    }
}
