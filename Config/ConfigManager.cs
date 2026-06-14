using System.Text.Json;

namespace CbandAutoTest.Config;

public class ConfigManager
{
    private readonly string _defaultsPath;
    private string? _userPath;

    public InstrumentConfig Instruments { get; set; } = new();

    public ConfigManager(string defaultsPath)
    {
        _defaultsPath = defaultsPath;
    }

    public void LoadDefaults()
    {
        if (!File.Exists(_defaultsPath)) return;
        var root = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(_defaultsPath));
        if (root.TryGetProperty("instruments", out var inst))
            Instruments = JsonSerializer.Deserialize<InstrumentConfig>(inst.GetRawText()) ?? new InstrumentConfig();
    }

    public void LoadUser(string path)
    {
        if (!File.Exists(path)) return;
        _userPath = path;
        var overlay = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(path));
        if (!overlay.TryGetProperty("instruments", out var inst)) return;
        DeepMerge(inst);
    }

    public void SaveUser(string path)
    {
        _userPath = path;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var json = new Dictionary<string, object> { ["instruments"] = Instruments };
        File.WriteAllText(path, JsonSerializer.Serialize(json, new JsonSerializerOptions { WriteIndented = true }));
    }

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
