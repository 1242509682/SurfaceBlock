using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace 禁地表弹幕
{
    [ApiVersion(2, 1)]
    public class 禁地表弹幕 : TerrariaPlugin
    {
        public override string Author => "羽学 感谢Cai 西江小子 熙恩";
        public override string Description => "禁止特定弹幕在地表产生";
        public override string Name => "禁地表弹幕";
        public override Version Version => new(1, 0, 0, 4);
        internal static Configuration Config;
        private bool _isEnabled; // 存储插件是否启用的状态，默认为false
        public 禁地表弹幕(Main game) : base(game)
        {
            Order = 40;
            _isEnabled = false; // 初始化为关闭状态
        }
        public override void Initialize()
        {
            LoadConfig();
            GetDataHandlers.NewProjectile += OnProjectileNew;
            GeneralHooks.ReloadEvent += ReloadConfig;
            Commands.ChatCommands.Add(new Command("禁地表弹幕", Command, "禁地表弹幕")); //添加一个指令权限
        }
        private static void LoadConfig()
        {
            Config = Configuration.ReadOrCreateDefault(Configuration.FilePath);
            Config.Write(Configuration.FilePath);
        }

        private static void ReloadConfig(ReloadEventArgs args)
        {
            LoadConfig();
            args.Player?.SendSuccessMessage("[{0}]重新加载配置完毕。", typeof(禁地表弹幕).Name);
        }

        // 定义Command方法处理玩家执行的相关命令
        public void Command(CommandArgs args)
        {
            // 检查玩家是否具有“禁地表弹幕”权限
            if (!args.Player.HasPermission("禁地表弹幕"))
            {
                // 如果没有权限，则向该玩家发送错误消息并退出方法
                args.Player.SendErrorMessage("你没有使用禁地表弹幕指令的权限");
                return;
            }

            // 切换插件启用状态（布尔值(!_isEnabled)意味着翻转当前状态）
            _isEnabled = !_isEnabled;

            // 当插件启用时的操作
            if (_isEnabled)
            {
                // 从配置中获取带有名称的弹幕ID列表
                var projectileIdsWithNames = Config.GetIdsWithNames();

                // 如果列表不为空，则进行后续操作
                if (projectileIdsWithNames != null)
                {
                    // 创建用于格式化输出弹幕项目的方法
                    string FormatItem(int index, KeyValuePair<int, string> kv)
                    {
                        string itemNumber = $"{index + 1}. ";
                        string separator = (index + 1) % 5 == 0 && index < projectileIdsWithNames.Count - 1 ? "\n" : "";
                        return $" {itemNumber}[c/EEF992:{kv.Value}]([c/73DCE6:{kv.Key}]){separator}";
                    }

                    // 构造一个格式化的弹幕列表，供玩家查看，仅在开启时显示：不想给它加命令，所以干脆写在里面了
                    int count = projectileIdsWithNames.Count;
                    string formattedIdsForPlayers = string.Join("", Enumerable.Range(0, count).Select(index => FormatItem(index, projectileIdsWithNames.ElementAt(index))));

                    // 移除列表末尾的多余换行符（如果存在）
                    if (formattedIdsForPlayers.EndsWith("\n") && formattedIdsForPlayers.Length > 1)
                    {
                        formattedIdsForPlayers = formattedIdsForPlayers.Substring(0, formattedIdsForPlayers.Length - 1);
                    }

                    // 构建并发送给玩家成功消息，包含启用/关闭状态及弹幕列表
                    string playerMessage = $"{args.Player.Name}[c/FFAE80:{(_isEnabled ? "开启了" : "关闭了")}]禁止地表弹幕功能,\n[c/FD7E83:禁止地表生成弹幕表]:\n{formattedIdsForPlayers}\n";
                    TSPlayer.All.SendSuccessMessage(playerMessage);

                    // 构建控制台消息前缀，并根据启用状态决定是否展示弹幕列表 之所以写2个是因为tshock控制台不能输出带颜色的代码
                    string consoleMessagePrefix = $"{args.Player.Name} {(_isEnabled ? "开启了" : "关闭了")}了禁止地表弹幕功能,\n";
                    string consoleProjectilesList = _isEnabled
                        ? $"禁止地表生成弹幕表：\n{string.Join(", ", projectileIdsWithNames.Select(kv => $"{kv.Value}({kv.Key})"))}\n"
                        : string.Empty;

                    // 组合完整控制台消息并记录到TShock日志
                    string fullConsoleMessage = $"{consoleMessagePrefix}{consoleProjectilesList}";
                    TShock.Log.ConsoleInfo(fullConsoleMessage);
                }
            }

            // 当插件关闭时的操作 
            if (!_isEnabled)
            {
                // 向所有玩家发送消息，通知已关闭禁地表弹幕功能，避免再次把弹幕ID发出来所以额外写了个关闭消息，同样写2个：1个给玩家看（带颜色），1个给控制台看。
                string playerMessage = $"{args.Player.Name}[c/FFAE80:关闭了]禁止地表弹幕功能.";
                TSPlayer.All.SendSuccessMessage(playerMessage);

                // 记录到控制台，只显示关闭通知，避免再次把弹幕ID发出来所以额外写了个关闭消息
                string consoleMessagePrefix = $"{args.Player.Name} 关闭了禁止地表弹幕功能.";
                string fullConsoleMessage = $"{consoleMessagePrefix}\n";
                TShock.Log.ConsoleInfo(fullConsoleMessage);
            }
        }
        //禁止生成的弹幕
        private static readonly HashSet<int> restrictedProjectiles = new HashSet<int>();
        private void OnProjectileNew(object sender, GetDataHandlers.NewProjectileEventArgs e)
        {
            if (e.Player.HasPermission("免检地表弹幕")) // 插件权限名
                return;

            // 根据插件当前启用状态决定是否清除弹幕
            if (_isEnabled && (Config.禁用地表弹幕id.Contains(e.Type) || restrictedProjectiles.Contains(e.Type)) && e.Position.Y < Main.worldSurface * 16)
            {
                e.Player.RemoveProjectile(e.Identity, e.Owner);
                e.Handled = true;
            }

        }
    }
}