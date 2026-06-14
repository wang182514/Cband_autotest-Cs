using System.IO.Ports;
using CbandAutoTest.Instruments.Abstractions;

namespace CbandAutoTest.Instruments;

public class SwitchMatrix : ISwitchMatrix
{
    private readonly string _comPort;
    private readonly int _baudRate;
    private readonly int _timeoutMs;
    private SerialPort? _serial;
    private string _idn = "", _lastError = "";
    private bool _disposed;
    private static readonly byte[] UdcHead = [0x55, 0x44, 0x43];

    public SwitchMatrix(string comPort, int baudRate = 115200, int timeoutMs = 1000)
    { _comPort = comPort; _baudRate = baudRate; _timeoutMs = timeoutMs; }

    public string Idn => _idn;
    public bool IsConnected => _serial?.IsOpen ?? false;
    public string LastError => _lastError;

    public string Connect()
    {
        Disconnect(); _lastError = "";
        _serial = new SerialPort(_comPort, _baudRate, Parity.None, 8, StopBits.One)
        { ReadTimeout = _timeoutMs, WriteTimeout = _timeoutMs };
        _serial.Open();
        _serial.DiscardInBuffer();
        _serial.DiscardOutBuffer();
        _idn = $"{_comPort} @ {_baudRate} baud";
        return _idn;
    }

    public void Disconnect()
    {
        if (_serial?.IsOpen ?? false) _serial.Close();
        _serial?.Dispose();
        _serial = null; _idn = "";
    }

    public void Dispose()
    {
        if (!_disposed) { Disconnect(); _disposed = true; }
    }

    public void SetUdcSwitches(int sw1, int sw2, int sw3, int sw4)
        => SetUdc(2, 3950, 40, 1, sw1, sw2, sw3, sw4);

    public Dictionary<string, object>? PsaSetMode(int mode, int[] swOn)
    {
        EnsureConnected();
        var swList = new List<int> { 0 };
        swList.AddRange(swOn);
        swList.Add(mode);
        var ctrlBody = GenerateFrame(swList, new[] { 5, 1, 1, 1, 8 });
        var cmd = new List<byte> { 0x51, 0xAA, 0x5A, (byte)ctrlBody.Count };
        cmd.AddRange(ctrlBody.Select(b => (byte)b));
        cmd.Add((byte)(cmd.Sum(b => (int)b) % 256));
        _serial!.Write(cmd.ToArray(), 0, cmd.Count);
        Thread.Sleep(50);
        try
        {
            var back = new byte[9];
            var r = _serial.Read(back, 0, 9);
            if (r >= 9 && back[0] == 0x51 && back[1] == 0xAA)
            {
                return new Dictionary<string, object>
                {
                    ["temperature_c"] = (back[6] * 256 + back[7]) / 100.0,
                    ["ack"] = back[4] * 256 + back[5],
                    ["lo_lock"] = back[8]
                };
            }
        }
        catch { }
        return null;
    }

    public void SaveUdcConfig() => SetUdc(31);

    private void SetUdc(int opMode, int freq = 3950, int tgain = 40, int loEn = 1,
                        int sw1 = 0, int sw2 = 0, int sw3 = 0, int sw4 = 0)
    {
        EnsureConnected();
        var payload = BuildCtrlPacket(opMode, freq, tgain, loEn, sw1, sw2, sw3, sw4);
        var checksum = (byte)((UdcHead.Sum(b => (int)b) + payload.Sum(b => (int)b)) % 256);
        var frame = UdcHead.Concat(payload).Append(checksum).ToArray();
        _serial!.Write(frame, 0, frame.Length);
        Thread.Sleep(50);
        try { _serial.Read(new byte[8], 0, 8); }
        catch { }
    }

    private static byte[] BuildCtrlPacket(int opMode, int freq, int tgain, int loEn,
                                           int sw1, int sw2, int sw3, int sw4)
    {
        int[] payload = opMode switch
        {
            1 => [freq >> 8 & 0xFF, freq & 0xFF, tgain * 2 & 0xFF, 0],
            2 => [0, 0, loEn & 0xFF, ((sw4 << 3) | (sw3 << 2) | (sw2 << 1) | sw1) & 0xFF],
            _ => [0, 0, 0, 0]
        };
        payload = [.. payload, opMode & 0xFF];
        return payload.Select(b => (byte)b).ToArray();
    }

    private static List<int> GenerateFrame(IList<int> ctrlElements, IList<int> ctrlLengths)
    {
        if (ctrlElements.Count != ctrlLengths.Count) return [0];
        var bitString = "";
        for (int i = 0; i < ctrlElements.Count; i++)
        {
            var val = Math.Max(0, Math.Min(ctrlElements[i], (1 << ctrlLengths[i]) - 1));
            bitString = Convert.ToString(val, 2).PadLeft(ctrlLengths[i], '0') + bitString;
        }
        var pad = (8 - bitString.Length % 8) % 8;
        bitString = new string('0', pad) + bitString;
        var body = new List<int>();
        for (int i = 0; i < bitString.Length; i += 8)
            body.Add(Convert.ToInt32(bitString.Substring(i, 8), 2));
        return body;
    }

    private void EnsureConnected()
    {
        if (_serial == null || !_serial.IsOpen)
            throw new InvalidOperationException("SwitchMatrix 未连接");
    }
}
