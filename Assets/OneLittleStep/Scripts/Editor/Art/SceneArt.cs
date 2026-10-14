using UnityEngine;

namespace OneLittleStep.EditorTools.Art
{
    /// <summary>
    /// Fixed scenery, drawn layer by layer in design-canvas coordinates (216x384, origin bottom-left).
    /// Detail is concentrated on the house; the backdrop gets flatter and lower-contrast with distance.
    /// </summary>
    public static class SceneArt
    {
        public static PixelCanvas Draw(LayerDef layer)
        {
            var c = new PixelCanvas(layer.Width, layer.Height) { OriginX = -layer.X, OriginY = -layer.Y };
            switch (layer.Name)
            {
                case "bg_sky": Sky(c); break;
                case "bg_far": Far(c); break;
                case "bg_mid": Mid(c); break;
                case "bg_street": Street(c); break;
                case "house_back": HouseBack(c); break;
                case "house_front": HouseFront(c); break;
                case "fg_plants": Foreground(c); break;
                default: throw new System.ArgumentException(layer.Name);
            }
            return c;
        }

        const int L = -36, R = 251, Bottom = -96, Top = 479;   // backdrop extent in design coordinates

        // ================================================================ sky
        static void Sky(PixelCanvas c)
        {
            c.Fill(L, Bottom, R, Top, P.SkyTop);
            StepBand(c, 352, P.SkyMid);
            StepBand(c, 284, P.SkyLow);

            // Long thin streaks, like wind-combed cirrus.
            Streak(c, -20, 446, 34, P.SkyMid);
            Streak(c, 120, 458, 46, P.SkyMid);
            Streak(c, 40, 404, 28, P.CloudS);
            Streak(c, 170, 392, 22, P.CloudS);

            // Stepped cumulus: one bank near the horizon, a few loose clouds higher up.
            Cumulus(c, 166, 336, new[] { 0, 0, 12, 18, 22, 10, 34, 14, 46, 8 }, 9);
            Cumulus(c, 6, 352, new[] { 0, 0, 10, 12, 20, 6 }, 8);
            Cumulus(c, 96, 418, new[] { 0, 0, 12, 10, 26, 16, 40, 6 }, 8);
            Cumulus(c, -30, 288, new[] { 0, 0, 14, 16, 30, 22, 46, 12, 60, 18, 76, 8 }, 11);
            Cumulus(c, 150, 280, new[] { 0, 0, 16, 20, 32, 14, 48, 24, 64, 10, 82, 16 }, 11);
        }

        /// <summary>Fills everything below `y` with `col`, with a stepped (not smooth) upper edge.</summary>
        static void StepBand(PixelCanvas c, int y, Color32 col)
        {
            int[] steps = { 0, 2, 3, 1, -1, -2, 0, 2, 1, -2, -3, -1 };
            for (int x = L, i = 0; x <= R; x += 24, i++)
            {
                int h = y + steps[i % steps.Length];
                c.Fill(x, Bottom, x + 23, h, col);
                // a short tongue of the band reaching into the colour above
                c.HLine(x + 4, x + 15, h + 1 + (i % 2), col);
            }
        }

        static void Streak(PixelCanvas c, int x, int y, int len, Color32 col)
        {
            c.HLine(x, x + len, y, col);
            c.HLine(x + 6, x + len - 8, y + 1, col);
        }

        /// <summary>Puffs given as (dx, height) pairs above a flat base; shaded cream / pale / blue-grey underside.</summary>
        static void Cumulus(PixelCanvas c, int x, int baseY, int[] puffs, int r)
        {
            var shape = new PixelCanvas(c.W, c.H) { OriginX = c.OriginX, OriginY = c.OriginY };
            for (int i = 0; i < puffs.Length; i += 2)
            {
                int px = x + puffs[i] + r, top = baseY + puffs[i + 1] + r;
                shape.FillOval(px - r, baseY, px + r, top, P.CloudW);
            }
            int width = puffs[puffs.Length - 2] + 2 * r;
            shape.Fill(x + r / 2, baseY, x + width - r / 2, baseY + r / 2, P.CloudW);
            // stepped shading: bottom rows bluish, a band of pale above, lit tops cream
            shape.Recolor((px, py) => py <= baseY + 2, P.CloudS);
            shape.Recolor((px, py) => py > baseY + 2 && py <= baseY + 5 && (px / 3) % 4 != 0, P.CloudM);
            shape.Recolor((px, py) => py > baseY + 5 && py <= baseY + 7 && (px / 5) % 3 == 1, P.CloudM);
            c.Blit(shape, 0, 0);
        }

