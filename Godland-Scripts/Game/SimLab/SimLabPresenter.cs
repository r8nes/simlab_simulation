using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Godland.Game.Presentation.Settings;
using Godland.Game.Simulation;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Godland.Game.SimLab
{
    public sealed class SimLabPresenter : IStartable, IDisposable
    {
        private const int VisibleLines = 400;
        private const int StepsPerFrame = 20;

        private static readonly string[] BotNames = { "писатель", "строитель", "чудотворец", "пассивный" };

        private static readonly Dictionary<string, string[]> Choices = new()
        {
            ["Бот"] = BotNames,
            ["События в лог"] = new[] { "да", "нет" }
        };

        private static int IndexOf(IReadOnlyList<string> options, string current) =>
            Math.Max(0, options.ToList().FindIndex(option => string.Equals(option, current, StringComparison.OrdinalIgnoreCase)));

        private readonly GameSettings _source;
        private readonly SimLabView _view;
        private readonly StringBuilder _full = new();
        private readonly Queue<string> _visible = new();
        private readonly Dictionary<string, string> _general = new()
        {
            ["Прогонов"] = "5",
            ["Лет"] = "3000",
            ["Первый сид"] = "1",
            ["Бот"] = "писатель",
            ["Снимок каждые, лет"] = "100",
            ["События в лог"] = "да"
        };

        private GameSettings _settings;
        private IReadOnlyList<ConfigSection> _sections;
        private CancellationTokenSource _cancel;

        public SimLabPresenter(GameSettings source, SimLabView view)
        {
            _source = source;
            _view = view;
        }

        public void Start()
        {
            _view.Bind();
            _view.StartClicked += OnStart;
            _view.StopClicked += OnStop;
            _view.ResetClicked += Rebuild;
            _view.DownloadClicked += OnDownload;
            Rebuild();
        }

        public void Dispose()
        {
            _cancel?.Cancel();
            _view.StartClicked -= OnStart;
            _view.StopClicked -= OnStop;
            _view.ResetClicked -= Rebuild;
            _view.DownloadClicked -= OnDownload;
        }

        private void Rebuild()
        {
            if (_settings != null) Object.Destroy(_settings);
            _settings = ConfigEditor.Clone(_source);
            _sections = ConfigEditor.Sections(_settings);
            _view.SetRunning(false);
            _view.ShowStatus("Настройте переменные и нажмите «Старт». Значения конфигов меняются только в этой лаборатории.");
            DrawFields();
        }

        private void DrawFields()
        {
            _view.ClearFields();
            _view.AddRow().ShowHeader("Прогон", () => { });
            foreach (var key in _general.Keys.ToList())
            {
                if (Choices.TryGetValue(key, out var options))
                    _view.AddRow().ShowChoice(key, options, IndexOf(options, _general[key]), index => _general[key] = options[index]);
                else
                    _view.AddRow().ShowField(key, _general[key], text => _general[key] = text.Trim());
            }

            foreach (var section in _sections)
            {
                _view.AddRow().ShowHeader($"{(section.Expanded ? "−" : "+")} {section.Title} ({section.Fields.Count})", () =>
                {
                    section.Expanded = !section.Expanded;
                    DrawFields();
                });
                if (!section.Expanded) continue;

                foreach (var field in section.Fields)
                {
                    if (field.Options is { } choices)
                    {
                        _view.AddRow().ShowChoice(field.Label, choices, IndexOf(choices, field.Value), index => field.TrySet(choices[index]));
                        continue;
                    }

                    var row = _view.AddRow();
                    row.ShowField(field.Label, field.Value, text =>
                    {
                        if (!field.TrySet(text)) row.SetValue(field.Value);
                    });
                }
            }
        }

        private int Number(string key, int fallback) => int.TryParse(_general[key], out var value) ? Math.Max(1, value) : fallback;

        private ISimulationBot Bot() => _general["Бот"].ToLowerInvariant() switch
        {
            "строитель" => new BuilderBot(false),
            "чудотворец" => new BuilderBot(true),
            "пассивный" => new PassiveBot(),
            _ => new WriterBot()
        };

        private void OnStart()
        {
            _cancel = new CancellationTokenSource();
            RunAsync(_cancel.Token).Forget();
        }

        private void OnStop() => _cancel?.Cancel();

        private void OnDownload()
        {
            var path = LogExport.Save($"simlab-{DateTime.Now:yyyyMMdd-HHmmss}.md", _full.ToString());
            _view.ShowStatus($"Лог сохранён: {path}");
        }

        private async UniTaskVoid RunAsync(CancellationToken cancel)
        {
            var runs = Number("Прогонов", 5);
            var years = Number("Лет", 3000);
            var firstSeed = Number("Первый сид", 1);
            var snapshotPeriod = Number("Снимок каждые, лет", 100);
            var verbose = _general["События в лог"] == "да";
            var results = new List<RunStats>();

            _full.Clear();
            _visible.Clear();
            _view.SetRunning(true);
            Write($"# SimLab — {DateTime.Now:yyyy-MM-dd HH:mm}");
            Write($"Прогонов {runs}, лет {years}, сиды {firstSeed}…{firstSeed + runs - 1}, бот {_general["Бот"]}");
            WriteChangedFields();

            for (var run = 0; run < runs && !cancel.IsCancellationRequested; run++)
            {
                var seed = firstSeed + run;
                Write($"\n## Прогон {run + 1}, сид {seed}");
                var session = new SimulationSession(_settings, Bot(), seed, years, null, verbose ? (year, text) => Write($"{year,5}  {text}") : null);
                while (!session.IsDone && !cancel.IsCancellationRequested)
                {
                    for (var i = 0; i < StepsPerFrame && !session.IsDone; i++)
                    {
                        session.Step();
                        if (session.Year % snapshotPeriod == 0) WriteSnapshot(session);
                    }

                    _view.ShowStatus($"Прогон {run + 1}/{runs} · год {session.Year}/{years}");
                    Flush();
                    await UniTask.Yield(PlayerLoopTiming.Update, cancel).SuppressCancellationThrow();
                }

                results.Add(session.Finish());
            }

            WriteSummary(results);
            Flush();
            _view.SetRunning(false);
            _view.ShowStatus(cancel.IsCancellationRequested ? "Остановлено" : $"Готово: {results.Count} прогонов");
        }

        private void WriteChangedFields()
        {
            var original = ConfigEditor.Sections(_source).ToDictionary(section => section.Title);
            foreach (var section in _sections)
            foreach (var field in section.Fields)
            {
                var before = original[section.Title].Fields.First(other => other.Label == field.Label).Value;
                if (before != field.Value) Write($"Изменено: {section.Title}.{field.Label} = {field.Value} (было {before})");
            }
        }

        private void WriteSnapshot(SimulationSession session)
        {
            var snapshot = SimulationRun.Snapshot(session.World);
            Write($"{session.Year,5}  СОСТОЯНИЕ: " + string.Join(", ", snapshot.Select(pair => $"{pair.Key} {pair.Value}")));
        }

        private void WriteSummary(IReadOnlyList<RunStats> results)
        {
            if (results.Count == 0) return;

            Write($"\n## Итог по {results.Count} прогонам");
            Write("| Событие | В скольких мирах | Среднее за прогон |");
            Write("|---|---|---|");
            foreach (var key in results.SelectMany(stats => stats.Counters.Keys).Distinct().OrderBy(key => key))
            {
                var worlds = results.Count(stats => stats.Counters.ContainsKey(key));
                var average = results.Sum(stats => stats.Counters.GetValueOrDefault(key)) / (float)results.Count;
                Write($"| {key} | {worlds * 100 / results.Count}% | {average:0.#} |");
            }
        }

        private void Write(string line)
        {
            _full.AppendLine(line);
            _visible.Enqueue(line);
            while (_visible.Count > VisibleLines) _visible.Dequeue();
        }

        private void Flush() => _view.ShowLog(string.Join("\n", _visible));
    }
}
