﻿using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using static SurfaceBlock.Tool;

namespace SurfaceBlock;

[ApiVersion(2, 1)]
public class SurfaceBlock : TerrariaPlugin
{
    #region 插件信息
    public override string Name => "禁地表弹幕";
    public override string Author => "羽学 Cai 西江小子 熙恩";
    public override string Description => "禁止特定弹幕在地表产生";
    public override Version Version => new(2, 0, 1);
    #endregion

    #region 注册与卸载钩子
    public SurfaceBlock(Main game) : base(game) { }
    public override void Initialize()
    {
        LoadConfig(); //开服自动建配置
        GeneralHooks.ReloadEvent += ReloadConfig;
        GetDataHandlers.TileEdit += OnTileEdit!;
        GetDataHandlers.NewProjectile += ProjectNew!;
        GetDataHandlers.PlayerUpdate += OnPlayerUpdate!;
        ServerApi.Hooks.NetGreetPlayer.Register(this, this.OnGreetPlayer);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DB.ClearData(); //关服自动清理玩家数据
            GeneralHooks.ReloadEvent -= ReloadConfig;
            GetDataHandlers.TileEdit -= OnTileEdit!;
            GetDataHandlers.NewProjectile -= ProjectNew!;
            GetDataHandlers.PlayerUpdate -= OnPlayerUpdate!;
            ServerApi.Hooks.NetGreetPlayer.Deregister(this, this.OnGreetPlayer);
        }
        base.Dispose(disposing);
    }
    #endregion

    #region 配置文件创建加载方法
    internal static Configuration Config = new();
    private static void ReloadConfig(ReloadEventArgs args = null!)
    {
        LoadConfig();
        if (args != null && args.Player != null)
        {
            args.Player.SendSuccessMessage("[禁地表弹幕]重新加载配置完毕。");
        }
    }
    private static void LoadConfig()
    {
        Config = Configuration.Read();
        Config.Write();
    }
    #endregion

    #region 玩家进服自动建数据
    public static Datas DB = new();
    private void OnGreetPlayer(GreetPlayerEventArgs args)
    {
        var plr = TShock.Players[args.Who];
        if (!Config.Enabled || plr == null)
        {
            return;
        }
        var data = DB.GetData(plr.Name);

        // 如果玩家不在数据表中，则创建新数据
        if (data == null)
        {
            var newData = new Datas.PlayerData
            {
                Name = plr.Name,
                Enabled = false,
                Time = default,
            };
            DB.AddData(newData); // 添加到数据库
        }
    }
    #endregion

    #region 弹幕更新时触发消除方法（计算世界大小）
    public void ProjectNew(object sender, GetDataHandlers.NewProjectileEventArgs e)
    {
        var plr = e.Player;
        if (!Config.Enabled && plr == null ||
            Config.ClearTable == null ||
            plr.HasPermission("SurfaceBlock") ||
            plr.HasPermission("免检地表弹幕"))
        {
            return;
        }

        if (Config.ClearTable.Contains(e.Type))
        {
            if (!Main.remixWorld) //正常种子
            {
                if (e.Position.Y < Main.worldSurface * 16)
                {
                    Remover(e);
                }
            }
            else //颠倒
            {
                if (GetWorldSize() == 3)
                {
                    RemixWorld(e, 54.5);
                }

                if (GetWorldSize() == 2)
                {
                    RemixWorld(e, 48.5);
                }

                if (GetWorldSize() == 1)
                {
                    RemixWorld(e, 40.0);
                }
            }
        }
    }
    #endregion

    #region 移除弹幕方法（并开启标识提供给其他方法作为参考使用）
    public static void Remover(GetDataHandlers.NewProjectileEventArgs e)
    {
        var data = DB.GetData(e.Player.Name);
        if (data == null)
            return;

        var now = DateTime.UtcNow;

        if (e.Index < 0 || e.Index >= Main.projectile.Length)
            return;

        string name = Terraria.Lang.GetProjectileName(e.Type).Value;
        if (name.StartsWith("ProjectileName."))
        {
            name = "未知";
        }

        // 发送广播消息
        if (Config.Mess)
        {
            TShock.Utils.Broadcast($"玩家 [c/4EA4F2:{e.Player.Name}] 使用禁地表弹幕 [c/15EDDB:{name}] 已清除!", 240, 255, 150);
        }

        // 安全删除弹幕
        TSPlayer.All.SendData(PacketTypes.ProjectileDestroy, "", e.Index, e.Owner);
        TSPlayer.All.RemoveProjectile(e.Index, e.Owner);

        // 更新数据库标识
        data.Enabled = true;
        data.Time = now;
        DB.UpdateData(data);
    }
    #endregion

    #region 玩家移动触发销毁方法(并用于关闭销毁标识)
    private void OnPlayerUpdate(object? sender, GetDataHandlers.PlayerUpdateEventArgs e)
    {
        var plr = e.Player;
        if (plr == null || !plr.IsLoggedIn || !plr.Active || !Config.Enabled ||
           plr.HasPermission("SurfaceBlock") || Config.ClearTable == null ||
           plr.HasPermission("免检地表弹幕"))
        {
            return;
        }

        var data = DB.GetData(plr.Name);
        if (data != null)
        {
            if (data.Enabled)
            {
                for (var i = 0; i < Main.projectile.Length; i++)
                {
                    var proj = Main.projectile[i];

                    if (proj.owner == plr.Index && Config.ClearTable.Contains(proj.type))
                    {
                        if (Config.ItemDorp)
                        {
                            ItemDorp(plr);
                        }

                        proj.type = 0;
                        proj.frame = 0;
                        proj.timeLeft = -1;
                        proj.active = false;
                        TSPlayer.All.RemoveProjectile(i, proj.owner);
                        TSPlayer.All.SendData(PacketTypes.ProjectileNew, "", i, 0f, 0f, 0f, 0);
                    }
                }

                //只有在超过了Config指定秒数再关闭，避免频繁发射弹幕不断开关影响update性能
                if ((DateTime.UtcNow - data.Time).TotalSeconds >= Config.Seconds)
                {
                    data.Enabled = false;
                    DB.UpdateData(data);
                }
            }
        }
    }
    #endregion

    #region 手持物品掉落方法
    private static void ItemDorp(TSPlayer plr)
    {
        var item = TShock.Utils.GetItemById(plr.SelectedItem.type);
        var stack = plr.SelectedItem.stack;
        var MyItem = Item.NewItem(null, (int)plr.X, (int)plr.Y, item.width, item.height, item.type, stack);
        item.wet = Collision.WetCollision(item.position, item.width, item.height);
        if (MyItem >= 0 && MyItem < Main.item.Length)
        {
            var newItem = Main.item[MyItem];
            newItem.playerIndexTheItemIsReservedFor = plr.Index;
            if (plr.TPlayer.selectedItem >= 0 && plr.TPlayer.selectedItem < plr.TPlayer.inventory.Length)
            {
                plr.TPlayer.inventory[plr.TPlayer.selectedItem].SetDefaults(0);
                NetMessage.SendData(5, -1, -1, null, plr.Index, plr.TPlayer.selectedItem);
            }

            plr.SendData(PacketTypes.PlayerSlot, null, MyItem);
            plr.SendData(PacketTypes.UpdateItemDrop, null, MyItem);
            plr.SendData(PacketTypes.ItemOwner, null, MyItem);
            plr.SendData(PacketTypes.TweakItem, null, MyItem, 255f, 63f);
        }
    }
    #endregion

    #region 恢复被破坏的图格方法
    private void OnTileEdit(object sender, GetDataHandlers.TileEditEventArgs args)
    {
        var plr = args.Player;
        if (plr == null || !plr.IsLoggedIn || !plr.Active ||
            !Config.Enabled || !Config.KillTile)
        {
            return;
        }

        var data = DB.GetData(plr.Name);
        if (data != null)
        {
            if (data.Enabled)
            {
                //获取图格回滚大小
                GetRollbackSize(args.X, args.Y, out var width, out var length, out var offY);
                var x = (short)(args.X - width);
                var y = (short)(args.Y + offY);
                var w = (byte)(width * 2);
                var h = (byte)(length + 1);

                TSPlayer.All.SendTileRect(x, y, w, h);
                args.Handled = true;
            }
        }
    }
    #endregion
}