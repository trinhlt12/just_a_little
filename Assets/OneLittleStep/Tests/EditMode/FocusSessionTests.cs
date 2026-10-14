using System;
using NUnit.Framework;
using UnityEngine;

namespace OneLittleStep.Tests
{
    /// <summary>Timer, background time and the one-time reward, driven by a fake wall clock.</summary>
    public class FocusSessionTests
    {
        DateTime now;
        GameModel model;

        [SetUp]
        public void SetUp()
        {
            now = new DateTime(2026, 10, 14, 9, 0, 0, DateTimeKind.Utc);
            model = new GameModel(new SaveData(), () => now);
            Assert.IsTrue(model.SetTask("Viết báo cáo", "Mở tài liệu"));
        }

        void Advance(double seconds) => now = now.AddSeconds(seconds);

        [Test]
        public void OnlyActiveTimeCounts_PauseIsExcluded()
        {
            Assert.IsTrue(model.StartSession(300));
            Advance(60);
            Assert.IsTrue(model.Pause());
            Advance(600);                       // a long pause must not count
            Assert.AreEqual(60, model.Timer.ActiveSeconds, 1e-6);
            Assert.IsTrue(model.Resume());
            Advance(30);
            Assert.AreEqual(90, model.Timer.ActiveSeconds, 1e-6);
            Assert.AreEqual(210, model.Timer.RemainingSeconds, 1e-6);
        }

        [Test]
        public void TimeKeepsRunningInBackground_FinishesOnceWithoutReward()
        {
            model.StartSession(300);
            Advance(3600);                      // the app was in the background for an hour
            Assert.IsTrue(model.Tick(), "first tick after returning ends the session");
            Assert.IsFalse(model.Tick(), "the transition happens only once");
            Assert.AreEqual(SessionPhase.AwaitingReflection, model.Timer.Phase);
            Assert.AreEqual(300, model.Timer.Data.bankedSeconds, 1e-6, "active time is capped at the duration");
            Assert.AreEqual(0, model.Data.wood, "finishing never grants wood by itself");
            Assert.AreEqual(0, model.Timer.RemainingSeconds);
        }

        [Test]
        public void ClockGoingBackwardsNeverGivesNegativeTime()
        {
            model.StartSession(120);
            Advance(-500);
            Assert.GreaterOrEqual(model.Timer.ActiveSeconds, 0);
            Assert.LessOrEqual(model.Timer.RemainingSeconds, 120);
            Assert.IsFalse(model.Tick());
        }

        [Test]
        public void ProductiveReflectionGrantsWoodExactlyOnce()
        {
            model.StartSession(300);
            Advance(45);
            Assert.IsTrue(model.EndEarly());
            var first = model.Reflect(Reflection.DoneStep);
            Assert.IsTrue(first.Accepted);
            Assert.AreEqual(1, first.WoodAwarded);
            var second = model.Reflect(Reflection.DoneStep);
            Assert.IsFalse(second.Accepted, "a session can only be reflected on once");
            Assert.AreEqual(1, model.Data.wood);
            Assert.IsTrue(model.Timer.Data.rewardGranted);
        }

        [Test]
        public void ShortSessionIsRecordedButEarnsNoWood()
        {
            model.StartSession(300);
            Advance(29);
            model.EndEarly();
            var r = model.Reflect(Reflection.DidALittle);
            Assert.IsTrue(r.Accepted);
            Assert.AreEqual(0, r.WoodAwarded);
            Assert.IsTrue(r.TooShortForWood);
            Assert.AreEqual(1, model.Data.sessionsRecorded);
            Assert.AreEqual(0, model.Data.wood);
        }

        [Test]
        public void ExactlyThirtySecondsEarnsWood()
        {
            model.StartSession(300);
            Advance(30);
            model.EndEarly();
            Assert.AreEqual(1, model.Reflect(Reflection.DidALittle).WoodAwarded);
        }

        [TestCase(Reflection.Distracted)]
        [TestCase(Reflection.NeedBreak)]
        public void RestfulAnswersEarnNothingAndLoseNothing(Reflection answer)
        {
            model.Data.wood = 3;
            model.StartSession(300);
            Advance(200);
            model.EndEarly();
            var r = model.Reflect(answer);
            Assert.IsTrue(r.Accepted);
            Assert.AreEqual(0, r.WoodAwarded);
            Assert.IsFalse(r.TooShortForWood);
            Assert.AreEqual(3, model.Data.wood);
            Assert.IsTrue(model.HasStep, "the step is kept");
        }

