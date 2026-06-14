using CbandAutoTest.Config;
using CbandAutoTest.Instruments.Abstractions;
using CbandAutoTest.Utils;

namespace CbandAutoTest;

public partial class MainForm : Form
{
    private readonly ConfigManager _config;
    private readonly Logger _logger;
    private InstrumentManager? _instrumentManager;

    public MainForm(ConfigManager config, Logger? logger = null)
    {
        _config = config;
        _logger = logger ?? new Logger(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs"));

        InitializeComponent();
        LoadConfigToUI();

        // 日志 → 状态栏
        _logger.OnLog += (_, args) =>
        {
            if (args.Level <= LogLevel.Info)
                BeginInvoke(() => _statusLabel.Text = args.Message);
        };

        // ---- 绑定所有按钮事件 ----
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

    private void EnsureInstrumentManager()
    {
        if (_instrumentManager == null)
        {
            SaveUIToConfig();
            _instrumentManager = new InstrumentManager(_config.Instruments, _logger);
        }
    }

    private void ConnectInstrument(Func<InstrumentManager, IInstrument> selector, string name,
        Label status, Label detail, Button connBtn, Button disconnBtn)
    {
        EnsureInstrumentManager();
        SaveUIToConfig();
        connBtn.Enabled = false;
        UseWaitCursor = true;

        Task.Run(() =>
        {
            try
            {
                var idn = selector(_instrumentManager!).Connect();
                BeginInvoke(() =>
                {
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

    private void ConnectAll()
    {
        EnsureInstrumentManager(); SaveUIToConfig(); UseWaitCursor = true;
        SetAllUIEnabled(false);
        Task.Run(() =>
        {
            _instrumentManager!.ConnectAll();
            BeginInvoke(() => { RefreshAllStatus(); UseWaitCursor = false; SetAllUIEnabled(true); });
        });
    }

    private void DisconnectAll()
    {
        _instrumentManager?.DisconnectAll();
        RefreshAllStatus();
    }

    // ========================================================================
    //  UI 状态
    // ========================================================================

    private void RefreshAllStatus()
    {
        RefreshOne(_instrumentManager?.RxPower, _lblRxPwrStatus, _lblRxPwrDetail, _btnRxPwrConn, _btnRxPwrDisconn);
        RefreshOne(_instrumentManager?.TxPower, _lblTxPwrStatus, _lblTxPwrDetail, _btnTxPwrConn, _btnTxPwrDisconn);
        RefreshOne(_instrumentManager?.SignalGenerator, _lblVsgStatus, _lblVsgDetail, _btnVsgConn, _btnVsgDisconn);
        RefreshOne(_instrumentManager?.SpectrumAnalyzer, _lblSaStatus, _lblSaDetail, _btnSaConn, _btnSaDisconn);
        RefreshOne(_instrumentManager?.SwitchMatrix, _lblSwitchStatus, _lblSwitchDetail, _btnSwitchConn, _btnSwitchDisconn);
    }

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
    //  配置
    // ========================================================================

    private void LoadConfigToUI()
    {
        var c = _config.Instruments;
        _txtRxPwrIp.Text = c.RxPowerSupply.Ip; _numRxPwrPort.Value = c.RxPowerSupply.Port;
        _txtTxPwrIp.Text = c.TxPowerSupply.Ip; _numTxPwrPort.Value = c.TxPowerSupply.Port;
        _txtVsgIp.Text = c.SignalGenerator.Ip;
        _txtSaIp.Text = c.SpectrumAnalyzer.Ip;
        _txtSwitchCom.Text = c.SwitchMatrix.ComPort; _numSwitchBaud.Value = c.SwitchMatrix.BaudRate;
    }

    private void SaveUIToConfig()
    {
        var c = _config.Instruments;
        c.RxPowerSupply.Ip = _txtRxPwrIp.Text.Trim(); c.RxPowerSupply.Port = (int)_numRxPwrPort.Value;
        c.TxPowerSupply.Ip = _txtTxPwrIp.Text.Trim(); c.TxPowerSupply.Port = (int)_numTxPwrPort.Value;
        c.SignalGenerator.Ip = _txtVsgIp.Text.Trim();
        c.SpectrumAnalyzer.Ip = _txtSaIp.Text.Trim();
        c.SwitchMatrix.ComPort = _txtSwitchCom.Text.Trim(); c.SwitchMatrix.BaudRate = (int)_numSwitchBaud.Value;
    }

    private void SaveCurrentConfig()
    {
        SaveUIToConfig();
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "user_settings.json");
        _config.SaveUser(path);
        _logger.Info($"配置已保存到 {path}");
    }

    // ========================================================================
    //  占位 Tab 页生成（供 Designer.cs 调用）
    // ========================================================================

    private static TabPage MakePlaceholderTab(string title, string text)
    {
        var tab = new TabPage(title);
        var label = new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.Gray,
            Font = new Font("Microsoft YaHei UI", 11F)
        };
        tab.Controls.Add(label);
        return tab;
    }
}
