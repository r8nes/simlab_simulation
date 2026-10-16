using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Life.Races
{
    [CreateAssetMenu(menuName = "Godland/Races Config", fileName = "RacesConfig")]
    public sealed class RacesConfig : ScriptableObject
    {
        [field: SerializeField, BoxGroup("Birth"), MinValue(1), SuffixLabel("years")] public int CheckPeriod { get; private set; } = 10;
        [field: SerializeField, BoxGroup("Birth"), MinValue(0), SuffixLabel("years"), InfoBox("Бог, проживший столько лет и построивший храмы, создаёт свой народ.")]
        public int MinGodAge { get; private set; } = 300;
        [field: SerializeField, BoxGroup("Birth"), MinValue(0)] public int MinTemples { get; private set; } = 3;
        [field: SerializeField, BoxGroup("Temperament"), MinValue(1), SuffixLabel("years")] public int DeedPeriod { get; private set; } = 50;
        [field: SerializeField, BoxGroup("Genome"), MinValue(0), InfoBox("На сколько может сдвинуться один ген за период деяний и при каждом расселении.")] public int MutationStep { get; private set; } = 6;
        [field: SerializeField, BoxGroup("Genome"), Range(0, 100), SuffixLabel("%"), InfoBox("Сколько генома победителя перенимает побеждённый.")] public int BlendPercent { get; private set; } = 30;
        [field: SerializeField, BoxGroup("Genome"), MinValue(1), InfoBox("Когда геном уходит так далеко от исходного, часть народа откалывается в новый народ.")] public int SplitDistance { get; private set; } = 70;
        [field: SerializeField, BoxGroup("Genome"), MinValue(2)] public int SplitMinCells { get; private set; } = 4;

        [field: SerializeField, BoxGroup("Wonders"), MinValue(1), InfoBox("Столько живых народов богов открывают Эпоху чудес.")]
        public int WonderRaces { get; private set; } = 3;
        [field: SerializeField, BoxGroup("Wonders"), MinValue(1), SuffixLabel("years")] public int WonderYears { get; private set; } = 400;
        [field: SerializeField, BoxGroup("Wonders"), MinValue(100), SuffixLabel("%")] public int EmergenceChancePercent { get; private set; } = 200;
        [field: SerializeField, BoxGroup("Wonders"), MinValue(1), SuffixLabel("years")] public int RelationPeriod { get; private set; } = 25;
        [field: SerializeField, BoxGroup("Wonders"), MinValue(1)] public int RelationRadius { get; private set; } = 3;
        [field: SerializeField, BoxGroup("Wonders"), MinValue(1), SuffixLabel("years"), InfoBox("Раз в период — чудо эпохи (аномалия).")]
        public int MiraclePeriod { get; private set; } = 100;
        [field: SerializeField, BoxGroup("Wonders"), Range(0, 100), SuffixLabel("%"), InfoBox("Эпоха кончается раньше, если один народ владеет такой долей всех поселений.")]
        public int DominancePercent { get; private set; } = 50;

        [SerializeField, AssetsOnly] private List<RaceDefinition> _races = new();

        public IReadOnlyList<RaceDefinition> Races => _races;
    }
}
