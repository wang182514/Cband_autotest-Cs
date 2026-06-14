using CbandAutoTest.Config;

namespace CbandAutoTest;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var defaultsPath = Path.Combine(baseDir, "Config", "default_settings.json");
        var config = new ConfigManager(defaultsPath);
        config.LoadDefaults();

        var userPath = Path.Combine(baseDir, "Config", "user_settings.json");
        config.LoadUser(userPath);

        Application.Run(new MainForm(config));
    }
}
