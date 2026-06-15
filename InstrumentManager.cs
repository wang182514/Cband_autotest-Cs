using CbandAutoTest.Config;
using CbandAutoTest.Instruments;
using CbandAutoTest.Instruments.Abstractions;
using CbandAutoTest.Utils;

namespace CbandAutoTest;

/// <summary>
/// 仪器管理器 —— 5 台仪器的"总管"
/// 负责：根据配置创建仪器实例、统一连接/断开、程序退出时清理资源
/// 
/// 【概念】IDisposable 是 .NET 的资源释放接口
///   实现它就可以用 using 语法自动清理，或者由 GC 在回收时调用 Dispose()
///   这里用来确保程序退出时断开所有仪器连接
/// </summary>
public class InstrumentManager : IDisposable
{
    private readonly Logger _logger;
    private bool _disposed;  // 防重复释放标志

    // ---- 5 台仪器，外部只读 ----
    // 属性的类型都是 interface（IPowerSupply / ISignalGenerator ...），而不是具体的类
    // 这样做的好处：将来换不同品牌的仪器，只要新驱动实现同一个接口，这里不用改
    public IPowerSupply RxPower { get; }   // 接收电源
    public IPowerSupply TxPower { get; }   // 发射电源
    public ISignalGenerator SignalGenerator { get; }  // 信号源 (R&S SMU200A)
    public ISpectrumAnalyzer SpectrumAnalyzer { get; } // 频谱仪 (Keysight N9020A)
    public ISwitchMatrix SwitchMatrix { get; }        // 开关矩阵 (UDC-0624F)

    /// <summary>
    /// 构造函数 —— new InstrumentManager(...) 时自动执行
    /// 相当于 Python 的 __init__
    /// 这里根据配置创建 5 台仪器的实例，但不连接（连接在 Connect 时进行）
    /// </summary>
    public InstrumentManager(InstrumentConfig config, Logger logger)
    {
        _logger = logger;
        // 两种电源都用 PowerSupply 类，但 IP/端口不同（TCP SCPI 通信）
        RxPower = new PowerSupply(config.RxPowerSupply.Ip, config.RxPowerSupply.Port, config.RxPowerSupply.TimeoutSec);
        TxPower = new PowerSupply(config.TxPowerSupply.Ip, config.TxPowerSupply.Port, config.TxPowerSupply.TimeoutSec);
        // 信号源和频谱仪用 VISA 通信（通过 Ivi.Visa NuGet 包）
        SignalGenerator = new SignalGenerator(config.SignalGenerator.Ip, (int)(config.SignalGenerator.TimeoutSec * 1000));
        SpectrumAnalyzer = new SpectrumAnalyzer(config.SpectrumAnalyzer.Ip, (int)(config.SpectrumAnalyzer.TimeoutSec * 1000));
        // 开关矩阵用串口通信
        SwitchMatrix = new SwitchMatrix(config.SwitchMatrix.ComPort, config.SwitchMatrix.BaudRate, (int)(config.SwitchMatrix.TimeoutSec * 1000));
    }

    /// <summary>
    /// 连接单台仪器，返回仪器的 IDN 标识字符串
    /// string? 返回值表示"可能返回 null"（连接失败时）
    /// </summary>
    public string? ConnectOne(IInstrument instrument, string displayName)
    {
        try
        {
            _logger.Info($"正在连接 {displayName}...");
            var idn = instrument.Connect();  // 调具体驱动的 Connect()
            _logger.Info($"  {displayName} 已连接: {idn}");
            return idn;
        }
        catch (Exception ex)
        {
            _logger.Error($"  {displayName} 连接失败: {ex.Message}");
            return null;  // 连接失败返回 null，不阻断后续仪器连接
        }
    }

    /// <summary>
    /// 依次连接全部 5 台仪器（顺序执行，不并行）
    /// </summary>
    public void ConnectAll()
    {
        _logger.Info("=== 连接全部仪表 ===");
        ConnectOne(RxPower, "接收电源 (RX PWR)");
        ConnectOne(TxPower, "发射电源 (TX PWR)");
        ConnectOne(SignalGenerator, "信号源 (VSG)");
        ConnectOne(SpectrumAnalyzer, "频谱仪 (SA)");
        ConnectOne(SwitchMatrix, "开关矩阵");
    }

    /// <summary>
    /// 断开全部仪器连接，释放网络/串口资源
    /// </summary>
    public void DisconnectAll()
    {
        _logger.Info("=== 断开全部仪表 ===");
        RxPower.Disconnect();
        TxPower.Disconnect();
        SignalGenerator.Disconnect();
        SpectrumAnalyzer.Disconnect();
        SwitchMatrix.Disconnect();
    }

    /// <summary>
    /// 程序退出时由 Windows 调用，确保断开所有连接
    /// 实现了 IDisposable 接口的方法
    /// </summary>
    public void Dispose()
    {
        if (!_disposed) { DisconnectAll(); _disposed = true; }
    }
}
