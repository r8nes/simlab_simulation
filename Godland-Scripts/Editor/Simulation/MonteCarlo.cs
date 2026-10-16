using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Godland.Game.Presentation.Settings;
using UnityEditor;
using UnityEngine;

using Godland.Game.Simulation;

namespace Godland.Editor.Simulation
{
    public static class MonteCarlo
    {
        private const string SettingsPath = "Assets/Godland/Configs/GameSettings.asset";

        [MenuItem("Godland/Симуляция (Монте-Карло)")]
        public static void RunFromMenu()
        {
            var path = Run(30, 1500);
            EditorUtility.RevealInFinder(path);
        }

        public static string Run(int runs, int years) =>
            Run(runs, years, new ISimulationBot[] { new PassiveBot(), new BuilderBot(false), new BuilderBot(true) });

        public static string RunFlow(int runs, int years)
        {
            var settings = AssetDatabase.LoadAssetAtPath<GameSettings>(SettingsPath);
            var rules = settings.Progression.FlowRules;
            var report = new StringBuilder();
            report.AppendLine($"# Режиссёр потока — {DateTime.Now:yyyy-MM-dd HH:mm}");
            report.AppendLine();
            report.AppendLine($"{runs} забегов подряд × {years} лет, бот Писатель. Статистика всех забегов, затухание {rules.DecayPercent}%, +{rules.BoostPercent}% / −{rules.CutPercent}%.");
            report.AppendLine();
            report.AppendLine("| Режим | Похожесть на 6 прошлых забегов (Жаккар, меньше — уникальнее) | Похожесть на прошлый забег | Разных событий за серию | Событий за забег |");
            report.AppendLine("|---|---|---|---|---|");
            try
            {
                foreach (var directed in new[] { false, true })
                {
                    var traces = new List<IReadOnlyCollection<string>>();
                    var stats = new List<Godland.Core.Flow.FlowStat>();
                    var similarity = 0f;
                    var previousSimilarity = 0f;
                    for (var seed = 1; seed <= runs; seed++)
                    {
                        EditorUtility.DisplayProgressBar("Режиссёр потока", $"{(directed ? "с режиссёром" : "без")}: забег {seed}/{runs}", seed / (float)runs);
                        (Dictionary<string, int>, int)? bias = directed ? Godland.Core.Flow.FlowStatistics.Bias(stats, seed - 1, rules) : null;
                        var run = SimulationRun.Play(settings, new WriterBot(), seed, years, bias);
                        var keys = run.FlowKeys.Where(key => !key.StartsWith("player:") && !key.StartsWith("word:")).ToList();
                        var recent = traces.Skip(Math.Max(0, traces.Count - 6)).ToList();
                        if (recent.Count > 0)
                        {
                            similarity += recent.Average(previous => Godland.Core.Flow.FlowStatistics.Similarity(keys, previous));
                            previousSimilarity += Godland.Core.Flow.FlowStatistics.Similarity(keys, recent[^1]);
                        }

                        Godland.Core.Flow.FlowStatistics.Fold(stats, seed, run.FlowKeys, rules);
                        traces.Add(keys);
                    }

                    var distinct = traces.SelectMany(trace => trace).Distinct().Count();
                    var pairs = Math.Max(1, runs - 1);
                    report.AppendLine($"| {(directed ? "С режиссёром" : "Без режиссёра")} | {similarity / pairs:0.000} | {previousSimilarity / pairs:0.000} | {distinct} | {traces.Average(trace => trace.Count):0.0} |");
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            var path = $"Logs/Simulation/flow-{DateTime.Now:yyyyMMdd-HHmmss}.md";
            System.IO.File.WriteAllText(path, report.ToString());
            return System.IO.Path.GetFullPath(path);
        }

        public static string RunWriter(int runs, int years) => Run(runs, years, new ISimulationBot[] { new WriterBot() });

        public static void RunWriterBatch() => RunWriter(ArgOr("-runs", 25), ArgOr("-years", 3000));

        private static int ArgOr(string name, int fallback)
        {
            var args = System.Environment.GetCommandLineArgs();
            var index = System.Array.IndexOf(args, name);
            return index >= 0 && index + 1 < args.Length && int.TryParse(args[index + 1], out var value) ? value : fallback;
        }

        public static string Run(int runs, int years, ISimulationBot[] bots)
        {
            var settings = AssetDatabase.LoadAssetAtPath<GameSettings>(SettingsPath);
            var report = new StringBuilder();
            report.AppendLine($"# Симуляция Godland — {DateTime.Now:yyyy-MM-dd HH:mm}");
            report.AppendLine();
            report.AppendLine($"{runs} забегов × {years} лет на каждый профиль бота. Сиды 1…{runs}.");
            report.AppendLine();

            try
            {
                for (var b = 0; b < bots.Length; b++)
                {
                    var results = new List<RunStats>();
                    for (var seed = 1; seed <= runs; seed++)
                    {
                        EditorUtility.DisplayProgressBar("Симуляция", $"{bots[b].Name}: забег {seed}/{runs}", (b * runs + seed) / (float)(bots.Length * runs));
                        results.Add(SimulationRun.Play(settings, bots[b], seed, years));
                    }

                    Describe(report, bots[b].Name, results);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            var folder = Path.Combine(Application.dataPath, "..", "Logs", "Simulation");
            Directory.CreateDirectory(folder);
            var file = Path.Combine(folder, $"sim-{DateTime.Now:yyyyMMdd-HHmmss}.md");
            File.WriteAllText(file, report.ToString());
            return Path.GetFullPath(file);
        }

        private static void Describe(StringBuilder report, string bot, List<RunStats> results)
        {
            report.AppendLine($"## {bot}");
            report.AppendLine();
            report.AppendLine("### Состояние мира (медиана [10%…90%])");
            report.AppendLine();
            var years = results.SelectMany(run => run.Snapshots.Keys).Distinct().OrderBy(year => year).ToList();
            var metrics = results.SelectMany(run => run.Snapshots.Values.SelectMany(snapshot => snapshot.Keys)).Distinct().ToList();
            report.AppendLine("| Показатель | " + string.Join(" | ", years.Select(year => $"{year} г.")) + " |");
            report.AppendLine("|---|" + string.Concat(years.Select(_ => "---|")));
            foreach (var metric in metrics)
            {
                var cells = years.Select(year =>
                {
                    var values = results.Where(run => run.Snapshots.ContainsKey(year)).Select(run => run.Snapshots[year].GetValueOrDefault(metric)).ToList();
                    return $"{Percentile(values, 50)} [{Percentile(values, 10)}…{Percentile(values, 90)}]";
                });
                report.AppendLine($"| {metric} | " + string.Join(" | ", cells) + " |");
            }

            report.AppendLine();
            report.AppendLine("### События за забег");
            report.AppendLine();
            report.AppendLine("| Событие | В скольких забегах | В среднем за забег | Медиана первого года |");
            report.AppendLine("|---|---|---|---|");
            var keys = results.SelectMany(run => run.Counters.Keys).Distinct().OrderBy(key => key);
            foreach (var key in keys)
            {
                var happened = results.Where(run => run.Counters.ContainsKey(key)).ToList();
                var share = happened.Count * 100 / results.Count;
                var average = results.Average(run => run.Counters.GetValueOrDefault(key));
                var first = Percentile(happened.Select(run => run.FirstYear[key]).ToList(), 50);
                report.AppendLine($"| {key} | {share}% | {average:0.#} | {first} |");
            }

            var extinct = results.Count(run => run.Snapshots.Count > 0 && run.Snapshots[run.Snapshots.Keys.Max()].GetValueOrDefault("Поселений") == 0
                                               && run.Counters.ContainsKey("Основано поселение"));
            report.AppendLine();
            report.AppendLine($"Вымерли к концу (были жители, не осталось): {extinct * 100 / results.Count}% забегов.");
            report.AppendLine();
        }

        private static int Percentile(List<int> values, int percent)
        {
            if (values.Count == 0) return 0;
            var sorted = values.OrderBy(value => value).ToList();
            return sorted[Math.Min(sorted.Count - 1, sorted.Count * percent / 100)];
        }
    }
}
