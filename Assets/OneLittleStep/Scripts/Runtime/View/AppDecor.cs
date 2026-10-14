using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace OneLittleStep
{
    /// <summary>
    /// Decorating mode: pick a furniture card, unlock it once with wood, then tap a glowing slot marker in the room.
    /// Only reachable from Home, so never during a focus session.
    /// </summary>
    public sealed partial class AppController
    {
        sealed class Card
        {
            public FurnitureDef Def;
            public Button Button;
            public Image Frame;
            public Text Tag;
            public Image Lock;
        }

        readonly List<Card> cards = new List<Card>();
        readonly Dictionary<string, Button> markers = new Dictionary<string, Button>();
        RectTransform markerLayer;
        Text decorInfo;
        Button decorAction;
        string selectedItem;

        const float CardW = 158f, CardH = 140f;

        void BuildDecor()
        {
            var screen = ScreenRoot(ScreenId.Decor);

            // Slot markers live under the cards layer but above the world.
            markerLayer = UIKit.Stretch(UIKit.Rect(screen, "Markers"));
            foreach (var slot in RoomLayout.Slots)
            {
                var id = slot.Id;
                var b = UIKit.IconButton(markerLayer, "icon_slot", () => PlaceSelected(id), 88);
                b.name = "Marker " + id;
                var cb = b.colors;
                cb.normalColor = cb.highlightedColor = cb.selectedColor = Theme.Hex("FFF7E8", 0.85f);
                b.colors = cb;
                var rt = (RectTransform)b.transform;
                rt.anchorMin = rt.anchorMax = Vector2.zero;
                rt.sizeDelta = new Vector2(88, 88);
                b.gameObject.SetActive(false);
                markers[id] = b;
            }

            var s = Sheet(screen);
            var v = s.GetComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(28, 28, 24, 28);
            v.spacing = 14;

            var header = UIKit.Row(s, "Header", 12, 72);
            header.childForceExpandWidth = false;
            var title = UIKit.Label(header.transform, Texts.DecorTitle, UIKit.TitleSize - 6, Theme.Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            UIKit.Size(title.gameObject, 72, -1, 1);
            var done = UIKit.Button(header.transform, Texts.DecorDone, ButtonStyle.Secondary, () => LeaveTo(ScreenId.Home), 72);
            UIKit.Size(done.gameObject, 72, 200);

            var grid = UIKit.Rect(s, "Grid");
            var g = grid.gameObject.AddComponent<GridLayoutGroup>();
            g.cellSize = new Vector2(CardW, CardH);
            g.spacing = new Vector2(8, 10);
            g.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            g.constraintCount = 6;
            g.childAlignment = TextAnchor.UpperCenter;
            UIKit.Size(grid.gameObject, CardH * 2 + 10);
            foreach (var def in RoomLayout.Furniture) cards.Add(BuildCard(grid, def));

            var actionRow = UIKit.Row(s, "Action", 16, SmallButton);
            actionRow.childForceExpandWidth = false;
            decorInfo = UIKit.Label(actionRow.transform, "", 32, Theme.Ink, TextAnchor.MiddleLeft);
            decorInfo.resizeTextForBestFit = true;
            decorInfo.resizeTextMinSize = 24;
            decorInfo.resizeTextMaxSize = 32;
            UIKit.Size(decorInfo.gameObject, SmallButton, -1, 1);
            decorAction = UIKit.Button(actionRow.transform, "", ButtonStyle.Primary, OnDecorAction, SmallButton);
            UIKit.Size(decorAction.gameObject, SmallButton, 380);

            refreshers[ScreenId.Decor] = RefreshDecor;
        }

        Card BuildCard(RectTransform grid, FurnitureDef def)
        {
            var card = new Card { Def = def };
            card.Button = UIKit.Button(grid, "", ButtonStyle.Secondary, () => SelectItem(def.Id), CardH);
            card.Button.name = "Card " + def.Id;
            card.Button.GetComponentInChildren<Text>().gameObject.SetActive(false);
            card.Frame = card.Button.GetComponent<Image>();

            // Pixel thumbnail at a whole-number scale so it stays crisp.
            var sprite = ArtLibrary.Sprite(def.SpriteName);
            var thumb = UIKit.Rect(card.Button.transform, "Thumb").gameObject.AddComponent<Image>();
            thumb.sprite = sprite;
            thumb.raycastTarget = false;
            int k = Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(110f / def.Width, 80f / def.Height)), 1, 4);
            var trt = thumb.rectTransform;
            trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 1f);
            trt.pivot = new Vector2(0.5f, 0.5f);
            trt.sizeDelta = new Vector2(def.Width * k, def.Height * k);
            trt.anchoredPosition = new Vector2(0, -52);

            card.Tag = UIKit.Label(card.Button.transform, "", 26, Theme.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            var tag = card.Tag.rectTransform;
            tag.anchorMin = new Vector2(0, 0); tag.anchorMax = new Vector2(1, 0);
            tag.pivot = new Vector2(0.5f, 0);
            tag.offsetMin = new Vector2(6, 10); tag.offsetMax = new Vector2(-6, 46);

            card.Lock = UIKit.Icon(card.Button.transform, "icon_lock", 40);
            var lrt = card.Lock.rectTransform;
            lrt.anchorMin = lrt.anchorMax = new Vector2(1, 1);
            lrt.pivot = new Vector2(1, 1);
            lrt.anchoredPosition = new Vector2(-8, -8);
            return card;
        }

        void EnterDecor()
        {
            if (model.Timer.IsBusy) return;   // decorating is only possible between sessions
            selectedItem = null;
            Show(ScreenId.Decor);
        }

        void ExitDecor()
        {
            selectedItem = null;
            foreach (var m in markers.Values) m.gameObject.SetActive(false);
        }

        void SelectItem(string id)
        {
            selectedItem = id;
            RefreshDecor();
        }

        void OnDecorAction()
        {
            if (selectedItem == null) return;
            var decor = model.Decor;
            var def = RoomLayout.GetFurniture(selectedItem);
            if (!decor.IsUnlocked(selectedItem))
            {
                if (decor.Unlock(selectedItem) == UnlockResult.Unlocked)
                {
                    ShowToast(Texts.UnlockedItem(def.Name), rise: false);
                    if (!ReduceMotion) room.Cat.PlayHappy();
                }
            }
            else if (decor.IsPlaced(selectedItem))
            {
                decor.Store(selectedItem);
                RefreshRoom();
            }
            RefreshDecor();
            RefreshHud();
        }

        void PlaceSelected(string slotId)
        {
            if (selectedItem == null) return;
            var displaced = model.Decor.ItemIn(slotId);
            if (model.Decor.Place(selectedItem, slotId) != PlaceResult.Placed) return;
            if (displaced != null && displaced != selectedItem)
                ShowToast(Texts.StoredItem(RoomLayout.GetFurniture(displaced).Name), rise: false);
            RefreshRoom();
            RefreshDecor();
        }

        void RefreshDecor()
        {
            var decor = model.Decor;
            foreach (var c in cards)
            {
                bool owned = decor.IsUnlocked(c.Def.Id);
                bool placed = decor.IsPlaced(c.Def.Id);
                c.Lock.gameObject.SetActive(!owned);
                c.Tag.text = !owned ? Texts.Price(c.Def.Price) : placed ? Texts.PlacedTag : Texts.InStorage;
                c.Tag.color = !owned && model.Data.wood < c.Def.Price ? Theme.InkMuted : Theme.Ink;
                UIKit.ApplyStyle(c.Button, c.Def.Id == selectedItem ? ButtonStyle.Chip : ButtonStyle.Secondary);
                if (c.Def.Id == selectedItem)
                {
                    var cb = c.Button.colors;
                    cb.normalColor = cb.highlightedColor = cb.selectedColor = Theme.Hex("B9D3A6");
                    c.Button.colors = cb;
                }
            }

            if (selectedItem == null)
            {
                decorInfo.text = Texts.DecorPickHint;
                decorAction.gameObject.SetActive(false);
                ShowMarkers(Enumerable.Empty<SlotDef>());
                return;
            }

            var def = RoomLayout.GetFurniture(selectedItem);
            decorAction.gameObject.SetActive(true);
            if (!decor.IsUnlocked(def.Id))
            {
                int missing = def.Price - model.Data.wood;
                decorInfo.text = def.Name;
                UIKit.ApplyStyle(decorAction, ButtonStyle.Primary);
                UIKit.SetText(decorAction, missing > 0 ? Texts.NeedMore(missing) : Texts.Unlock(def.Price));
                UIKit.SetInteractable(decorAction, missing <= 0);
                ShowMarkers(Enumerable.Empty<SlotDef>());
                return;
            }

            var slots = decor.CompatibleSlots(def.Id).Where(s => decor.SlotOf(def.Id) != s.Id).ToList();
            var current = decor.SlotOf(def.Id);
            if (current != null)
                decorInfo.text = $"{def.Name} · {Texts.PlacedAt(RoomLayout.GetSlot(current).Name)}";
            else
                decorInfo.text = $"{def.Name} · {(slots.Count > 0 ? Texts.ChooseSpot : Texts.NeedSurface)}";
            decorAction.gameObject.SetActive(current != null);
            UIKit.ApplyStyle(decorAction, ButtonStyle.Secondary);
            UIKit.SetText(decorAction, Texts.StoreItem);
            UIKit.SetInteractable(decorAction, true);
            ShowMarkers(slots);
        }

        void ShowMarkers(IEnumerable<SlotDef> slots)
        {
            var on = new HashSet<string>(slots.Select(s => s.Id));
            foreach (var kv in markers) kv.Value.gameObject.SetActive(on.Contains(kv.Key));
            UpdateDecorMarkers();
        }

        /// <summary>Keeps the on-screen markers over their slots (camera size can change with the screen).</summary>
        void UpdateDecorMarkers()
        {
            if (markerLayer == null) return;
            var canvas = markerLayer.GetComponentInParent<Canvas>().rootCanvas;
            var cam = room.GetComponentInParent<Bootstrap>().WorldCamera;
            var uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            foreach (var kv in markers)
            {
                if (!kv.Value.gameObject.activeSelf) continue;
                var slot = RoomLayout.GetSlot(kv.Key);
                var world = room.SlotWorldCenter(slot, model.Decor.ItemIn, selectedItem);
                var screen = cam.WorldToScreenPoint(world);
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(markerLayer, screen, uiCam, out var local))
                {
                    var rt = (RectTransform)kv.Value.transform;
                    rt.anchoredPosition = local + markerLayer.rect.size * markerLayer.pivot;
                }
            }
        }

        public void NotifySaveRecovered() => ShowToast(Texts.SaveRecovered, rise: false);
    }
}
