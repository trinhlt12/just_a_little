using System.Collections.Generic;
using UnityEngine;

namespace OneLittleStep.EditorTools.Art
{
    /// <summary>
    /// Shared palette. Warm woods, cream plaster, warm greys, blue-grey roof tiles, a cyan sky with cream clouds,
    /// low-contrast teal backdrop, leaf greens and small coral / yellow / sage accents. Light comes from top-left.
    /// </summary>
    public static class P
    {
        static Color32 C(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var c);
            return c;
        }

        public static readonly Color32 Clear = new Color32(0, 0, 0, 0);
        public static readonly Color32 Ink = C("2B2028");          // selective outline / deepest shadow

        public static readonly Color32 WoodDD = C("4A2E22");
        public static readonly Color32 WoodD = C("6E4430");
        public static readonly Color32 WoodM = C("9A6542");
        public static readonly Color32 WoodL = C("C48A5A");
        public static readonly Color32 WoodHL = C("E0B07E");

        public static readonly Color32 CreamL = C("FBF0D9");
        public static readonly Color32 CreamM = C("F0DDB8");
        public static readonly Color32 CreamD = C("D9BF94");

        public static readonly Color32 GreyL = C("C9C2B6");
        public static readonly Color32 GreyM = C("A39A8E");
        public static readonly Color32 GreyD = C("6F6860");

        public static readonly Color32 RoofL = C("8FA3B3");
        public static readonly Color32 RoofM = C("6A8094");
        public static readonly Color32 RoofD = C("4A5C70");
        public static readonly Color32 RoofDD = C("34404F");

        public static readonly Color32 SkyTop = C("3FB5E6");
        public static readonly Color32 SkyMid = C("5CC8EE");
        public static readonly Color32 SkyLow = C("8ADDF2");
        public static readonly Color32 CloudW = C("FFF9EE");
        public static readonly Color32 CloudM = C("E3F1F4");
        public static readonly Color32 CloudS = C("B5DCEA");

        public static readonly Color32 FarL = C("B4E0DF");
        public static readonly Color32 FarM = C("9CCFD2");
        public static readonly Color32 FarD = C("84BCC4");

        public static readonly Color32 LeafD = C("2F6B4F");
        public static readonly Color32 LeafM = C("4F9A5C");
        public static readonly Color32 LeafL = C("8CC560");
        public static readonly Color32 LeafHL = C("C6E07A");

        public static readonly Color32 CoralL = C("F4A58E");
        public static readonly Color32 CoralM = C("E0725C");
        public static readonly Color32 CoralD = C("B4533F");
        public static readonly Color32 YellowL = C("F8DE8A");
        public static readonly Color32 YellowM = C("E8B84A");
        public static readonly Color32 SageL = C("BCD5A8");
        public static readonly Color32 SageM = C("8DB383");
        public static readonly Color32 SageD = C("5E8A63");

        public static readonly Color32 CatD = C("33343E");
        public static readonly Color32 CatM = C("4B4D5A");
        public static readonly Color32 CatL = C("676A7A");
        public static readonly Color32 Belly = C("F2E6CF");
        public static readonly Color32 Eye = C("D6F06A");

        public static readonly Color32 Glass = C("A8E3F0");
        public static readonly Color32 Warm = C("FFE9B8");        // lamp light / lit plaster
        public static readonly Color32 White = C("FFFFFF");       // UI 9-slice base only (tinted at runtime)
        public static readonly Color32 UiEdge = C("D6CABA");

        /// <summary>Flat-alpha contact shadow (single value, never a gradient).</summary>
        public static readonly Color32 Shadow = new Color32(0x2B, 0x20, 0x28, 90);

        public static IEnumerable<(string name, Color32 c)> All()
        {
            foreach (var f in typeof(P).GetFields())
            {
                if (f.FieldType != typeof(Color32) || f.Name == nameof(Clear)) continue;
                yield return (f.Name, (Color32)f.GetValue(null));
            }
        }
    }

    /// <summary>Tiny 3x5 bitmap font, used only for labels on QA contact sheets (never in game sprites).</summary>
    public static class PixelFont
    {
        static readonly Dictionary<char, string> glyphs = new Dictionary<char, string>
        {
            ['A'] = "010101111101101", ['B'] = "110101110101110", ['C'] = "011100100100011", ['D'] = "110101101101110",
            ['E'] = "111100110100111", ['F'] = "111100110100100", ['G'] = "011100101101011", ['H'] = "101101111101101",
            ['I'] = "111010010010111", ['J'] = "001001001101010", ['K'] = "101101110101101", ['L'] = "100100100100111",
            ['M'] = "101111111101101", ['N'] = "110101101101101", ['O'] = "010101101101010", ['P'] = "110101110100100",
            ['Q'] = "010101101110011", ['R'] = "110101110101101", ['S'] = "011100010001110", ['T'] = "111010010010010",
            ['U'] = "101101101101111", ['V'] = "101101101101010", ['W'] = "101101111111101", ['X'] = "101101010101101",
            ['Y'] = "101101010010010", ['Z'] = "111001010100111",
            ['0'] = "111101101101111", ['1'] = "010110010010111", ['2'] = "110001010100111", ['3'] = "110001010001110",
            ['4'] = "101101111001001", ['5'] = "111100110001110", ['6'] = "011100111101111", ['7'] = "111001010010010",
            ['8'] = "111101111101111", ['9'] = "111101111001110",
            ['_'] = "000000000000111", ['-'] = "000000111000000", ['.'] = "000000000000010", [':'] = "000010000010000",
            [' '] = "000000000000000", ['/'] = "001001010100100",
        };

        public static int Measure(string s) => s.Length * 4;

        public static void Draw(PixelCanvas c, string text, int x, int y, Color32 color)
        {
            foreach (var ch in text.ToUpperInvariant())
            {
                if (glyphs.TryGetValue(ch, out var g))
                    for (int r = 0; r < 5; r++)
                        for (int k = 0; k < 3; k++)
                            if (g[r * 3 + k] == '1') c.SetPixel(x + k, y + 4 - r, color);
                x += 4;
            }
        }
    }
}
