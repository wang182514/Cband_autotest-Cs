namespace CbandAutoTest.Instruments.Abstractions;

/// <summary>频谱仪工作模式：SA=频谱分析, NF=噪声系数, PN=相位噪声</summary>
public enum SAnalyzerMode { SA, NF, PN }

/// <summary>
/// 频谱分析仪接口 —— 三种模式下的配置、测量、截图
/// </summary>
public interface ISpectrumAnalyzer : IInstrument
{
    void SetMode(SAnalyzerMode mode);
    void LoadState(string templateName);
    string CheckError();
    void ClearMarkers();
    void SaConfigure(double startGHz, double stopGHz, double rbwKHz, double vbwKHz, double refLevelDbm);
    (double freqGHz, double ampDBm) SaMarkerPeak();
    double SaMarkerPtP();
    double SaMarkerNoise(double freqMHz);
    void SaSetOffset(double offsetDb);
    void NfInitCal();
    bool NfIsCalibrated();
    void NfInitMeasurement();
    double NfSetMarker(int marker, int trace, double freqGHz);
    void PnSetCenterFreq(double freqGHz);
    void PnInitMeasurement();
    (double offsetHz, double pnDbcHz) PnReadSpot(int markerIndex);
    string Screenshot(string localDir, string localFilename, string theme = "FCOL", string? internalPath = null);
}