        // ================================================================ distant city
        static void Far(PixelCanvas c)
        {
            // soft hills across the whole backdrop, behind the buildings
            c.FillPolygon(P.FarL, L, 200, L, 262, -6, 276, 40, 270, 90, 282, 150, 272, 200, 284, R, 268, R, 200);
            // apartment blocks and shops: (x, width, height, roof style)
            Building(c, -34, 30, 352, 0);
            Building(c, -6, 22, 330, 1);
            Building(c, 18, 26, 316, 2);
            Building(c, 60, 34, 310, 0);
            Building(c, 108, 28, 314, 1);
            Building(c, 150, 30, 312, 2);
            Building(c, 186, 22, 334, 1);
            Building(c, 206, 40, 346, 0);
        }

        static void Building(PixelCanvas c, int x, int w, int top, int style)
        {
            int x1 = x + w - 1;
            c.Fill(x, 200, x1, top, P.FarM);
            c.VLine(x1, 200, top, P.FarD);                     // shaded right side
            c.HLine(x, x1, top, P.FarL);                       // lit roof edge
            // windows in a strict grid
            for (int wy = top - 6; wy > 210; wy -= 6)
                for (int wx = x + 3; wx < x1 - 2; wx += 5)
                {
                    c.SetPixel(wx, wy, P.FarL);
                    c.SetPixel(wx + 1, wy, P.FarL);
                }
            switch (style)
            {
                case 0: // water tank on the roof
                    c.Fill(x + 4, top + 1, x + 9, top + 5, P.FarD);
                    c.HLine(x + 4, x + 9, top + 5, P.FarM);
                    c.VLine(x + 5, top + 1, top + 2, P.FarM);
                    break;
                case 1: // antenna
                    c.VLine(x + w / 2, top + 1, top + 9, P.FarD);
                    c.HLine(x + w / 2 - 2, x + w / 2 + 2, top + 6, P.FarD);
                    break;
                case 2: // stepped penthouse
                    c.Fill(x + 4, top + 1, x1 - 6, top + 4, P.FarM);
                    c.HLine(x + 4, x1 - 6, top + 4, P.FarL);
                    break;
            }
        }

        // ================================================================ trees, utility pole and wires
        static void Mid(PixelCanvas c)
        {
            // left tree, partly behind the house
            Trunk(c, -14, 112, 250);
            TreeCrown(c, -34, 246, 40, 70);
            // right tree
            Trunk(c, 228, 112, 236);
            TreeCrown(c, 204, 232, 46, 62);
            // far-right shrub row peeking at the street edge
            Shrub(c, 222, 112, 30, 18);
            Shrub(c, -36, 112, 26, 16);

            // wooden utility pole with a cross-arm and two sagging wires across the sky
            int pole = 236;
            c.Fill(pole, 112, pole + 2, 356, P.GreyD);
            c.VLine(pole, 112, 356, P.GreyM);
            c.Fill(pole - 10, 344, pole + 12, 345, P.GreyD);
            c.HLine(pole - 10, pole + 12, 345, P.GreyM);
            foreach (var ix in new[] { pole - 8, pole + 10 })
            {
                c.Fill(ix, 346, ix + 1, 348, P.CreamM);
            }
            c.Fill(pole + 3, 300, pole + 6, 306, P.GreyD);   // transformer box
            c.VLine(pole + 3, 300, 306, P.GreyM);
            Wire(c, pole - 8, 348, L, 352, 9);
            Wire(c, pole + 10, 348, L, 340, 12);
        }

        static void Trunk(PixelCanvas c, int x, int y0, int y1)
        {
            c.Fill(x, y0, x + 4, y1, P.WoodD);
            c.VLine(x, y0, y1, P.WoodM);
            c.VLine(x + 4, y0, y1, P.WoodDD);
            c.Line(x + 2, y1 - 20, x - 8, y1 - 2, P.WoodD);
            c.Line(x + 3, y1 - 16, x + 12, y1 - 4, P.WoodD);
        }

