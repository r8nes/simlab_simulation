using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Writer.Threads
{
    [CreateAssetMenu(menuName = "Godland/Writer Config", fileName = "WriterConfig")]
    public sealed class WriterConfig : ScriptableObject
    {
        [field: SerializeField, BoxGroup("Толкования"), MinValue(0), InfoBox("Поселения в этом радиусе видят вмешательство писателя и толкуют его по своей вере.")]
        public int WitnessRadius { get; private set; } = 2;

        [field: SerializeField, BoxGroup("Толкования"), MinValue(1), SuffixLabel("years")] public int SignPeriod { get; private set; } = 50;
        [field: SerializeField, BoxGroup("Толкования"), MinValue(1)] public int SignThreshold { get; private set; } = 8;

        [field: SerializeField, BoxGroup("Невидимые нити"), MinValue(1), InfoBox("Место, которому писатель помогает слишком часто, однажды выставит счёт: тиран или культ против писателя.")]
        public int DebtThreshold { get; private set; } = 25;

        [field: SerializeField, BoxGroup("Невидимые нити"), MinValue(1), SuffixLabel("years")] public int DebtPeriod { get; private set; } = 100;
        [field: SerializeField, BoxGroup("Невидимые нити"), Range(0, 100), SuffixLabel("%")] public int TyrantPercent { get; private set; } = 50;

        [field: SerializeField, BoxGroup("Пророки"), MinValue(1)] public int ProphetThreshold { get; private set; } = 15;
        [field: SerializeField, BoxGroup("Пророки"), MinValue(0), SuffixLabel("years")] public int ProphetGapYears { get; private set; } = 250;
        [field: SerializeField, BoxGroup("Пророки"), MinValue(1), SuffixLabel("years")] public int ProphetPeriod { get; private set; } = 40;
        [field: SerializeField, BoxGroup("Пророки"), MinValue(1), SuffixLabel("years")] public int ProphetLifeYears { get; private set; } = 160;
        [field: SerializeField, BoxGroup("Пророки"), Range(0, 100), SuffixLabel("%")] public int ProphetLiePercent { get; private set; } = 25;
        [field: SerializeField, BoxGroup("Пророки"), Range(0, 100), SuffixLabel("%")] public int ProphetDiscountPercent { get; private set; } = 20;
        [field: SerializeField, BoxGroup("Пророки")] public List<string> ProphetNames { get; private set; } = new() { "Ионах", "Сафира", "Элиф", "Маргаст", "Тиру", "Аннеле", "Ведислав", "Оро" };

        [field: SerializeField, BoxGroup("Новая заповедь"), MinValue(0), InfoBox("Переписать заповедь посреди забега: все веры остывают, пантеоны гневаются, часть верующих хранит старый завет.")]
        public int RewriteCool { get; private set; } = 40;

        [field: SerializeField, BoxGroup("Новая заповедь"), Range(0, 100), SuffixLabel("%")] public int OldCovenantPercent { get; private set; } = 50;

        [field: SerializeField, BoxGroup("Пути писателя"), InfoBox("Опция: стиль игры копится (Творец / Режиссёр / Наблюдатель) и даёт свои бонусы.")]
        public bool PathsEnabled { get; private set; } = true;

        [field: SerializeField, BoxGroup("Пути писателя"), MinValue(1), SuffixLabel("years")] public int PathPeriod { get; private set; } = 100;
        [field: SerializeField, BoxGroup("Пути писателя"), MinValue(1), InfoBox("Приказ или чудо весит как столько тайлов: Режиссёр — когда приказов × вес ≥ тайлов.")] public int DirectorWeight { get; private set; } = 4;
        [field: SerializeField, BoxGroup("Пути писателя"), MinValue(0)] public int ObserverMaxTouches { get; private set; } = 2;
        [field: SerializeField, BoxGroup("Пути писателя"), MinValue(1), SuffixLabel("years")] public int CreatorPulsePeriod { get; private set; } = 20;
        [field: SerializeField, BoxGroup("Пути писателя"), MinValue(1)] public int CreatorPulseCells { get; private set; } = 3;
        [field: SerializeField, BoxGroup("Пути писателя"), Range(0, 100), SuffixLabel("%")] public int DirectorDiscountPercent { get; private set; } = 25;
        [field: SerializeField, BoxGroup("Пути писателя"), MinValue(100), SuffixLabel("%")] public int ObserverEmergencePercent { get; private set; } = 150;
    }
}
