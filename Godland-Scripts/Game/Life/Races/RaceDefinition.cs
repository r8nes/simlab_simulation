using System.Collections.Generic;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Life.Races
{
    [CreateAssetMenu(menuName = "Godland/Race", fileName = "Race")]
    public sealed class RaceDefinition : ScriptableObject
    {
        [field: SerializeField, BoxGroup("Race")] public string DisplayName { get; private set; }
        [field: SerializeField, BoxGroup("Race"), TextArea] public string Text { get; private set; }
        [field: SerializeField, BoxGroup("Race"), InfoBox("Какой бог может создать народ (пусто — любой бог; «-» — никакой).")]
        public string GodId { get; private set; }
        [field: SerializeField, BoxGroup("Race"), Required, AssetsOnly, InfoBox("Тайл родины: его культура задаёт уклад народа.")]
        public TileDefinition Home { get; private set; }
        [field: SerializeField, BoxGroup("Race"), MinValue(1)] public int Population { get; private set; } = 4;
        [field: SerializeField, BoxGroup("Genome")] public RaceGenome Genome { get; private set; } = new();

        [SerializeField, BoxGroup("Origins"), InfoBox("Случайные обстоятельства, из которых народ может родиться сам, без бога.")]
        private List<RaceOrigin> _origins = new();

        [SerializeField, BoxGroup("Temperament"), InfoBox("Что народ оставляет в памяти мира на своих землях каждый период.")]
        private List<MemoryImprint> _imprints = new();
        [SerializeField, BoxGroup("Temperament"), InfoBox("Что народ умеет. Раз в период его разум выбирает одно действие: по нраву, привычке, подражанию, замыслу или на авось — и учится на результате.")]
        private List<RaceAction> _actions = new();
        [SerializeField, BoxGroup("Relations"), AssetsOnly] private List<RaceDefinition> _rivals = new();
        [SerializeField, BoxGroup("Relations"), AssetsOnly] private List<RaceDefinition> _friends = new();

        public IReadOnlyList<RaceOrigin> Origins => _origins;
        public IReadOnlyList<MemoryImprint> Imprints => _imprints;
        public IReadOnlyList<RaceAction> Actions => _actions;
        public IReadOnlyList<RaceDefinition> Rivals => _rivals;
        public IReadOnlyList<RaceDefinition> Friends => _friends;
    }
}
