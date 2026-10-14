using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace OneLittleStep.EditorTools
{
    /// <summary>Rebuilds the prototype scene from scratch. Safe to run repeatedly: it overwrites the same file.</summary>
    public static class SceneBuilder
    {
        public const string ScenePath = "Assets/OneLittleStep/Scenes/OneLittleStep.unity";

        [MenuItem("One Little Step/Rebuild Scene", priority = 20)]
        public static void RebuildSceneMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            RebuildScene();
            EditorSceneManager.OpenScene(ScenePath);
        }

        public static void RebuildScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<UniversalAdditionalCameraData>();
            CameraRig.Configure(cam);

            var lightGo = new GameObject("Global Light 2D");
            var light = lightGo.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.intensity = 1f;
            light.color = Color.white;

            new GameObject("OneLittleStep").AddComponent<Bootstrap>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);
            Debug.Log("[OneLittleStep] Scene rebuilt at " + ScenePath);
        }

        /// <summary>Adds the scene once, keeping any scenes that are already listed.</summary>
        public static void AddSceneToBuildSettings(string path)
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(s => s.path == path)) return;
            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        [MenuItem("One Little Step/Apply Android Portrait Settings", priority = 40)]
        public static void ApplyProjectSettings()
        {
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            var android = NamedBuildTarget.Android;
            var id = PlayerSettings.GetApplicationIdentifier(android);
            if (string.IsNullOrEmpty(id) || id.StartsWith("com.DefaultCompany", System.StringComparison.Ordinal))
                PlayerSettings.SetApplicationIdentifier(android, "com.onelittlestep.prototype");
            AssetDatabase.SaveAssets();
            Debug.Log("[OneLittleStep] Android portrait settings applied.");
        }
    }
}
