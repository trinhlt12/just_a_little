using UnityEngine;

namespace OneLittleStep.EditorTools.Art
{
    /// <summary>
    /// One drawing function per furniture sprite. Front view with a sliver of top surface, light from top-left,
    /// 2-3 tones per material, outline in the darkest tone of each material, flat contact shadow on row 0.
    /// </summary>
    public static class FurnitureArt
    {
        public static PixelCanvas Draw(string id)
        {
            switch (id)
            {
                case "desk": return Desk();
                case "chair": return Chair();
                case "lamp": return Lamp();
                case "rug": return Rug();
                case "smallplant": return SmallPlant();
                case "bookshelf": return Bookshelf();
                case "painting": return Painting();
                case "clock": return Clock();
                case "bed": return Bed();
                case "catbed": return CatBed();
                case "drawer": return Drawer();
                case "bigplant": return BigPlant();
            }
            throw new System.ArgumentException("No art for furniture " + id);
        }

        /// <summary>Box with highlight on top/left and shadow on bottom/right (inclusive corners).</summary>
        internal static void Box(PixelCanvas c, int x0, int y0, int x1, int y1, Color32 light, Color32 mid, Color32 dark)
        {
            c.Fill(x0, y0, x1, y1, mid);
            c.HLine(x0, x1, y1, light);
            c.VLine(x0, y0, y1, light);
            c.HLine(x0, x1, y0, dark);
            c.VLine(x1, y0, y1, dark);
        }

        static void ContactShadow(PixelCanvas c, int x0, int x1) => c.HLine(x0, x1, 0, P.Shadow);

        // ---------------------------------------------------------------- desk
        static PixelCanvas Desk()
        {
            var c = new PixelCanvas(48, 32);
            ContactShadow(c, 3, 44);
            // back modesty panel (further away, darker)
            c.Fill(19, 10, 39, 22, P.WoodD);
            c.HLine(19, 39, 10, P.WoodDD);
            // pedestal with two drawers
            Box(c, 4, 1, 18, 23, P.WoodL, P.WoodM, P.WoodD);
            DrawerFront(c, 6, 13, 16, 21);
            DrawerFront(c, 6, 3, 16, 11);
            // right leg
            c.Fill(40, 1, 43, 23, P.WoodM);
            c.VLine(40, 1, 23, P.WoodL);
            c.VLine(43, 1, 23, P.WoodD);
            // apron under the top
            c.Fill(19, 21, 39, 23, P.WoodM);
            c.HLine(19, 39, 21, P.WoodD);
            // top: front edge then visible top surface
            c.Fill(2, 24, 45, 26, P.WoodM);
            c.HLine(2, 45, 24, P.WoodD);
            c.VLine(2, 24, 26, P.WoodL);
            c.Fill(2, 27, 45, 28, P.WoodL);
            c.HLine(2, 45, 28, P.WoodHL);
            // board seam on the top surface
            c.SetPixel(24, 27, P.WoodM);
            c.Outline(P.WoodDD, (x, y) => y >= 1);
            return c;
        }

        static void DrawerFront(PixelCanvas c, int x0, int y0, int x1, int y1)
        {
            c.Fill(x0, y0, x1, y1, P.WoodL);
            c.HLine(x0, x1, y1, P.WoodHL);
            c.HLine(x0, x1, y0, P.WoodD);
            c.VLine(x1, y0, y1, P.WoodM);
            int kx = (x0 + x1) / 2, ky = (y0 + y1) / 2;
            c.SetPixel(kx, ky, P.YellowM);
            c.SetPixel(kx + 1, ky, P.YellowM);
            c.SetPixel(kx, ky - 1, P.WoodD);
            c.SetPixel(kx + 1, ky - 1, P.WoodD);
        }

