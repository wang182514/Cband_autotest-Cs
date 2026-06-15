namespace CbandAutoTest.Instruments.Abstractions;

/// <summary>
/// 信号发生器接口 —— 点频 / 扫频 / RF开关
/// </summary>
public interface ISignalGenerator : IInstrument
{
    void SetCw(double freqMHz, double powerDbm);
    void ConfigureSweep(double startGHz, double stopGHz, double stepKHz, double dwellMs, double powerDbm);
    void SetCwMode();
    void RfOn();
    void RfOff();
    void ModOff();
}
