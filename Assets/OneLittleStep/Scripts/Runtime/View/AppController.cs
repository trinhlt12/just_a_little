using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace OneLittleStep
{
    public enum ScreenId { Intro, Home, TaskForm, Duration, Session, Reflection, AfterDone, AfterLittle, AfterDistracted, Break, TaskComplete, Decor }

    public enum OverlayId { None, Note, Settings, NotesList }

    /// <summary>
    /// Screen flow and presentation. Rules live in <see cref="GameModel"/>; this class only reacts to the player
    /// and refreshes what is shown.
    /// </summary>
    public sealed partial class AppController : MonoBehaviour
    {
        public static readonly int[] DurationMinutes = { 2, 5, 10, 25 };
        public const int QuickTestSeconds = 10;

        GameModel model;
        RoomView room;
        RectTransform root;

        readonly Dictionary<ScreenId, RectTransform> screens = new Dictionary<ScreenId, RectTransform>();
        readonly Dictionary<ScreenId, Action> refreshers = new Dictionary<ScreenId, Action>();
        readonly Dictionary<OverlayId, RectTransform> overlays = new Dictionary<OverlayId, RectTransform>();
        readonly Dictionary<OverlayId, Action> overlayRefreshers = new Dictionary<OverlayId, Action>();

        public ScreenId Current { get; private set; } = ScreenId.Home;
        public OverlayId CurrentOverlay { get; private set; } = OverlayId.None;
        public GameModel Model => model;

        int selectedSeconds = 5 * 60;
        ReflectionResult lastResult;
        string lastStep = "";

        CanvasGroup fading;
        float fadeT = 1f;
        const float FadeDuration = 0.15f;

        public bool ReduceMotion => model.Data.reduceMotion;

        public void Init(GameModel gameModel, RoomView roomView, RectTransform safeRoot)
        {
            model = gameModel;
            room = roomView;
            root = safeRoot;
            BuildAll();
            room.Cat.SetReduceMotion(ReduceMotion);
            RefreshRoom();
            model.Tick();
            RouteFromState();
        }

        /// <summary>Picks the right screen for the current saved state (first launch, running session, ...).</summary>
        public void RouteFromState()
        {
            CloseOverlay();
            if (!model.Data.introSeen) { Show(ScreenId.Intro); return; }
            switch (model.Timer.Phase)
            {
                case SessionPhase.Running:
                case SessionPhase.Paused: Show(ScreenId.Session); break;
                case SessionPhase.AwaitingReflection: Show(ScreenId.Reflection); break;
                default: Show(ScreenId.Home); break;
            }
        }

        public void Show(ScreenId id)
        {
            Current = id;
            foreach (var kv in screens) kv.Value.gameObject.SetActive(kv.Key == id);
            if (refreshers.TryGetValue(id, out var r)) r();
            RefreshHud();
            room.Cat.SetLoop(id == ScreenId.Break ? CatView.Mood.Sleep : CatView.Mood.Idle);
            StartFade(screens[id]);
        }

        public void OpenOverlay(OverlayId id)
        {
            CurrentOverlay = id;
            foreach (var kv in overlays) kv.Value.gameObject.SetActive(kv.Key == id);
            if (overlayRefreshers.TryGetValue(id, out var r)) r();
            if (overlays.TryGetValue(id, out var o)) StartFade(o);
        }

        public void CloseOverlay()
        {
            CurrentOverlay = OverlayId.None;
            foreach (var kv in overlays) kv.Value.gameObject.SetActive(false);
        }

        void StartFade(RectTransform rt)
        {
            if (!rt.TryGetComponent(out fading)) fading = rt.gameObject.AddComponent<CanvasGroup>();
            fadeT = ReduceMotion ? 1f : 0f;
            fading.alpha = fadeT;
        }

        void Update()
        {
            if (model == null) return;
            if (model.Tick()) OnTimeUp();
            if (Current == ScreenId.Session) RefreshTimer();
            if (Current == ScreenId.Decor) UpdateDecorMarkers();
            if (fading != null && fadeT < 1f)
            {
                fadeT = Mathf.Min(1f, fadeT + Time.unscaledDeltaTime / FadeDuration);
                fading.alpha = fadeT;
            }
            UpdateToast();
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) OnBack();
        }

        /// <summary>Called by Bootstrap when the app returns from the background.</summary>
        public void OnResumedFromBackground()
        {
            if (model.Tick()) OnTimeUp();
            else if (Current == ScreenId.Session) RefreshTimer();
        }

        void OnTimeUp()
        {
            // The time ran out (possibly while the app was in the background). Ask once; never auto-reward.
            if (Current == ScreenId.Session) Show(ScreenId.Reflection);
        }

        void OnBack()
        {
            if (CurrentOverlay != OverlayId.None) { CloseOverlay(); return; }
            switch (Current)
            {
                case ScreenId.TaskForm:
                case ScreenId.Duration:
                case ScreenId.Decor:
                case ScreenId.Break:
                case ScreenId.TaskComplete:
                    LeaveTo(ScreenId.Home);
                    break;
            }
        }

        void LeaveTo(ScreenId id)
        {
            if (Current == ScreenId.Decor) ExitDecor();
            Show(id);
        }

        // ------------------------------------------------------------------ actions
        void SubmitTask(string title, string step)
        {
            if (!model.SetTask(title, step)) return;
            Show(ScreenId.Duration);
        }

        void BeginSession()
        {
            if (!model.StartSession(selectedSeconds)) return;
            Show(ScreenId.Session);
        }

        void TogglePause()
        {
            if (model.Timer.Phase == SessionPhase.Running) model.Pause();
            else if (model.Timer.Phase == SessionPhase.Paused) model.Resume();
            if (model.Timer.Phase == SessionPhase.AwaitingReflection) { Show(ScreenId.Reflection); return; }
            RefreshSession();
        }

        void FinishEarly()
        {
            if (model.EndEarly()) Show(ScreenId.Reflection);
        }

        void Choose(Reflection r)
        {
            lastStep = model.Timer.Data.step;
            var res = model.Reflect(r);
            if (!res.Accepted) { Show(ScreenId.Home); return; }
            lastResult = res;
            switch (r)
            {
                case Reflection.DoneStep: Show(ScreenId.AfterDone); break;
                case Reflection.DidALittle: Show(ScreenId.AfterLittle); break;
                case Reflection.Distracted: Show(ScreenId.AfterDistracted); break;
                default: Show(ScreenId.Break); break;
            }
            if (res.WoodAwarded > 0) CelebrateReward(res.WoodAwarded);
        }

        void CelebrateReward(int wood)
        {
            room.Cat.PlayHappy();
            ShowToast(Texts.WoodGained, rise: true);
            RefreshHud();
        }

        void SetReduceMotion(bool on)
        {
            model.SetReduceMotion(on);
            room.Cat.SetReduceMotion(on);
        }

        void RefreshRoom()
        {
            room.ShowPlacements(ItemInSlot);
        }

        string ItemInSlot(string slot)
        {
            foreach (var p in model.Data.placements)
                if (p.slot == slot) return p.item;
            return null;
        }

        // ------------------------------------------------------------------ toast
        Text toastText;
        Image toastIcon;
        RectTransform toastRt;
        float toastTime;
        bool toastRise;
        const float ToastDuration = 1.6f;
        const float ToastY = 40f;

        void ShowToast(string message, bool rise)
        {
            toastText.text = message;
            toastIcon.gameObject.SetActive(rise);
            toastRt.gameObject.SetActive(true);
            toastTime = 0f;
            toastRise = rise && !ReduceMotion;
            toastRt.anchoredPosition = new Vector2(0, ToastY);
        }

        void UpdateToast()
        {
            if (toastRt == null || !toastRt.gameObject.activeSelf) return;
            toastTime += Time.unscaledDeltaTime;
            if (toastRise) toastRt.anchoredPosition = new Vector2(0, ToastY + Mathf.Min(toastTime, 0.4f) * 60f);
            if (toastTime >= ToastDuration) toastRt.gameObject.SetActive(false);
        }
    }
}
