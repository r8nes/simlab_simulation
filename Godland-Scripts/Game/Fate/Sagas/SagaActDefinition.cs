using System;
using System.Collections.Generic;
using Godland.Game.Writer.Actions;
using Godland.Game.Fate.Catastrophes;
using Godland.Game.World.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Fate.Sagas
{
    [CreateAssetMenu(menuName = "Godland/Saga Act", fileName = "Act")]
    public sealed class SagaActDefinition : ScriptableObject
    {
        [field: SerializeField, Required] public string Title { get; private set; }
        [field: SerializeField, TextArea, InfoBox("Тон акта: строка под названием и текст баннера.")] public string Text { get; private set; }

        [SerializeField, BoxGroup("Sides"), AssetsOnly, InfoBox("Стороны акта (одна или две). Порядок важен при расщеплении: первая — левая половина.")]
        private List<SagaFactionDefinition> _factions = new();
        [SerializeField, BoxGroup("Sides"), AssetsOnly, InfoBox("Если не пусто — первая сторона выбирается случайно из этих обличий (Архангел: Пастырь, Судия...).")]
        private List<SagaFactionDefinition> _firstFactionVariants = new();

        [field: SerializeField, BoxGroup("Split"), InfoBox("Мир делится на две половины сторон, между ними бездна. Игрок не строит на них.")] public bool Split { get; private set; }
        [field: SerializeField, BoxGroup("Split"), ShowIf(nameof(Split)), MinValue(1), SuffixLabel("years")] public int BridgeMinGap { get; private set; } = 60;
        [field: SerializeField, BoxGroup("Split"), ShowIf(nameof(Split)), MinValue(1), SuffixLabel("years")] public int BridgeMaxGap { get; private set; } = 120;
        [field: SerializeField, BoxGroup("Split"), ShowIf(nameof(Split)), MinValue(1), SuffixLabel("years")] public int BridgeYears { get; private set; } = 20;
        [field: SerializeField, BoxGroup("Split"), ShowIf(nameof(Split)), InfoBox("Мост открыт всегда (вечная война).")] public bool BridgeAlwaysOpen { get; private set; }

        [field: SerializeField, BoxGroup("Story"), InfoBox("Стороны ищут артефакт, который ломает баланс.")] public bool ArtifactHunt { get; private set; }
        [field: SerializeField, BoxGroup("Story"), InfoBox("Внутри сторон идут заговоры и перевороты.")] public bool Intrigues { get; private set; } = true;
        [field: SerializeField, BoxGroup("Story"), MinValue(0), InfoBox("Сколько клеток за краем мира занимает первая сторона (небесный предел).")] public int OutsideCells { get; private set; }

        [SerializeReference, BoxGroup("Laws")] private List<IAgeLaw> _laws = new();
        [SerializeField, BoxGroup("Laws"), AssetsOnly, InfoBox("Чудеса, которыми писатель толкает историю в этом акте.")] private List<TileActionDefinition> _playerActions = new();

        [SerializeField, BoxGroup("Transitions")] private List<SagaTransition> _transitions = new();

        public IReadOnlyList<SagaFactionDefinition> Factions => _factions;
        public IReadOnlyList<SagaFactionDefinition> FirstFactionVariants => _firstFactionVariants;
        public IReadOnlyList<IAgeLaw> Laws => _laws;
        public IReadOnlyList<TileActionDefinition> PlayerActions => _playerActions;
        public IReadOnlyList<SagaTransition> Transitions => _transitions;
    }

    [Serializable]
    public sealed class SagaTransition
    {
        [field: SerializeField, Required, InfoBox("Что случилось — для баннера и журнала.")] public string Text { get; private set; }
        [SerializeReference, Required] private ISagaCondition _condition;
        [field: SerializeField, Range(1, 100), SuffixLabel("%")] public int ChancePercent { get; private set; } = 30;
        [field: SerializeField, AssetsOnly, InfoBox("Следующий акт этой саги.")] public SagaActDefinition NextAct { get; private set; }
        [field: SerializeField, AssetsOnly, InfoBox("Или новая сага (превращение мира).")] public SagaDefinition NextSaga { get; private set; }
        [field: SerializeField, ShowIf(nameof(EndsSaga)), InfoBox("Что остаётся миру, когда сага кончилась (тайл для захваченных земель, пусто — оставить как есть).")]
        public TileDefinition Restore { get; private set; }

        public bool EndsSaga => NextAct == null && NextSaga == null;
        public ISagaCondition Condition => _condition;
    }
}