        // ---------------------------------------------------------------- chair (seen from behind, facing the desk)
        static PixelCanvas Chair()
        {
            var c = new PixelCanvas(16, 32);
            ContactShadow(c, 2, 13);
            // far legs (darker)
            c.VLine(5, 1, 10, P.WoodD);
            c.VLine(10, 1, 10, P.WoodD);
            // seat: overhangs the frame by a pixel on each side so it reads as a seat
            c.Fill(1, 10, 14, 12, P.WoodM);
            c.HLine(1, 14, 12, P.WoodL);
            c.HLine(1, 14, 10, P.WoodD);
            // cushion seen between slats
            c.Fill(4, 13, 11, 15, P.SageM);
            c.HLine(4, 11, 15, P.SageL);
            // back posts
            c.Fill(2, 1, 3, 27, P.WoodM);
            c.VLine(2, 1, 27, P.WoodL);
            c.Fill(12, 1, 13, 27, P.WoodM);
            c.VLine(13, 1, 27, P.WoodD);
            // slats
            foreach (var sx in new[] { 5, 9 })
            {
                c.Fill(sx, 13, sx + 1, 24, P.WoodL);
                c.VLine(sx + 1, 13, 24, P.WoodM);
            }
            // rails
            c.Fill(2, 18, 13, 19, P.WoodM);
            c.HLine(2, 13, 19, P.WoodL);
            c.Fill(2, 25, 13, 27, P.WoodM);
            c.HLine(2, 13, 27, P.WoodHL);
            c.HLine(3, 13, 25, P.WoodD);
            c.Outline(P.WoodDD, (x, y) => y >= 1);
            return c;
        }

        // ---------------------------------------------------------------- desk lamp
        static PixelCanvas Lamp()
        {
            var c = new PixelCanvas(16, 16);
            ContactShadow(c, 4, 10);
            // base
            c.HLine(4, 10, 1, P.RoofD);
            c.HLine(5, 9, 2, P.RoofM);
            c.SetPixel(5, 2, P.RoofL);
            // arm
            c.Line(7, 3, 5, 8, P.RoofD);
            c.Line(5, 8, 8, 11, P.RoofD);
            c.SetPixel(5, 8, P.RoofL);
            // shade (opening faces down-right)
            c.FillPolygon(P.CoralM, 6, 9, 14, 9, 12, 14, 8, 14);
            c.Line(7, 10, 8, 13, P.CoralL);
            c.HLine(9, 11, 13, P.CoralL);
            c.HLine(7, 13, 9, P.CoralD);
            c.Line(12, 10, 12, 13, P.CoralD);
            // bulb glow under the shade
            c.SetPixel(10, 8, P.YellowL);
            c.SetPixel(11, 8, P.Warm);
            c.Outline(P.Ink, (x, y) => y >= 1);
            return c;
        }

        // ---------------------------------------------------------------- rug (seen almost edge-on)
        static PixelCanvas Rug()
        {
            var c = new PixelCanvas(64, 16);
            ContactShadow(c, 5, 58);
            // thickness
            c.Fill(4, 1, 59, 2, P.CoralD);
            for (int x = 6; x <= 57; x += 4) c.SetPixel(x, 2, P.CreamD);
            // top surface: border, pattern row, border
            c.HLine(4, 59, 3, P.CreamM);
            c.HLine(4, 59, 4, P.CoralM);
            c.HLine(4, 59, 5, P.CreamL);
            for (int x = 8; x <= 56; x += 6)
            {
                c.SetPixel(x - 1, 4, P.SageM);
                c.SetPixel(x, 4, P.YellowL);
                c.SetPixel(x + 1, 4, P.SageM);
            }
            // fringe
            foreach (var fx in new[] { 2, 3, 60, 61 })
                for (int y = 1; y <= 5; y += 2)
                    c.SetPixel(fx, y, P.CreamM);
            c.Outline(P.WoodDD, (x, y) => y >= 1 && x >= 4 && x <= 59);
            return c;
        }

        // ---------------------------------------------------------------- small plant
        static PixelCanvas SmallPlant()
        {
            var c = new PixelCanvas(16, 16);
            ContactShadow(c, 5, 10);
            // terracotta pot
            c.FillPolygon(P.CoralM, 5, 1, 11, 1, 12, 6, 4, 6);
            c.VLine(5, 1, 5, P.CoralL);
            c.Line(10, 1, 11, 5, P.CoralD);
            c.HLine(5, 10, 1, P.CoralD);
            c.Fill(3, 6, 12, 7, P.CoralM);
            c.HLine(3, 12, 7, P.CoralL);
            c.SetPixel(12, 6, P.CoralD);
            c.HLine(4, 11, 8, P.WoodDD);
            // three leaf clusters
            LeafCluster(c, 2, 8, 7, 12);
            LeafCluster(c, 8, 8, 13, 12);
            LeafCluster(c, 5, 10, 10, 15);
            c.Outline(P.LeafD, (x, y) => y >= 8);
            c.Outline(P.WoodDD, (x, y) => y >= 1 && y < 8);
            return c;
        }

