using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OneLittleStep
{
    /// <summary>Builds every screen once at start-up; refreshers fill in the current data when a screen is shown.</summary>
    public sealed partial class AppController
    {
        const float Inset = 24f;
        const float SmallButton = 112f;

        // HUD
        RectTransform hud;
        Text hudTitle, woodText;

        void BuildAll()
        {
            BuildIntro();
            BuildHome();
            BuildTaskForm();
            BuildDuration();
            BuildSession();
            BuildReflection();
            BuildAfterDone();
            BuildAfterLittle();
            BuildAfterDistracted();
            BuildBreak();
            BuildTaskComplete();
            BuildDecor();
            BuildHud();
            BuildNoteOverlay();
            BuildSettingsOverlay();
            BuildNotesListOverlay();
            BuildToast();
            CloseOverlay();
        }

        // ------------------------------------------------------------------ building blocks
        RectTransform ScreenRoot(ScreenId id)
        {
            var rt = UIKit.Stretch(UIKit.Rect(root, "Screen " + id));
            rt.gameObject.AddComponent<CanvasGroup>();
            screens[id] = rt;
            return rt;
        }

        /// <summary>Bottom sheet that grows upwards to fit its content.</summary>
        static RectTransform Sheet(Transform parent, string name = "Sheet")
        {
            var img = UIKit.Panel(parent, name, Theme.PanelSoft);
            var rt = img.rectTransform;
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.offsetMin = new Vector2(Inset, Inset);
            rt.offsetMax = new Vector2(-Inset, Inset + 100);
            UIKit.Column(rt, 40, 20);
            var fit = rt.gameObject.AddComponent<ContentSizeFitter>();
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return rt;
        }

        static RectTransform TopCard(Transform parent, string name)
        {
            var img = UIKit.Panel(parent, name, Theme.PanelSoft);
            var rt = img.rectTransform;
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.offsetMin = new Vector2(Inset, -Inset - 100);
            rt.offsetMax = new Vector2(-Inset, -Inset);
            UIKit.Column(rt, 32, 8);
            var fit = rt.gameObject.AddComponent<ContentSizeFitter>();
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return rt;
        }

        static Text Title(Transform parent, string text) =>
            UIKit.FlowLabel(parent, text, UIKit.TitleSize, Theme.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);

        static Text Body(Transform parent, string text, int size = UIKit.BodySize) =>
            UIKit.FlowLabel(parent, text, size, Theme.Ink, TextAnchor.MiddleCenter);

        static Text Muted(Transform parent, string text, int size = UIKit.SmallSize) =>
            UIKit.FlowLabel(parent, text, size, Theme.InkMuted, TextAnchor.MiddleCenter);

        /// <summary>Row of suggestion chips; a chip only fills the field, the player can still edit it.</summary>
        static void Chips(Transform parent, InputField target)
        {
            var row = UIKit.Row(parent, "Chips", 14, 96);
            foreach (var chip in Texts.StepChips)
            {
                var text = chip;
                UIKit.Button(row.transform, chip, ButtonStyle.Chip, () => target.text = text, 96);
            }
        }

        static void Validate(Button b, params InputField[] fields)
        {
            bool ok = true;
            foreach (var f in fields) ok &= !TextRules.IsBlank(f.text);
            UIKit.SetInteractable(b, ok);
        }

        static void WatchFields(Button b, params InputField[] fields)
        {
            foreach (var f in fields) f.onValueChanged.AddListener(_ => Validate(b, fields));
            Validate(b, fields);
        }

        /// <summary>Line explaining what the last reflection earned (or why it did not, kindly).</summary>
        void SetRewardLine(Text t)
        {
            if (lastResult.WoodAwarded > 0) { t.text = $"Bạn nhận được {lastResult.WoodAwarded} Gỗ."; t.gameObject.SetActive(true); }
            else if (lastResult.TooShortForWood) { t.text = Texts.ShortSession; t.gameObject.SetActive(true); }
            else t.gameObject.SetActive(false);
        }

        // ------------------------------------------------------------------ HUD
        void BuildHud()
        {
            hud = UIKit.DockTop(UIKit.Rect(root, "HUD"), 136, Inset);
            var row = hud.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.spacing = 16;
            row.childControlWidth = true; row.childControlHeight = true;
            row.childForceExpandWidth = false; row.childForceExpandHeight = true;

            var titleCard = UIKit.Panel(hud, "Task", Theme.PanelSoft);
            UIKit.Size(titleCard.gameObject, -1, -1, 1);
            hudTitle = UIKit.Label(titleCard.transform, "", 36, Theme.Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            UIKit.Stretch(hudTitle.rectTransform, 32, 24, 10, 10);
            hudTitle.resizeTextForBestFit = true;
            hudTitle.resizeTextMinSize = 26;
            hudTitle.resizeTextMaxSize = 36;

            var wood = UIKit.Panel(hud, "Wood", Theme.PanelSoft);
            UIKit.Size(wood.gameObject, -1, 230);
            var woodRow = wood.gameObject.AddComponent<HorizontalLayoutGroup>();
            woodRow.padding = new RectOffset(24, 24, 20, 20);
            woodRow.spacing = 12;
            woodRow.childAlignment = TextAnchor.MiddleCenter;
            woodRow.childControlWidth = true; woodRow.childControlHeight = true;
            woodRow.childForceExpandWidth = false; woodRow.childForceExpandHeight = false;
            UIKit.Icon(wood.transform, "icon_wood", 80);
            woodText = UIKit.Label(wood.transform, "0", 48, Theme.Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            UIKit.Size(woodText.gameObject, 80, 90);

            var gear = UIKit.IconButton(hud, "icon_gear", () => OpenOverlay(OverlayId.Settings), 136);
            UIKit.Size(gear.gameObject, -1, 136);
        }

        void RefreshHud()
        {
            if (hud == null) return;
            // Screens with their own top card use the sky area for it; the HUD shows on the others.
            bool hidden = Current != ScreenId.Home && Current != ScreenId.TaskForm && Current != ScreenId.Decor;
            hud.gameObject.SetActive(!hidden);
            hudTitle.text = model.HasTask ? model.TaskTitle : Texts.AppTitle;
            woodText.text = model.Data.wood.ToString();
        }

        // ------------------------------------------------------------------ intro
        void BuildIntro()
        {
            var screen = ScreenRoot(ScreenId.Intro);
            var top = TopCard(screen, "Intro");
            Title(top, Texts.AppTitle);
            Body(top, Texts.IntroBody, 36);
            var s = Sheet(screen);
            Muted(s, Texts.IntroNote, 28);
            UIKit.Button(s, Texts.StartLittle, ButtonStyle.Primary, () => { model.MarkIntroSeen(); OpenTaskForm(); });
            UIKit.Button(s, Texts.Skip, ButtonStyle.Ghost, () => { model.MarkIntroSeen(); Show(ScreenId.Home); }, SmallButton);
        }

        // ------------------------------------------------------------------ home
        void BuildHome()
        {
            var s = Sheet(ScreenRoot(ScreenId.Home));
            var empty = Body(s, Texts.HomeEmpty, 42);
            var label = Muted(s, Texts.NextStepLabel);
            var step = UIKit.FlowLabel(s, "", 44, Theme.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            var primary = UIKit.Button(s, Texts.StartLittle, ButtonStyle.Primary, null);
            var row = UIKit.Row(s, "Secondary", 14, SmallButton);
            UIKit.Button(row.transform, Texts.EditTask, ButtonStyle.Secondary, OpenTaskForm, SmallButton);
            var decorBtn = UIKit.Button(row.transform, Texts.Decorate, ButtonStyle.Secondary, EnterDecor, SmallButton);
            UIKit.Button(row.transform, Texts.Notes, ButtonStyle.Secondary, () => OpenOverlay(OverlayId.NotesList), SmallButton);

            refreshers[ScreenId.Home] = () =>
            {
                bool hasTask = model.HasTask;
                empty.gameObject.SetActive(!hasTask);
                label.gameObject.SetActive(hasTask);
                step.gameObject.SetActive(hasTask);
                step.text = model.HasStep ? model.Step : Texts.NoStepYet;
                UIKit.SetText(primary, !hasTask ? Texts.StartLittle : model.HasStep ? Texts.DoThisStep : Texts.ChooseStep);
                primary.onClick.RemoveAllListeners();
                primary.onClick.AddListener(() =>
                {
                    if (model.HasStep) Show(ScreenId.Duration);
                    else OpenTaskForm();
                });
                decorBtn.gameObject.SetActive(true);
            };
        }

        // ------------------------------------------------------------------ task form
        InputField taskField, stepField;

        void BuildTaskForm()
        {
            var s = Sheet(ScreenRoot(ScreenId.TaskForm));
            UIKit.FlowLabel(s, Texts.TaskLabel, UIKit.BodySize, Theme.Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            taskField = UIKit.Input(s, Texts.TaskPlaceholder, TextRules.TaskMax);
            UIKit.FlowLabel(s, Texts.NextStepLabel, UIKit.BodySize, Theme.Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            stepField = UIKit.Input(s, Texts.StepPlaceholder, TextRules.StepMax);
            Chips(s, stepField);
            Muted(s, Texts.StepHint, 30);
            var next = UIKit.Button(s, Texts.Continue, ButtonStyle.Primary, () => SubmitTask(taskField.text, stepField.text));
            UIKit.Button(s, Texts.Later, ButtonStyle.Ghost, () => Show(ScreenId.Home), SmallButton);
            WatchFields(next, taskField, stepField);
        }

        void OpenTaskForm()
        {
            taskField.text = model.HasTask ? model.TaskTitle : "";
            stepField.text = model.HasStep ? model.Step : "";
            Show(ScreenId.TaskForm);
        }

        // ------------------------------------------------------------------ duration
        void BuildDuration()
        {
            var screen = ScreenRoot(ScreenId.Duration);
            var top = TopCard(screen, "Step");
            Muted(top, Texts.StepToDo);
            var step = UIKit.FlowLabel(top, "", 46, Theme.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            var s = Sheet(screen);
            Muted(s, Texts.HowLong);
            var row = UIKit.Row(s, "Durations", 14, 112);
            var options = new List<(Button b, int seconds)>();
            foreach (var m in DurationMinutes)
            {
                int secs = m * 60;
                options.Add((UIKit.Button(row.transform, Texts.Minutes(m), ButtonStyle.Chip, null, 112), secs));
            }
            if (Debug.isDebugBuild)   // Editor / development builds only: never shown in release builds
                options.Add((UIKit.Button(s, Texts.QuickTest, ButtonStyle.Chip, null, 96), QuickTestSeconds));

            void Paint()
            {
                foreach (var (b, secs) in options)
                {
                    bool on = secs == selectedSeconds;
                    UIKit.ApplyStyle(b, on ? ButtonStyle.Primary : ButtonStyle.Chip);
                    if (on)
                    {
                        var cb = b.colors;
                        cb.normalColor = cb.highlightedColor = cb.selectedColor = Theme.ChipSelected;
                        b.colors = cb;
                    }
                }
            }
            foreach (var (b, secs) in options)
            {
                int v = secs;
                b.onClick.AddListener(() => { selectedSeconds = v; Paint(); });
            }
            UIKit.Button(s, Texts.Begin, ButtonStyle.Primary, BeginSession);
            UIKit.Button(s, Texts.Back, ButtonStyle.Ghost, () => Show(ScreenId.Home), SmallButton);
            refreshers[ScreenId.Duration] = () =>
            {
                step.text = model.Step;
                if (selectedSeconds <= 0) selectedSeconds = 5 * 60;
                Paint();
            };
        }

        // ------------------------------------------------------------------ session
        Text sessionStep, sessionClock, sessionPaused;
        Button pauseButton;
        Image pauseIcon;
        int shownSecond = -1;

        void BuildSession()
        {
            var screen = ScreenRoot(ScreenId.Session);
            var top = TopCard(screen, "Timer");
            Muted(top, Texts.Working);
            sessionStep = UIKit.FlowLabel(top, "", 42, Theme.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            sessionClock = UIKit.Label(top, "05:00", 150, Theme.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            UIKit.Size(sessionClock.gameObject, 170);
            sessionPaused = Muted(top, Texts.PausedLabel);

            var s = Sheet(screen);
            var row = UIKit.Row(s, "Controls", 16);
            pauseButton = UIKit.Button(row.transform, Texts.Pause, ButtonStyle.Secondary, TogglePause);
            pauseIcon = UIKit.ButtonIcon(pauseButton, "icon_pause");
            var note = UIKit.Button(row.transform, Texts.SendThought, ButtonStyle.Secondary, () => OpenOverlay(OverlayId.Note));
            UIKit.ButtonIcon(note, "icon_note");
            var end = UIKit.Button(s, Texts.EndEarly, ButtonStyle.Ghost, FinishEarly, SmallButton);
            UIKit.ButtonIcon(end, "icon_stop");
            refreshers[ScreenId.Session] = RefreshSession;
        }

        void RefreshSession()
        {
            sessionStep.text = model.Timer.Data.step;
            bool paused = model.Timer.Phase == SessionPhase.Paused;
            sessionPaused.gameObject.SetActive(paused);
            UIKit.SetText(pauseButton, paused ? Texts.Resume : Texts.Pause);
            pauseIcon.sprite = ArtLibrary.Sprite(paused ? "icon_play" : "icon_pause");
            shownSecond = -1;
            RefreshTimer();
        }

        void RefreshTimer()
        {
            int s = (int)System.Math.Ceiling(model.Timer.RemainingSeconds - 1e-6);
            if (s == shownSecond) return;
            shownSecond = s;
            sessionClock.text = Texts.Clock(model.Timer.RemainingSeconds);
        }

        // ------------------------------------------------------------------ reflection
        void BuildReflection()
        {
            var screen = ScreenRoot(ScreenId.Reflection);
            var top = TopCard(screen, "Summary");
            var title = Title(top, Texts.TimeUp);
            var stayed = Body(top, "", 36);
            var shortHint = Muted(top, Texts.ShortSession, 30);
            var s = Sheet(screen);
            Muted(s, Texts.HowWasIt, UIKit.BodySize);
            // Four equal choices in a 2x2 grid: no option is pushed as "the right one".
            var row1 = UIKit.Row(s, "Choices1", 16, 128);
            UIKit.Button(row1.transform, Texts.OptDone, ButtonStyle.Secondary, () => Choose(Reflection.DoneStep), 128);
            UIKit.Button(row1.transform, Texts.OptLittle, ButtonStyle.Secondary, () => Choose(Reflection.DidALittle), 128);
            var row2 = UIKit.Row(s, "Choices2", 16, 128);
            UIKit.Button(row2.transform, Texts.OptDistracted, ButtonStyle.Secondary, () => Choose(Reflection.Distracted), 128);
            UIKit.Button(row2.transform, Texts.OptBreak, ButtonStyle.Secondary, () => Choose(Reflection.NeedBreak), 128);
            refreshers[ScreenId.Reflection] = () =>
            {
                var d = model.Timer.Data;
                title.text = d.endedEarly ? Texts.StoppedEarly : Texts.TimeUp;
                stayed.text = Texts.StayedWith(d.bankedSeconds);
                shortHint.gameObject.SetActive(d.bankedSeconds < FocusTimer.MinRewardSeconds);
            };
        }

        // ------------------------------------------------------------------ after reflection
        void BuildAfterDone()
        {
            var screen = ScreenRoot(ScreenId.AfterDone);
            var top = TopCard(screen, "Done");
            Title(top, Texts.DoneTitle);
            var reward = Muted(top, "", 34);
            var s = Sheet(screen);
            UIKit.FlowLabel(s, Texts.NextStepLabel, UIKit.BodySize, Theme.Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            var next = UIKit.Input(s, Texts.StepPlaceholder, TextRules.StepMax);
            Chips(s, next);
            var go = UIKit.Button(s, Texts.DoNextStep, ButtonStyle.Primary, () =>
            {
                if (model.SetStep(next.text)) Show(ScreenId.Duration);
            });
            var row = UIKit.Row(s, "More", 16, SmallButton);
            UIKit.Button(row.transform, Texts.FinishTask, ButtonStyle.Secondary, () =>
            {
                completedTitle = model.TaskTitle;
                if (model.CompleteTask()) Show(ScreenId.TaskComplete);
            }, SmallButton);
            UIKit.Button(row.transform, Texts.Later, ButtonStyle.Ghost, () => { model.ClearStep(); Show(ScreenId.Home); }, SmallButton);
            WatchFields(go, next);
            refreshers[ScreenId.AfterDone] = () => { next.text = ""; SetRewardLine(reward); };
        }

        void BuildAfterLittle()
        {
            var screen = ScreenRoot(ScreenId.AfterLittle);
            var top = TopCard(screen, "Little");
            Title(top, Texts.LittleTitle);
            var body = Body(top, "", 36);
            var reward = Muted(top, "", 32);
            var s = Sheet(screen);
            UIKit.Button(s, Texts.BackToRoom, ButtonStyle.Primary, () => Show(ScreenId.Home));
            UIKit.Button(s, Texts.KeepGoing, ButtonStyle.Secondary, () => Show(ScreenId.Duration), SmallButton);
            refreshers[ScreenId.AfterLittle] = () => { body.text = Texts.LittleBody(lastStep); SetRewardLine(reward); };
        }

        void BuildAfterDistracted()
        {
            var screen = ScreenRoot(ScreenId.AfterDistracted);
            var top = TopCard(screen, "Distracted");
            Title(top, Texts.DistractedTitle);
            Body(top, Texts.DistractedBody, 36);
            var s = Sheet(screen);
            UIKit.FlowLabel(s, Texts.NextStepLabel, UIKit.BodySize, Theme.Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            var step = UIKit.Input(s, Texts.StepPlaceholder, TextRules.StepMax);
            Chips(s, step);
            var restart = UIKit.Button(s, Texts.Restart, ButtonStyle.Primary, () =>
            {
                if (model.SetStep(step.text)) Show(ScreenId.Duration);
            });
            UIKit.Button(s, Texts.BackToRoom, ButtonStyle.Ghost, () => { model.SetStep(step.text); Show(ScreenId.Home); }, SmallButton);
            WatchFields(restart, step);
            refreshers[ScreenId.AfterDistracted] = () => step.text = model.HasStep ? model.Step : lastStep;
        }

        void BuildBreak()
        {
            var screen = ScreenRoot(ScreenId.Break);
            var top = TopCard(screen, "Rest");
            UIKit.Icon(top, "icon_moon", 80);
            Title(top, Texts.BreakTitle);
            Body(top, Texts.BreakBody, 36);
            var s = Sheet(screen);
            UIKit.Button(s, Texts.BackFromBreak, ButtonStyle.Primary, () => Show(ScreenId.Home));
        }

        string completedTitle = "";

        void BuildTaskComplete()
        {
            var screen = ScreenRoot(ScreenId.TaskComplete);
            var top = TopCard(screen, "Complete");
            UIKit.Icon(top, "icon_check", 80);
            Title(top, Texts.TaskDoneTitle);
            var body = Body(top, "", 36);
            var s = Sheet(screen);
            UIKit.Button(s, Texts.BackToRoom, ButtonStyle.Primary, () => Show(ScreenId.Home));
            refreshers[ScreenId.TaskComplete] = () => body.text = Texts.TaskDoneBody(completedTitle);
        }

        // ------------------------------------------------------------------ overlays
        RectTransform OverlayRoot(OverlayId id)
        {
            var rt = UIKit.Stretch(UIKit.Rect(root, "Overlay " + id));
            var scrim = rt.gameObject.AddComponent<Image>();
            scrim.color = Theme.Scrim;
            rt.gameObject.AddComponent<CanvasGroup>();
            overlays[id] = rt;
            return rt;
        }

        void BuildNoteOverlay()
        {
            var s = Sheet(OverlayRoot(OverlayId.Note));
            Title(s, Texts.NoteTitle);
            var hint = Muted(s, Texts.NoteTimerRuns, 30);
            var field = UIKit.Input(s, Texts.NotePlaceholder, TextRules.NoteMax, true, 220);
            var save = UIKit.Button(s, Texts.SaveNote, ButtonStyle.Primary, null);
            save.onClick.AddListener(() =>
            {
                if (!model.AddNote(field.text)) return;
                CloseOverlay();
                ShowToast(Texts.NoteSaved, rise: false);
            });
            UIKit.Button(s, Texts.Close, ButtonStyle.Ghost, CloseOverlay, SmallButton);
            WatchFields(save, field);
            overlayRefreshers[OverlayId.Note] = () =>
            {
                field.text = "";
                hint.gameObject.SetActive(model.Timer.IsRunningOrPaused);
            };
        }

        void BuildSettingsOverlay()
        {
            var s = Sheet(OverlayRoot(OverlayId.Settings));
            Title(s, Texts.SettingsTitle);
            Button toggle = null;
            toggle = UIKit.Button(s, Texts.ReduceMotionOff, ButtonStyle.Secondary, () =>
            {
                SetReduceMotion(!ReduceMotion);
                UIKit.SetText(toggle, ReduceMotion ? Texts.ReduceMotionOn : Texts.ReduceMotionOff);
            });
            Muted(s, Texts.ReduceMotionHint, 30);
            UIKit.Button(s, Texts.Notes, ButtonStyle.Secondary, () => OpenOverlay(OverlayId.NotesList), SmallButton);
            Muted(s, Texts.IntroNote, 28);
            UIKit.Button(s, Texts.Close, ButtonStyle.Ghost, CloseOverlay, SmallButton);
            overlayRefreshers[OverlayId.Settings] = () => UIKit.SetText(toggle, ReduceMotion ? Texts.ReduceMotionOn : Texts.ReduceMotionOff);
        }

        void BuildNotesListOverlay()
        {
            var s = Sheet(OverlayRoot(OverlayId.NotesList));
            Title(s, Texts.NotesTitle);
            var lines = new List<Text>();
            for (int i = 0; i < 5; i++)
            {
                var t = UIKit.FlowLabel(s, "", 34, Theme.Ink, TextAnchor.MiddleLeft);
                lines.Add(t);
            }
            var empty = Muted(s, Texts.NotesEmpty);
            UIKit.Button(s, Texts.SendThought, ButtonStyle.Secondary, () => OpenOverlay(OverlayId.Note), SmallButton);
            UIKit.Button(s, Texts.Close, ButtonStyle.Ghost, CloseOverlay, SmallButton);
            overlayRefreshers[OverlayId.NotesList] = () =>
            {
                var notes = model.Data.notes;
                empty.gameObject.SetActive(notes.Count == 0);
                for (int i = 0; i < lines.Count; i++)
                {
                    int idx = notes.Count - 1 - i;
                    lines[i].gameObject.SetActive(idx >= 0);
                    if (idx >= 0) lines[i].text = "• " + notes[idx].text;
                }
            };
        }

        void BuildToast()
        {
            var panel = UIKit.Panel(root, "Toast", Theme.Panel);
            toastRt = panel.rectTransform;
            toastRt.anchorMin = toastRt.anchorMax = new Vector2(0.5f, 0.5f);   // over the room's back wall, clear of the cards
            toastRt.pivot = new Vector2(0.5f, 0.5f);
            toastRt.sizeDelta = new Vector2(900, 128);
            panel.raycastTarget = false;
            var row = panel.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.padding = new RectOffset(28, 28, 16, 16);
            row.spacing = 12;
            row.childAlignment = TextAnchor.MiddleCenter;
            row.childControlWidth = true; row.childControlHeight = true;
            row.childForceExpandWidth = false; row.childForceExpandHeight = false;
            toastIcon = UIKit.Icon(panel.transform, "icon_wood", 64);
            toastText = UIKit.Label(panel.transform, "", 38, Theme.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            toastText.resizeTextForBestFit = true;
            toastText.resizeTextMinSize = 24;
            toastText.resizeTextMaxSize = 38;
            UIKit.Size(toastText.gameObject, 96, 740);
            toastRt.gameObject.SetActive(false);
        }
    }
}
