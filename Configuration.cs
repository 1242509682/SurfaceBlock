using Newtonsoft.Json;
using TShockAPI;
using System.IO;

namespace 禁地表弹幕
{
    public class Configuration
    {
        public static readonly string FilePath = Path.Combine(TShock.SavePath, "禁地表弹幕表.json");
        public int[] 禁用地表弹幕id = new int[] { 28, 29, 37, 65, 68, 99, 108, 136, 137, 138, 139, 142, 143, 144, 146, 147, 149, 164, 339, 341, 354, 453, 516, 519, 637, 716, 718, 727, 773, 780, 781, 782, 783, 784, 785, 786, 787, 788, 789, 790, 791, 792, 796, 797, 798, 799, 800, 801, 804, 805, 806, 807, 809, 810, 863, 868, 869, 904, 905, 906, 910, 911, 949, 1013, 1014 };

        // 添加一个新的方法来获取带有名称的列表
        public Dictionary<int, string> GetIdsWithNames()
        {
            var dict = new Dictionary<int, string>();

            foreach (var id in 禁用地表弹幕id)
            {
                string name = (string)Terraria.Lang.GetProjectileName(id);

                // 检查出现名字为ProjectileName. 时替换为“未知”
                if (name.StartsWith("ProjectileName."))
                {
                    name = "未知";
                }

                dict[id] = name;
            }
            return dict;
        }

        public void Write(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                var str = JsonConvert.SerializeObject(this, Formatting.Indented);
                using (var sw = new StreamWriter(fs))
                {
                    sw.Write(str);
                }
            }
        }

        public static Configuration ReadOrCreateDefault(string path)
        {
            if (!File.Exists(path))
            {
                var defaultConfig = new Configuration();
                defaultConfig.Write(path);
                return defaultConfig;
            }

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var sr = new StreamReader(fs))
                {
                    var cf = JsonConvert.DeserializeObject<Configuration>(sr.ReadToEnd());
                    return cf;
                }
            }
        }
    }
}