        /// <summary>Clustered leaf blob: dark base, mid body, light top-left, one sparkle.</summary>
        internal static void LeafCluster(PixelCanvas c, int x0, int y0, int x1, int y1)
        {
            c.FillOval(x0, y0, x1, y1, P.LeafD);
            c.FillOval(x0, y0 + 1, x1 - 1, y1, P.LeafM);
            int mx = (x0 + x1) / 2, my = (y0 + y1) / 2;
            c.FillOval(x0 + 1, my, mx, y1 - 1, P.LeafL);
            c.SetPixel(x0 + 2, y1 - 1, P.LeafHL);
        }

        // ---------------------------------------------------------------- bookshelf
        static PixelCanvas Bookshelf()
        {
            var c = new PixelCanvas(32, 48);
            ContactShadow(c, 3, 28);
            c.Fill(4, 4, 27, 43, P.WoodD);                 // back panel
            c.Fill(2, 1, 3, 44, P.WoodM); c.VLine(2, 1, 44, P.WoodL);
            c.Fill(28, 1, 29, 44, P.WoodM); c.VLine(29, 1, 44, P.WoodDD);
            c.Fill(2, 1, 29, 3, P.WoodM); c.HLine(2, 29, 1, P.WoodD); c.HLine(4, 27, 3, P.WoodL);
            foreach (var sy in new[] { 16, 30 })
            {
                c.HLine(4, 27, sy, P.WoodM);
                c.HLine(4, 27, sy + 1, P.WoodL);
            }
            c.Fill(1, 44, 30, 46, P.WoodL);
            c.HLine(1, 30, 46, P.WoodHL);
            c.HLine(1, 30, 44, P.WoodM);

            // top compartment: upright books, one leaning, a small vase
            Book(c, 4, 32, 3, 10, P.CoralM, P.CoralL, P.CoralD);
            Book(c, 7, 32, 2, 11, P.SageD, P.SageM, P.LeafD);
            Book(c, 9, 32, 3, 9, P.YellowM, P.YellowL, P.WoodM);
            Book(c, 12, 32, 2, 11, P.RoofM, P.RoofL, P.RoofD);
            Book(c, 14, 32, 3, 10, P.CreamM, P.CreamL, P.CreamD);
            c.FillPolygon(P.CoralD, 17, 32, 19, 32, 23, 40, 21, 41);
            c.Line(18, 33, 21, 39, P.CoralM);
            c.Fill(23, 32, 26, 36, P.SageM); c.VLine(23, 32, 36, P.SageL); c.VLine(26, 32, 36, P.SageD);
            c.Fill(24, 37, 25, 37, P.SageD);
            c.SetPixel(24, 38, P.LeafM); c.SetPixel(25, 39, P.LeafL); c.SetPixel(23, 39, P.LeafM);

            // middle compartment: a stack lying down, then upright books
            c.Fill(5, 18, 13, 19, P.CoralD); c.HLine(5, 13, 19, P.CoralM);
            c.Fill(6, 20, 12, 21, P.SageM); c.HLine(6, 12, 21, P.SageL);
            c.Fill(6, 22, 11, 23, P.YellowM); c.HLine(6, 11, 23, P.YellowL);
            Book(c, 15, 18, 3, 11, P.CoralM, P.CoralL, P.CoralD);
            Book(c, 18, 18, 2, 10, P.LeafM, P.LeafL, P.LeafD);
            Book(c, 20, 18, 3, 11, P.RoofL, P.CreamL, P.RoofM);
            Book(c, 23, 18, 2, 9, P.WoodL, P.WoodHL, P.WoodM);
            Book(c, 25, 18, 2, 10, P.CoralD, P.CoralM, P.WoodDD);

            // bottom compartment: woven basket and books
            c.Fill(5, 4, 14, 10, P.WoodL);
            for (int y = 5; y <= 9; y += 2)
                for (int x = 5 + (y / 2) % 2; x <= 14; x += 2) c.SetPixel(x, y, P.WoodM);
            c.HLine(5, 14, 10, P.WoodHL);
            c.HLine(5, 14, 4, P.WoodD);
            Book(c, 16, 4, 3, 11, P.SageM, P.SageL, P.SageD);
            Book(c, 19, 4, 2, 10, P.YellowM, P.YellowL, P.WoodM);
            Book(c, 21, 4, 3, 11, P.CoralM, P.CoralL, P.CoralD);
            Book(c, 24, 4, 3, 9, P.RoofM, P.RoofL, P.RoofD);
            c.Outline(P.WoodDD, (x, y) => y >= 1);
            return c;
        }

