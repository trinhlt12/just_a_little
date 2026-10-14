namespace OneLittleStep.EditorTools.Art
{
    /// <summary>16x16 UI icons and the 9-slice frames used by the screen-resolution UI.</summary>
    public static class IconArt
    {
        public static readonly string[] Icons =
        {
            "icon_wood", "icon_pause", "icon_play", "icon_note", "icon_stop", "icon_decor",
            "icon_gear", "icon_check", "icon_back", "icon_lock", "icon_box", "icon_moon", "icon_slot",
        };

        public static PixelCanvas Draw(string name)
        {
            var c = new PixelCanvas(16, 16);
            switch (name)
            {
                case "icon_wood":
                    // a short log: bark body and a ringed end grain
                    c.Fill(2, 4, 11, 11, P.WoodM);
                    c.HLine(2, 11, 11, P.WoodL);
                    c.HLine(2, 11, 4, P.WoodD);
                    c.HLine(4, 9, 8, P.WoodD); c.HLine(3, 7, 6, P.WoodD);
                    c.FillOval(10, 4, 14, 11, P.WoodL);
                    c.FillOval(11, 6, 13, 9, P.WoodHL);
                    c.SetPixel(12, 7, P.WoodM); c.SetPixel(12, 8, P.WoodM);
                    c.SetPixel(3, 12, P.LeafL); c.SetPixel(4, 13, P.LeafM); c.SetPixel(4, 12, P.LeafM);
                    c.Outline(P.WoodDD);
                    break;
                case "icon_pause":
                    c.Fill(4, 3, 6, 12, P.Ink); c.Fill(9, 3, 11, 12, P.Ink);
                    break;
                case "icon_play":
                    c.FillPolygon(P.Ink, 5, 3, 5, 13, 12, 8);
                    break;
                case "icon_note":
                    c.Fill(3, 2, 11, 13, P.CreamL);
                    c.RectOutline(3, 2, 9, 12, P.Ink);
                    for (int y = 5; y <= 10; y += 2) c.HLine(5, 9, y, P.GreyM);
                    c.Line(9, 3, 14, 8, P.CoralM); c.Line(10, 3, 14, 7, P.CoralD);
                    c.SetPixel(8, 2, P.Ink); c.SetPixel(14, 9, P.Ink);
                    break;
                case "icon_stop":
                    c.Fill(4, 4, 11, 11, P.Ink);
                    c.Fill(5, 5, 10, 10, P.CoralM);
                    break;
                case "icon_decor":
                    // hammer
                    c.Line(4, 2, 10, 8, P.WoodM); c.Line(5, 2, 11, 8, P.WoodD);
                    c.FillPolygon(P.GreyD, 7, 10, 11, 14, 14, 11, 10, 7);
                    c.Line(8, 10, 11, 13, P.GreyL);
                    c.Outline(P.Ink);
                    break;
                case "icon_gear":
                    c.FillEllipse(8, 8, 5, 5, P.GreyD);
                    foreach (var (dx, dy) in new[] { (0, 6), (0, -6), (6, 0), (-6, 0), (4, 4), (-4, 4), (4, -4), (-4, -4) })
                        c.Fill(8 + dx - (dx > 0 ? 1 : 0), 8 + dy - (dy > 0 ? 1 : 0), 8 + dx, 8 + dy, P.GreyD);
                    c.FillEllipse(8, 8, 2, 2, P.Clear);
                    break;
                case "icon_check":
                    c.Line(3, 8, 6, 5, P.SageD); c.Line(3, 9, 6, 6, P.SageD);
                    c.Line(6, 5, 13, 12, P.SageD); c.Line(6, 6, 13, 13, P.SageD);
                    break;
                case "icon_back":
                    c.Line(3, 8, 8, 13, P.Ink); c.Line(3, 8, 8, 3, P.Ink);
                    c.Line(4, 8, 9, 13, P.Ink); c.Line(4, 8, 9, 3, P.Ink);
                    c.Fill(4, 7, 13, 8, P.Ink);
                    break;
                case "icon_lock":
                    c.Fill(5, 8, 10, 12, P.Clear);
                    c.RectOutline(5, 8, 6, 6, P.GreyD);
                    c.Fill(3, 2, 12, 8, P.YellowM);
                    c.HLine(3, 12, 8, P.YellowL);
                    c.SetPixel(7, 5, P.Ink); c.SetPixel(8, 5, P.Ink); c.SetPixel(7, 4, P.Ink);
                    c.Outline(P.WoodDD);
                    break;
                case "icon_box":
                    c.Fill(2, 2, 13, 9, P.WoodL);
                    c.FillPolygon(P.WoodHL, 2, 10, 13, 10, 11, 13, 4, 13);
                    c.HLine(2, 13, 9, P.WoodM);
                    c.Fill(6, 5, 9, 6, P.CreamL);
                    c.Outline(P.WoodDD);
                    break;
                case "icon_moon":
                    c.FillEllipse(8, 8, 5, 5, P.YellowL);
                    c.FillEllipse(10, 10, 4, 4, P.Clear);
                    c.SetPixel(12, 4, P.YellowM); c.SetPixel(13, 6, P.YellowL);
                    c.Outline(P.WoodD);
                    break;
                case "icon_slot":
                    // dashed placement marker with a plus
                    for (int i = 1; i <= 14; i++)
                        if ((i / 2) % 2 == 0)
                        {
                            c.SetPixel(i, 1, P.White); c.SetPixel(i, 14, P.White);
                            c.SetPixel(1, i, P.White); c.SetPixel(14, i, P.White);
                        }
                    c.Fill(7, 4, 8, 11, P.White); c.Fill(4, 7, 11, 8, P.White);
                    c.Outline(P.Ink);
                    break;
                default:
                    throw new System.ArgumentException(name);
            }
            return c;
        }

        /// <summary>White rounded panel with a soft edge; tinted by Image.color at runtime. 9-slice border = 5.</summary>
        public static PixelCanvas Panel()
        {
            var c = new PixelCanvas(16, 16);
            c.Fill(1, 0, 14, 15, P.UiEdge);
            c.Fill(0, 1, 15, 14, P.UiEdge);
            c.Fill(2, 1, 13, 14, P.White);
            c.Fill(1, 2, 14, 13, P.White);
            return c;
        }

        /// <summary>Button: rounded white face with a darker lip at the bottom for a pressable look.</summary>
        public static PixelCanvas Button()
        {
            var c = new PixelCanvas(16, 16);
            c.Fill(1, 0, 14, 15, P.GreyM);
            c.Fill(0, 1, 15, 14, P.GreyM);
            c.Fill(2, 3, 13, 14, P.White);
            c.Fill(1, 4, 14, 13, P.White);
            c.HLine(2, 13, 2, P.GreyL);
            c.Fill(1, 3, 14, 3, P.GreyL);
            c.HLine(2, 13, 3, P.White);
            return c;
        }
    }
}