        /// <summary>Crown built from overlapping clusters: dark mass, mid body, lit top-left, few highlights.</summary>
        static void TreeCrown(PixelCanvas c, int x, int y, int w, int h)
        {
            int[][] blobs =
            {
                new[] { 0, 0, 22, 26 }, new[] { 16, -4, 30, 24 }, new[] { 6, 18, 24, 22 }, new[] { 22, 22, 22, 22 },
                new[] { 12, 38, 22, 22 }, new[] { -4, 26, 18, 20 }, new[] { 30, 6, 18, 20 },
            };
            foreach (var b in blobs)
            {
                int bx = x + b[0] * w / 46, by = y + b[1] * h / 70, bw = b[2] * w / 46, bh = b[3] * h / 70;
                c.FillOval(bx, by, bx + bw, by + bh, P.LeafD);
            }
            foreach (var b in blobs)
            {
                int bx = x + b[0] * w / 46, by = y + b[1] * h / 70, bw = b[2] * w / 46, bh = b[3] * h / 70;
                c.FillOval(bx + 1, by + 3, bx + bw - 3, by + bh, P.LeafM);
            }
            foreach (var b in blobs)
            {
                int bx = x + b[0] * w / 46, by = y + b[1] * h / 70, bw = b[2] * w / 46, bh = b[3] * h / 70;
                c.FillOval(bx + 2, by + bh / 2 + 1, bx + bw / 2 + 1, by + bh - 1, P.LeafL);
                c.SetPixel(bx + 4, by + bh - 2, P.LeafHL);
                c.SetPixel(bx + 5, by + bh - 2, P.LeafHL);
                c.SetPixel(bx + 3, by + bh - 3, P.LeafHL);
            }
        }

        static void Shrub(PixelCanvas c, int x, int y, int w, int h)
        {
            c.FillOval(x, y, x + w, y + h, P.LeafD);
            c.FillOval(x + 1, y + 2, x + w - 3, y + h, P.LeafM);
            c.FillOval(x + 3, y + h / 2, x + w / 2, y + h - 1, P.LeafL);
        }

        /// <summary>Wire hanging between two points with a little sag in the middle.</summary>
        static void Wire(PixelCanvas c, int x0, int y0, int x1, int y1, int sag)
        {
            int mx = (x0 + x1) / 2, my = (y0 + y1) / 2 - sag;
            int qx0 = (x0 + mx) / 2, qy0 = (y0 + my) / 2 - sag / 3;
            int qx1 = (mx + x1) / 2, qy1 = (my + y1) / 2 - sag / 3;
            c.Polyline(P.RoofD, x0, y0, qx0, qy0, mx, my, qx1, qy1, x1, y1);
        }

        // ================================================================ street
        static void Street(PixelCanvas c)
        {
            // road
            c.Fill(L, Bottom, R, 86, P.GreyD);
            c.Fill(L, 60, R, 86, P.GreyM);
            c.HLine(L, R, 60, P.GreyD);
            for (int x = L + 6; x <= R; x += 28) c.Fill(x, 30, x + 13, 31, P.CreamL);   // dashed centre line
            c.HLine(L, R, 84, P.GreyL);
            // gutter with drain grates
            c.Fill(L, 86, R, 89, P.GreyD);
            for (int x = L + 20; x <= R; x += 64)
            {
                c.Fill(x, 86, x + 11, 89, P.RoofDD);
                for (int gx = x + 1; gx <= x + 10; gx += 2) c.VLine(gx, 87, 88, P.GreyD);
            }
            // curb
            c.Fill(L, 90, R, 93, P.GreyL);
            c.HLine(L, R, 93, P.CreamM);
            for (int x = L + 10; x <= R; x += 22) c.VLine(x, 90, 92, P.GreyM);
            // paving stones
            c.Fill(L, 94, R, 111, P.GreyL);
            for (int row = 0; row < 3; row++)
            {
                int y = 94 + row * 6;
                c.HLine(L, R, y, P.GreyM);
                for (int x = L + (row % 2) * 8; x <= R; x += 16) c.VLine(x, y, y + 5, P.GreyM);
            }
            c.HLine(L, R, 111, P.GreyM);
        }

