using System;
using System.Collections.Generic;
using System.Linq;

namespace OneLittleStep
{
    public enum UnlockResult { Unlocked, AlreadyOwned, NotEnoughWood, UnknownItem, SessionActive }

    public enum PlaceResult { Placed, AlreadyThere, NotOwned, WrongSlot, SlotUnavailable, UnknownItem, SessionActive }

    /// <summary>
    /// Furniture unlocks (paid once with wood) and free placement into fixed slots. Replacing or storing never
    /// loses an item or wood: displaced items simply go back to storage.
    /// </summary>
    public sealed class DecorService
    {
        readonly SaveData data;
        readonly Func<bool> sessionBusy;

        public event Action Changed;

        public DecorService(SaveData data, Func<bool> sessionBusy)
        {
            this.data = data;
            this.sessionBusy = sessionBusy;
            Sanitize();
        }

        public bool IsUnlocked(string item) => data.unlocked.Contains(item);
        public string ItemIn(string slot) => data.placements.FirstOrDefault(p => p.slot == slot)?.item;
        public string SlotOf(string item) => data.placements.FirstOrDefault(p => p.item == item)?.slot;
        public bool IsPlaced(string item) => SlotOf(item) != null;

        public UnlockResult Unlock(string item)
        {
            var def = RoomLayout.GetFurniture(item);
            if (def == null) return UnlockResult.UnknownItem;
            if (sessionBusy()) return UnlockResult.SessionActive;
            if (IsUnlocked(item)) return UnlockResult.AlreadyOwned;
            if (data.wood < def.Price) return UnlockResult.NotEnoughWood;
            data.wood -= def.Price;
            data.unlocked.Add(item);
            Changed?.Invoke();
            return UnlockResult.Unlocked;
        }

        /// <summary>True if the slot can take this item right now (category match and something to stand on).</summary>
        public bool CanPlace(string item, string slotId)
        {
            var def = RoomLayout.GetFurniture(item);
            var slot = RoomLayout.GetSlot(slotId);
            if (def == null || slot == null || slot.Category != def.Category) return false;
            if (slot.ParentSlot == null) return true;
            // a tabletop item cannot stand on itself being moved, and needs a surface below
            return RoomLayout.TryGetAnchor(slot, ItemIn, out _, out _);
        }

        public IEnumerable<SlotDef> CompatibleSlots(string item) =>
            RoomLayout.Slots.Where(s => CanPlace(item, s.Id));

        public PlaceResult Place(string item, string slotId)
        {
            var def = RoomLayout.GetFurniture(item);
            var slot = RoomLayout.GetSlot(slotId);
            if (def == null || slot == null) return PlaceResult.UnknownItem;
            if (sessionBusy()) return PlaceResult.SessionActive;
            if (!IsUnlocked(item)) return PlaceResult.NotOwned;
            if (slot.Category != def.Category) return PlaceResult.WrongSlot;
            if (!CanPlace(item, slotId)) return PlaceResult.SlotUnavailable;
            if (SlotOf(item) == slotId) return PlaceResult.AlreadyThere;

            // Move: leave the old slot first (things standing on it go to storage).
            RemovePlacement(item);
            // Replace: whatever was in the target slot goes back to storage.
            var displaced = ItemIn(slotId);
            if (displaced != null)
            {
                data.placements.RemoveAll(p => p.slot == slotId);
                if (def.SurfaceHeight <= 0) StoreChildrenOf(slotId);
            }
            data.placements.Add(new PlacementData { slot = slotId, item = item });
            Changed?.Invoke();
            return PlaceResult.Placed;
        }

        /// <summary>Puts an item back into storage (free). Items standing on it go to storage too.</summary>
        public bool Store(string item)
        {
            if (sessionBusy() || !IsPlaced(item)) return false;
            RemovePlacement(item);
            Changed?.Invoke();
            return true;
        }

        void RemovePlacement(string item)
        {
            var slot = SlotOf(item);
            if (slot == null) return;
            data.placements.RemoveAll(p => p.item == item);
            StoreChildrenOf(slot);
        }

        void StoreChildrenOf(string parentSlot)
        {
            foreach (var child in RoomLayout.Slots.Where(s => s.ParentSlot == parentSlot))
                data.placements.RemoveAll(p => p.slot == child.Id);
        }

        /// <summary>Drops anything impossible from a loaded save: unknown ids, duplicates, unowned or floating items.</summary>
        public void Sanitize()
        {
            if (data.unlocked == null) data.unlocked = new List<string>();
            if (data.placements == null) data.placements = new List<PlacementData>();
            data.unlocked = data.unlocked.Where(id => RoomLayout.GetFurniture(id) != null).Distinct().ToList();

            var clean = new List<PlacementData>();
            foreach (var p in data.placements)
            {
                if (p == null) continue;
                var def = RoomLayout.GetFurniture(p.item);
                var slot = RoomLayout.GetSlot(p.slot);
                if (def == null || slot == null || slot.Category != def.Category) continue;
                if (!data.unlocked.Contains(p.item)) continue;
                if (clean.Any(c => c.slot == p.slot || c.item == p.item)) continue;
                clean.Add(new PlacementData { slot = p.slot, item = p.item });
            }
            // tabletop items need a surface under them
            clean.RemoveAll(p =>
            {
                var slot = RoomLayout.GetSlot(p.slot);
                return slot.ParentSlot != null &&
                       !RoomLayout.TryGetAnchor(slot, s => clean.FirstOrDefault(c => c.slot == s)?.item, out _, out _);
            });
            data.placements = clean;
        }
    }
}
