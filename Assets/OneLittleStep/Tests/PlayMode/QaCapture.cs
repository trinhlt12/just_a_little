using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace OneLittleStep.Tests
{
    /// <summary>
    /// Renders the real scene (world camera + UI) into PNGs under &lt;project&gt;/QA/screens so layouts can be
    /// reviewed without a device. Run with: -runTests -testPlatform PlayMode -testCategory QA
    /// </summary>
    [Category("QA")]
    public class QaCapture
    {
        public static string Folder => Path.Combine(Directory.GetParent(Application.dataPath).FullName, "QA", "screens");

        /// <summary>Points the game at an empty temporary save folder so tests never touch real player data.</summary>
        public static string FreshSaveFolder()
        {
            var dir = Path.Combine(Path.GetTempPath(), "ols_qa_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            Bootstrap.SaveFolderOverride = dir;
            return dir;
        }

        public static IEnumerator LoadScene()
        {
            if (Bootstrap.SaveFolderOverride == null) FreshSaveFolder();
            SafeAreaFitter.IgnoreSafeArea = true;
            SceneManager.LoadScene("OneLittleStep");
            yield return null;
            yield return null;
        }

        /// <summary>Renders one frame at the given resolution, with the UI drawn by the world camera.</summary>
        public static IEnumerator Shot(string name, int width = 1080, int height = 1920)
        {
            var boot = Object.FindAnyObjectByType<Bootstrap>();
            var cam = boot.WorldCamera;
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var canvas = boot.Canvas;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 5f;
            canvas.sortingOrder = 1000;   // Overlay mode draws UI above sprites; keep that order in the capture
            cam.targetTexture = rt;
            yield return new WaitForSecondsRealtime(0.25f);   // let screen fades finish
            Canvas.ForceUpdateCanvases();
            cam.Render();
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;
            Directory.CreateDirectory(Folder);
            File.WriteAllBytes(Path.Combine(Folder, name + ".png"), tex.EncodeToPNG());
            cam.targetTexture = null;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            Object.Destroy(tex);
            rt.Release();
            Debug.Log("[OneLittleStep] QA shot " + name);
        }

        [UnityTest]
        public IEnumerator RoomRendersAtPortraitSizes()
        {
            yield return LoadScene();
            yield return Shot("room_1080x1920");
            yield return Shot("room_1080x2400", 1080, 2400);
            yield return Shot("room_720x1280", 720, 1280);
            Assert.IsNotNull(Object.FindAnyObjectByType<RoomView>());
        }
    }
}
