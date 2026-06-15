namespace CbandAutoTest.Instruments.Abstractions;

/// <summary>
/// 直流电源接口 —— 继承 IInstrument，额外定义电源特有的操作
/// </summary>
public interface IPowerSupply : IInstrument
{
    void SetOutput(bool on);
    double MeasureVoltage();
    double MeasureCurrent();
    void SetVoltage(double volts);
    void SetCurrent(double amps);
}