        /// <summary>Upright book: spine colour, lit left edge and top, dark right edge, a label band.</summary>
        static void Book(PixelCanvas c, int x, int y, int w, int h, Color32 mid, Color32 light, Color32 dark)
        {
            c.Fill(x, y, x + w - 1, y + h - 1, mid);
            c.VLine(x, y, y + h - 1, light);
            c.HLine(x, x + w - 1, y + h - 1, light);
            if (w > 2) c.VLine(x + w - 1, y, y + h - 2, dark);
            c.HLine(x + 1, x + w - 1, y + h - 4, dark);
        }

        // ---------------------------------------------------------------- framed painting
        static PixelCanvas Painting()
        {
            var c = new PixelCanvas(32, 32);
            // hanging string and nail
            c.Line(7, 26, 15, 29, P.WoodD);
            c.Line(24, 26, 16, 29, P.WoodD);
            c.SetPixel(15, 29, P.GreyD); c.SetPixel(16, 29, P.GreyD);
            // frame
            c.Fill(3, 5, 28, 26, P.WoodM);
            c.HLine(3, 28, 26, P.WoodHL); c.VLine(3, 5, 26, P.WoodL);
            c.HLine(3, 28, 5, P.WoodD); c.VLine(28, 5, 26, P.WoodD);
            // canvas: little landscape that echoes the world outside
            c.Fill(5, 7, 26, 24, P.SkyLow);
            c.Fill(5, 21, 26, 24, P.SkyMid);
            c.Fill(8, 18, 13, 19, P.CloudW); c.Fill(10, 20, 12, 20, P.CloudW); c.HLine(8, 13, 18, P.CloudM);
            c.Fill(21, 19, 22, 20, P.YellowL); c.SetPixel(22, 19, P.YellowM);
            c.FillPolygon(P.FarM, 5, 7, 5, 12, 11, 15, 17, 11, 26, 13, 26, 7);
            c.FillOval(3, 4, 18, 13, P.LeafL);
            c.FillOval(14, 3, 30, 11, P.LeafM);
            c.Fill(5, 7, 26, 7, P.LeafM);
            c.VLine(9, 11, 14, P.WoodD); c.FillOval(7, 13, 11, 16, P.LeafD); c.SetPixel(8, 15, P.LeafM);
            // re-draw the frame over the canvas edges so the hills stay inside
            c.HLine(4, 27, 6, P.WoodM); c.HLine(4, 27, 25, P.WoodM);
            c.VLine(4, 6, 25, P.WoodM); c.VLine(27, 6, 25, P.WoodM);
            for (int x = 0; x < 32; x++)
                for (int y = 0; y < 32; y++)
                    if ((x < 3 || x > 28 || y < 5) && c.IsOpaque(x, y) && y < 26) c.SetPixel(x, y, P.Clear);
            c.HLine(3, 28, 5, P.WoodD); c.VLine(28, 5, 26, P.WoodD);
            c.VLine(3, 5, 26, P.WoodL); c.HLine(3, 28, 26, P.WoodHL);
            c.Outline(P.WoodDD, (x, y) => y <= 27);
            return c;
        }

