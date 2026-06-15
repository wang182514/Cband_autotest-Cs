using CbandAutoTest.Config;
using CbandAutoTest.Instruments.Abstractions;
using CbandAutoTest.Utils;

namespace CbandAutoTest;

/// <summary>
/// 主窗体 —— 程序唯一窗口
/// 
/// 【重要】partial class 表示这个类的代码分布在两个文件中：
///   MainForm.cs          ← 你写业务逻辑的地方（当前文件）
///   MainForm.Designer.cs ← VS 设计器自动管理的地方（控件声明 + InitializeComponent）
///   编译时两个文件合并成一个类，和写在一个文件里效果一样
/// </summary>
public partial class MainForm : Form
{
    // ---- 构造函数注入的依赖（从 Program.cs 传进来） ----
    private readonly ConfigManager _config;
    private readonly Logger _logger;

    /// <summary>
    /// 仪器管理器 —— 只在实际需要连接时才创建（懒初始化）
    /// 后面的 ? 表示这个字段可以是 null（C# 的可空引用类型）
    /// </summary>
    private InstrumentManager? _instrumentManager;

    public MainForm(ConfigManager config, Logger? logger = null)
    {
        _config = config;
        // ?? 是空合并运算符：左边不为 null 就用左边，否则用右边
        // 相当于 Python: logger = logger or Logger(...)
        _logger = logger ?? new Logger(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs"));

        // 调用 Designer.cs 里的 InitializeComponent()，创建所有控件并设置位置
        // 这行必须在构造函数第一件事做，否则控件还没创建就去操作会崩溃
        InitializeComponent();

        // 把配置文件里的 IP/端口 值填入输入框
        LoadConfigToUI();

        // ---- 订阅日志事件：每写一条日志就更新底部状态栏 ----
        // OnLog 是 Logger 类中定义的 event（事件），+= 表示"当这件事发生时调用我"
        // (_, args) => { ... } 是 Lambda 表达式：匿名函数，参数是 sender 和事件数据
        _logger.OnLog += (_, args) =>
        {
            if (args.Level <= LogLevel.Info)
                // BeginInvoke：切回 UI 线程更新控件
                BeginInvoke(() => _statusLabel.Text = args.Message);
        };

        // ====================================================================
        //  按钮事件绑定 —— 把"用户点击"和"业务逻辑"关联起来
        //  (_, _) => 方法名()  是一种 Lambda 写法，两个 _ 表示不关心 sender 和 EventArgs 参数
        //  m => m.RxPower 是一种委托：告诉 ConnectInstrument"从仪器管理器里取哪台仪器"
        // ====================================================================

        _btnRxPwrConn.Click += (_, _) => ConnectInstrument(m => m.RxPower, "接收电源", _lblRxPwrStatus, _lblRxPwrDetail, _btnRxPwrConn, _btnRxPwrDisconn);
        _btnRxPwrDisconn.Click += (_, _) => DisconnectInstrument(m => m.RxPower, "接收电源", _lblRxPwrStatus, _lblRxPwrDetail, _btnRxPwrConn, _btnRxPwrDisconn);
        _btnTxPwrConn.Click += (_, _) => ConnectInstrument(m => m.TxPower, "发射电源", _lblTxPwrStatus, _lblTxPwrDetail, _btnTxPwrConn, _btnTxPwrDisconn);
        _btnTxPwrDisconn.Click += (_, _) => DisconnectInstrument(m => m.TxPower, "发射电源", _lblTxPwrStatus, _lblTxPwrDetail, _btnTxPwrConn, _btnTxPwrDisconn);
        _btnVsgConn.Click += (_, _) => ConnectInstrument(m => m.SignalGenerator, "信号源", _lblVsgStatus, _lblVsgDetail, _btnVsgConn, _btnVsgDisconn);
        _btnVsgDisconn.Click += (_, _) => DisconnectInstrument(m => m.SignalGenerator, "信号源", _lblVsgStatus, _lblVsgDetail, _btnVsgConn, _btnVsgDisconn);
        _btnSaConn.Click += (_, _) => ConnectInstrument(m => m.SpectrumAnalyzer, "频谱仪", _lblSaStatus, _lblSaDetail, _btnSaConn, _btnSaDisconn);
        _btnSaDisconn.Click += (_, _) => DisconnectInstrument(m => m.SpectrumAnalyzer, "频谱仪", _lblSaStatus, _lblSaDetail, _btnSaConn, _btnSaDisconn);
        _btnSwitchConn.Click += (_, _) => ConnectInstrument(m => m.SwitchMatrix, "开关矩阵", _lblSwitchStatus, _lblSwitchDetail, _btnSwitchConn, _btnSwitchDisconn);
        _btnSwitchDisconn.Click += (_, _) => DisconnectInstrument(m => m.SwitchMatrix, "开关矩阵", _lblSwitchStatus, _lblSwitchDetail, _btnSwitchConn, _btnSwitchDisconn);
        _btnConnectAll.Click += (_, _) => ConnectAll();
        _btnDisconnectAll.Click += (_, _) => DisconnectAll();
        _btnSaveDefaultConfig.Click += (_, _) => SaveCurrentConfig();
    }

    // ========================================================================
    //  连接 / 断开
    // ========================================================================

    /// <summary>
    /// 确保仪器管理器已创建 —— 懒初始化（第一次连仪器时才 new）
    /// 这样做的好处：程序启动时不会尝试连接仪器，只有用户点了连接按钮才初始化
    /// </summary>
    private void EnsureInstrumentManager()
    {
        if (_instrumentManager == null)
        {
            SaveUIToConfig();  // 连之前先把 UI 里的 IP/端口 收集到配置对象
            _instrumentManager = new InstrumentManager(_config.Instruments, _logger);
        }
    }

    /// <summary>
    /// 连接一台仪器（5 台仪器共用这个方法）
    /// 
    /// 【重要】为什么用 Func<InstrumentManager, IInstrument>？
    ///   这是一个"函数指针"类型的参数：调用者传一个 lambda，告诉本方法"从 mgr 里取哪台仪器"
    ///   调用示例：ConnectInstrument(m => m.RxPower, ...)   —— 取接收电源
    ///            ConnectInstrument(m => m.TxPower, ...)   —— 取发射电源
    ///   这样 5 台仪器用同一个方法，不用写 5 份几乎一样的代码
    /// 
    /// 【重要】Task.Run + BeginInvoke 异步模式：
    ///   Task.Run(() => { ... })  —— 启动后台线程执行耗时操作（连接仪器可能等几秒）
    ///   BeginInvoke(() => { ... })—— 回到 UI 线程更新控件（Windows 规定只有创建控件的线程才能修改它）
    ///   如果不这样做，点连接按钮后整个窗口会卡死几秒
    /// </summary>
    private void ConnectInstrument(Func<InstrumentManager, IInstrument> selector, string name,
        Label status, Label detail, Button connBtn, Button disconnBtn)
    {
        EnsureInstrumentManager();
        SaveUIToConfig();
        connBtn.Enabled = false;    // 禁用按钮，防止重复点击
        UseWaitCursor = true;       // 鼠标变沙漏

        Task.Run(() =>
        {
            try
            {
                // _instrumentManager! 中的 ! 是 null 容错运算符
                // 告诉编译器"我知道这不是 null"，因为 EnsureInstrumentManager() 已经确保了
                var idn = selector(_instrumentManager!).Connect();   // ← 这行在后台线程执行
                BeginInvoke(() =>
                {
                    // ← 这段在 UI 线程执行，可以安全修改控件
                    status.Text = "● 已连接"; status.ForeColor = Color.Green;
                    detail.Text = idn;
                    connBtn.Enabled = false; disconnBtn.Enabled = true;
                });
            }
            catch (Exception ex)
            {
                BeginInvoke(() =>
                {
                    status.Text = "✗ 失败"; status.ForeColor = Color.Red;
                    detail.Text = ex.Message;
                    connBtn.Enabled = true;
                });
            }
            finally { BeginInvoke(() => UseWaitCursor = false); }
        });
    }

    /// <summary>
    /// 断开一台仪器（和上面的 ConnectInstrument 镜像对称）
    /// 断开操作很快（毫秒级），不需要后台线程，直接在主线程执行即可
    /// </summary>
    private void DisconnectInstrument(Func<InstrumentManager, IInstrument> selector, string name,
        Label status, Label detail, Button connBtn, Button disconnBtn)
    {
        if (_instrumentManager == null) return;
        try
        {
            selector(_instrumentManager).Disconnect();
            status.Text = "○ 未连接"; status.ForeColor = Color.Gray; detail.Text = "";
            connBtn.Enabled = true; disconnBtn.Enabled = false;
        }
        catch (Exception ex) { _logger.Warning($"断开 {name} 异常: {ex.Message}"); }
    }

    /// <summary>
    /// 全部连接 —— 点「全部连接」按钮时调用
    /// 同样用 Task.Run 避免卡 UI
    /// </summary>
    private void ConnectAll()
    {
        EnsureInstrumentManager(); SaveUIToConfig(); UseWaitCursor = true;
        SetAllUIEnabled(false);
        Task.Run(() =>
        {
            _instrumentManager!.ConnectAll();  // InstrumentManager 依次连接 5 台仪器
            BeginInvoke(() => { RefreshAllStatus(); UseWaitCursor = false; SetAllUIEnabled(true); });
        });
    }

    /// <summary>
    /// 全部断开 —— 同样不需要后台线程（断开很快）
    /// </summary>
    private void DisconnectAll()
    {
        _instrumentManager?.DisconnectAll();  // ?. 表示：_instrumentManager 不是 null 才调用
        RefreshAllStatus();
    }

    // ========================================================================
    //  UI 状态同步 —— 根据仪器的实际连接状态刷新指示灯和按钮
    // ========================================================================

    /// <summary>
    /// 刷新 5 台仪器的 UI 状态（指示灯颜色、按钮开/关、IDN 信息）
    /// 通常在连接/断开操作完成后调用
    /// </summary>
    private void RefreshAllStatus()
    {
        RefreshOne(_instrumentManager?.RxPower, _lblRxPwrStatus, _lblRxPwrDetail, _btnRxPwrConn, _btnRxPwrDisconn);
        RefreshOne(_instrumentManager?.TxPower, _lblTxPwrStatus, _lblTxPwrDetail, _btnTxPwrConn, _btnTxPwrDisconn);
        RefreshOne(_instrumentManager?.SignalGenerator, _lblVsgStatus, _lblVsgDetail, _btnVsgConn, _btnVsgDisconn);
        RefreshOne(_instrumentManager?.SpectrumAnalyzer, _lblSaStatus, _lblSaDetail, _btnSaConn, _btnSaDisconn);
        RefreshOne(_instrumentManager?.SwitchMatrix, _lblSwitchStatus, _lblSwitchDetail, _btnSwitchConn, _btnSwitchDisconn);
    }

    /// <summary>
    /// 刷新单台仪器的 UI 状态
    /// </summary>
    /// <param name="i">仪器对象（可为 null）</param>
    /// <param name="s">状态指示灯 Label（显示 ●已连接 / ○未连接 / ✗失败）</param>
    /// <param name="d">详情 Label（显示 IDN 字符串 或 错误信息）</param>
    /// <param name="c">连接按钮</param>
    /// <param name="dc">断开按钮</param>
    private void RefreshOne(IInstrument? i, Label s, Label d, Button c, Button dc)
    {
        if (i == null) { s.Text = "○ 未连接"; s.ForeColor = Color.Gray; d.Text = ""; c.Enabled = true; dc.Enabled = false; return; }
        if (i.IsConnected) { s.Text = "● 已连接"; s.ForeColor = Color.Green; d.Text = i.Idn; c.Enabled = false; dc.Enabled = true; }
        else { s.Text = "○ 未连接"; s.ForeColor = Color.Gray; d.Text = string.IsNullOrEmpty(i.LastError) ? "" : $"失败: {i.LastError}"; c.Enabled = true; dc.Enabled = false; }
    }

    private void SetAllUIEnabled(bool en)
    {
        _btnConnectAll.Enabled = en; _btnDisconnectAll.Enabled = en; _btnSaveDefaultConfig.Enabled = en;
    }

    // ========================================================================
    //  配置 —— UI 数值 和 配置对象 之间的双向同步
    // ========================================================================

    /// <summary>
    /// 配置 → UI：程序启动时把 config 里的值填入输入框
    /// </summary>
    private void LoadConfigToUI()
    {
        var c = _config.Instruments;
        _txtRxPwrIp.Text = c.RxPowerSupply.Ip; _numRxPwrPort.Value = c.RxPowerSupply.Port;
        _txtTxPwrIp.Text = c.TxPowerSupply.Ip; _numTxPwrPort.Value = c.TxPowerSupply.Port;
        _txtVsgIp.Text = c.SignalGenerator.Ip;
        _txtSaIp.Text = c.SpectrumAnalyzer.Ip;
        _txtSwitchCom.Text = c.SwitchMatrix.ComPort; _numSwitchBaud.Value = c.SwitchMatrix.BaudRate;
    }

    /// <summary>
    /// UI → 配置：连接前把输入框当前值读回配置对象
    /// 这样即使用户修改了 IP/端口但没有点保存，连接时也会使用最新值
    /// </summary>
    private void SaveUIToConfig()
    {
        var c = _config.Instruments;
        c.RxPowerSupply.Ip = _txtRxPwrIp.Text.Trim(); c.RxPowerSupply.Port = (int)_numRxPwrPort.Value;
        c.TxPowerSupply.Ip = _txtTxPwrIp.Text.Trim(); c.TxPowerSupply.Port = (int)_numTxPwrPort.Value;
        c.SignalGenerator.Ip = _txtVsgIp.Text.Trim();
        c.SpectrumAnalyzer.Ip = _txtSaIp.Text.Trim();
        c.SwitchMatrix.ComPort = _txtSwitchCom.Text.Trim(); c.SwitchMatrix.BaudRate = (int)_numSwitchBaud.Value;
    }

    /// <summary>
    /// 点击「保存为默认配置」按钮：把当前 UI 值写入 user_settings.json
    /// </summary>
    private void SaveCurrentConfig()
    {
        SaveUIToConfig();
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "user_settings.json");
        _config.SaveUser(path);
        _logger.Info($"配置已保存到 {path}");
    }

    // ========================================================================
    //  占位 Tab 页（测试执行 / 参数设置 / 结果查看 / 系统日志）
    //  目前只显示一行提示文字，待后续开发
    // ========================================================================

    /// <summary>
    /// 创建一个只有居中提示文字的占位 Tab 页
    /// static 表示这个方法属于"类本身"而非"某个实例"，不依赖任何实例字段
    /// </summary>
    private static TabPage MakePlaceholderTab(string title, string text)
    {
        var tab = new TabPage(title);
        var label = new Label
        {
            Text = text,
            Dock = DockStyle.Fill,                      // 填满整个 Tab 页
            TextAlign = ContentAlignment.MiddleCenter,  // 文字居中
            ForeColor = Color.Gray,
            Font = new Font("Microsoft YaHei UI", 11F)
        };
        tab.Controls.Add(label);  // 把 Label 添加为 TabPage 的子控件
        return tab;
    }
}
