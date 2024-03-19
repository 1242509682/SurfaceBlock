using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace 禁地表弹幕
{
    [ApiVersion(2, 1)]
    public class Surfaceprojban : TerrariaPlugin
    {
        public override string Author => "羽学 Q群644435865 感谢Cai 西江小子";
        public override string Description => "禁止特定弹幕在地表产生";
        public override string Name => "禁地表弹幕";
        public override Version Version => new(1, 0, 0, 3);
        internal static Configuration Config;
        public Surfaceprojban(Main game) : base(game)
        {
            Order = 40;
        }
        public override void Initialize()
        {
            LoadConfig();
            GetDataHandlers.NewProjectile += OnProjectileNew;
            GeneralHooks.ReloadEvent += ReloadConfig;
        }
        private static void LoadConfig()
        {
            Config = Configuration.Read(Configuration.FilePath);
            Config.Write(Configuration.FilePath);
        }

        private static void ReloadConfig(ReloadEventArgs args)
        {
            LoadConfig();
            args.Player?.SendSuccessMessage("[{0}]重新加载配置完毕。", typeof(Surfaceprojban).Name);
        }
        //禁止生成的弹幕
        private static readonly HashSet<int> restrictedProjectiles = new HashSet<int>();
        private void OnProjectileNew(object sender, GetDataHandlers.NewProjectileEventArgs e)
        {
            if (e.Player.HasPermission("免检地表弹幕")) //插件权限名
                return;

            //调用Config里的弹幕ID，如果产生这些弹幕ID，则根据地图Y轴小于地表范围作为参考范围，清除这些弹幕。
            if ((Config.禁用地表弹幕id.Contains(e.Type) || restrictedProjectiles.Contains(e.Type)) && e.Position.Y < Main.worldSurface * 16) 
            {
                e.Player.RemoveProjectile(e.Identity, e.Owner);
                e.Handled = true;
            }
        }
    }
}