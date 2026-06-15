using CbandAutoTest.Instruments.Abstractions;
using Ivi.Visa;

namespace CbandAutoTest.Instruments;

/// <summary>
/// 频谱分析仪驱动 —— 通过 VISA 协议控制 Keysight N9020A
/// 支持三种模式：SA（频谱分析）、NF（噪声系数）、PN（相位噪声）
/// 
/// 通信方式和 SignalGenerator 一样（VISA + SCPI），只是命令集不同
/// </summary>
public class SpectrumAnalyzer : ISpectrumAnalyzer
{
    private readonly string _ip;
    private readonly int _timeoutMs;
    private string _idn = "", _lastError = "";
    private bool _disposed;
    private IMessageBasedSession? _session;  // VISA 会话对象

    public SpectrumAnalyzer(string ip, int timeoutMs = 10000)
    { _ip = ip; _timeoutMs = timeoutMs; }

    public string Idn => _idn;
    public bool IsConnected => _session != null;
    public string LastError => _lastError;

    public string Connect()
    {
        Disconnect(); _lastError = "";
        _session = (IMessageBasedSession)GlobalResourceManager.Open(
            $"TCPIP0::{_ip}::inst0::INSTR",
            AccessModes.None,
            _timeoutMs);
        _session.TerminationCharacterEnabled = true;
        _session.TerminationCharacter = 0x0A; // \n
        Write("*CLS");
        _idn = Query("*IDN?").Trim();
        return _idn;
    }

    public void Disconnect()
    {
        try { _session?.Dispose(); } catch { }
        _session = null; _idn = "";
    }

    public void Dispose()
    {
        if (!_disposed) { Disconnect(); _disposed = true; }
    }

    public void SetMode(SAnalyzerMode mode)
    {
        Write(mode == SAnalyzerMode.SA ? ":INST SA"
            : mode == SAnalyzerMode.NF ? ":INST:SEL NFIGURE"
            : ":INST PNOISE");
        Thread.Sleep(500);
    }

    public void LoadState(string templateName)
    {
        Write($":MMEM:LOAD:STAT \"{templateName}\"");
        Thread.Sleep(1000);
    }

    public string CheckError() => Query(":SYST:ERR?").Trim();

    public void ClearMarkers()
    {
        Write(":CALCulate:MARKer:AOFF");
        Write(":CALCulate:MARKer1:FUNCtion OFF");
    }

    public void SaConfigure(double startGHz, double stopGHz, double rbwKHz, double vbwKHz, double refLevelDbm)
    {
        Write($":SENS:FREQ:STAR {startGHz:F3}GHz");
        Write($":SENS:FREQ:STOP {stopGHz:F3}GHz");
        Write($":SENS:BAND:RES {rbwKHz:F0}KHz");
        Write($":SENS:BAND:VID {vbwKHz:F0}KHz");
        Write($":DISP:WIND:TRAC:Y:RLEV {refLevelDbm:F0}dBm");
        Write(":SENS:SWE:TIME:AUTO ON");
        Write(":TRAC1:TYPE WRIT");
        Write(":INIT:CONT ON");
    }

    public (double freqGHz, double ampDBm) SaMarkerPeak()
    {
        Write(":CALC:MARK1:STAT ON");
        Write(":CALCulate:MARKer1:MAXimum");
        Thread.Sleep(100);
        return (double.Parse(Query("CALC:MARK1:X?")), double.Parse(Query("CALC:MARK1:Y?")));
    }

    public double SaMarkerPtP()
    {
        Write(":CALC:MARK1:PTP");
        return double.Parse(Query(":CALC:MARK1:Y?"));
    }

    public double SaMarkerNoise(double freqMHz)
    {
        Write(":CALCulate:MARKer:AOFF");
        Write(":CALCulate:MARKer1:STATe ON");
        Write($":CALCulate:MARKer1:X {freqMHz:F0}MHz");
        Write(":CALCulate:MARKer1:FUNCtion NOIS");
        Thread.Sleep(2000);
        return double.Parse(Query(":CALCulate:MARKer1:Y?"));
    }

    public void SaSetOffset(double offsetDb) => Write($":DISPlay:WIND1:TRACe:Y:RLEVel:OFFSet {offsetDb:F1}");

    public void NfInitCal() { Write(":NFIG:CAL:INIT"); Query("*OPC?"); }
    public bool NfIsCalibrated() => Query(":NFIG:CAL:STAT?").Trim() == "1";

    public void NfInitMeasurement()
    {
        Write(":INIT:CONT ON");
        Write(":INIT:IMM");
        Query("*OPC?");
    }

    public double NfSetMarker(int marker, int trace, double freqGHz)
    {
        Write($":CALC:NFIG:MARK{marker}:STAT ON");
        Write($":CALC:NFIG:MARK{marker}:TRAC TRAC{trace}");
        Write($":CALC:NFIG:MARK{marker}:X {freqGHz:F2}GHz");
        Thread.Sleep(50);
        return double.Parse(Query($":CALC:NFIG:MARK{marker}:Y?"));
    }

    public void PnSetCenterFreq(double freqGHz) => Write($":FREQ:CENT {freqGHz:F3}GHz");

    public void PnInitMeasurement()
    {
        Write(":INIT:CONT OFF");
        Write(":INIT:IMM");
        Query("*OPC?");
    }

    public (double offsetHz, double pnDbcHz) PnReadSpot(int markerIndex)
    {
        var freqStr = Query($":CALCulate:LPLot:MARK{markerIndex}:X?");
        var pnStr = Query($":CALCulate:LPLot:MARK{markerIndex}:Y?");
        return (double.Parse(freqStr), double.Parse(pnStr));
    }

    public string Screenshot(string localDir, string localFilename, string theme = "FCOL", string? internalPath = null)
    {
        internalPath ??= @"D:\User_My_Documents\Instrument\My Documents\tmp.png";
        Write(":DISP:FSCR ON");
        Write($":MMEM:STOR:SCR:THEM {theme}");
        Write($":MMEM:STOR:SCR \"{internalPath}\"");
        Query("*OPC?");
        Thread.Sleep(500);
        Write($":MMEM:DATA? \"{internalPath}\"");
        var imgData = ReadRaw(); // ReadBinaryBlockOfByte 自动处理 IEEE 488.2 二进制块头
        Directory.CreateDirectory(localDir);
        var fullPath = Path.Combine(localDir, localFilename);
        File.WriteAllBytes(fullPath, imgData);
        Write(":DISP:FSCR OFF");
        return fullPath;
    }

    private void Write(string cmd)
    {
        EnsureConnected();
        _session!.FormattedIO.WriteLine(cmd);
    }

    private string Query(string cmd)
    {
        EnsureConnected();
        _session!.FormattedIO.WriteLine(cmd);
        return _session!.FormattedIO.ReadLine();
    }

    private byte[] ReadRaw()
    {
        EnsureConnected();
        return _session!.FormattedIO.ReadBinaryBlockOfByte();
    }

    private void EnsureConnected()
    {
        if (_session == null) throw new InvalidOperationException("SpectrumAnalyzer 未连接");
    }
}