        // ================================================================ house: interior shell (behind furniture)
        static void HouseBack(PixelCanvas c)
        {
            int l = RoomLayout.InteriorLeft, r = RoomLayout.InteriorRight;
            int bl = RoomLayout.BackWallLeft, br = RoomLayout.BackWallRight;
            const int floorFront = 126, floorBack = RoomLayout.FloorBack, wallTop = RoomLayout.WallTop, ceilFront = 254;

            // back wall plaster
            c.Fill(bl, floorBack, br, wallTop, P.CreamM);
            c.Fill(bl, 242, br, wallTop, P.CreamD);              // shadow under the ceiling
            // lower wall paneling and rails
            c.Fill(bl, floorBack, br, 150, P.CreamD);
            for (int x = bl + 8; x < br; x += 14) c.VLine(x, 135, 149, P.WoodL);
            c.HLine(bl, br, 151, P.WoodM);
            c.HLine(bl, br, 152, P.WoodHL);
            c.Fill(bl, floorBack, br, floorBack + 2, P.WoodD);    // skirting
            c.HLine(bl, br, floorBack + 2, P.WoodM);
            c.HLine(bl, br, 238, P.WoodM);                       // nageshi rail
            c.HLine(bl, br, 239, P.WoodL);

            // side walls in one-point perspective (left in shade, right lit)
            c.FillPolygon(P.CreamD, l, floorFront, bl + 1, floorBack, bl + 1, wallTop + 1, l, ceilFront);
            c.FillPolygon(P.CreamL, br, floorBack, r + 1, floorFront, r + 1, ceilFront, br, wallTop + 1);
            c.Line(l, floorFront + 2, bl, floorBack + 2, P.WoodD);
            c.Line(r, floorFront + 2, br, floorBack + 2, P.WoodD);
            c.Line(l, 151 - 4, bl, 151, P.WoodM);
            c.Line(r, 151 - 4, br, 151, P.WoodM);

            // ceiling underside with beams
            c.FillPolygon(P.WoodD, l, ceilFront + 1, bl, wallTop + 1, br + 1, wallTop + 1, r + 1, ceilFront + 1);
            for (int x = bl + 14; x < br; x += 22) c.Line(x, wallTop + 1, x + (x - 108) / 18, ceilFront, P.WoodDD);

            // floor boards (front edge 126, back edge 132)
            c.FillPolygon(P.WoodL, l, floorFront, r + 1, floorFront, br + 1, floorBack, bl, floorBack);
            c.HLine(l + 2, r - 2, 128, P.WoodM);
            c.HLine(l + 4, r - 4, 130, P.WoodM);
            for (int x = l + 6, i = 0; x < r; x += 19, i++)
            {
                c.SetPixel(x + (i % 2) * 7, 127, P.WoodM);
                c.SetPixel(x + 9 - (i % 2) * 5, 129, P.WoodM);
                c.SetPixel(x + 3 + (i % 3) * 3, 131, P.WoodM);
            }
            c.HLine(l, r, floorFront, P.WoodHL);

            Window(c);
            PendantLamp(c, 101);
            // light switch plate
            c.Fill(96, 160, 98, 163, P.CreamL);
            c.SetPixel(97, 161, P.GreyM);
        }

        static void Window(PixelCanvas c)
        {
            int x0 = RoomLayout.WindowX, y0 = RoomLayout.WindowY;
            int x1 = x0 + RoomLayout.WindowW - 1, y1 = y0 + RoomLayout.WindowH - 1;
            // view outside: sky, a cloud, distant roofs and trees
            c.Fill(x0, y0, x1, y1, P.SkyLow);
            c.Fill(x0, y1 - 12, x1, y1, P.SkyMid);
            c.Fill(x0 + 6, y0 + 26, x0 + 15, y0 + 28, P.CloudW);
            c.Fill(x0 + 9, y0 + 29, x0 + 13, y0 + 30, P.CloudW);
            c.HLine(x0 + 6, x0 + 15, y0 + 26, P.CloudM);
            c.FillPolygon(P.FarM, x0, y0, x0, y0 + 12, x0 + 10, y0 + 14, x0 + 22, y0 + 11, x1, y0 + 15, x1, y0);
            c.FillOval(x0 + 20, y0 - 2, x1 + 6, y0 + 12, P.LeafM);
            c.FillOval(x0 + 23, y0 + 6, x0 + 30, y0 + 11, P.LeafL);
            // frame, mullions, sill
            c.RectOutline(x0 - 2, y0 - 2, RoomLayout.WindowW + 4, RoomLayout.WindowH + 4, P.WoodM);
            c.RectOutline(x0 - 1, y0 - 1, RoomLayout.WindowW + 2, RoomLayout.WindowH + 2, P.WoodD);
            c.HLine(x0 - 2, x1 + 2, y1 + 2, P.WoodL);
            c.VLine(x0 - 2, y0 - 2, y1 + 2, P.WoodL);
            int mx = (x0 + x1) / 2, my = y0 + 27;
            c.VLine(mx, y0, y1, P.WoodM); c.VLine(mx + 1, y0, y1, P.WoodD);
            c.HLine(x0, x1, my, P.WoodM); c.HLine(x0, x1, my - 1, P.WoodD);
            c.Fill(x0 - 4, y0 - 4, x1 + 4, y0 - 3, P.WoodL);
            c.HLine(x0 - 4, x1 + 4, y0 - 4, P.WoodD);
            c.SetPixel(x0 + 3, y1 - 3, P.White); c.SetPixel(x0 + 4, y1 - 4, P.White); c.SetPixel(mx + 4, y1 - 3, P.White);
            // curtain rod and tied-back curtains
            c.HLine(x0 - 7, x1 + 7, y1 + 4, P.WoodDD);
            c.SetPixel(x0 - 8, y1 + 4, P.WoodM); c.SetPixel(x1 + 8, y1 + 4, P.WoodM);
            Curtain(c, x0 - 6, y1 + 3, false);
            Curtain(c, x1 - 1, y1 + 3, true);
        }