        // ---------------------------------------------------------------- wall clock
        static PixelCanvas Clock()
        {
            var c = new PixelCanvas(16, 16);
            c.FillEllipse(8, 8, 6, 6, P.WoodM);
            c.Recolor((x, y) => (x - 8) + (8 - y) < -5, P.WoodL);       // upper-left rim
            c.Recolor((x, y) => (x - 8) + (8 - y) > 5, P.WoodD);        // lower-right rim
            c.FillEllipse(8, 8, 4, 4, P.CreamL);
            c.Recolor((x, y) => (x - 8) * (x - 8) + (y - 8) * (y - 8) <= 20 && (x - 8) - (y - 8) >= 5, P.CreamM);
            c.SetPixel(8, 12, P.WoodD); c.SetPixel(12, 8, P.WoodD); c.SetPixel(8, 4, P.WoodD); c.SetPixel(4, 8, P.WoodD);
            c.Line(8, 8, 8, 11, P.Ink);
            c.Line(8, 8, 6, 9, P.Ink);
            c.SetPixel(8, 8, P.CoralD);
            c.Outline(P.WoodDD);
            return c;
        }

        // ---------------------------------------------------------------- bed
        static PixelCanvas Bed()
        {
            var c = new PixelCanvas(64, 32);
            ContactShadow(c, 4, 59);
            // legs
            foreach (var lx in new[] { 10, 51 })
            {
                c.Fill(lx, 1, lx + 2, 3, P.WoodM);
                c.VLine(lx, 1, 3, P.WoodL); c.VLine(lx + 2, 1, 3, P.WoodD);
            }
            // side rail
            c.Fill(8, 4, 56, 9, P.WoodM);
            c.HLine(8, 56, 9, P.WoodL);
            c.HLine(8, 56, 4, P.WoodD);
            c.HLine(10, 54, 6, P.WoodD);
            // mattress
            c.Fill(9, 10, 54, 14, P.CreamL);
            c.HLine(9, 54, 10, P.CreamD);
            c.HLine(9, 54, 11, P.CreamM);
            // pillow
            c.FillOval(9, 13, 20, 20, P.CreamL);
            c.Recolor((x, y) => x >= 9 && x <= 20 && y >= 13 && y <= 15, P.CreamM);
            c.HLine(11, 18, 13, P.CreamD);
            c.SetPixel(11, 19, P.Warm);
            // blanket with folded sheet band and quilt stitches, draping over the rail
            c.FillPolygon(P.SageM, 21, 7, 56, 7, 56, 18, 23, 18);
            c.HLine(23, 56, 18, P.SageL);
            c.Fill(21, 7, 56, 9, P.SageD);
            c.Fill(21, 10, 24, 18, P.CreamM);
            c.VLine(21, 10, 17, P.CreamL);
            c.HLine(22, 24, 18, P.CreamL);
            for (int x = 31; x <= 52; x += 7)
                for (int y = 11; y <= 17; y += 2) c.SetPixel(x, y, P.SageD);
            for (int x = 27; x <= 54; x += 2) c.SetPixel(x, 14, P.SageL);
            // headboard (left) and footboard (right)
            Board(c, 3, 8, 28);
            Board(c, 55, 60, 18);
            c.Outline(P.WoodDD, (x, y) => y >= 1);
            return c;
        }

        static void Board(PixelCanvas c, int x0, int x1, int top)
        {
            c.Fill(x0, 1, x1, top, P.WoodM);
            c.VLine(x0, 1, top, P.WoodL);
            c.VLine(x1, 1, top, P.WoodD);
            c.HLine(x0 + 1, x1 - 1, top, P.WoodHL);
            c.SetPixel(x0, top, P.Clear);
            c.SetPixel(x1, top, P.Clear);
            c.Fill(x0 + 2, top - 13, x1 - 2, top - 3, P.WoodD);
            c.VLine(x0 + 2, top - 13, top - 3, P.WoodDD);
        }

