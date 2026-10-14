using System;
using System.Collections.Generic;
using UnityEngine;

namespace OneLittleStep.EditorTools.Art
{
    /// <summary>
    /// Integer pixel canvas, origin bottom-left (same as Unity textures). Everything is hard-edged:
    /// no blending except an explicit flat-alpha shadow colour, no anti-aliasing.
    /// </summary>
    public sealed class PixelCanvas
    {
        public readonly int W, H;
        readonly Color32[] px;

        /// <summary>Drawing-space offset: user (x, y) lands on raw pixel (x + OriginX, y + OriginY).</summary>
        public int OriginX, OriginY;

        public PixelCanvas(int w, int h)
        {
            W = w; H = h;
            px = new Color32[w * h];
        }

        public bool InBounds(int x, int y) => RawIn(x + OriginX, y + OriginY);
        bool RawIn(int rx, int ry) => rx >= 0 && ry >= 0 && rx < W && ry < H;

        public void SetPixel(int x, int y, Color32 c)
        {
            x += OriginX; y += OriginY;
            if (!RawIn(x, y)) return;
            px[y * W + x] = c;
        }

        public Color32 GetPixel(int x, int y) => InBounds(x, y) ? px[(y + OriginY) * W + x + OriginX] : default;
        public bool IsOpaque(int x, int y) => GetPixel(x, y).a > 0;

        /// <summary>Filled rectangle, inclusive of (x, y), size w x h.</summary>
        public void Rectangle(int x, int y, int w, int h, Color32 c)
        {
            for (int j = y; j < y + h; j++)
                for (int i = x; i < x + w; i++)
                    SetPixel(i, j, c);
        }

        /// <summary>Filled rectangle given two inclusive corners.</summary>
        public void Fill(int x0, int y0, int x1, int y1, Color32 c)
        {
            if (x1 < x0) (x0, x1) = (x1, x0);
            if (y1 < y0) (y0, y1) = (y1, y0);
            Rectangle(x0, y0, x1 - x0 + 1, y1 - y0 + 1, c);
        }

        public void RectOutline(int x, int y, int w, int h, Color32 c)
        {
            HLine(x, x + w - 1, y, c);
            HLine(x, x + w - 1, y + h - 1, c);
            VLine(x, y, y + h - 1, c);
            VLine(x + w - 1, y, y + h - 1, c);
        }

        public void HLine(int x0, int x1, int y, Color32 c)
        {
            if (x1 < x0) (x0, x1) = (x1, x0);
            for (int x = x0; x <= x1; x++) SetPixel(x, y, c);
        }

        public void VLine(int x, int y0, int y1, Color32 c)
        {
            if (y1 < y0) (y0, y1) = (y1, y0);
            for (int y = y0; y <= y1; y++) SetPixel(x, y, c);
        }

        /// <summary>Bresenham line.</summary>
        public void Line(int x0, int y0, int x1, int y1, Color32 c)
        {
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;
            while (true)
            {
                SetPixel(x0, y0, c);
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }
        }

        /// <summary>Polyline through consecutive points (x0,y0,x1,y1,...).</summary>
        public void Polyline(Color32 c, params int[] pts)
        {
            for (int i = 0; i + 3 < pts.Length; i += 2) Line(pts[i], pts[i + 1], pts[i + 2], pts[i + 3], c);
        }

        /// <summary>Scanline polygon fill sampling pixel centres (even-odd rule). Points are x,y pairs.</summary>
        public void FillPolygon(Color32 c, params int[] pts)
        {
            int n = pts.Length / 2;
            int minY = int.MaxValue, maxY = int.MinValue;
            for (int i = 0; i < n; i++) { minY = Math.Min(minY, pts[i * 2 + 1]); maxY = Math.Max(maxY, pts[i * 2 + 1]); }
            var xs = new List<float>();
            for (int y = minY; y <= maxY; y++)
            {
                float sy = y + 0.5f;
                xs.Clear();
                for (int i = 0; i < n; i++)
                {
                    float ax = pts[i * 2], ay = pts[i * 2 + 1];
                    float bx = pts[(i + 1) % n * 2], by = pts[(i + 1) % n * 2 + 1];
                    if ((ay <= sy && by > sy) || (by <= sy && ay > sy))
                        xs.Add(ax + (sy - ay) / (by - ay) * (bx - ax));
                }
                xs.Sort();
                for (int k = 0; k + 1 < xs.Count; k += 2)
                {
                    int xa = Mathf.CeilToInt(xs[k] - 0.5f), xb = Mathf.FloorToInt(xs[k + 1] - 0.5f);
                    for (int x = xa; x <= xb; x++) SetPixel(x, y, c);
                }
            }
        }