        static void Curtain(PixelCanvas c, int x, int top, bool right)
        {
            // straight drop, pinched at the tie, flaring below
            int tie = top - 30, bottom = top - 46;
            int inner = right ? x : x + 7;
            c.FillPolygon(P.SageL,
                x, top + 1, x + 8, top + 1,
                right ? x + 8 : x + 5, tie, right ? x + 3 : x, tie);
            c.FillPolygon(P.SageL,
                right ? x + 3 : x, tie, right ? x + 8 : x + 5, tie,
                right ? x + 8 : x + 6, bottom, right ? x + 2 : x, bottom);
            int fold = right ? x + 5 : x + 2;
            c.VLine(fold, tie + 2, top - 2, P.SageM);
            c.VLine(fold + (right ? -1 : 1), bottom + 1, tie - 2, P.SageM);
            c.HLine(right ? x + 3 : x, right ? x + 8 : x + 5, tie, P.CoralM);
            c.HLine(right ? x + 2 : x, right ? x + 8 : x + 6, bottom, P.SageM);
            c.Line(inner, top, inner, tie + 2, P.SageD);
        }

        static void PendantLamp(PixelCanvas c, int x)
        {
            c.VLine(x, 236, RoomLayout.WallTop + 4, P.Ink);
            c.FillPolygon(P.YellowM, x - 5, 228, x + 6, 228, x + 3, 236, x - 2, 236);
            c.Line(x - 4, 229, x - 2, 235, P.YellowL);
            c.HLine(x - 5, x + 5, 228, P.WoodM);
            c.SetPixel(x, 227, P.Warm); c.SetPixel(x + 1, 227, P.Warm);
            c.SetPixel(x, 237, P.WoodDD); c.SetPixel(x + 1, 237, P.WoodDD);
        }

