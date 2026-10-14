namespace OneLittleStep.EditorTools.Art
{
    /// <summary>
    /// Dark-grey cat with a cream belly and bright eyes, 32x32 frames, pivot bottom-centre.
    /// Animation comes from parts (eyes, ears, tail, breathing) - the body never shifts as a whole.
    /// </summary>
    public static class CatArt
    {
        public enum Eyes { Open, Blink, Happy, Closed }

        public struct Pose
        {
            public Eyes eyes;
            public int earLift;     // 0..1 px
            public int tailTipX;    // tail tip offset for sitting poses
            public int tailTipY;
            public bool tailUp;     // happy tail
            public bool blush;
        }

        public static PixelCanvas[] Idle()
        {
            return new[]
            {
                Sitting(new Pose { eyes = Eyes.Open, tailTipX = 0, tailTipY = 0 }),
                Sitting(new Pose { eyes = Eyes.Open, tailTipX = -1, tailTipY = 1 }),
                Sitting(new Pose { eyes = Eyes.Blink, tailTipX = -1, tailTipY = 2 }),
                Sitting(new Pose { eyes = Eyes.Open, tailTipX = 0, tailTipY = 1 }),
            };
        }

        public static PixelCanvas[] Happy()
        {
            return new[]
            {
                Sitting(new Pose { eyes = Eyes.Happy, tailUp = true, tailTipX = 0, blush = true }),
                Sitting(new Pose { eyes = Eyes.Happy, tailUp = true, tailTipX = -1, earLift = 1, blush = true }),
                Sitting(new Pose { eyes = Eyes.Happy, tailUp = true, tailTipX = 1, earLift = 1, blush = true }),
                Sitting(new Pose { eyes = Eyes.Open, tailUp = true, tailTipX = 0 }),
            };
        }

        public static PixelCanvas[] Sleep()
        {
            return new[] { Lying(0, 0), Lying(1, 0), Lying(1, 1), Lying(0, 1) };
        }

        // ---------------------------------------------------------------- sitting (facing right, 3/4 view)
        static PixelCanvas Sitting(Pose p)
        {
            var c = new PixelCanvas(32, 32);
            c.HLine(9, 22, 0, P.Shadow);

            // tail (behind the body)
            if (p.tailUp)
            {
                c.Fill(9, 2, 10, 4, P.CatM);
                c.Line(9, 4, 8, 7, P.CatM); c.Line(10, 4, 9, 7, P.CatM);
                c.Fill(7 + p.tailTipX, 8, 8 + p.tailTipX, 13, P.CatM);
                c.Fill(7 + p.tailTipX, 12, 8 + p.tailTipX, 14, P.CatD);
                c.Line(8, 7, 7 + p.tailTipX, 8, P.CatM);
            }
            else
            {
                c.Line(10, 1, 6, 2, P.CatM); c.Line(10, 2, 6, 3, P.CatM);
                c.Line(6, 2, 5, 5 + p.tailTipY, P.CatM); c.Line(5, 3, 4, 5 + p.tailTipY, P.CatM);
                c.Fill(4 + p.tailTipX, 5 + p.tailTipY, 5 + p.tailTipX, 7 + p.tailTipY, P.CatD);
            }

            // body / haunch and chest
            c.FillOval(9, 1, 19, 11, P.CatM);
            c.FillOval(14, 1, 21, 10, P.CatM);
            c.FillOval(16, 2, 20, 8, P.Belly);
            // shading: lit back (top-left), darker haunch underside
            c.Recolor((x, y) => y >= 8 && x <= 13 && Is(c, x, y, P.CatM), P.CatL);
            c.Recolor((x, y) => y <= 3 && x <= 14 && Is(c, x, y, P.CatM), P.CatD);
            c.SetPixel(11, 7, P.CatD); c.SetPixel(12, 8, P.CatD); c.SetPixel(10, 6, P.CatD);   // back stripes
            // front paws
            c.Fill(16, 1, 17, 1, P.Belly); c.Fill(19, 1, 20, 1, P.Belly);
            c.SetPixel(18, 1, P.CatD);

            // head
            c.FillOval(13, 9, 23, 17, P.CatM);
            c.Recolor((x, y) => y >= 15 && x <= 17 && Is(c, x, y, P.CatM), P.CatL);
            // ears
            int lift = p.earLift;
            c.FillPolygon(P.CatM, 13, 15, 15, 20 + lift, 18, 16);
            c.FillPolygon(P.CatM, 18, 16, 21, 20 + lift, 23, 15);
            c.SetPixel(15, 17 + lift, P.CoralL);
            c.SetPixel(21, 17 + lift, P.CoralL);
            c.SetPixel(15, 18 + lift, P.CatL);
            // forehead stripes
            c.SetPixel(17, 16, P.CatD); c.SetPixel(19, 16, P.CatD); c.SetPixel(18, 15, P.CatD);
            // muzzle and nose
            c.Fill(18, 10, 22, 11, P.Belly);
            c.SetPixel(19, 12, P.Belly); c.SetPixel(21, 12, P.Belly);
            c.SetPixel(20, 12, P.CoralM);
            c.SetPixel(20, 11, P.CatD);
            if (p.blush) { c.SetPixel(15, 12, P.CoralL); c.SetPixel(23, 12, P.CoralL); }

            // eyes
            DrawEye(c, 16, 13, p.eyes);
            DrawEye(c, 22, 13, p.eyes);

            c.Outline(P.Ink, (x, y) => y >= 1);
            return c;
        }

        static void DrawEye(PixelCanvas c, int x, int y, Eyes e)
        {
            switch (e)
            {
                case Eyes.Open:
                    c.SetPixel(x, y + 1, P.Eye);
                    c.SetPixel(x, y, P.Ink);
                    c.SetPixel(x - 1, y + 1, P.Eye);
                    c.SetPixel(x - 1, y, P.Eye);
                    break;
                case Eyes.Blink:
                    c.SetPixel(x, y, P.Ink);
                    c.SetPixel(x - 1, y, P.Ink);
                    break;
                case Eyes.Happy:
                    c.SetPixel(x - 1, y, P.Ink);
                    c.SetPixel(x, y + 1, P.Ink);
                    c.SetPixel(x + 1, y, P.Ink);
                    break;
                case Eyes.Closed:
                    c.SetPixel(x - 1, y, P.Ink);
                    c.SetPixel(x, y, P.Ink);
                    break;
            }
        }

        // ---------------------------------------------------------------- lying, curled up asleep
        static PixelCanvas Lying(int breath, int tail)
        {
            var c = new PixelCanvas(32, 32);
            c.HLine(5, 26, 0, P.Shadow);
            // body loaf; breathing raises the back by one pixel
            c.FillOval(5, 1, 22, 9 + breath, P.CatM);
            c.Recolor((x, y) => y >= 6 + breath && x <= 14 && Is(c, x, y, P.CatM), P.CatL);
            c.Recolor((x, y) => y <= 2 && Is(c, x, y, P.CatM), P.CatD);
            c.SetPixel(10, 7 + breath, P.CatD); c.SetPixel(12, 8 + breath, P.CatD); c.SetPixel(14, 8 + breath, P.CatD);
            // head resting on paws at the front-right
            c.FillOval(17, 1, 26, 8, P.CatM);
            c.Recolor((x, y) => y >= 6 && x <= 21 && Is(c, x, y, P.CatM), P.CatL);
            c.FillPolygon(P.CatM, 18, 7, 19, 11, 21, 8);
            c.FillPolygon(P.CatM, 22, 8, 24, 11, 25, 7);
            c.SetPixel(19, 9, P.CoralL); c.SetPixel(24, 9, P.CoralL);
            // closed eyes, nose, muzzle
            c.SetPixel(19, 5, P.Ink); c.SetPixel(20, 5, P.Ink);
            c.SetPixel(23, 5, P.Ink); c.SetPixel(24, 5, P.Ink);
            c.Fill(20, 2, 24, 3, P.Belly);
            c.SetPixel(22, 4, P.CoralM);
            // paws peeking under the chin
            c.Fill(18, 1, 20, 1, P.Belly);
            // tail wrapped around the front
            c.Line(5, 2, 14, 1, P.CatM);
            c.Line(5, 3, 14, 2, P.CatM);
            c.Fill(14 + tail, 1, 16 + tail, 2, P.CatD);
            c.Outline(P.Ink, (x, y) => y >= 1);
            return c;
        }

        static bool Is(PixelCanvas c, int x, int y, UnityEngine.Color32 col) => PixelCanvas.Same(c.GetPixel(x, y), col);
    }
}
