using System.Text;

namespace CbandAutoTest.Utils;

/// <summary>日志级别：Debug < Info < Warning < Error</summary>
public enum LogLevel { Debug, Info, Warning, Error }

/// <summary>
/// 日志事件数据 —— 当一条日志被写入时，附带的详细信息
/// 继承 EventArgs 才能作为 event 的参数类型
/// </summary>
public class LogEventArgs : EventArgs
{
    public DateTime Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public string Message { get; set; } = "";
}

/// <summary>
/// 日志器 —— 同时写文件 + 触发事件
/// 
/// 【概念】event（事件）= "当某事发生时通知订阅者"
///   写日志 → 触发 OnLog 事件 → MainForm 收到后更新状态栏
///   lock (_lock) 保证多线程写文件时不会交错乱码
/// </summary>
public class Logger : IDisposable
{
    private readonly StreamWriter _writer;
    // lock 对象：类似 Python 的 threading.Lock
    private readonly object _lock = new();
    private bool _disposed;

    /// <summary>
    /// 日志事件 —— 每写一条日志触发一次
    /// EventHandler<LogEventArgs> 是 C# 的泛型委托，表示"接受 (object sender, LogEventArgs args) 的方法"
    /// 末尾的 ? 表示这个事件可能没有任何订阅者
    /// </summary>
    public event EventHandler<LogEventArgs>? OnLog;

    /// <summary>
    /// 创建日志器，自动在指定目录下创建带时间戳的日志文件
    /// 文件名格式：test_log_20260115_143052.log
    /// </summary>
    public Logger(string logDir)
    {
        Directory.CreateDirectory(logDir);
        var file = Path.Combine(logDir, $"test_log_{DateTime.Now:yyyyMMdd_HHmmss}.log");
        // AutoFlush = true：每条日志立即写入磁盘（不用等缓冲区满）
        _writer = new StreamWriter(file, append: true, Encoding.UTF8) { AutoFlush = true };
    }

    public void Debug(string msg) => Write(LogLevel.Debug, msg);
    public void Info(string msg) => Write(LogLevel.Info, msg);
    public void Warning(string msg) => Write(LogLevel.Warning, msg);
    public void Error(string msg) => Write(LogLevel.Error, msg);

    /// <summary>
    /// 写一条日志（线程安全）
    /// lock (_lock) { ... } 确保同一时刻只有一个线程在执行花括号里的代码
    /// </summary>
    private void Write(LogLevel level, string msg)
    {
        var ts = DateTime.Now;
        var line = $"[{ts:HH:mm:ss}] [{level,-7}] {msg}";  // -7 表示左对齐占 7 格，让级别列整齐
        lock (_lock) _writer.WriteLine(line);
        // OnLog?.Invoke()：? 表示如果有订阅者才触发（没有也不报错）
        OnLog?.Invoke(this, new LogEventArgs { Timestamp = ts, Level = level, Message = msg });
    }

    public void Dispose()
    {
        if (!_disposed) { _writer?.Close(); _writer?.Dispose(); _disposed = true; }
    }
}
