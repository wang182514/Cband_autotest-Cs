namespace CbandAutoTest.Instruments.Abstractions;

public interface ISignalGenerator : IInstrument
{
    void SetCw(double freqMHz, double powerDbm);
    void ConfigureSweep(double startGHz, double stopGHz, double stepKHz, double dwellMs, double powerDbm);
    void SetCwMode();
    void RfOn();
    void RfOff();
    void ModOff();
}
