using System.Text.Json.Serialization;

namespace CbandAutoTest.Config;

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

public class DeviceConfig
{
    [JsonPropertyName("ip")] public string Ip { get; set; } = "";
    [JsonPropertyName("port")] public int Port { get; set; } = 2268;
    [JsonPropertyName("timeout_sec")] public double TimeoutSec { get; set; } = 1.0;
}

public class VisaDeviceConfig
{
    [JsonPropertyName("ip")] public string Ip { get; set; } = "";
    [JsonPropertyName("vendor")] public string Vendor { get; set; } = "agilent";
    [JsonPropertyName("timeout_sec")] public double TimeoutSec { get; set; } = 5.0;
}

public class SerialDeviceConfig
{
    [JsonPropertyName("com_port")] public string ComPort { get; set; } = "COM6";
    [JsonPropertyName("baud_rate")] public int BaudRate { get; set; } = 115200;
    [JsonPropertyName("timeout_sec")] public double TimeoutSec { get; set; } = 1.0;
}
