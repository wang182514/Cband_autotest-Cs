using System.Text;

namespace CbandAutoTest.Utils;

public enum LogLevel { Debug, Info, Warning, Error }

public class LogEventArgs : EventArgs
{
    public DateTime Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public string Message { get; set; } = "";
}

public class Logger : IDisposable
{
    private readonly StreamWriter _writer;
    private readonly object _lock = new();
    private bool _disposed;

    public event EventHandler<LogEventArgs>? OnLog;

    public Logger(string logDir)
    {
        Directory.CreateDirectory(logDir);
        var file = Path.Combine(logDir, $"test_log_{DateTime.Now:yyyyMMdd_HHmmss}.log");
        _writer = new StreamWriter(file, append: true, Encoding.UTF8) { AutoFlush = true };
    }

    public void Debug(string msg) => Write(LogLevel.Debug, msg);
    public void Info(string msg) => Write(LogLevel.Info, msg);
    public void Warning(string msg) => Write(LogLevel.Warning, msg);
    public void Error(string msg) => Write(LogLevel.Error, msg);

    private void Write(LogLevel level, string msg)
    {
        var ts = DateTime.Now;
        var line = $"[{ts:HH:mm:ss}] [{level,-7}] {msg}";
        lock (_lock) _writer.WriteLine(line);
        OnLog?.Invoke(this, new LogEventArgs { Timestamp = ts, Level = level, Message = msg });
    }

    public void Dispose()
    {
        if (!_disposed) { _writer?.Close(); _writer?.Dispose(); _disposed = true; }
    }
}
