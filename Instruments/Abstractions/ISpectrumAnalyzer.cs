namespace CbandAutoTest.Instruments.Abstractions;

public enum SAnalyzerMode { SA, NF, PN }

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