        [Test]
        public void SecondTimerCannotStartWhileOneExists()
        {
            Assert.IsTrue(model.StartSession(300));
            var id = model.Timer.Data.id;
            Assert.IsFalse(model.StartSession(120));
            model.Pause();
            Assert.IsFalse(model.StartSession(120));
            model.EndEarly();
            Assert.IsFalse(model.StartSession(120), "must reflect first");
            Assert.AreEqual(id, model.Timer.Data.id);
        }

        [Test]
        public void PausedSessionPastItsDurationDoesNotFinishWhilePaused()
        {
            model.StartSession(60);
            Advance(50);
            model.Pause();
            Advance(1000);
            Assert.IsFalse(model.Tick());
            Assert.AreEqual(SessionPhase.Paused, model.Timer.Phase);
            model.Resume();
            Advance(10);
            Assert.IsTrue(model.Tick());
        }

        [Test]
        public void RunningSessionSurvivesSaveAndReload()
        {
            model.StartSession(300);
            Advance(100);
            var json = JsonUtility.ToJson(model.Data);
            Advance(50);                        // app closed for 50 s
            var reloaded = new GameModel(JsonUtility.FromJson<SaveData>(json), () => now);
            Assert.AreEqual(SessionPhase.Running, reloaded.Timer.Phase);
            Assert.AreEqual(150, reloaded.Timer.ActiveSeconds, 1e-3);
        }

        [Test]
        public void PausedSessionSurvivesSaveAndReload()
        {
            model.StartSession(300);
            Advance(40);
            model.Pause();
            var json = JsonUtility.ToJson(model.Data);
            Advance(5000);
            var reloaded = new GameModel(JsonUtility.FromJson<SaveData>(json), () => now);
            Assert.AreEqual(SessionPhase.Paused, reloaded.Timer.Phase);
            Assert.AreEqual(40, reloaded.Timer.ActiveSeconds, 1e-3);
        }

        [Test]
        public void RewardFlagSurvivesReloadSoItCannotBeClaimedTwice()
        {
            model.StartSession(60);
            Advance(61);
            model.Tick();
            var savedBeforeChoice = JsonUtility.ToJson(model.Data);
            model.Reflect(Reflection.DoneStep);
            var savedAfter = JsonUtility.ToJson(model.Data);

            var again = new GameModel(JsonUtility.FromJson<SaveData>(savedAfter), () => now);
            Assert.IsFalse(again.Reflect(Reflection.DoneStep).Accepted);
            Assert.AreEqual(1, again.Data.wood);

            var before = new GameModel(JsonUtility.FromJson<SaveData>(savedBeforeChoice), () => now);
            Assert.AreEqual(SessionPhase.AwaitingReflection, before.Timer.Phase, "an unanswered session waits for the player");
            Assert.AreEqual(0, before.Data.wood);
        }

        [Test]
        public void WhitespaceOnlyTaskOrStepIsRejected()
        {
            var fresh = new GameModel(new SaveData(), () => now);
            Assert.IsFalse(fresh.SetTask("   ", "Mở tài liệu"));
            Assert.IsFalse(fresh.SetTask("Dọn phòng", " \n\t "));
            Assert.IsFalse(fresh.HasTask);
            Assert.IsFalse(fresh.StartSession(120));
            Assert.IsTrue(fresh.SetTask("  Dọn   phòng ", " Cất một   món đồ "));
            Assert.AreEqual("Dọn phòng", fresh.TaskTitle);
            Assert.AreEqual("Cất một món đồ", fresh.Step);
        }

        [Test]
        public void LongTextIsCutToLimit()
        {
            Assert.IsTrue(model.SetTask(new string('a', 500), new string('b', 500)));
            Assert.AreEqual(TextRules.TaskMax, model.TaskTitle.Length);
            Assert.AreEqual(TextRules.StepMax, model.Step.Length);
        }

        [Test]
        public void CompletingTaskClearsItAndCounts()
        {
            Assert.IsTrue(model.CompleteTask());
            Assert.IsFalse(model.HasTask);
            Assert.AreEqual(1, model.Data.tasksCompleted);
        }

        [Test]
        public void NotesAreKeptWhileTheTimerRuns()
        {
            model.StartSession(300);
            Advance(10);
            Assert.IsTrue(model.AddNote("  gọi điện cho mẹ  "));
            Assert.IsFalse(model.AddNote("   "));
            Assert.AreEqual(SessionPhase.Running, model.Timer.Phase, "writing a note does not pause");
            Assert.AreEqual("gọi điện cho mẹ", model.Data.notes[0].text);
            Assert.AreEqual("Mở tài liệu", model.Data.notes[0].step);
        }
    }
}
