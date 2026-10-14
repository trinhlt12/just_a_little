using UnityEngine;
using UnityEngine.UI;

namespace OneLittleStep
{
    /// <summary>Colours used by the screen-resolution UI layer.</summary>
    public static class Theme
    {
        public static readonly Color Panel = Hex("FFF7E8");
        public static readonly Color PanelSoft = Hex("FFF7E8", 0.94f);
        public static readonly Color Ink = Hex("3A2A22");
        public static readonly Color InkMuted = Hex("6B5647");
        public static readonly Color Primary = Hex("B4533F");
        public static readonly Color PrimaryPressed = Hex("8F3F2F");
        public static readonly Color OnPrimary = Hex("FFFFFF");
        public static readonly Color Secondary = Hex("F0DDB8");
        public static readonly Color SecondaryPressed = Hex("D9BF94");
        public static readonly Color Chip = Hex("DDE8CF");
        public static readonly Color ChipSelected = Hex("5E8A63");
        public static readonly Color OnChip = Hex("2F4A33");
        public static readonly Color Disabled = Hex("D8CFC2");
        public static readonly Color OnDisabled = Hex("8A7E72");
        public static readonly Color Scrim = Hex("2B2028", 0.45f);
        public static readonly Color InputBg = Hex("FFFFFF");

