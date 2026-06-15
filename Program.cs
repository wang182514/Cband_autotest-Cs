using CbandAutoTest.Config;

namespace CbandAutoTest;

/// <summary>
/// 程序入口 —— 相当于 C 的 main() 或 Python 的 if __name__ == "__main__"
/// </summary>
static class Program
{
    /// <summary>
    /// [STAThread] 是 WinForms 的要求，表示使用单线程单元模型（UI 控件只能在创建它的线程上操作）
    /// </summary>
    [STAThread]
    static void Main()
    {
        // 初始化 WinForms 默认外观（字体、高 DPI 等）
        ApplicationConfiguration.Initialize();

        // 获取 exe 所在目录
        // 相当于 Python: os.path.dirname(sys.executable)
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;

        // 先加载出厂默认配置（default_settings.json）
        var defaultsPath = Path.Combine(baseDir, "Config", "default_settings.json");
        var config = new ConfigManager(defaultsPath);
        config.LoadDefaults();

        // 再用用户自定义配置覆盖（user_settings.json），用户未设置的项保留默认值
        var userPath = Path.Combine(baseDir, "Config", "user_settings.json");
        config.LoadUser(userPath);

        // Application.Run 启动 Windows 消息循环
        // 这行代码会"阻塞"直到窗口关闭 —— 之前之后写的代码都不会执行
        Application.Run(new MainForm(config));
    }
}
