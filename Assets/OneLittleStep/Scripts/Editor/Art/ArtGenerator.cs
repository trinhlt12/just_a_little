using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OneLittleStep.EditorTools.Art
{
    /// <summary>
    /// Writes every game sprite as PNG into Resources/OneLittleStepArt and QA material (previews, contact sheet,
    /// palette) into &lt;project&gt;/QA/art. Output is deterministic; unchanged files are not rewritten.
    /// Only runs from the menu or command line - never on editor start or entering Play mode.
    /// </summary>
    public static class ArtGenerator
    {
        public const string OutputFolder = "Assets/OneLittleStep/Resources/" + ArtLibrary.ResourceFolder;
        public static string QaFolder => Path.Combine(Directory.GetParent(Application.dataPath).FullName, "QA", "art");

        sealed class SpriteSpec
        {
            public string Name;
            public PixelCanvas Canvas;
            public Vector2 Pivot;
            public Vector4 Border;   // 9-slice border for UI frames
        }

        [MenuItem("One Little Step/Regenerate Pixel Art", priority = 0)]
        public static void GenerateMenu()
        {
            var changed = GenerateAll();
            EditorUtility.DisplayDialog("One Little Step", $"Pixel art regenerated.\n{changed} file(s) changed.\nQA previews: {QaFolder}", "OK");
        }

        public static int GenerateAll()
        {
            var specs = BuildSpecs();
            Directory.CreateDirectory(OutputFolder);
            int changed = 0;
            var paths = new List<(string path, SpriteSpec spec)>();
            foreach (var s in specs)
            {
                var path = $"{OutputFolder}/{s.Name}.png";
                var bytes = s.Canvas.EncodePng();
                if (!File.Exists(path) || !File.ReadAllBytes(path).SequenceEqual(bytes))
                {
                    File.WriteAllBytes(path, bytes);
                    changed++;
                }
                paths.Add((path, s));
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach (var (path, spec) in paths) ApplyImportSettings(path, spec.Pivot, spec.Border);
            ArtLibrary.ClearCache();
            WriteQa(specs);
            Debug.Log($"[OneLittleStep] Art generated: {specs.Count} sprites, {changed} changed. QA: {QaFolder}");
            return changed;
        }

        static List<SpriteSpec> BuildSpecs()
        {
            var list = new List<SpriteSpec>();
            foreach (var layer in RoomLayout.Layers)
                list.Add(new SpriteSpec { Name = layer.Name, Canvas = SceneArt.Draw(layer), Pivot = Vector2.zero });
            foreach (var f in RoomLayout.Furniture)
                list.Add(new SpriteSpec { Name = f.SpriteName, Canvas = FurnitureArt.Draw(f.Id), Pivot = PivotOf(f.Pivot) });
            AddFrames(list, "cat_idle", CatArt.Idle());
            AddFrames(list, "cat_sleep", CatArt.Sleep());
            AddFrames(list, "cat_happy", CatArt.Happy());
            foreach (var icon in IconArt.Icons)
                list.Add(new SpriteSpec { Name = icon, Canvas = IconArt.Draw(icon), Pivot = new Vector2(0.5f, 0.5f) });
            list.Add(new SpriteSpec { Name = "ui_panel", Canvas = IconArt.Panel(), Pivot = new Vector2(0.5f, 0.5f), Border = new Vector4(5, 5, 5, 5) });
            list.Add(new SpriteSpec { Name = "ui_button", Canvas = IconArt.Button(), Pivot = new Vector2(0.5f, 0.5f), Border = new Vector4(5, 5, 5, 5) });
            return list;
        }

        static void AddFrames(List<SpriteSpec> list, string prefix, PixelCanvas[] frames)
        {
            for (int i = 0; i < frames.Length; i++)
                list.Add(new SpriteSpec { Name = $"{prefix}_{i}", Canvas = frames[i], Pivot = new Vector2(0.5f, 0f) });
        }

        static Vector2 PivotOf(SpritePivot p) =>
            p == SpritePivot.Center ? new Vector2(0.5f, 0.5f) : p == SpritePivot.BottomLeft ? Vector2.zero : new Vector2(0.5f, 0f);

        static void ApplyImportSettings(string path, Vector2 pivot, Vector4 border)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) return;
            var settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);
            bool dirty = ti.textureType != TextureImporterType.Sprite
                         || ti.spriteImportMode != SpriteImportMode.Single
                         || ti.spritePixelsPerUnit != PixelGrid.PixelsPerUnit
                         || ti.filterMode != FilterMode.Point
                         || ti.textureCompression != TextureImporterCompression.Uncompressed
                         || ti.mipmapEnabled
                         || settings.spriteAlignment != (int)SpriteAlignment.Custom
                         || settings.spritePivot != pivot
                         || ti.spriteBorder != border
                         || settings.spriteMeshType != SpriteMeshType.FullRect
                         || ti.wrapMode != TextureWrapMode.Clamp;
            if (!dirty) return;
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = PixelGrid.PixelsPerUnit;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.npotScale = TextureImporterNPOTScale.None;
            ti.maxTextureSize = 1024;
            ti.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = pivot;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteExtrude = 0;
            ti.SetTextureSettings(settings);
            ti.spriteBorder = border;
            ti.SaveAndReimport();
        }

        // ---------------------------------------------------------------- QA output (not used by the game)
        static void WriteQa(List<SpriteSpec> specs)
        {
            Directory.CreateDirectory(QaFolder);
            var byName = specs.ToDictionary(s => s.Name, s => s.Canvas);
            var empty = ComposeRoom(byName, false);
            var full = ComposeRoom(byName, true);
            Save(empty, "preview_empty_1x.png");
            Save(empty.Scaled(3), "preview_empty_3x.png");
            Save(full, "preview_full_1x.png");
            Save(full.Scaled(3), "preview_full_3x.png");
            Save(full.Crop(0, 96, 216, 176).Scaled(4), "preview_room_zoom_4x.png");
            Save(ContactSheet(specs).Scaled(3), "contact_sheet_3x.png");
            Save(PaletteSheet().Scaled(3), "palette_3x.png");
        }

        static void Save(PixelCanvas c, string file) => File.WriteAllBytes(Path.Combine(QaFolder, file), c.EncodePng());

        /// <summary>Same placement maths as RoomView: pivot pixel = (W/2, 0) or (W/2, H/2) at the slot anchor.</summary>
        public static PixelCanvas ComposeRoom(Dictionary<string, PixelCanvas> art, bool furnished)
        {
            var c = new PixelCanvas(PixelGrid.BackdropWidth, PixelGrid.BackdropHeight);
            var draws = new List<(int order, string name, int x, int y)>();
            foreach (var l in RoomLayout.Layers)
                draws.Add((l.SortingOrder, l.Name, l.X, l.Y));

            var placement = furnished ? FullPlacement : new Dictionary<string, string>();
            foreach (var kv in placement)
            {
                var slot = RoomLayout.GetSlot(kv.Key);
                var item = RoomLayout.GetFurniture(kv.Value);
                if (!RoomLayout.TryGetAnchor(slot, s => placement.TryGetValue(s, out var v) ? v : null, out int ax, out int ay)) continue;
                int bx = ax - item.Width / 2;
                int by = item.Pivot == SpritePivot.Center ? ay - item.Height / 2 : ay;
                draws.Add((slot.SortingOrder, item.SpriteName, bx, by));
            }
            // cat: on the cat bed when there is one, otherwise on the floor
            string catName = furnished ? "cat_sleep_0" : "cat_idle_0";
            int catX = RoomLayout.CatDefaultX, catY = RoomLayout.FrontRowY;
            if (furnished)
            {
                var bedSlot = RoomLayout.GetSlot("floor_left");
                catX = bedSlot.X; catY = bedSlot.Y + RoomLayout.CatOnCatBedLift;
            }
            draws.Add((RoomLayout.OrderCat, catName, catX - 16, catY));

            foreach (var d in draws.OrderBy(d => d.order))
                c.Blit(art[d.name], d.x + PixelGrid.BackdropOffsetX, d.y + PixelGrid.BackdropOffsetY);
            return c.Crop(PixelGrid.BackdropOffsetX, PixelGrid.BackdropOffsetY, PixelGrid.SceneWidth, PixelGrid.SceneHeight);
        }

        public static readonly Dictionary<string, string> FullPlacement = new Dictionary<string, string>
        {
            ["cabinet_left"] = "bookshelf", ["bed"] = "bed", ["cabinet_mid"] = "drawer", ["desk"] = "desk",
            ["top_desk_l"] = "lamp", ["top_cab_mid"] = "smallplant", ["wall_bed"] = "painting", ["wall_mid"] = "clock",
            ["floor_left"] = "catbed", ["floor_mid"] = "rug", ["chair"] = "chair", ["bigplant"] = "bigplant",
        };

        static PixelCanvas ContactSheet(List<SpriteSpec> specs)
        {
            var items = specs.Where(s => !s.Name.StartsWith("bg_") && !s.Name.StartsWith("house_") && !s.Name.StartsWith("fg_")).ToList();
            const int cellW = 72, cellH = 64, cols = 8;
            int rows = (items.Count + cols - 1) / cols;
            var sheet = new PixelCanvas(cols * cellW, rows * cellH + 10);
            sheet.Clear(P.CreamL);
            PixelFont.Draw(sheet, "ONE LITTLE STEP - SPRITES", 3, sheet.H - 8, P.Ink);
            for (int i = 0; i < items.Count; i++)
            {
                int col = i % cols, row = i / cols;
                int x0 = col * cellW, y0 = sheet.H - 10 - (row + 1) * cellH;
                sheet.Fill(x0 + 2, y0 + 9, x0 + cellW - 3, y0 + cellH - 3, P.CreamM);   // backing so light pixels read
                var s = items[i].Canvas;
                sheet.Blit(s, x0 + (cellW - s.W) / 2, y0 + 10 + (cellH - 13 - s.H) / 2);
                var label = items[i].Name.Replace("furniture_", "").ToUpperInvariant();
                PixelFont.Draw(sheet, label, x0 + 3, y0 + 2, P.Ink);
            }
            return sheet;
        }

        static PixelCanvas PaletteSheet()
        {
            var colors = P.All().ToList();
            const int rowH = 10, colW = 64, perCol = 16;
            int cols = (colors.Count + perCol - 1) / perCol;
            var sheet = new PixelCanvas(cols * colW, perCol * rowH + 10);
            sheet.Clear(P.CreamL);
            PixelFont.Draw(sheet, "PALETTE " + colors.Count, 3, sheet.H - 8, P.Ink);
            for (int i = 0; i < colors.Count; i++)
            {
                int x0 = (i / perCol) * colW, y0 = sheet.H - 10 - (i % perCol + 1) * rowH;
                sheet.Fill(x0 + 2, y0 + 1, x0 + 12, y0 + 8, colors[i].c);
                sheet.RectOutline(x0 + 1, y0, 13, 10, P.Ink);
                PixelFont.Draw(sheet, colors[i].name, x0 + 16, y0 + 3, P.Ink);
            }
            return sheet;
        }
    }
}
