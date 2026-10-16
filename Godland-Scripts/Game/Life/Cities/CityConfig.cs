using System.Collections.Generic;
using System.Linq;
using Godland.Core.Life.Cities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Godland.Game.Life.Cities
{
    [CreateAssetMenu(menuName = "Godland/City Config", fileName = "CityConfig")]
    public sealed class CityConfig : ScriptableObject, ICityRules
    {
        [field: SerializeField, BoxGroup("Rise"), MinValue(0), InfoBox("Номер эпохи в EpochConfig, с которой деревни могут стать городами. 3 — Эпоха богов.")]
        public int CitiesFromEpoch { get; private set; } = 3;

        [field: SerializeField, BoxGroup("Rise"), MinValue(0), SuffixLabel("years"), InfoBox("Сколько лет после начала этой эпохи города ещё не появляются.")]
        public int CityDelayYears { get; private set; } = 100;

        [field: SerializeField, BoxGroup("Rise"), MinValue(1), SuffixLabel("years")]
        public int RisePeriod { get; private set; } = 10;

        [field: SerializeField, BoxGroup("Rise"), MinValue(1)]
        public int CityPopulation { get; private set; } = 6;

        [field: SerializeField, BoxGroup("Rise"), MinValue(0), InfoBox("Сколько соседних поселений нужно городу.")]
        public int CityMinNeighbors { get; private set; } = 2;

        [field: SerializeField, BoxGroup("Rise"), MinValue(0), SuffixLabel("years"), InfoBox("Сколько лет поселение должно прожить, чтобы стать городом.")]
        public int CityMinAge { get; private set; } = 50;

        [field: SerializeField, BoxGroup("Fall"), MinValue(1), InfoBox("Ниже этой численности город снова становится деревней. Город не пустеет от одного удара: он падает до 1 жителя.")]
        public int CityFallPopulation { get; private set; } = 3;

        [field: SerializeField, BoxGroup("Faith"), MinValue(1)]
        public int CityFaithMultiplier { get; private set; } = 3;

        [field: SerializeField, BoxGroup("Build"), MinValue(1), SuffixLabel("years"), InfoBox("Раз в период город застраивает один соседний тайл. Святыни и престолы богов не трогает.")]
        public int BuildPeriod { get; private set; } = 40;

        [SerializeField, BoxGroup("Build")]
        private List<CityBuild> _builds = new();

        public string BuildResult(string tileId) => _builds.FirstOrDefault(build => build.From.Id == tileId)?.To.Id;
    }
}