        /// <summary>Filled ellipse centred on pixel (cx, cy) with integer radii; symmetric and hard-edged.</summary>
        public void FillEllipse(int cx, int cy, int rx, int ry, Color32 c)
        {
            float rx2 = (rx + 0.5f) * (rx + 0.5f), ry2 = (ry + 0.5f) * (ry + 0.5f);
            for (int y = -ry; y <= ry; y++)
                for (int x = -rx; x <= rx; x++)
                    if (x * x / rx2 + y * y / ry2 <= 1f) SetPixel(cx + x, cy + y, c);
        }

        /// <summary>Ellipse whose bounding box is (x0..x1, y0..y1) inclusive; works for even sizes.</summary>
        public void FillOval(int x0, int y0, int x1, int y1, Color32 c)
        {
            float cx = (x0 + x1) / 2f, cy = (y0 + y1) / 2f;
            float rx = (x1 - x0 + 1) / 2f, ry = (y1 - y0 + 1) / 2f;
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                {
                    float dx = (x - cx) / rx, dy = (y - cy) / ry;
                    if (dx * dx + dy * dy <= 1.0f) SetPixel(x, y, c);
                }
        }

        /// <summary>Fills only pixels that are already opaque and inside the predicate (used for shading regions).</summary>
        public void Recolor(Func<int, int, bool> where, Color32 c)
        {
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                    if (px[y * W + x].a > 0 && where(x - OriginX, y - OriginY)) px[y * W + x] = c;
        }

        public void Replace(Color32 from, Color32 to)
        {
            for (int i = 0; i < px.Length; i++)
                if (Same(px[i], from)) px[i] = to;
        }

        /// <summary>Draws an outline into transparent pixels that touch opaque ones (4-neighbour).</summary>
        public void Outline(Color32 c, Func<int, int, bool> where = null)
        {
            var mark = new List<int>();
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    if (px[y * W + x].a > 0) continue;
                    if (where != null && !where(x - OriginX, y - OriginY)) continue;
                    if (IsSolid(x - 1, y) || IsSolid(x + 1, y) || IsSolid(x, y - 1) || IsSolid(x, y + 1))
                        mark.Add(y * W + x);
                }
            foreach (var i in mark) px[i] = c;
        }

        bool IsSolid(int x, int y) => RawIn(x, y) && px[y * W + x].a == 255;

        /// <summary>Copies opaque pixels of src onto this canvas at offset (alpha &lt; 255 composites as flat overlay).</summary>
        public void Blit(PixelCanvas src, int ox, int oy)
        {
            for (int y = 0; y < src.H; y++)
                for (int x = 0; x < src.W; x++)
                {
                    var c = src.px[y * src.W + x];
                    if (c.a == 0) continue;
                    int tx = ox + x, ty = oy + y;
                    if (!RawIn(tx, ty)) continue;
                    if (c.a == 255) { px[ty * W + tx] = c; continue; }
                    var d = px[ty * W + tx];
                    float a = c.a / 255f;
                    if (d.a == 0) { px[ty * W + tx] = c; continue; }
                    px[ty * W + tx] = new Color32(
                        (byte)Mathf.RoundToInt(d.r + (c.r - d.r) * a),
                        (byte)Mathf.RoundToInt(d.g + (c.g - d.g) * a),
                        (byte)Mathf.RoundToInt(d.b + (c.b - d.b) * a), 255);
                }
        }

        public PixelCanvas Scaled(int k)
        {
            var o = new PixelCanvas(W * k, H * k);
            for (int y = 0; y < o.H; y++)
                for (int x = 0; x < o.W; x++)
                    o.px[y * o.W + x] = px[(y / k) * W + x / k];
            return o;
        }

        public PixelCanvas Crop(int x, int y, int w, int h)
        {
            var o = new PixelCanvas(w, h);
            for (int j = 0; j < h; j++)
                for (int i = 0; i < w; i++)
                    o.px[j * w + i] = RawIn(x + i, y + j) ? px[(y + j) * W + x + i] : default;
            return o;
        }

        public PixelCanvas FlipX()
        {
            var o = new PixelCanvas(W, H);
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                    o.px[y * W + (W - 1 - x)] = px[y * W + x];
            return o;
        }

        public void Clear(Color32 c)
        {
            for (int i = 0; i < px.Length; i++) px[i] = c;
        }

        public IEnumerable<Color32> Pixels => px;

        public byte[] EncodePng()
        {
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            tex.SetPixels32(px);
            tex.Apply(false);
            var bytes = tex.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(tex);
            return bytes;
        }

        public static bool Same(Color32 a, Color32 b) => a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
    }
}
