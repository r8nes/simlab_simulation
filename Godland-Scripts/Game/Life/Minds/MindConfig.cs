using Godland.Core.Life.Minds;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Life.Minds
{
    [CreateAssetMenu(menuName = "Godland/Mind Config", fileName = "MindConfig")]
    public sealed class MindConfig : ScriptableObject, IMindRules
    {
        [field: SerializeField, Range(1, 100), SuffixLabel("%"), InfoBox("Разум народов и правителей: насколько быстро опыт меняет привычки и доверие к способам думать (нрав, привычка, подражание, замысел, авось).")]
        public int LearningPercent { get; private set; } = 30;

        [field: SerializeField, MinValue(0), InfoBox("Случайный разброс в оценке действий.")]
        public int Noise { get; private set; } = 15;

        [field: SerializeField, InfoBox("Результат не выше этого числа — неудача: растёт отчаяние, разум чаще рискует и строит замыслы.")]
        public int DespairReward { get; private set; } = -1;

        [field: SerializeField, Range(0, 100), SuffixLabel("%"), InfoBox("Сколько опыта перенимает побеждённый или наследник.")]
        public int InheritPercent { get; private set; } = 30;

        [field: SerializeField, Range(0, 100), SuffixLabel("%"), InfoBox("Награда за непохожесть (quality-diversity): разум получает до 4 очков успеха, если поступил не так, как другие. Мешает всем сойтись к одной тактике.")]
        public int NoveltyPercent { get; private set; } = 50;
    }
}
