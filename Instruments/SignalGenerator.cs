using CbandAutoTest.Instruments.Abstractions;

namespace CbandAutoTest.Instruments;

public class SignalGenerator : ISignalGenerator
{
    private readonly string _ip;
    private readonly int _timeoutMs;
    private string _idn = "", _lastError = "";
    private bool _disposed;
    private dynamic? _session;

    public SignalGenerator(string ip, int timeoutMs = 5000)
    { _ip = ip; _timeoutMs = timeoutMs; }

    public string Idn => _idn;
    public bool IsConnected => _session != null;
    public string LastError => _lastError;

    public string Connect()
    {
        Disconnect(); _lastError = "";
        var rmType = Type.GetType("Ivi.Visa.ResourceManager, Ivi.Visa")
                     ?? Type.GetType("NationalInstruments.Visa.ResourceManager, NationalInstruments.Visa");
        if (rmType == null)
            throw new NotSupportedException("未找到 VISA 运行时。请安装 NI-VISA 或 R&S VISA。");
        var rm = Activator.CreateInstance(rmType);
        _session = rmType.GetMethod("Open")?.Invoke(rm, [$"TCPIP0::{_ip}::inst0::INSTR"]);
        if (_session == null) throw new Exception("VISA Open 失败");
        var tProp = _session.GetType().GetProperty("TimeoutMilliseconds");
        tProp?.SetValue(_session, _timeoutMs);
        SetTermination();
        Write("*CLS");
        _idn = Query("*IDN?").Trim();
        return _idn;
    }

    public void Disconnect()
    {
        try { _session?.Close(); } catch { }
        _session = null; _idn = "";
    }

    public void Dispose()
    {
        if (!_disposed) { Disconnect(); _disposed = true; }
    }

    public void SetCw(double freqMHz, double powerDbm)
    {
        Write($"FREQ {freqMHz:F3}MHz");
        Write($"POW {powerDbm:F2}dBm");
        Write(":FREQ:MODE CW");
    }

    public void ConfigureSweep(double startGHz, double stopGHz, double stepKHz, double dwellMs, double powerDbm)
    {
        Write($"POW {powerDbm:F2}dBm");
        Write($"FREQ:STAR {startGHz:F3}GHz");
        Write($"FREQ:STOP {stopGHz:F3}GHz");
        Write($"SWE:STEP {stepKHz:F0}KHz");
        Write($"SWE:DWEL {dwellMs:F0}ms");
        Write("SWE:SPAC LIN");
        Write("SWE:MODE AUTO");
        Write("FREQ:MODE SWE");
    }

    public void SetCwMode() => Write(":FREQ:MODE CW");
    public void RfOn() => Write("OUTP ON");
    public void RfOff() => Write("OUTP OFF");
    public void ModOff() { Write(":MOD:STAT OFF"); Write(":SOUR:BB:DM:STAT OFF"); }

    private void Write(string cmd) { EnsureConnected(); _session!.Write(cmd); }
    private string Query(string cmd) { EnsureConnected(); return _session!.Query(cmd); }

    private void SetTermination()
    {
        try
        {
            _session!.GetType().GetProperty("ReadTermination")?.SetValue(_session, "\n");
            _session!.GetType().GetProperty("WriteTermination")?.SetValue(_session, "\n");
        }
        catch { }
    }

    private void EnsureConnected()
    {
        if (_session == null) throw new InvalidOperationException("SignalGenerator 未连接");
    }
}