        // ================================================================ house: front frame, roof, foundation (in front of furniture)
        static void HouseFront(PixelCanvas c)
        {
            // foundation stones
            c.Fill(2, 104, 213, 123, P.GreyM);
            c.HLine(2, 213, 123, P.GreyL);
            c.HLine(2, 213, 104, P.GreyD);
            c.HLine(2, 213, 113, P.GreyD);
            for (int x = 10, i = 0; x < 213; x += 16, i++)
            {
                c.VLine(x + (i % 2) * 8, 105, 112, P.GreyD);
                c.VLine(x + 4, 114, 122, P.GreyD);
            }
            c.HLine(2, 213, 114, P.GreyL);
            foreach (var vx in new[] { 28, 176 })
            {
                c.Fill(vx, 108, vx + 12, 119, P.WoodDD);
                for (int gx = vx + 1; gx < vx + 12; gx += 2) c.VLine(gx, 109, 118, P.GreyD);
                c.HLine(vx, vx + 12, 120, P.GreyL);
            }
            // stepping stone in front of the room
            c.Fill(94, 104, 122, 110, P.GreyL);
            c.HLine(94, 122, 110, P.CreamM);
            c.HLine(94, 122, 104, P.GreyM);
            c.VLine(122, 104, 110, P.GreyM);

            // engawa beam: the front edge of the floor
            c.Fill(6, 124, 210, 125, P.WoodD);
            c.HLine(6, 210, 125, P.WoodM);

            // pillars
            Pillar(c, 6);
            Pillar(c, 203);
            // mailbox on the left pillar
            c.Fill(4, 158, 12, 166, P.CoralD);
            c.Fill(4, 162, 11, 166, P.CoralM);
            c.HLine(5, 10, 164, P.Ink);
            c.HLine(4, 12, 166, P.CoralL);

            // lintel and plaster band under the eave
            c.Fill(6, 254, 210, 266, P.CreamM);
            c.Fill(6, 254, 210, 256, P.WoodM);
            c.HLine(6, 210, 256, P.WoodL);
            c.HLine(6, 210, 254, P.WoodD);
            foreach (var sx in new[] { 58, 108, 158 }) { c.VLine(sx, 257, 266, P.WoodM); c.VLine(sx + 1, 257, 266, P.WoodD); }
            c.Fill(6, 262, 210, 266, P.CreamD);                     // eave shadow

            Roof(c);

            // paper lantern hanging under the left eave
            c.VLine(22, 252, 262, P.Ink);
            c.FillOval(18, 241, 26, 252, P.CoralM);
            c.Recolor((x, y) => x <= 20 && y >= 244 && y <= 250 && x >= 18 && y <= 252, P.CoralL);
            for (int y = 243; y <= 250; y += 3) c.HLine(19, 25, y, P.CoralD);
            c.HLine(19, 25, 252, P.WoodDD); c.HLine(19, 25, 241, P.WoodDD);
            c.SetPixel(22, 240, P.YellowM);

            // rain gutter and downspout on the right
            c.Fill(211, 104, 212, 262, P.GreyM);
            c.VLine(211, 104, 262, P.GreyL);
            c.Fill(210, 104, 214, 106, P.GreyD);
        }

        static void Pillar(PixelCanvas c, int x)
        {
            c.Fill(x, 124, x + 7, 256, P.WoodM);
            c.VLine(x, 124, 256, P.WoodL);
            c.VLine(x + 1, 124, 256, P.WoodHL);
            c.VLine(x + 6, 124, 256, P.WoodD);
            c.VLine(x + 7, 124, 256, P.WoodDD);
            c.Fill(x - 1, 122, x + 8, 125, P.GreyL);      // base stone
            c.HLine(x - 1, x + 8, 122, P.GreyM);
            for (int y = 140; y < 250; y += 37) c.SetPixel(x + 3, y, P.WoodD);   // wood knots
        }

        static void Roof(PixelCanvas c)
        {
            const int eaveY = 266, ridgeY = 300;
            int el = -4, er = 219, rl = 22, rr = 193;
            // soffit with rafter ends
            c.Fill(el + 2, 263, er - 2, 266, P.WoodDD);
            for (int x = el + 6; x < er - 4; x += 8) c.Fill(x, 263, x + 2, 265, P.WoodM);
            // roof body
            c.FillPolygon(P.RoofM, el, eaveY + 2, er + 1, eaveY + 2, rr + 1, ridgeY, rl, ridgeY);
            // tile rows: lit upper lip, dark lower lip, staggered vertical joints
            for (int y = eaveY + 3, row = 0; y < ridgeY; y += 5, row++)
            {
                c.Recolor((x, yy) => yy == y + 4 && Is(c, x, yy, P.RoofM), P.RoofL);
                c.Recolor((x, yy) => yy == y && Is(c, x, yy, P.RoofM), P.RoofD);
                for (int x = el + (row % 2) * 3; x <= er; x += 6)
                    for (int yy = y + 1; yy <= y + 3; yy++)
                        if (Is(c, x, yy, P.RoofM)) c.SetPixel(x, yy, P.RoofD);
            }
            // verge tiles along the sloped edges
            c.Line(el, eaveY + 2, rl, ridgeY, P.RoofDD);
            c.Line(el + 1, eaveY + 2, rl + 1, ridgeY, P.RoofDD);
            c.Line(er + 1, eaveY + 2, rr + 1, ridgeY, P.RoofDD);
            c.Line(er, eaveY + 2, rr, ridgeY, P.RoofDD);
            // eave: row of round tile ends
            c.Fill(el, eaveY - 1, er + 1, eaveY + 2, P.RoofD);
            for (int x = el; x <= er - 2; x += 6)
            {
                c.FillOval(x, eaveY - 2, x + 4, eaveY + 3, P.RoofD);
                c.SetPixel(x + 1, eaveY + 2, P.RoofL);
                c.SetPixel(x + 2, eaveY + 3, P.RoofL);
                c.SetPixel(x + 3, eaveY - 1, P.RoofDD);
            }
            c.HLine(el, er + 1, eaveY - 3, P.RoofDD);
            // ridge with end ornaments
            c.Fill(rl - 2, ridgeY, rr + 3, ridgeY + 5, P.RoofDD);
            c.HLine(rl - 2, rr + 3, ridgeY + 5, P.RoofM);
            c.HLine(rl - 2, rr + 3, ridgeY + 4, P.RoofD);
            for (int x = rl + 4; x < rr; x += 10) c.SetPixel(x, ridgeY + 2, P.RoofD);
            foreach (var ox in new[] { rl - 6, rr + 1 })
            {
                c.FillOval(ox, ridgeY - 1, ox + 7, ridgeY + 10, P.RoofD);
                c.FillOval(ox + 1, ridgeY + 4, ox + 4, ridgeY + 9, P.RoofL);
                c.SetPixel(ox + 3, ridgeY + 3, P.RoofDD);
            }
        }

