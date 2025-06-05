using Terraria;
using TShockAPI;
using Terraria.ObjectData;
using static SurfaceBlock.SurfaceBlock;

namespace SurfaceBlock;

internal class Tool
{
    #region 获取世界属性 判断世界大小
    public static int GetWorldSize()
    {
        if (Main.maxTilesX == 8400 && Main.maxTilesY == 2400)
        {
            return 3;
        }
        if (Main.maxTilesX == 6400 && Main.maxTilesY == 1800)
        {
            return 2;
        }
        if (Main.maxTilesX == 4200 && Main.maxTilesY == 1200)
        {
            return 1;
        }
        return 0;
    }

    //颠倒世界
    public static void RemixWorld(GetDataHandlers.NewProjectileEventArgs e, double num)
    {
        if (e.Position.Y > Main.worldSurface * num)
        {
            Remover(e);
        }
    }
    #endregion

    #region 获取图格回滚大小方法(TS里抄来的)
    public static void GetRollbackSize(int tileX, int tileY, out byte width, out byte length, out int offsetY)
    {
        byte topWidth = 0, topLength = 0, botWidth = 0, botLength = 0;

        CheckForTileObjectsAbove(tileY, out topWidth, out topLength, out offsetY);
        CheckForTileObjectsBelow(tileY, out botWidth, out botLength);

        width = Math.Max((byte)1, Math.Max(topWidth, botWidth));
        length = Math.Max((byte)1, (byte)(topLength + botLength));

        void CheckForTileObjectsAbove(int y, out byte objWidth, out byte objLength, out int yOffset)
        {
            objWidth = 0;
            objLength = 0;
            yOffset = 0;

            if (y <= 0) return;

            ITile above = Main.tile[tileX, y - 1];
            if (above.type < TileObjectData._data.Count && TileObjectData._data[above.type] != null)
            {
                var data = TileObjectData._data[above.type];
                objWidth = (byte)data.Width;
                objLength = (byte)data.Height;
                yOffset = -data.Height;
            }
        }

        void CheckForTileObjectsBelow(int y, out byte objWidth, out byte objLength)
        {
            objWidth = 0;
            objLength = 0;

            if (y >= Main.maxTilesY - 1) return;

            ITile below = Main.tile[tileX, y + 1];
            if (below.type < TileObjectData._data.Count && TileObjectData._data[below.type] != null)
            {
                var data = TileObjectData._data[below.type];
                objWidth = (byte)data.Width;
                objLength = (byte)data.Height;
            }
        }
    }
    #endregion
}
