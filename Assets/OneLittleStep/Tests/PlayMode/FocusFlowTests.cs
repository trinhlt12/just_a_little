using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static OneLittleStep.Tests.UiDriver;

namespace OneLittleStep.Tests
{
    /// <summary>
    /// End-to-end focus flows through the real UI with a controllable clock. Also captures screenshots
    /// (QA/screens) of every screen it passes.
    /// </summary>
    [Category("QA")]
    public class FocusFlowTests
    {
        static DateTime fakeNow;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            fakeNow = new DateTime(2026, 10, 14, 9, 0, 0, DateTimeKind.Utc);
            Bootstrap.ClockOverride = () => fakeNow;
            yield return QaCapture.LoadScene();
        }

        [TearDown]
        public void TearDown() => Bootstrap.ClockOverride = null;

        static IEnumerator Advance(double seconds)
        {
            fakeNow = fakeNow.AddSeconds(seconds);
            yield return null;
            yield return null;
        }

        [UnityTest]
        public IEnumerator CreateTask_RunSession_ReflectAndFinish()
        {
            Assert.AreEqual(ScreenId.Intro, App.Current);
            yield return QaCapture.Shot("01_intro");
            yield return Click(Texts.StartLittle);
            Assert.AreEqual(ScreenId.TaskForm, App.Current);

            Assert.IsFalse(IsInteractable(Texts.Continue), "blank fields cannot start");
            Type(Texts.TaskPlaceholder, "   ");
            yield return Click("Viết một dòng");       // chip fills the step field
            Assert.IsFalse(IsInteractable(Texts.Continue), "whitespace-only task is refused");
            Type(Texts.TaskPlaceholder, "Viết báo cáo quý cho nhóm thiết kế");
            yield return null;
            yield return QaCapture.Shot("02_task_form");
            yield return Click(Texts.Continue);
            Assert.AreEqual(ScreenId.Duration, App.Current);
            Assert.AreEqual("Viết một dòng", Model.Step);
            yield return QaCapture.Shot("03_duration");

            yield return Click(Texts.Begin);           // default 5 minutes
            Assert.AreEqual(ScreenId.Session, App.Current);
            Assert.AreEqual(300, Model.Timer.DurationSeconds);
            yield return Advance(61);
            yield return QaCapture.Shot("04_session");

            // A thought while the clock keeps running.
            yield return Click(Texts.SendThought);
            Assert.AreEqual(OverlayId.Note, App.CurrentOverlay);
            Type(Texts.NotePlaceholder, "Nhớ gửi email cho chị Lan");
            yield return QaCapture.Shot("05_note");
            yield return Advance(20);
            Assert.AreEqual(SessionPhase.Running, Model.Timer.Phase, "notes do not pause the timer");
            yield return Click(Texts.SaveNote);
            Assert.AreEqual(1, Model.Data.notes.Count);
            Assert.AreEqual(81, Model.Timer.ActiveSeconds, 1e-3);

            yield return Click(Texts.Pause);
            yield return Advance(500);
            Assert.AreEqual(81, Model.Timer.ActiveSeconds, 1e-3, "paused time is not counted");
            yield return QaCapture.Shot("06_paused");
            yield return Click(Texts.Resume);

            // Time runs out (e.g. while the phone was locked).
            yield return Advance(1000);
            App.OnResumedFromBackground();
            yield return null;
            Assert.AreEqual(ScreenId.Reflection, App.Current);
            Assert.AreEqual(0, Model.Data.wood, "no automatic reward");
            yield return QaCapture.Shot("07_reflection_time_up");

            yield return Click(Texts.OptDone);
            Assert.AreEqual(ScreenId.AfterDone, App.Current);
            Assert.AreEqual(1, Model.Data.wood);
            yield return QaCapture.Shot("08_after_done_reward");
            Assert.IsFalse(Model.Reflect(Reflection.DoneStep).Accepted, "cannot claim the same session again");
            Assert.AreEqual(1, Model.Data.wood);

            Type(Texts.StepPlaceholder, "Viết phần mở đầu");
            yield return Click(Texts.DoNextStep);
            Assert.AreEqual(ScreenId.Duration, App.Current);
            Assert.AreEqual("Viết phần mở đầu", Model.Step);

            // Very short session: still recorded, no wood, gentle explanation.
            yield return Click(Texts.Begin);
            yield return Advance(12);
            yield return Click(Texts.EndEarly);
            Assert.AreEqual(ScreenId.Reflection, App.Current);
            yield return QaCapture.Shot("09_reflection_short");
            yield return Click(Texts.OptLittle);
            Assert.AreEqual(ScreenId.AfterLittle, App.Current);
            Assert.AreEqual(1, Model.Data.wood);
            Assert.AreEqual(2, Model.Data.sessionsRecorded);
            yield return QaCapture.Shot("10_after_little_short");
            yield return Click(Texts.BackToRoom);

            // Distracted: edit the step and start again.
            Assert.AreEqual(ScreenId.Home, App.Current);
            yield return QaCapture.Shot("11_home_with_task");
            yield return Click(Texts.DoThisStep);
            yield return Click(Texts.Begin);
            yield return Advance(90);
            yield return Click(Texts.EndEarly);
            yield return Click(Texts.OptDistracted);
            Assert.AreEqual(ScreenId.AfterDistracted, App.Current);
            Assert.AreEqual(1, Model.Data.wood, "distraction costs nothing");
            yield return QaCapture.Shot("12_distracted");
            Type(Texts.StepPlaceholder, "Đọc lại một đoạn");
            yield return Click(Texts.Restart);
            Assert.AreEqual(ScreenId.Duration, App.Current);
            Assert.AreEqual("Đọc lại một đoạn", Model.Step);

            // Need a break: simple rest screen, no forced timer.
            yield return Click(Texts.Begin);
            yield return Advance(45);
            yield return Click(Texts.EndEarly);
            yield return Click(Texts.OptBreak);
            Assert.AreEqual(ScreenId.Break, App.Current);
            Assert.AreEqual(SessionPhase.Idle, Model.Timer.Phase);
            yield return QaCapture.Shot("13_break");
            yield return Click(Texts.BackFromBreak);

            // Finish the whole task.
            yield return Click(Texts.DoThisStep);
            yield return Click(Texts.Begin);
            yield return Advance(400);
            yield return Click(Texts.OptDone);
            Assert.AreEqual(2, Model.Data.wood);
            yield return Click(Texts.FinishTask);
            Assert.AreEqual(ScreenId.TaskComplete, App.Current);
            yield return QaCapture.Shot("14_task_complete");
            yield return Click(Texts.BackToRoom);
            Assert.IsFalse(Model.HasTask);
            yield return QaCapture.Shot("15_home_empty");

            yield return Click(Texts.StartLittle);
            yield return QaCapture.Shot("16_task_form_tall", 1080, 2400);
            yield return QaCapture.Shot("17_task_form_720", 720, 1280);
        }

        [UnityTest]
        public IEnumerator SettingsAndReducedMotion()
        {
            yield return Click(Texts.Skip);
            Assert.AreEqual(ScreenId.Home, App.Current);
            App.OpenOverlay(OverlayId.Settings);
            yield return null;
            yield return QaCapture.Shot("18_settings");
            yield return Click(Texts.ReduceMotionOff);
            Assert.IsTrue(Model.Data.reduceMotion);
            Assert.IsNotNull(FindButton(Texts.ReduceMotionOn));
            yield return Click(Texts.Close);
            Assert.AreEqual(OverlayId.None, App.CurrentOverlay);
        }
    }
}
