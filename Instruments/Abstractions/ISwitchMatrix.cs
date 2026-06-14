namespace CbandAutoTest.Instruments.Abstractions;

public interface ISwitchMatrix : IInstrument
{
    void SetUdcSwitches(int sw1, int sw2, int sw3, int sw4);
    Dictionary<string, object>? PsaSetMode(int mode, int[] swOn);
    void SaveUdcConfig();
}
