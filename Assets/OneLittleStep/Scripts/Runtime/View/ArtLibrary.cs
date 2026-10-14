using System.Collections.Generic;
using UnityEngine;

namespace OneLittleStep
{
    /// <summary>Loads generated sprites from Resources/OneLittleStepArt by file name.</summary>
    public static class ArtLibrary
    {
        public const string ResourceFolder = "OneLittleStepArt";
        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        public static Sprite Sprite(string name)
        {
            if (cache.TryGetValue(name, out var s) && s) return s;
            s = Resources.Load<Sprite>(ResourceFolder + "/" + name);
            cache[name] = s;
            return s;
        }

        public static void ClearCache() => cache.Clear();
    }

    /// <summary>World-space pixel grid shared by the art generator and the runtime room.</summary>
    public static class PixelGrid
    {
        public const int PixelsPerUnit = 16;
        public const int SceneWidth = 216;   // 9:16 design canvas
        public const int SceneHeight = 384;
        // Backdrop layers are larger than the design canvas so wider / taller phones never see an edge.
        public const int BackdropWidth = 288;
        public const int BackdropHeight = 576;
        public const int BackdropOffsetX = (BackdropWidth - SceneWidth) / 2;   // 36
        public const int BackdropOffsetY = (BackdropHeight - SceneHeight) / 2; // 96

        public static Vector3 ToWorld(int px, int py, float z = 0f) =>
            new Vector3(px / (float)PixelsPerUnit, py / (float)PixelsPerUnit, z);

        public static Vector3 SceneCenter => ToWorld(SceneWidth / 2, SceneHeight / 2, -10f);
    }
}
