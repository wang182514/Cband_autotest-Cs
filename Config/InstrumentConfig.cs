using System.Text.Json.Serialization;

namespace CbandAutoTest.Config;

/// <summary>
/// 仪器配置 —— 对应 default_settings.json 里的 "instruments" 段
/// 
/// 【概念】{ get; set; } 是 C# 的"自动属性"
///   相当于同时声明了一个私有字段 + 公开的 getter/setter 方法
///   等价于 C: struct { char ip[64]; int port; } + get/set 函数
///   等价于 Python: @property
/// 
/// 【概念】[JsonPropertyName("xxx")] 是"特性（Attribute）"
///   告诉 JSON 序列化器：这个 C# 属性对应 JSON 里的字段名叫什么
///   比如 RxPowerSupply 属性 → JSON 的 "rx_power_supply" 字段
/// </summary>
public class InstrumentConfig
{
    [JsonPropertyName("rx_power_supply")]
    public DeviceConfig RxPowerSupply { get; set; } = new() { Ip = "192.168.1.11", Port = 2268 };

    [JsonPropertyName("tx_power_supply")]
    public DeviceConfig TxPowerSupply { get; set; } = new() { Ip = "192.168.1.108", Port = 2268 };

    [JsonPropertyName("signal_generator")]
    public VisaDeviceConfig SignalGenerator { get; set; } = new() { Ip = "192.168.1.90", Vendor = "rs" };

    [JsonPropertyName("spectrum_analyzer")]
    public VisaDeviceConfig SpectrumAnalyzer { get; set; } = new() { Ip = "192.168.1.102", Vendor = "agilent" };

    [JsonPropertyName("switch_matrix")]
    public SerialDeviceConfig SwitchMatrix { get; set; } = new() { ComPort = "COM6", BaudRate = 115200 };
}

/// <summary>
/// TCP/IP 设备配置（电源用）
/// </summary>
public class DeviceConfig
{
    [JsonPropertyName("ip")] public string Ip { get; set; } = "";
    [JsonPropertyName("port")] public int Port { get; set; } = 2268;
    [JsonPropertyName("timeout_sec")] public double TimeoutSec { get; set; } = 1.0;
}

/// <summary>
/// VISA 设备配置（信号源 / 频谱仪用）
/// </summary>
public class VisaDeviceConfig
{
    [JsonPropertyName("ip")] public string Ip { get; set; } = "";
    [JsonPropertyName("vendor")] public string Vendor { get; set; } = "agilent";
    [JsonPropertyName("timeout_sec")] public double TimeoutSec { get; set; } = 5.0;
}

/// <summary>
/// 串口设备配置（开关矩阵用）
/// </summary>
public class SerialDeviceConfig
{
    [JsonPropertyName("com_port")] public string ComPort { get; set; } = "COM6";
    [JsonPropertyName("baud_rate")] public int BaudRate { get; set; } = 115200;
    [JsonPropertyName("timeout_sec")] public double TimeoutSec { get; set; } = 1.0;
}
