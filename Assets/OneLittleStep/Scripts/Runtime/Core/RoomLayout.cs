using System.Collections.Generic;

namespace OneLittleStep
{
    public enum SlotCategory { Desk, Chair, Tabletop, Floor, Wall, Bed, Cabinet, BigPlant }

    public enum SpritePivot { BottomCenter, Center, BottomLeft }

    public sealed class FurnitureDef
    {
        public readonly string Id;
        public readonly string Name;
        public readonly int Price;
        public readonly SlotCategory Category;
        public readonly int Width, Height;      // sprite canvas size in pixels (multiple of 8)
        public readonly int SurfaceHeight;      // >0: tabletop items can stand this many px above the contact line

        public FurnitureDef(string id, string name, int price, SlotCategory category, int w, int h, int surface = 0)
        {
            Id = id; Name = name; Price = price; Category = category; Width = w; Height = h; SurfaceHeight = surface;
        }

        public string SpriteName => "furniture_" + Id;
        public SpritePivot Pivot => Category == SlotCategory.Wall ? SpritePivot.Center : SpritePivot.BottomCenter;
    }

    public sealed class SlotDef
    {
        public readonly string Id;
        public readonly string Name;
        public readonly SlotCategory Category;
        public readonly int X, Y;             // design-canvas pixels: bottom-centre contact point (centre for wall slots)
        public readonly int SortingOrder;
        public readonly string ParentSlot;    // tabletop slots sit on whatever stands in this slot

        public SlotDef(string id, string name, SlotCategory category, int x, int y, int order, string parent = null)
        {
            Id = id; Name = name; Category = category; X = x; Y = y; SortingOrder = order; ParentSlot = parent;
        }
    }

    public sealed class LayerDef
    {
        public readonly string Name;
        public readonly int X, Y, Width, Height, SortingOrder;

        public LayerDef(string name, int x, int y, int w, int h, int order)
        {
            Name = name; X = x; Y = y; Width = w; Height = h; SortingOrder = order;
        }
    }

    /// <summary>
    /// Single source of truth for the room: the art generator draws previews from it and the runtime places
    /// sprites with it. Coordinates are pixels on the 216x384 design canvas, origin bottom-left.
    /// </summary>
    public static class RoomLayout
    {
        // House geometry (design-canvas pixels).
        public const int HouseX = -8, HouseY = 104, HouseWidth = 232, HouseHeight = 208;
        public const int InteriorLeft = 14, InteriorRight = 202, BackWallLeft = 20, BackWallRight = 196;
        public const int FloorFront = 124, FloorBack = 132, WallTop = 248;
        public const int BackRowY = 130, FrontRowY = 126;
        public const int WindowX = 142, WindowY = 176, WindowW = 40, WindowH = 54;
        public const int CatDefaultX = 116, CatOnCatBedLift = 3, CatOnRugLift = 2;

        // Sorting orders for world sprites.
        public const int OrderSky = -100, OrderFar = -90, OrderMid = -80, OrderStreet = -70;
        public const int OrderHouseBack = -50, OrderHouseFront = 60, OrderForeground = 80, OrderCat = 50;

        public static readonly FurnitureDef[] Furniture =
        {
            new FurnitureDef("desk",       "Bàn gỗ",            2, SlotCategory.Desk,     48, 32, surface: 27),
            new FurnitureDef("chair",      "Ghế",               1, SlotCategory.Chair,    16, 32),
            new FurnitureDef("lamp",       "Đèn bàn",           1, SlotCategory.Tabletop, 16, 16),
            new FurnitureDef("rug",        "Thảm",              1, SlotCategory.Floor,    64, 16),
            new FurnitureDef("smallplant", "Chậu cây nhỏ",      1, SlotCategory.Tabletop, 16, 16),
            new FurnitureDef("bookshelf",  "Kệ sách",           3, SlotCategory.Cabinet,  32, 48, surface: 46),
            new FurnitureDef("painting",   "Tranh treo tường",  2, SlotCategory.Wall,     32, 32),
            new FurnitureDef("clock",      "Đồng hồ treo tường", 1, SlotCategory.Wall,    16, 16),
            new FurnitureDef("bed",        "Giường nhỏ",        3, SlotCategory.Bed,      64, 32),
            new FurnitureDef("catbed",     "Đệm cho mèo",       1, SlotCategory.Floor,    32, 16),
            new FurnitureDef("drawer",     "Tủ ngăn kéo",       2, SlotCategory.Cabinet,  32, 32, surface: 27),
            new FurnitureDef("bigplant",   "Chậu cây lớn",      2, SlotCategory.BigPlant, 32, 48),
        };

