using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace OneLittleStep.Tests
{
    public class DecorTests
    {
        DateTime now = new DateTime(2026, 10, 14, 9, 0, 0, DateTimeKind.Utc);
        GameModel model;
        DecorService Decor => model.Decor;

        [SetUp]
        public void SetUp() => model = new GameModel(new SaveData(), () => now);

        [Test]
        public void CannotUnlockWithoutEnoughWood()
        {
            model.Data.wood = 2;
            Assert.AreEqual(UnlockResult.NotEnoughWood, Decor.Unlock("bed"));   // costs 3
            Assert.AreEqual(2, model.Data.wood, "a refused purchase costs nothing");
            Assert.IsFalse(Decor.IsUnlocked("bed"));
        }

        [Test]
        public void UnlockChargesOnceAndOnlyOnce()
        {
            model.Data.wood = 5;
            Assert.AreEqual(UnlockResult.Unlocked, Decor.Unlock("desk"));      // costs 2
            Assert.AreEqual(3, model.Data.wood);
            Assert.AreEqual(UnlockResult.AlreadyOwned, Decor.Unlock("desk"));
            Assert.AreEqual(3, model.Data.wood);
        }

        [Test]
        public void NothingCanBeBoughtOrMovedDuringASession()
        {
            model.Data.wood = 5;
            Decor.Unlock("chair");
            model.SetTask("Dọn bàn", "Cất một món đồ");
            model.StartSession(120);
            Assert.AreEqual(UnlockResult.SessionActive, Decor.Unlock("lamp"));
            Assert.AreEqual(PlaceResult.SessionActive, Decor.Place("chair", "chair"));
            Assert.AreEqual(4, model.Data.wood);
        }

        [Test]
        public void PlaceMoveReplaceAndStoreAreFreeAndLoseNothing()
        {
            model.Data.wood = 10;
            Decor.Unlock("bookshelf");
            Decor.Unlock("drawer");
            int wood = model.Data.wood;

            Assert.AreEqual(PlaceResult.Placed, Decor.Place("bookshelf", "cabinet_left"));
            Assert.AreEqual(PlaceResult.Placed, Decor.Place("bookshelf", "cabinet_mid"), "move");
            Assert.IsNull(Decor.ItemIn("cabinet_left"));
            Assert.AreEqual(PlaceResult.Placed, Decor.Place("drawer", "cabinet_mid"), "replace");
            Assert.IsFalse(Decor.IsPlaced("bookshelf"), "the replaced item goes back to storage");
            Assert.IsTrue(Decor.IsUnlocked("bookshelf"), "and is still owned");
            Assert.IsTrue(Decor.Store("drawer"));
            Assert.IsEmpty(model.Data.placements);
            Assert.AreEqual(wood, model.Data.wood);
        }

        [Test]
        public void ItemsOnlyGoIntoMatchingSlots()
        {
            model.Data.wood = 10;
            Decor.Unlock("rug");
            Decor.Unlock("lamp");
            Assert.AreEqual(PlaceResult.WrongSlot, Decor.Place("rug", "wall_mid"));
            Assert.AreEqual(PlaceResult.SlotUnavailable, Decor.Place("lamp", "top_desk_l"), "no desk to stand on yet");
            Assert.AreEqual(PlaceResult.NotOwned, Decor.Place("clock", "wall_mid"));
        }

        [Test]
        public void StoringTheDeskReturnsWhatStoodOnIt()
        {
            model.Data.wood = 10;
            Decor.Unlock("desk");
            Decor.Unlock("lamp");
            Decor.Place("desk", "desk");
            Assert.AreEqual(PlaceResult.Placed, Decor.Place("lamp", "top_desk_l"));
            Decor.Store("desk");
            Assert.IsFalse(Decor.IsPlaced("lamp"));
            Assert.IsTrue(Decor.IsUnlocked("lamp"));
        }

        [Test]
        public void SwappingCabinetsKeepsTheItemOnTop()
        {
            model.Data.wood = 10;
            Decor.Unlock("drawer");
            Decor.Unlock("bookshelf");
            Decor.Unlock("smallplant");
            Decor.Place("drawer", "cabinet_mid");
            Decor.Place("smallplant", "top_cab_mid");
            Decor.Place("bookshelf", "cabinet_mid");
            Assert.AreEqual("smallplant", Decor.ItemIn("top_cab_mid"), "the plant now stands on the bookshelf");
        }

        [Test]
        public void DamagedPlacementsAreCleanedOnLoad()
        {
            var data = new SaveData { wood = 1 };
            data.unlocked.AddRange(new[] { "desk", "lamp", "desk", "ghost" });
            data.placements.Add(new PlacementData { slot = "desk", item = "desk" });
            data.placements.Add(new PlacementData { slot = "desk", item = "lamp" });          // wrong category
            data.placements.Add(new PlacementData { slot = "wall_mid", item = "clock" });     // not owned
            data.placements.Add(new PlacementData { slot = "nowhere", item = "lamp" });       // unknown slot
            data.placements.Add(new PlacementData { slot = "top_cab_mid", item = "lamp" });   // nothing under it
            var m = new GameModel(data, () => now);
            CollectionAssert.AreEquivalent(new[] { "desk", "lamp" }, m.Data.unlocked);
            Assert.AreEqual(1, m.Data.placements.Count);
            Assert.AreEqual("desk", m.Decor.ItemIn("desk"));
        }

        [Test]
        public void AllTwelvePiecesHaveAPriceAndAtLeastOneSlot()
        {
            Assert.AreEqual(12, RoomLayout.Furniture.Length);
            foreach (var f in RoomLayout.Furniture)
            {
                Assert.That(f.Price, Is.InRange(1, 3), f.Id);
                Assert.IsTrue(RoomLayout.Slots.Any(s => s.Category == f.Category), f.Id);
                Assert.AreEqual(0, f.Width % 8, f.Id);
                Assert.AreEqual(0, f.Height % 8, f.Id);
            }
        }
    }

    public class SaveStoreTests
    {
        string dir;

        [SetUp]
        public void SetUp()
        {
            dir = Path.Combine(Path.GetTempPath(), "ols_save_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }

        [Test]
        public void RoundTripKeepsEverything()
        {
            var store = new SaveStore(dir);
            var data = new SaveData { wood = 4, introSeen = true, reduceMotion = true };
            data.task = new TaskData { active = true, title = "Học tiếng Nhật", step = "Ôn 5 chữ kanji" };
            data.notes.Add(new NoteData { text = "mua trà", createdUtcTicks = 1 });
            data.unlocked.Add("rug");
            data.placements.Add(new PlacementData { slot = "floor_mid", item = "rug" });
            data.session = new SessionData { id = "abc", phase = (int)SessionPhase.Paused, durationSeconds = 300, bankedSeconds = 42, step = "Ôn 5 chữ kanji" };
            Assert.IsTrue(store.Save(data));

            var loaded = new SaveStore(dir).Load();
            Assert.AreEqual(4, loaded.wood);
            Assert.AreEqual("Học tiếng Nhật", loaded.task.title);
            Assert.AreEqual("mua trà", loaded.notes[0].text);
            Assert.AreEqual("rug", loaded.placements[0].item);
            Assert.IsTrue(loaded.reduceMotion);
            Assert.AreEqual(42, loaded.session.bankedSeconds, 1e-6);
            Assert.AreEqual((int)SessionPhase.Paused, loaded.session.phase);
        }

        [Test]
        public void CorruptSaveFallsBackToBackupAndKeepsACopy()
        {
            var store = new SaveStore(dir);
            store.Save(new SaveData { wood = 1 });
            store.Save(new SaveData { wood = 2 });        // first save becomes .bak
            File.WriteAllText(store.MainPath, "{ this is not json");

            var loaded = store.Load();
            Assert.AreEqual(1, loaded.wood, "previous good save is used");
            Assert.IsNotNull(store.LastRecoveryNote);
            Assert.IsTrue(Directory.GetFiles(dir, "*corrupt*").Length == 1, "the damaged file is kept aside");
        }

        [Test]
        public void GarbageWithoutBackupStartsFresh()
        {
            var store = new SaveStore(dir);
            File.WriteAllText(store.MainPath, "\0\0garbage");
            var loaded = store.Load();
            Assert.AreEqual(0, loaded.wood);
            Assert.IsNotNull(store.LastRecoveryNote);
            Assert.IsTrue(store.Save(loaded), "saving works again after recovery");
        }

        [Test]
        public void MissingFileStartsFresh()
        {
            var loaded = new SaveStore(dir).Load();
            Assert.IsFalse(loaded.introSeen);
            Assert.AreEqual(0, loaded.wood);
        }
    }
}
