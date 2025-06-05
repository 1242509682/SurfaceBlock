namespace SurfaceBlock;

public class Datas
{
    #region ���ݵĽṹ��
    public class PlayerData
    {
        // �������
        public string Name { get; set; }
        // ���ٿ���
        public bool Enabled { get; set; }
        // ����ʱ��
        public DateTime Time { get; set; }

        internal PlayerData(string name = "", bool enabled = false, DateTime time = default)
        {
            this.Name = name ?? "";
            this.Enabled = enabled;
            this.Time = time;
        }
    }
    #endregion

    #region �ڴ�洢����
    private static readonly Dictionary<string, PlayerData> PlayerDataDict = new(StringComparer.OrdinalIgnoreCase);
    #endregion

    #region Ϊ��Ҵ������ݷ���
    public bool AddData(PlayerData data)
    {
        if (string.IsNullOrWhiteSpace(data.Name))
            return false;

        if (PlayerDataDict.ContainsKey(data.Name))
            return false; // �Ѵ��ڣ����ظ����

        PlayerDataDict[data.Name] = data;
        return true;
    }
    #endregion

    #region �������ݷ���
    public bool UpdateData(PlayerData data)
    {
        if (string.IsNullOrWhiteSpace(data.Name))
            return false;

        if (!PlayerDataDict.ContainsKey(data.Name))
            return false; // û�и��������

        PlayerDataDict[data.Name] = data;
        return true;
    }
    #endregion

    #region ��ȡ���ݷ���
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

    #region �����������ݷ���
    public bool ClearData()
    {
        PlayerDataDict.Clear();
        return true;
    }
    #endregion
}