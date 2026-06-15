namespace CbandAutoTest.Instruments.Abstractions;

/// <summary>
/// 开关矩阵接口 —— UDC 专有协议控制射频通道切换
/// </summary>
public interface ISwitchMatrix : IInstrument
{
    void SetUdcSwitches(int sw1, int sw2, int sw3, int sw4);
    Dictionary<string, object>? PsaSetMode(int mode, int[] swOn);
    void SaveUdcConfig();
}