        public static Color Hex(string hex, float alpha = 1f)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var c);
            c.a = alpha;
            return c;
        }
    }

    public enum ButtonStyle { Primary, Secondary, Chip, Ghost }

    /// <summary>Small helpers for building uGUI hierarchies from code (no prefabs, no manual references).</summary>
    public static class UIKit
    {
        static Font font;
        public static Font Font => font ? font : (font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));

        public const int BodySize = 40;
        public const int SmallSize = 32;
        public const int TitleSize = 52;
        public const float ButtonHeight = 132f;

        public static Sprite PanelSprite => ArtLibrary.Sprite("ui_panel");
        public static Sprite ButtonSprite => ArtLibrary.Sprite("ui_button");

        public static RectTransform Rect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = 5; // UI
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        public static RectTransform Stretch(RectTransform rt, float left = 0, float right = 0, float bottom = 0, float top = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
            return rt;
        }

        /// <summary>Anchors to the bottom edge, full width, fixed height.</summary>
        public static RectTransform DockBottom(RectTransform rt, float height, float inset = 0)
        {
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.offsetMin = new Vector2(inset, inset);
            rt.offsetMax = new Vector2(-inset, inset + height);
            return rt;
        }

        public static RectTransform DockTop(RectTransform rt, float height, float inset = 0)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.offsetMin = new Vector2(inset, -inset - height);
            rt.offsetMax = new Vector2(-inset, -inset);
            return rt;
        }

        public static Image Panel(Transform parent, string name, Color color, bool sliced = true)
        {
            var rt = Rect(parent, name);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            var sprite = sliced ? PanelSprite : null;
            if (sprite)
            {
                img.sprite = sprite;
                img.type = Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = 0.2f; // chunky pixel corners at screen resolution
            }
            return img;
        }

        public static VerticalLayoutGroup Column(RectTransform rt, int padding = 36, float spacing = 20, TextAnchor align = TextAnchor.UpperCenter)
        {
            var v = rt.gameObject.AddComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(padding, padding, padding, padding);
            v.spacing = spacing;
            v.childAlignment = align;
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = false;
            return v;
        }

        public static HorizontalLayoutGroup Row(Transform parent, string name, float spacing = 16, float height = ButtonHeight)
        {
            var rt = Rect(parent, name);
            var h = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = spacing;
            h.childAlignment = TextAnchor.MiddleCenter;
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = true;
            h.childForceExpandHeight = true;
            Size(rt.gameObject, height);
            return h;
        }

        public static LayoutElement Size(GameObject go, float height = -1, float width = -1, float flexW = -1)
        {
            var le = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
            if (height >= 0) { le.minHeight = height; le.preferredHeight = height; }
            if (width >= 0) { le.minWidth = width; le.preferredWidth = width; }
            if (flexW >= 0) le.flexibleWidth = flexW;
            return le;
        }

        public static Text Label(Transform parent, string text, int size = BodySize, Color? color = null,
            TextAnchor align = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Normal)
        {
            var rt = Rect(parent, "Label");
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Font;
            t.text = text;
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color ?? Theme.Ink;
            t.alignment = align;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Truncate;
            t.lineSpacing = 1.1f;
            t.raycastTarget = false;
            return t;
        }

        /// <summary>A label that sizes its height to its wrapped content inside layout groups.</summary>
        public static Text FlowLabel(Transform parent, string text, int size = BodySize, Color? color = null,
            TextAnchor align = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Normal)
        {
            var t = Label(parent, text, size, color, align, style);
            t.verticalOverflow = VerticalWrapMode.Overflow;
            var fit = t.gameObject.AddComponent<ContentSizeFitter>();
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return t;
        }

        public static Button Button(Transform parent, string text, ButtonStyle style, System.Action onClick, float height = ButtonHeight)
        {
            var rt = Rect(parent, "Button " + text);
            var img = rt.gameObject.AddComponent<Image>();
            var sprite = ButtonSprite;
            if (sprite)
            {
                img.sprite = sprite;
                img.type = Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = 0.2f;
            }
            var btn = rt.gameObject.AddComponent<Button>();
            var label = Label(rt, text, style == ButtonStyle.Chip ? SmallSize : BodySize);
            label.fontStyle = style == ButtonStyle.Primary ? FontStyle.Bold : FontStyle.Normal;
            Stretch(label.rectTransform, 20, 20, 6, 6);
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 24;
            label.resizeTextMaxSize = label.fontSize;
            ApplyStyle(btn, style);
            if (onClick != null) btn.onClick.AddListener(() => onClick());
            Size(rt.gameObject, height);
            return btn;
        }

        public static void ApplyStyle(Button btn, ButtonStyle style)
        {
            var img = btn.GetComponent<Image>();
            btn.targetGraphic = img;
            Color bg, fg;
            switch (style)
            {
                case ButtonStyle.Primary: bg = Theme.Primary; fg = Theme.OnPrimary; break;
                case ButtonStyle.Chip: bg = Theme.Chip; fg = Theme.OnChip; break;
                case ButtonStyle.Ghost: bg = new Color(1, 1, 1, 0); fg = Theme.Ink; break;
                default: bg = Theme.Secondary; fg = Theme.Ink; break;
            }
            img.color = Color.white;
            var cb = btn.colors;
            cb.normalColor = bg;
            cb.highlightedColor = bg;
            cb.selectedColor = bg;
            cb.pressedColor = Color.Lerp(bg, Theme.Ink, 0.25f);
            cb.disabledColor = Theme.Disabled;
            cb.colorMultiplier = 1f;
            cb.fadeDuration = 0.05f;
            btn.colors = cb;
            var label = btn.GetComponentInChildren<Text>();
            if (label) label.color = fg;
            // ColorTint multiplies the white image, so the block colours are the visible fill.
            btn.transition = Selectable.Transition.ColorTint;
        }

        public static void SetText(Button b, string text)
        {
            var t = b.GetComponentInChildren<Text>();
            if (t) t.text = text;
        }

        public static void SetInteractable(Button b, bool on)
        {
            b.interactable = on;
            var t = b.GetComponentInChildren<Text>();
            if (t)
            {
                var c = t.color;
                c.a = on ? 1f : 0.55f;
                t.color = c;
            }
        }

        public static InputField Input(Transform parent, string placeholder, int charLimit, bool multiline = false, float height = 120f)
        {
            var rt = Rect(parent, "Input " + placeholder);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = Theme.InputBg;
            var sprite = PanelSprite;
            if (sprite)
            {
                img.sprite = sprite;
                img.type = Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = 0.25f;
            }
            var field = rt.gameObject.AddComponent<InputField>();
            var text = Label(rt, "", BodySize, Theme.Ink, multiline ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft);
            text.supportRichText = false;
            text.verticalOverflow = multiline ? VerticalWrapMode.Truncate : VerticalWrapMode.Overflow;
            text.horizontalOverflow = multiline ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
            Stretch(text.rectTransform, 30, 30, 14, 14);
            var ph = Label(rt, placeholder, BodySize, Theme.InkMuted, text.alignment, FontStyle.Italic);
            ph.horizontalOverflow = HorizontalWrapMode.Wrap;
            Stretch(ph.rectTransform, 30, 30, 14, 14);
            field.textComponent = text;
            field.placeholder = ph;
            field.characterLimit = charLimit;
            field.lineType = multiline ? InputField.LineType.MultiLineSubmit : InputField.LineType.SingleLine;
            field.caretColor = Theme.Ink;
            field.selectionColor = Theme.Hex("B9D3A6", 0.8f);
            field.caretWidth = 3;
            Size(rt.gameObject, height);
            return field;
        }

        public static Image Icon(Transform parent, string spriteName, float size)
        {
            var rt = Rect(parent, "Icon " + spriteName);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = ArtLibrary.Sprite(spriteName);
            img.preserveAspect = true;
            img.raycastTarget = false;
            img.enabled = img.sprite != null;
            rt.sizeDelta = new Vector2(size, size);
            Size(rt.gameObject, size, size);
            return img;
        }

        public static GameObject Spacer(Transform parent, float height)
        {
            var rt = Rect(parent, "Spacer");
            Size(rt.gameObject, height);
            return rt.gameObject;
        }

        public static GameObject FlexSpacer(Transform parent)
        {
            var rt = Rect(parent, "FlexSpacer");
            var le = rt.gameObject.AddComponent<LayoutElement>();
            le.flexibleHeight = 1;
            le.flexibleWidth = 1;
            return rt.gameObject;
        }
    }
}
