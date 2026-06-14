using CbandAutoTest.Config;
using CbandAutoTest.Instruments;
using CbandAutoTest.Instruments.Abstractions;
using CbandAutoTest.Utils;

namespace CbandAutoTest;

public class InstrumentManager : IDisposable
{
    private readonly Logger _logger;
    private bool _disposed;

    public IPowerSupply RxPower { get; }
    public IPowerSupply TxPower { get; }
    public ISignalGenerator SignalGenerator { get; }
    public ISpectrumAnalyzer SpectrumAnalyzer { get; }
    public ISwitchMatrix SwitchMatrix { get; }

    public InstrumentManager(InstrumentConfig config, Logger logger)
    {
        _logger = logger;
        RxPower = new PowerSupply(config.RxPowerSupply.Ip, config.RxPowerSupply.Port, config.RxPowerSupply.TimeoutSec);
        TxPower = new PowerSupply(config.TxPowerSupply.Ip, config.TxPowerSupply.Port, config.TxPowerSupply.TimeoutSec);
        SignalGenerator = new SignalGenerator(config.SignalGenerator.Ip, (int)(config.SignalGenerator.TimeoutSec * 1000));
        SpectrumAnalyzer = new SpectrumAnalyzer(config.SpectrumAnalyzer.Ip, (int)(config.SpectrumAnalyzer.TimeoutSec * 1000));
        SwitchMatrix = new SwitchMatrix(config.SwitchMatrix.ComPort, config.SwitchMatrix.BaudRate, (int)(config.SwitchMatrix.TimeoutSec * 1000));
    }

    public string? ConnectOne(IInstrument instrument, string displayName)
    {
        try
        {
            _logger.Info($"正在连接 {displayName}...");
            var idn = instrument.Connect();
            _logger.Info($"  {displayName} 已连接: {idn}");
            return idn;
        }
        catch (Exception ex)
        {
            _logger.Error($"  {displayName} 连接失败: {ex.Message}");
            return null;
        }
    }

    public void ConnectAll()
    {
        _logger.Info("=== 连接全部仪表 ===");
        ConnectOne(RxPower, "接收电源 (RX PWR)");
        ConnectOne(TxPower, "发射电源 (TX PWR)");
        ConnectOne(SignalGenerator, "信号源 (VSG)");
        ConnectOne(SpectrumAnalyzer, "频谱仪 (SA)");
        ConnectOne(SwitchMatrix, "开关矩阵");
    }

    public void DisconnectAll()
    {
        _logger.Info("=== 断开全部仪表 ===");
        RxPower.Disconnect();
        TxPower.Disconnect();
        SignalGenerator.Disconnect();
        SpectrumAnalyzer.Disconnect();
        SwitchMatrix.Disconnect();
    }

    public void Dispose()
    {
        if (!_disposed) { DisconnectAll(); _disposed = true; }
    }
}
