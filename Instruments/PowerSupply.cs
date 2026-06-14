using System.Net.Sockets;
using System.Text;
using CbandAutoTest.Instruments.Abstractions;

namespace CbandAutoTest.Instruments;

public class PowerSupply : IPowerSupply
{
    private readonly string _ip;
    private readonly int _port;
    private readonly int _timeoutMs;
    private TcpClient? _tcp;
    private NetworkStream? _stream;
    private string _idn = "";
    private string _lastError = "";
    private bool _disposed;

    public PowerSupply(string ip, int port = 2268, double timeoutSec = 1.0)
    {
        _ip = ip; _port = port; _timeoutMs = (int)(timeoutSec * 1000);
    }

    public string Idn => _idn;
    public bool IsConnected => _tcp?.Connected ?? false;
    public string LastError => _lastError;

    public string Connect()
    {
        Disconnect(); _lastError = "";
        _tcp = new TcpClient();
        _tcp.ReceiveTimeout = _timeoutMs;
        _tcp.SendTimeout = _timeoutMs;
        _tcp.Connect(_ip, _port);
        _stream = _tcp.GetStream();
        _idn = Query("*IDN?");
        return _idn;
    }

    public void Disconnect()
    {
        _stream?.Close(); _stream = null;
        _tcp?.Close(); _tcp = null;
        _idn = "";
    }

    public void Dispose()
    {
        if (!_disposed) { Disconnect(); _disposed = true; }
    }

    public void SetOutput(bool on)
    {
        Send($"OUTP {(on ? "1" : "0")}");
        Thread.Sleep(200);
    }

    public double MeasureVoltage()
    {
        var resp = Query("MEAS:VOLT?");
        return double.TryParse(resp, out var v) ? v : double.NaN;
    }

    public double MeasureCurrent()
    {
        var resp = Query("MEAS:CURR?");
        return double.TryParse(resp, out var v) ? v : double.NaN;
    }

    public void SetVoltage(double volts) => Send($"SOUR:VOLT {volts:F3}");

    public void SetCurrent(double amps) => Send($"SOUR:CURR {amps:F3}");

    private void Send(string cmd)
    {
        if (_tcp == null || !_tcp.Connected)
            throw new InvalidOperationException("PowerSupply 未连接");
        var data = Encoding.ASCII.GetBytes(cmd.EndsWith("\n") ? cmd : cmd + "\n");
        _stream!.Write(data, 0, data.Length);
    }

    private string Query(string cmd)
    {
        Send(cmd);
        Thread.Sleep(50);
        var buffer = new byte[4096];
        var sb = new StringBuilder();
        try
        {
            int bytes;
            do
            {
                bytes = _stream!.Read(buffer, 0, buffer.Length);
                if (bytes > 0)
                    sb.Append(Encoding.ASCII.GetChars(buffer, 0, bytes));
            } while (bytes > 0 && !sb.ToString().Contains('\n'));
        }
        catch (IOException) { }
        return sb.ToString().Trim();
    }
}
