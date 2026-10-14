using System;
using System.Collections.Generic;
using UnityEngine;

namespace OneLittleStep
{
    /// <summary>Builds the pixel room from RoomLayout and shows whatever furniture is placed in each slot.</summary>
    public sealed class RoomView : MonoBehaviour
    {
        readonly Dictionary<string, SpriteRenderer> slotRenderers = new Dictionary<string, SpriteRenderer>();
        Transform furnitureRoot;
        public CatView Cat { get; private set; }

        public static RoomView Create(Transform parent)
        {
            var go = new GameObject("Room");
            go.transform.SetParent(parent, false);
            var view = go.AddComponent<RoomView>();
            view.Build();
            return view;
        }

        void Build()
        {
            var layersRoot = new GameObject("Scenery").transform;
            layersRoot.SetParent(transform, false);
            foreach (var layer in RoomLayout.Layers)
            {
                var sr = NewRenderer(layer.Name, layersRoot, layer.SortingOrder);
                sr.sprite = ArtLibrary.Sprite(layer.Name);
                sr.transform.localPosition = PixelGrid.ToWorld(layer.X, layer.Y);
            }

            furnitureRoot = new GameObject("Furniture").transform;
            furnitureRoot.SetParent(transform, false);
            foreach (var slot in RoomLayout.Slots)
            {
                var sr = NewRenderer("Slot " + slot.Id, furnitureRoot, slot.SortingOrder);
                sr.enabled = false;
                slotRenderers[slot.Id] = sr;
            }

            var catGo = new GameObject("Cat");
            catGo.transform.SetParent(transform, false);
            Cat = catGo.AddComponent<CatView>();
            Cat.Init(NewRendererOn(catGo, RoomLayout.OrderCat));
            PlaceCat(_ => null);
        }

        static SpriteRenderer NewRenderer(string name, Transform parent, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return NewRendererOn(go, order);
        }

        static SpriteRenderer NewRendererOn(GameObject go, int order)
        {
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = order;
            return sr;
        }

        /// <summary>Shows the given placement (slot id -> furniture id). Slots that cannot be shown stay empty.</summary>
        public void ShowPlacements(Func<string, string> itemInSlot)
        {
            foreach (var slot in RoomLayout.Slots)
            {
                var sr = slotRenderers[slot.Id];
                var item = RoomLayout.GetFurniture(itemInSlot(slot.Id));
                if (item == null || !RoomLayout.TryGetAnchor(slot, itemInSlot, out int x, out int y))
                {
                    sr.enabled = false;
                    continue;
                }
                sr.sprite = ArtLibrary.Sprite(item.SpriteName);
                sr.transform.localPosition = PixelGrid.ToWorld(x, y);
                sr.enabled = sr.sprite != null;
            }
            PlaceCat(itemInSlot);
        }

        /// <summary>The cat curls up on its cushion when there is one, otherwise sits on the floor (on the rug if present).</summary>
        void PlaceCat(Func<string, string> itemInSlot)
        {
            int x = RoomLayout.CatDefaultX, y = RoomLayout.FrontRowY;
            SlotDef catBedSlot = null, rugSlot = null;
            foreach (var slot in RoomLayout.Slots)
            {
                var id = itemInSlot(slot.Id);
                if (id == "catbed") catBedSlot = slot;
                else if (id == "rug") rugSlot = slot;
            }
            if (catBedSlot != null) { x = catBedSlot.X; y = catBedSlot.Y + RoomLayout.CatOnCatBedLift; }
            else if (rugSlot != null) { x = rugSlot.X; y = rugSlot.Y + RoomLayout.CatOnRugLift; }
            Cat.transform.localPosition = PixelGrid.ToWorld(x, y);
        }

        /// <summary>World position of a slot's anchor (used to place on-screen slot markers).</summary>
        public Vector3 SlotWorldCenter(SlotDef slot, Func<string, string> itemInSlot, string forItem)
        {
            RoomLayout.TryGetAnchor(slot, itemInSlot, out int x, out int y);
            var item = RoomLayout.GetFurniture(forItem);
            int h = item != null ? item.Height : 16;
            if (slot.Category != SlotCategory.Wall) y += Mathf.Min(h, 24) / 2;
            return transform.TransformPoint(PixelGrid.ToWorld(x, y));
        }
    }

    /// <summary>Frame-based cat animation. Idle/Sleep loop slowly; Happy plays once and returns to the previous loop.</summary>
    public sealed class CatView : MonoBehaviour
    {
        public enum Mood { Idle, Sleep, Happy }

        static readonly float[] IdleTimes = { 1.1f, 0.6f, 0.16f, 0.7f };
        static readonly float[] SleepTimes = { 0.9f, 0.9f, 0.9f, 0.9f };
        static readonly float[] HappyTimes = { 0.2f, 0.22f, 0.22f, 0.4f };

        SpriteRenderer sr;
        Sprite[] idle, sleep, happy;
        Mood mood = Mood.Idle, loopMood = Mood.Idle;
        int frame;
        float timer;
        bool reduceMotion;
        float happyHold;

        public Mood Current => mood;

        public void Init(SpriteRenderer renderer)
        {
            sr = renderer;
            idle = Load("cat_idle");
            sleep = Load("cat_sleep");
            happy = Load("cat_happy");
            Apply();
        }

        static Sprite[] Load(string prefix)
        {
            var frames = new Sprite[4];
            for (int i = 0; i < 4; i++) frames[i] = ArtLibrary.Sprite($"{prefix}_{i}");
            return frames;
        }

        public void SetReduceMotion(bool on)
        {
            reduceMotion = on;
            frame = 0;
            timer = 0;
            Apply();
        }

        /// <summary>Sets the looping mood (Idle or Sleep). A Happy in progress finishes first.</summary>
        public void SetLoop(Mood m)
        {
            if (m == Mood.Happy) { PlayHappy(); return; }
            loopMood = m;
            if (mood == Mood.Happy) return;
            if (mood == m) return;
            mood = m;
            frame = 0;
            timer = 0;
            Apply();
        }

        public void PlayHappy()
        {
            mood = Mood.Happy;
            frame = 0;
            timer = 0;
            happyHold = 1.2f;
            Apply();
        }

        void Update()
        {
            if (sr == null) return;
            float dt = Time.unscaledDeltaTime;
            if (reduceMotion)
            {
                // No decorative animation: hold a single frame; a reward shows one still happy frame briefly.
                if (mood == Mood.Happy && (happyHold -= dt) <= 0) { mood = loopMood; Apply(); }
                return;
            }
            var times = mood == Mood.Idle ? IdleTimes : mood == Mood.Sleep ? SleepTimes : HappyTimes;
            timer += dt;
            if (timer < times[frame]) return;
            timer = 0;
            frame++;
            if (frame >= 4)
            {
                if (mood == Mood.Happy) { mood = loopMood; }
                frame = 0;
            }
            Apply();
        }

        void Apply()
        {
            if (sr == null) return;
            var set = mood == Mood.Idle ? idle : mood == Mood.Sleep ? sleep : happy;
            sr.sprite = set[Mathf.Clamp(frame, 0, 3)];
        }
    }
}
