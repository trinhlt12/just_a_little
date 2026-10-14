using UnityEngine;

namespace OneLittleStep
{
    /// <summary>Keeps a RectTransform inside Screen.safeArea (notches, camera cut-outs, gesture bars).</summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        /// <summary>Set by QA capture to render the full frame when there is no real screen.</summary>
        public static bool IgnoreSafeArea;

        /// <summary>QA hook: a normalised safe rect (e.g. to imitate a notch and gesture bar). Null on devices.</summary>
        public static Rect? SimulatedNormalized;

        Rect lastSafe;
        Vector2Int lastScreen;

        void OnEnable() => Apply(true);
        void Update() => Apply(SimulatedNormalized.HasValue);

        void Apply(bool force)
        {
            var safe = IgnoreSafeArea ? new Rect(0, 0, Screen.width, Screen.height) : Screen.safeArea;
            var screen = new Vector2Int(Screen.width, Screen.height);
            if (!force && safe == lastSafe && screen == lastScreen) return;
            lastSafe = safe;
            lastScreen = screen;
            if (screen.x <= 0 || screen.y <= 0) return;

            var rt = (RectTransform)transform;
            var min = new Vector2(safe.xMin / screen.x, safe.yMin / screen.y);
            var max = new Vector2(safe.xMax / screen.x, safe.yMax / screen.y);
            if (SimulatedNormalized.HasValue) { min = SimulatedNormalized.Value.min; max = SimulatedNormalized.Value.max; }
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