        public static readonly SlotDef[] Slots =
        {
            new SlotDef("cabinet_left", "Góc trái",        SlotCategory.Cabinet,  34, BackRowY, 10),
            new SlotDef("bed",          "Chỗ giường",      SlotCategory.Bed,      78, BackRowY, 11),
            new SlotDef("cabinet_mid",  "Cạnh giường",     SlotCategory.Cabinet, 124, BackRowY, 10),
            new SlotDef("desk",         "Dưới cửa sổ",     SlotCategory.Desk,    162, BackRowY, 12),
            new SlotDef("top_desk_l",   "Mặt bàn trái",    SlotCategory.Tabletop, 150, 0, 20, "desk"),
            new SlotDef("top_desk_r",   "Mặt bàn phải",    SlotCategory.Tabletop, 174, 0, 20, "desk"),
            new SlotDef("top_cab_mid",  "Trên tủ giữa",    SlotCategory.Tabletop, 124, 0, 20, "cabinet_mid"),
            new SlotDef("top_cab_left", "Trên tủ trái",    SlotCategory.Tabletop, 34, 0, 20, "cabinet_left"),
            new SlotDef("wall_left",    "Tường trái",      SlotCategory.Wall,     34, 212, 5),
            new SlotDef("wall_bed",     "Tường trên giường", SlotCategory.Wall,   78, 190, 5),
            new SlotDef("wall_mid",     "Tường giữa",      SlotCategory.Wall,    124, 214, 5),
            new SlotDef("floor_left",   "Sàn trước giường", SlotCategory.Floor,   68, FrontRowY, 30),
            new SlotDef("floor_mid",    "Sàn giữa",        SlotCategory.Floor,   116, FrontRowY, 30),
            new SlotDef("chair",        "Trước bàn",       SlotCategory.Chair,   154, FrontRowY, 36),
            new SlotDef("bigplant",     "Góc phải",        SlotCategory.BigPlant, 188, FrontRowY, 38),
        };

        /// <summary>Fixed scenery layers, back to front. X/Y is the bottom-left corner on the design canvas.</summary>
        public static readonly LayerDef[] Layers =
        {
            new LayerDef("bg_sky",      -PixelGrid.BackdropOffsetX, -PixelGrid.BackdropOffsetY, PixelGrid.BackdropWidth, PixelGrid.BackdropHeight, OrderSky),
            new LayerDef("bg_far",      -PixelGrid.BackdropOffsetX, -PixelGrid.BackdropOffsetY, PixelGrid.BackdropWidth, PixelGrid.BackdropHeight, OrderFar),
            new LayerDef("bg_mid",      -PixelGrid.BackdropOffsetX, -PixelGrid.BackdropOffsetY, PixelGrid.BackdropWidth, PixelGrid.BackdropHeight, OrderMid),
            new LayerDef("bg_street",   -PixelGrid.BackdropOffsetX, -PixelGrid.BackdropOffsetY, PixelGrid.BackdropWidth, PixelGrid.BackdropHeight, OrderStreet),
            new LayerDef("house_back",  HouseX, HouseY, HouseWidth, HouseHeight, OrderHouseBack),
            new LayerDef("house_front", HouseX, HouseY, HouseWidth, HouseHeight, OrderHouseFront),
            new LayerDef("fg_plants",   -PixelGrid.BackdropOffsetX, 64, PixelGrid.BackdropWidth, 112, OrderForeground),
        };

        static Dictionary<string, FurnitureDef> furnitureById;
        static Dictionary<string, SlotDef> slotById;

        public static FurnitureDef GetFurniture(string id)
        {
            if (furnitureById == null)
            {
                furnitureById = new Dictionary<string, FurnitureDef>();
                foreach (var f in Furniture) furnitureById[f.Id] = f;
            }
            return id != null && furnitureById.TryGetValue(id, out var d) ? d : null;
        }

        public static SlotDef GetSlot(string id)
        {
            if (slotById == null)
            {
                slotById = new Dictionary<string, SlotDef>();
                foreach (var s in Slots) slotById[s.Id] = s;
            }
            return id != null && slotById.TryGetValue(id, out var d) ? d : null;
        }

        /// <summary>
        /// Contact point of a slot given what is currently placed (tabletop slots ride on their parent's surface).
        /// Returns false when a tabletop slot has nothing (or nothing with a surface) to stand on.
        /// </summary>
        public static bool TryGetAnchor(SlotDef slot, System.Func<string, string> itemInSlot, out int x, out int y)
        {
            x = slot.X;
            y = slot.Y;
            if (slot.ParentSlot == null) return true;
            var parentSlot = GetSlot(slot.ParentSlot);
            var parentItem = GetFurniture(itemInSlot(slot.ParentSlot));
            if (parentSlot == null || parentItem == null || parentItem.SurfaceHeight <= 0) return false;
            y = parentSlot.Y + parentItem.SurfaceHeight;
            return true;
        }
    }
}
