using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace OneLittleStep
{
    /// <summary>Configures the main camera for crisp integer-scaled pixel art.</summary>
    public static class CameraRig
    {
        public static readonly Color ClearColor = Theme.Hex("86DCF2");

        public static Camera Ensure()
        {
            var cam = Camera.main;
            if (!cam)
            {
                var go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                cam = go.AddComponent<Camera>();
            }
            Configure(cam);
            return cam;
        }

        public static void Configure(Camera cam)
        {
            cam.orthographic = true;
            cam.orthographicSize = PixelGrid.SceneHeight / 2f / PixelGrid.PixelsPerUnit;
            cam.transform.position = PixelGrid.SceneCenter;
            cam.transform.rotation = Quaternion.identity;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = ClearColor;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 50f;

            var ppc = cam.GetComponent<PixelPerfectCamera>();
            if (!ppc) ppc = cam.gameObject.AddComponent<PixelPerfectCamera>();
            ppc.assetsPPU = PixelGrid.PixelsPerUnit;
            ppc.refResolutionX = PixelGrid.SceneWidth;
            ppc.refResolutionY = PixelGrid.SceneHeight;
            ppc.gridSnapping = PixelPerfectCamera.GridSnapping.PixelSnapping;
            ppc.cropFrame = PixelPerfectCamera.CropFrame.None;
        }
    }
}
