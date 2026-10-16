using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

namespace Godland.Editor.SimLab
{
    public static class SimLabBuild
    {
        private const string Output = "Builds/SimLab";

        [MenuItem("Godland/SimLab/Собрать WebGL")]
        public static void BuildWebGL()
        {
            if (!File.Exists(SimLabSceneBuilder.ScenePath)) SimLabSceneBuilder.Build();

            var compression = PlayerSettings.WebGL.compressionFormat;
            var fallback = PlayerSettings.WebGL.decompressionFallback;
            var defaultApis = PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.WebGL);
            var apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.WebGL);
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.WebGL, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL, new[] { GraphicsDeviceType.WebGPU });
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false;
            var scene = SimLabDataCopy.CreateScene(SimLabSceneBuilder.ScenePath, SimLabSceneBuilder.SettingsPath);
            SimLabShaderStripper.Active = true;
            BuildReport report;
            try
            {
                report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { scene },
                    locationPathName = Output,
                    target = BuildTarget.WebGL,
                    options = BuildOptions.None
                });
            }
            finally
            {
                SimLabShaderStripper.Active = false;
                SimLabDataCopy.Delete();
                PlayerSettings.WebGL.compressionFormat = compression;
                PlayerSettings.WebGL.decompressionFallback = fallback;
                PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL, apis);
                PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.WebGL, defaultApis);
            }

            Debug.Log($"SimLab build: {report.summary.result}, {report.summary.totalSize / (1024 * 1024)} MB → {Path.GetFullPath(Output)}");
        }
    }
}