        // ---------------------------------------------------------------- cat bed
        static PixelCanvas CatBed()
        {
            var c = new PixelCanvas(32, 16);
            // back rim, visible above the cushion
            c.FillOval(5, 5, 26, 12, P.CoralM);
            c.Recolor((x, y) => y >= 11 && x < 20, P.CoralL);
            c.Recolor((x, y) => y >= 9 && x >= 24, P.CoralD);
            // cushion
            c.FillOval(8, 5, 23, 10, P.CreamM);
            c.HLine(11, 16, 9, P.CreamL);
            c.HLine(12, 20, 6, P.CreamD);
            // front rim: lower half of a wide bowl
            c.FillOval(3, -3, 28, 7, P.CoralM);
            c.HLine(0, 31, 0, P.Clear);
            c.Recolor((x, y) => y == 7 && x < 16, P.CoralL);
            c.Recolor((x, y) => y <= 2 || (x >= 26 && y <= 6), P.CoralD);
            // stitching along the rim
            for (int x = 6; x <= 25; x += 2) c.SetPixel(x, 4, P.CreamL);
            ContactShadow(c, 5, 26);
            c.Outline(P.Ink, (x, y) => y >= 1);
            return c;
        }

        static bool Equal(Color32 a, Color32 b) => PixelCanvas.Same(a, b);

        // ---------------------------------------------------------------- chest of drawers
        static PixelCanvas Drawer()
        {
            var c = new PixelCanvas(32, 32);
            ContactShadow(c, 3, 28);
            c.Fill(4, 1, 5, 2, P.WoodD); c.Fill(26, 1, 27, 2, P.WoodD);
            Box(c, 3, 3, 28, 25, P.WoodL, P.WoodM, P.WoodD);
            DrawerFront(c, 5, 18, 26, 23);
            DrawerFront(c, 5, 11, 26, 16);
            DrawerFront(c, 5, 4, 26, 9);
            c.Fill(2, 26, 29, 28, P.WoodL);
            c.HLine(2, 29, 28, P.WoodHL);
            c.HLine(2, 29, 26, P.WoodM);
            c.Outline(P.WoodDD, (x, y) => y >= 1);
            return c;
        }

        // ---------------------------------------------------------------- big plant
        static PixelCanvas BigPlant()
        {
            var c = new PixelCanvas(32, 48);
            ContactShadow(c, 9, 22);
            // stems first so leaves overlap them
            c.Line(15, 13, 11, 26, P.LeafD);
            c.Line(16, 13, 21, 28, P.LeafD);
            c.Line(15, 13, 14, 38, P.LeafD);
            c.Line(16, 13, 19, 34, P.LeafD);
            BigLeaf(c, 3, 18, 13, 27);
            BigLeaf(c, 17, 20, 28, 29);
            BigLeaf(c, 6, 28, 16, 37);
            BigLeaf(c, 16, 30, 26, 39);
            BigLeaf(c, 10, 36, 21, 45);
            // glazed cream pot with a sage band
            c.FillPolygon(P.CreamM, 10, 1, 22, 1, 24, 11, 8, 11);
            c.Line(9, 3, 9, 10, P.CreamL);
            c.Line(21, 1, 23, 10, P.CreamD);
            c.Fill(9, 5, 23, 6, P.SageM);
            c.HLine(9, 23, 5, P.SageD);
            c.HLine(10, 21, 1, P.CreamD);
            c.Fill(7, 11, 24, 13, P.CreamM);
            c.HLine(7, 24, 13, P.CreamL);
            c.SetPixel(24, 11, P.CreamD); c.SetPixel(24, 12, P.CreamD);
            c.HLine(8, 23, 14, P.WoodDD);
            c.Outline(P.LeafD, (x, y) => y >= 15);
            c.Outline(P.WoodDD, (x, y) => y >= 1 && y < 15);
            return c;
        }

        static void BigLeaf(PixelCanvas c, int x0, int y0, int x1, int y1)
        {
            c.FillOval(x0, y0, x1, y1, P.LeafD);
            c.FillOval(x0, y0 + 1, x1 - 1, y1, P.LeafM);
            c.FillOval(x0 + 1, (y0 + y1) / 2, (x0 + x1) / 2, y1 - 1, P.LeafL);
            c.Line(x0 + 2, y0 + 2, x1 - 2, y1 - 2, P.LeafD);
            c.SetPixel(x0 + 2, y1 - 2, P.LeafHL);
            c.SetPixel(x0 + 3, y1 - 1, P.LeafHL);
        }
    }
}
