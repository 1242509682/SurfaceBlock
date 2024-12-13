using Newtonsoft.Json;
using Terraria;
using TShockAPI;

namespace SurfaceBlock
{
    public class Configuration
    {
        [JsonProperty("启用", Order = 1)]
        public bool Enabled { get; set; } = true;

        [JsonProperty("销毁秒数", Order = 2)]
        public int Seconds { get; set; } = 5;

        [JsonProperty("物品掉落", Order = 3)]
        public bool ItemDorp { get; set; } = false;

        [JsonProperty("还原图格", Order = 4)]
        public bool KillTile { get; set; } = true;

        [JsonProperty("是否广播", Order = 5)]
        public bool Mess { get; set; } = true;

        [JsonProperty("禁用弹幕", Order = 7)]
        public HashSet<int>? ClearTable { get; set; } = new HashSet<int>();

        #region 预设参数方法
        private void Setdefault()
        {
            this.ClearTable = new HashSet<int>
            {
                28, 29, 37, 65, 68, 99, 108, 136, 137,
                138, 139, 142, 143, 144, 146, 147, 149,
                164, 339, 341, 354, 453, 516, 519, 637,
                716, 718, 727, 773, 780, 781, 782, 783,
                784, 785, 786, 787, 788, 789, 790, 791,
                792, 796, 797, 798, 799, 800, 801, 804,
                805, 806, 807, 809, 810, 863, 868, 869,
                904, 905, 906, 910, 911, 949, 1013, 1014
            };
        }
        #endregion

        #region 配置文件写入与读取方法
        public static readonly string FilePath = Path.Combine(TShock.SavePath, "禁地表弹幕.json");
        public void Write()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(FilePath, json);
        }

        public static Configuration Read()
        {
            if (!File.Exists(FilePath))
            {
                var NewConfig = new Configuration();
                NewConfig.Setdefault();
                new Configuration().Write();
                return NewConfig;
            }
            else
            {
                var jsonContent = File.ReadAllText(FilePath);
                return JsonConvert.DeserializeObject<Configuration>(jsonContent)!;
            }
        }
        #endregion

    }
}