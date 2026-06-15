using System.Net.Sockets;
using System.Text;
using CbandAutoTest.Instruments.Abstractions;

namespace CbandAutoTest.Instruments;

/// <summary>
/// 直流电源驱动 —— 通过 TCP/IP 发 SCPI 命令控制
/// 
/// 【通信原理】和 Python socket 完全一样：
///   new TcpClient() → Connect(ip, port) → GetStream() → Write/Read
///   命令格式：SCPI（Standard Commands for Programmable Instruments）
///   例：发 "*IDN?\n"  收 "TDK-Lambda GEN-600...\n"
/// 
/// 【概念】private 字段（_tcp, _stream 等）= 只在类内部使用
///   下划线前缀是 C# 命名惯例，表示"这是私有成员变量"
///   外部只能通过 public 方法和属性访问
/// </summary>
public class PowerSupply : IPowerSupply
{
    // ---- 构造参数（readonly：构造后不可变） ----
    private readonly string _ip;
    private readonly int _port;
    private readonly int _timeoutMs;

    // ---- 运行时状态 ----
    private TcpClient? _tcp;          // TcpClient? 的 ? 表示可以为 null（连接前就是 null）
    private NetworkStream? _stream;   // 从 TcpClient 获取的数据流
    private string _idn = "";         // 仪器 IDN 标识
    private string _lastError = "";
    private bool _disposed;           // 是否已释放

    public PowerSupply(string ip, int port = 2268, double timeoutSec = 1.0)
    {
        _ip = ip; _port = port; _timeoutMs = (int)(timeoutSec * 1000);
    }

    // ---- 属性 ----
    // => 是表达式体成员（Lambda body），等价于 { get { return _idn; } }
    public string Idn => _idn;

    /// <summary>
    /// ?. 是 null 条件运算符：_tcp 不为 null 时才访问 .Connected
    /// ?? 是 null 合并运算符：左边为 null 时返回右边的值
    /// 合起来：如果 _tcp 为 null 或被断开，返回 false
    /// </summary>
    public bool IsConnected => _tcp?.Connected ?? false;
    public string LastError => _lastError;

    /// <summary>
    /// 连接电源 —— TCP 握手 + 发 *IDN? 确认通信
    /// </summary>
    public string Connect()
    {
        Disconnect(); _lastError = "";
        _tcp = new TcpClient();
        _tcp.ReceiveTimeout = _timeoutMs;  // 收数据超时（毫秒）
        _tcp.SendTimeout = _timeoutMs;     // 发数据超时
        _tcp.Connect(_ip, _port);           // TCP 三次握手
        _stream = _tcp.GetStream();         // 获取数据流（相当于 Python socket.recv/send）
        _idn = Query("*IDN?");              // 发 SCPI 查询设备身份
        return _idn;
    }

    public void Disconnect()
    {
        _stream?.Close(); _stream = null;  // ?.Close()：不为 null 才调用 Close
        _tcp?.Close(); _tcp = null;
        _idn = "";
    }

    /// <summary>
    /// 实现 IDisposable —— 保证对象被回收时断开 TCP 连接
    /// </summary>
    public void Dispose()
    {
        if (!_disposed) { Disconnect(); _disposed = true; }
    }

    public void SetOutput(bool on)
    {
        Send($"OUTP {(on ? "1" : "0")}");
        Thread.Sleep(200);
    }

    /// <summary>
    /// 测量当前电压（伏特）
    /// double.TryParse 是 C# 的安全解析：成功则 v 有值，失败返回 false
    /// double.NaN 是"Not a Number"，相当于 Python 的 float('nan')
    /// </summary>
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

    // ========================================================================
    //  底层 SCPI 通信方法
    // ========================================================================

    /// <summary>
    /// 发送 SCPI 命令（自动追加换行符 \n）
    /// Encoding.ASCII.GetBytes 把字符串转成字节数组（TCP 只能传字节）
    /// </summary>
    private void Send(string cmd)
    {
        if (_tcp == null || !_tcp.Connected)
            throw new InvalidOperationException("PowerSupply 未连接");
        var data = Encoding.ASCII.GetBytes(cmd.EndsWith("\n") ? cmd : cmd + "\n");
        _stream!.Write(data, 0, data.Length);  // _stream! 的 ! 是 null 容错（我们已检查 Connected 所以不会是 null）
    }

    /// <summary>
    /// 发送查询命令并读取回复（SCPI 的经典三步：发命令 → 等 50ms → 读回复）
    /// 循环读取直到收到换行符 \n（仪器回复以换行结尾）
    /// </summary>
    private string Query(string cmd)
    {
        Send(cmd);
        Thread.Sleep(50);  // 等仪器处理命令并准备回复
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
            } while (bytes > 0 && !sb.ToString().Contains('\n'));  // 读到换行符才停
        }
        catch (IOException) { }  // 超时等 IO 异常静默处理
        return sb.ToString().Trim();
    }
}
