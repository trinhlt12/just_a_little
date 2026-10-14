using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace OneLittleStep.EditorTools
{
    /// <summary>
    /// Menu items and command-line entry points:
    /// Unity -batchmode -projectPath &lt;project&gt; -executeMethod OneLittleStep.EditorTools.BatchTasks.X -quit
    /// </summary>
    public static class BatchTasks
    {
        public const string ApkPath = "Builds/Android/OneLittleStep.apk";

        public static void SetupProject()
        {
            SceneBuilder.ApplyProjectSettings();
            SceneBuilder.RebuildScene();
            AssetDatabase.SaveAssets();
            Debug.Log("[OneLittleStep] SetupProject done");
        }

        public static void GenerateArt()
        {
            Art.ArtGenerator.GenerateAll();
            AssetDatabase.SaveAssets();
        }

        [MenuItem("One Little Step/Build Android APK (release)", priority = 60)]
        public static void BuildAndroidMenu() => BuildAndroid(false);

        [MenuItem("One Little Step/Build Android APK (development, 10 s test timer)", priority = 61)]
        public static void BuildAndroidDevMenu() => BuildAndroid(true);

        /// <summary>Command line: -buildTarget Android -executeMethod ...BatchTasks.BuildAndroidRelease -quit</summary>
        public static void BuildAndroidRelease()
        {
            if (!BuildAndroid(false)) EditorApplication.Exit(1);
        }

        public static bool BuildAndroid(bool development)
        {
            SceneBuilder.ApplyProjectSettings();
            Directory.CreateDirectory(Path.GetDirectoryName(ApkPath));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { SceneBuilder.ScenePath },
                locationPathName = ApkPath,
                target = BuildTarget.Android,
                options = development ? BuildOptions.Development : BuildOptions.None,
            };
            EditorUserBuildSettings.buildAppBundle = false;
            var report = BuildPipeline.BuildPlayer(options);
            var s = report.summary;
            Debug.Log($"[OneLittleStep] Android build {s.result}: {s.outputPath} ({s.totalSize / (1024f * 1024f):0.0} MB, {s.totalErrors} errors, {s.totalTime})");
            return s.result == BuildResult.Succeeded;
        }
    }
}
