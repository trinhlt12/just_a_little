using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static OneLittleStep.Tests.UiDriver;

namespace OneLittleStep.Tests
{
    /// <summary>Unlock / place / replace / store through the UI, and data surviving an app restart.</summary>
    [Category("QA")]
    public class DecorAndPersistenceTests
    {
        static DateTime fakeNow;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            fakeNow = new DateTime(2026, 10, 14, 9, 0, 0, DateTimeKind.Utc);
            Bootstrap.ClockOverride = () => fakeNow;
            QaCapture.FreshSaveFolder();
            yield return null;
        }

        [TearDown]
        public void TearDown()
        {
            Bootstrap.ClockOverride = null;
            Bootstrap.SaveFolderOverride = null;
        }

        static void Seed(Action<SaveData> fill)
        {
            var data = new SaveData { introSeen = true };
            data.task = new TaskData { active = true, title = "Dọn góc học tập", step = "Cất một món đồ" };
            fill(data);
            new SaveStore(Bootstrap.SaveFolderOverride).Save(data);
        }

        static IEnumerator Restart()
        {
            yield return QaCapture.LoadScene();   // a new Bootstrap loads from the same save folder
        }

        [UnityTest]
        public IEnumerator UnlockPlaceReplaceStore_AndReload()
        {
            Seed(d => d.wood = 3);
            yield return Restart();
            Assert.AreEqual(ScreenId.Home, App.Current);
            yield return Click(Texts.Decorate);
            Assert.AreEqual(ScreenId.Decor, App.Current);
            yield return QaCapture.Shot("20_decor_open");

            yield return ClickNamed("Card desk");
            Assert.IsFalse(IsVisible("Marker desk"), "locked items cannot be placed");
            yield return Click(Texts.Unlock(2));
            Assert.AreEqual(1, Model.Data.wood);
            Assert.IsTrue(IsVisible("Marker desk"));
            yield return QaCapture.Shot("21_decor_slot_marker");
            yield return ClickNamed("Marker desk");
            Assert.AreEqual("desk", Model.Decor.ItemIn("desk"));

            yield return ClickNamed("Card chair");
            yield return Click(Texts.Unlock(1));
            Assert.AreEqual(0, Model.Data.wood);
            yield return ClickNamed("Marker chair");

            yield return ClickNamed("Card bed");
            Assert.IsFalse(IsInteractable(Texts.NeedMore(3)), "cannot buy without enough wood");
            Assert.AreEqual(0, Model.Data.wood);
            Assert.IsFalse(Model.Decor.IsUnlocked("bed"));
            yield return QaCapture.Shot("22_decor_need_more");

            yield return ClickNamed("Card chair");
            yield return Click(Texts.StoreItem);
            Assert.IsFalse(Model.Decor.IsPlaced("chair"));
            Assert.IsTrue(Model.Decor.IsUnlocked("chair"));
            yield return ClickNamed("Marker chair");
            Assert.IsTrue(Model.Decor.IsPlaced("chair"));
            yield return Click(Texts.DecorDone);
            Assert.AreEqual(ScreenId.Home, App.Current);

            yield return Restart();
            Assert.AreEqual("desk", Model.Decor.ItemIn("desk"));
            Assert.AreEqual("chair", Model.Decor.ItemIn("chair"));
            Assert.AreEqual(0, Model.Data.wood);
            Assert.AreEqual(2, Model.Data.unlocked.Count);
            yield return QaCapture.Shot("23_after_restart");
        }

        [UnityTest]
        public IEnumerator ReplacingSendsTheOldItemToStorage()
        {
            Seed(d =>
            {
                d.unlocked.AddRange(new[] { "painting", "clock" });
                d.placements.Add(new PlacementData { slot = "wall_mid", item = "painting" });
            });
            yield return Restart();
            yield return Click(Texts.Decorate);
            yield return ClickNamed("Card clock");
            yield return ClickNamed("Marker wall_mid");
            Assert.AreEqual("clock", Model.Decor.ItemIn("wall_mid"));
            Assert.IsFalse(Model.Decor.IsPlaced("painting"));
            Assert.IsTrue(Model.Decor.IsUnlocked("painting"));
        }

        [UnityTest]
        public IEnumerator FullyFurnishedRoom()
        {
            Seed(d =>
            {
                foreach (var f in RoomLayout.Furniture) d.unlocked.Add(f.Id);
                d.placements.Add(new PlacementData { slot = "cabinet_left", item = "bookshelf" });
                d.placements.Add(new PlacementData { slot = "bed", item = "bed" });
                d.placements.Add(new PlacementData { slot = "cabinet_mid", item = "drawer" });
                d.placements.Add(new PlacementData { slot = "desk", item = "desk" });
                d.placements.Add(new PlacementData { slot = "top_desk_l", item = "lamp" });
                d.placements.Add(new PlacementData { slot = "top_cab_mid", item = "smallplant" });
                d.placements.Add(new PlacementData { slot = "wall_bed", item = "painting" });
                d.placements.Add(new PlacementData { slot = "wall_mid", item = "clock" });
                d.placements.Add(new PlacementData { slot = "floor_left", item = "catbed" });
                d.placements.Add(new PlacementData { slot = "floor_mid", item = "rug" });
                d.placements.Add(new PlacementData { slot = "chair", item = "chair" });
                d.placements.Add(new PlacementData { slot = "bigplant", item = "bigplant" });
                d.wood = 7;
            });
            yield return Restart();
            Assert.AreEqual(12, Model.Data.placements.Count);
            yield return QaCapture.Shot("24_room_full");
            yield return QaCapture.Shot("25_room_full_tall", 1080, 2400);
            yield return Click(Texts.DoThisStep);
            yield return Click(Texts.Begin);
            yield return QaCapture.Shot("26_session_full_room");
            yield return Click(Texts.EndEarly);
            yield return Click(Texts.OptBreak);
            yield return QaCapture.Shot("27_break_full_room");
        }

        [UnityTest]
        public IEnumerator RunningAndPausedSessionsSurviveRestart()
        {
            Seed(d => d.wood = 1);
            yield return Restart();
            yield return Click(Texts.DoThisStep);
            yield return Click(Texts.Begin);
            fakeNow = fakeNow.AddSeconds(100);

            // App killed while running; reopened 50 s later.
            yield return Restart();
            fakeNow = fakeNow.AddSeconds(50);
            yield return null;
            Assert.AreEqual(ScreenId.Session, App.Current);
            Assert.AreEqual(150, Model.Timer.ActiveSeconds, 1e-3);

            yield return Click(Texts.Pause);
            fakeNow = fakeNow.AddSeconds(3000);
            yield return Restart();
            Assert.AreEqual(ScreenId.Session, App.Current);
            Assert.AreEqual(SessionPhase.Paused, Model.Timer.Phase);
            Assert.AreEqual(150, Model.Timer.ActiveSeconds, 1e-3);

            // Resume, then the time runs out while the app is closed.
            yield return Click(Texts.Resume);
            fakeNow = fakeNow.AddSeconds(10000);
            yield return Restart();
            Assert.AreEqual(ScreenId.Reflection, App.Current, "asks once on return");
            Assert.AreEqual(1, Model.Data.wood, "no automatic reward");
            yield return Click(Texts.OptLittle);
            Assert.AreEqual(2, Model.Data.wood);

            // The answered session cannot pay out again after another restart.
            yield return Restart();
            Assert.AreEqual(ScreenId.Home, App.Current);
            Assert.IsFalse(Model.Reflect(Reflection.DoneStep).Accepted);
            Assert.AreEqual(2, Model.Data.wood);
        }
    }
}
