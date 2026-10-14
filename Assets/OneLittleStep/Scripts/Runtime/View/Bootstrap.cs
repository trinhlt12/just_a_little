using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace OneLittleStep
{
    /// <summary>
    /// Composition root of the prototype. The scene only contains a camera, a global 2D light and this
    /// component; everything else (room, cat, UI) is created from code so no inspector wiring is needed.
    /// </summary>
    public sealed class Bootstrap : MonoBehaviour
    {
        public Camera WorldCamera { get; private set; }
        public Canvas Canvas { get; private set; }
        public RectTransform SafeRoot { get; private set; }
        public RoomView Room { get; private set; }

        void Awake()
        {
            Application.targetFrameRate = 60;
            WorldCamera = CameraRig.Ensure();
            EnsureGlobalLight();
            EnsureEventSystem();
            Room = RoomView.Create(transform);
            BuildCanvas();
            BuildPlaceholder();
        }

        /// <summary>Sprites use the 2D renderer's lit material; one global light keeps colours exactly as painted.</summary>
        static void EnsureGlobalLight()
        {
            foreach (var l in FindObjectsByType<Light2D>(FindObjectsSortMode.None))
                if (l.lightType == Light2D.LightType.Global) return;
            var go = new GameObject("Global Light 2D");
            var light = go.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.intensity = 1f;
        }

        static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>()) return;
            var go = new GameObject("EventSystem", typeof(EventSystem));
            var module = go.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }

        void BuildCanvas()
        {
            var go = new GameObject("UI Canvas", typeof(RectTransform));
            go.layer = 5;
            go.transform.SetParent(transform, false);
            Canvas = go.AddComponent<Canvas>();
            Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Canvas.sortingOrder = 10;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0f; // portrait: keep widths stable, taller phones get more height
            go.AddComponent<GraphicRaycaster>();

            SafeRoot = UIKit.Rect(go.transform, "SafeArea");
            UIKit.Stretch(SafeRoot);
            SafeRoot.gameObject.AddComponent<SafeAreaFitter>();
        }

        void BuildPlaceholder()
        {
            var top = UIKit.Panel(SafeRoot, "Top", Theme.PanelSoft);
            UIKit.DockTop(top.rectTransform, 200, 24);
            var title = UIKit.Label(top.transform, "Một Chút Thôi", UIKit.TitleSize, Theme.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            UIKit.Stretch(title.rectTransform);

            var bottom = UIKit.Panel(SafeRoot, "Bottom", Theme.PanelSoft);
            UIKit.DockBottom(bottom.rectTransform, 260, 24);
            UIKit.Column(bottom.rectTransform, 40, 16, TextAnchor.MiddleCenter);
            UIKit.Button(bottom.transform, "Bắt đầu một chút", ButtonStyle.Primary, () => Debug.Log("[OneLittleStep] start"));
        }
    }
}
