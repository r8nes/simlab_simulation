using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Godland.Game.SimLab
{
    public static class LogExport
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void SimLabDownload(string name, string text);

        public static string Save(string name, string text)
        {
            SimLabDownload(name, text);
            return name;
        }
#else
        public static string Save(string name, string text)
        {
            var folder = Path.Combine(Application.dataPath, "..", "Logs", "SimLab");
            Directory.CreateDirectory(folder);
            var path = Path.GetFullPath(Path.Combine(folder, name));
            File.WriteAllText(path, text);
            return path;
        }
#endif
    }
}
