using CbandAutoTest.Instruments.Abstractions;
using Ivi.Visa;

namespace CbandAutoTest.Instruments;

/// <summary>
/// 信号发生器驱动 —— 通过 VISA 协议控制 R&S SMU200A
/// 
/// 【通信原理】VISA (Virtual Instrument Software Architecture) 是仪器通信的工业标准
///   VISA 资源字符串格式：TCPIP0::192.168.1.90::inst0::INSTR
///   底层通过 visa32.dll 和仪器通信（和 MATLAB/Python 版用的是同一个 DLL）
///   本项目通过 NuGet 包 IviFoundation.Visa 做 .NET 绑定
/// 
/// 【概念】IMessageBasedSession 是 VISA 的"会话"对象
///   相当于 TCP 的 socket：通过它发 SCPI 命令、读回复
///   FormattedIO.WriteLine() → 发命令（自动加终止符）
///   FormattedIO.ReadLine()  → 读回复（读到终止符为止）
/// </summary>
public class SignalGenerator : ISignalGenerator
{
    private readonly string _ip;
    private readonly int _timeoutMs;
    private string _idn = "", _lastError = "";
    private bool _disposed;
    private IMessageBasedSession? _session;  // VISA 会话对象

    public SignalGenerator(string ip, int timeoutMs = 5000)
    { _ip = ip; _timeoutMs = timeoutMs; }

    public string Idn => _idn;
    public bool IsConnected => _session != null;
    public string LastError => _lastError;

    /// <summary>
    /// 通过 VISA 连接信号源
    /// TCPIP0::192.168.1.90::inst0::INSTR 是 VISA 资源地址格式
    /// AccessModes.None 表示不锁定资源
    /// 0x0A 是换行符 \n 的 ASCII 码（SCPI 命令终止符）
    /// </summary>
    public string Connect()
    {
        Disconnect(); _lastError = "";
        _session = (IMessageBasedSession)GlobalResourceManager.Open(
            $"TCPIP0::{_ip}::inst0::INSTR",
            AccessModes.None,
            _timeoutMs);
        _session.TerminationCharacterEnabled = true;
        _session.TerminationCharacter = 0x0A; // \n — SCPI 命令终止符
        Write("*CLS");                         // 清除仪器状态
        _idn = Query("*IDN?").Trim();          // 查询身份
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

    private void EnsureConnected()
    {
        if (_session == null) throw new InvalidOperationException("SignalGenerator 未连接");
    }
}
