using UnityEditor;
using UnityEngine;

namespace OneLittleStep.EditorTools
{
    /// <summary>Entry points for command-line runs: Unity -batchmode -executeMethod OneLittleStep.EditorTools.BatchTasks.X</summary>
    public static class BatchTasks
    {
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
    }
}
