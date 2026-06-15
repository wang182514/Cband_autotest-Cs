using System.Text.Json;

namespace CbandAutoTest.Config;

/// <summary>
/// 配置管理器 —— 负责 JSON 配置文件的读/写/合并
/// 
/// 采用"默认 + 用户覆盖"两层模型：
///   default_settings.json  出厂默认（提交到 git，所有人共享）
///   user_settings.json      用户自定义（不提交 git，每个人的 IP/COM 口不同）
///   两者深度合并：用户只写需要覆盖的字段，其余保留默认值
/// </summary>
public class ConfigManager
{
    private readonly string _defaultsPath;
    private string? _userPath;  // string? 表示这个字段可以是 null

    /// <summary>仪器配置 —— 运行时始终持有最新的合并后配置</summary>
    public InstrumentConfig Instruments { get; set; } = new();

    public ConfigManager(string defaultsPath)
    {
        _defaultsPath = defaultsPath;
    }

    /// <summary>
    /// 加载出厂默认配置（default_settings.json）
    /// 如果文件不存在则跳过（比如开发时还没创建）
    /// </summary>
    public void LoadDefaults()
    {
        if (!File.Exists(_defaultsPath)) return;
        // JsonElement 是 System.Text.Json 的 DOM 类型，类似 Python 的 dict
        var root = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(_defaultsPath));
        if (root.TryGetProperty("instruments", out var inst))
            Instruments = JsonSerializer.Deserialize<InstrumentConfig>(inst.GetRawText()) ?? new InstrumentConfig();
    }

    /// <summary>
    /// 加载用户自定义配置（user_settings.json），深度合并到已有配置上
    /// 用户只需写想覆盖的字段，未写到的字段保留默认值不变
    /// </summary>
    public void LoadUser(string path)
    {
        if (!File.Exists(path)) return;
        _userPath = path;
        var overlay = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(path));
        if (!overlay.TryGetProperty("instruments", out var inst)) return;
        DeepMerge(inst);
    }

    /// <summary>
    /// 保存当前配置到 user_settings.json（带缩进的人类可读 JSON）
    /// </summary>
    public void SaveUser(string path)
    {
        _userPath = path;
        // Path.GetDirectoryName(path)!  —— 末尾的 ! 告诉编译器"我知道这不是 null"
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var json = new Dictionary<string, object> { ["instruments"] = Instruments };
        File.WriteAllText(path, JsonSerializer.Serialize(json, new JsonSerializerOptions { WriteIndented = true }));
    }

    /// <summary>
    /// 深度合并：逐字段检查用户 JSON 里是否写了某个值，写了才覆盖
    /// 不写全量反序列化是因为用户可能只写了部分字段
    /// </summary>
    private void DeepMerge(JsonElement o)
    {
        if (o.TryGetProperty("rx_power_supply", out var r))
        {
            if (r.TryGetProperty("ip", out var ip)) Instruments.RxPowerSupply.Ip = ip.GetString()!;
            if (r.TryGetProperty("port", out var p)) Instruments.RxPowerSupply.Port = p.GetInt32();
            if (r.TryGetProperty("timeout_sec", out var t)) Instruments.RxPowerSupply.TimeoutSec = t.GetDouble();
        }
        if (o.TryGetProperty("tx_power_supply", out var tx))
        {
            if (tx.TryGetProperty("ip", out var ip)) Instruments.TxPowerSupply.Ip = ip.GetString()!;
            if (tx.TryGetProperty("port", out var p)) Instruments.TxPowerSupply.Port = p.GetInt32();
            if (tx.TryGetProperty("timeout_sec", out var t)) Instruments.TxPowerSupply.TimeoutSec = t.GetDouble();
        }
        if (o.TryGetProperty("signal_generator", out var v))
        {
            if (v.TryGetProperty("ip", out var ip)) Instruments.SignalGenerator.Ip = ip.GetString()!;
            if (v.TryGetProperty("vendor", out var ve)) Instruments.SignalGenerator.Vendor = ve.GetString()!;
        }
        if (o.TryGetProperty("spectrum_analyzer", out var s))
        {
            if (s.TryGetProperty("ip", out var ip)) Instruments.SpectrumAnalyzer.Ip = ip.GetString()!;
            if (s.TryGetProperty("vendor", out var ve)) Instruments.SpectrumAnalyzer.Vendor = ve.GetString()!;
        }
        if (o.TryGetProperty("switch_matrix", out var w))
        {
            if (w.TryGetProperty("com_port", out var cp)) Instruments.SwitchMatrix.ComPort = cp.GetString()!;
            if (w.TryGetProperty("baud_rate", out var br)) Instruments.SwitchMatrix.BaudRate = br.GetInt32();
        }
    }
}
