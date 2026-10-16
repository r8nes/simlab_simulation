using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godland.Game.Presentation.Settings;
using Godland.Game.Simulation;
using Godland.Game.SimLab;
using UnityEditor;

namespace Godland.Editor.SimLab
{
    public static class SimLabSelfTest
    {
        public static void Run()
        {
            var source = AssetDatabase.LoadAssetAtPath<GameSettings>("Assets/Godland/Configs/GameSettings.asset");
            var clone = ConfigEditor.Clone(source);
            var sections = ConfigEditor.Sections(clone);
            var field = sections.SelectMany(section => section.Fields).First(candidate => candidate.Label == "Шанс сделки, %");
            var changed = field.TrySet("100");
            var lines = 0;
            var session = new SimulationSession(clone, new WriterBot(), 1, 1500, null, (_, _) => lines++);
            while (!session.IsDone) session.Step();
            var stats = session.Finish();
            var originalUntouched = ConfigEditor.Sections(source).SelectMany(section => section.Fields).First(candidate => candidate.Label == "Шанс сделки, %").Value;
            File.WriteAllText("Logs/simlab-selftest.txt",
                $"sections {sections.Count}, fields {sections.Sum(section => section.Fields.Count)}, set {changed} -> {field.Value}, original {originalUntouched}, events {lines}, counters {stats.Counters.Count}, deals {stats.Counters.GetValueOrDefault("Сделка сил")}");
        }
    }
}