        // ================================================================ foreground planters
        static void Foreground(PixelCanvas c)
        {
            // left: long wooden planter box with flowers
            c.Fill(-30, 104, 16, 114, P.WoodM);
            c.HLine(-30, 16, 114, P.WoodL);
            c.HLine(-30, 16, 104, P.WoodD);
            c.VLine(16, 104, 114, P.WoodD);
            for (int x = -24; x < 16; x += 10) c.VLine(x, 105, 113, P.WoodD);
            Shrub(c, -32, 112, 22, 16);
            Shrub(c, -14, 112, 18, 13);
            Shrub(c, 0, 112, 14, 11);
            Flower(c, -26, 124, P.CloudW); Flower(c, -18, 127, P.CoralL); Flower(c, -8, 122, P.CloudW);
            Flower(c, 4, 120, P.CoralL); Flower(c, -2, 125, P.CloudW);

            // right: glazed pot with tall grass and a small terracotta pot
            c.FillPolygon(P.RoofM, 216, 104, 232, 104, 234, 118, 214, 118);
            c.VLine(215, 106, 117, P.RoofL);
            c.Fill(213, 118, 235, 120, P.RoofL);
            c.HLine(213, 235, 118, P.RoofD);
            c.Line(232, 104, 234, 117, P.RoofD);
            foreach (var g in new[] { 216, 219, 222, 225, 228, 231 })
            {
                c.Line(g, 121, g - 3 + (g % 3) * 2, 146 - (g % 4) * 3, P.LeafM);
                c.Line(g + 1, 121, g - 1 + (g % 3) * 2, 140 - (g % 4) * 2, P.LeafL);
            }
            c.FillPolygon(P.CoralM, 202, 104, 210, 104, 211, 111, 201, 111);
            c.Fill(200, 111, 212, 112, P.CoralL);
            c.VLine(209, 104, 110, P.CoralD);
            Shrub(c, 201, 112, 11, 8);
            Flower(c, 205, 119, P.YellowL);

            // grass tufts and little white flowers along the pavement edge
            int[] tufts = { -30, -6, 30, 60, 90, 128, 150, 186, 240 };
            foreach (var t in tufts)
            {
                c.Line(t, 100, t - 2, 104, P.LeafM);
                c.Line(t + 1, 100, t + 1, 105, P.LeafL);
                c.Line(t + 2, 100, t + 4, 103, P.LeafM);
            }
            Flower(c, 34, 103, P.CloudW); Flower(c, 132, 102, P.CloudW); Flower(c, 190, 103, P.CloudW);
        }

        /// <summary>Five-pixel cross flower with a yellow heart, like the meadow flowers in the reference.</summary>
        static void Flower(PixelCanvas c, int x, int y, Color32 petal)
        {
            c.SetPixel(x, y + 1, petal); c.SetPixel(x, y - 1, petal);
            c.SetPixel(x - 1, y, petal); c.SetPixel(x + 1, y, petal);
            c.SetPixel(x, y, P.YellowM);
        }

        static bool Is(PixelCanvas c, int x, int y, Color32 col) => PixelCanvas.Same(c.GetPixel(x, y), col);
    }
}
