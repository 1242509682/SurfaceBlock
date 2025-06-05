namespace SurfaceBlock;

public class Datas
{
    #region 数据的结构体
    public class PlayerData
    {
        // 玩家名字
        public string Name { get; set; }
        // 销毁开关
        public bool Enabled { get; set; }
        // 销毁时间
        public DateTime Time { get; set; }

        internal PlayerData(string name = "", bool enabled = false, DateTime time = default)
        {
            this.Name = name ?? "";
            this.Enabled = enabled;
            this.Time = time;
        }
    }
    #endregion

    #region 内存存储容器
    private static readonly Dictionary<string, PlayerData> PlayerDataDict = new(StringComparer.OrdinalIgnoreCase);
    #endregion

    #region 为玩家创建数据方法
    public bool AddData(PlayerData data)
    {
        if (string.IsNullOrWhiteSpace(data.Name))
            return false;

        if (PlayerDataDict.ContainsKey(data.Name))
            return false; // 已存在，不重复添加

        PlayerDataDict[data.Name] = data;
        return true;
    }
    #endregion

    #region 更新数据方法
    public bool UpdateData(PlayerData data)
    {
        if (string.IsNullOrWhiteSpace(data.Name))
            return false;

        if (!PlayerDataDict.ContainsKey(data.Name))
            return false; // 没有该玩家数据

        PlayerDataDict[data.Name] = data;
        return true;
    }
    #endregion

    #region 获取数据方法
    public PlayerData? GetData(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        if (PlayerDataDict.TryGetValue(name, out var data))
        {
            return new PlayerData
            {
                Name = data.Name,
                Enabled = data.Enabled,
                Time = data.Time
            };
        }

        return null;
    }
    #endregion

    #region 清理所有数据方法
    public bool ClearData()
    {
        PlayerDataDict.Clear();
        return true;
    }
    #endregion
}