// ============================================================================
//  MainForm.Designer.cs  —— 由 VS 设计器自动生成/维护
//
// 【概念】partial class = 同一个类拆成多个文件写
//   本文件负责：  声明控件字段 + InitializeComponent() 创建/布局所有控件
//   MainForm.cs 负责：业务逻辑（按钮事件、连接仪器、配置读写）
//   编译时会自动合并成一个类，和全写在一个文件里完全等价
//
// 【重要】不要手动改 InitializeComponent()，应该用 VS 设计器拖拽控件
//   VS 设计器会重写这个方法，手动改的内容可能被覆盖
//   如果需要动态创建控件，写在 MainForm.cs 的构造函数里
// ============================================================================

namespace CbandAutoTest;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    // ---- 控件字段（设计器依赖） ----
    private TabControl _tabControl;
    private TabPage _tabConnection;
    private GroupBox _grpRxPwr;
    private TextBox _txtRxPwrIp;
    private NumericUpDown _numRxPwrPort;
    private Label _lblRxPwrStatus;
    private Label _lblRxPwrDetail;
    private Button _btnRxPwrConn;
    private Button _btnRxPwrDisconn;
    private GroupBox _grpTxPwr;
    private TextBox _txtTxPwrIp;
    private NumericUpDown _numTxPwrPort;
    private Label _lblTxPwrStatus;
    private Label _lblTxPwrDetail;
    private Button _btnTxPwrConn;
    private Button _btnTxPwrDisconn;
    private GroupBox _grpVSG;
    private TextBox _txtVsgIp;
    private Label _lblVsgStatus;
    private Label _lblVsgDetail;
    private Button _btnVsgConn;
    private Button _btnVsgDisconn;
    private GroupBox _grpSA;
    private TextBox _txtSaIp;
    private Label _lblSaStatus;
    private Label _lblSaDetail;
    private Button _btnSaConn;
    private Button _btnSaDisconn;
    private GroupBox _grpSwitch;
    private TextBox _txtSwitchCom;
    private NumericUpDown _numSwitchBaud;
    private Label _lblSwitchStatus;
    private Label _lblSwitchDetail;
    private Button _btnSwitchConn;
    private Button _btnSwitchDisconn;
    private Button _btnConnectAll;
    private Button _btnDisconnectAll;
    private Button _btnSaveDefaultConfig;
    private TabPage _tabTest, _tabSettings, _tabResults, _tabLog;
    private StatusStrip _statusStrip;
    private ToolStripStatusLabel _statusLabel;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        // ---- 窗体 ----
        this.Text = "C波段射频模块自动化测试系统";
        this.Size = new System.Drawing.Size(1200, 850);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);

        // ---- TabControl ----
        _tabControl = new TabControl();
        _tabControl.Dock = DockStyle.Fill;

        // ========== Tab 1: 仪器连接 ==========
        _tabConnection = new TabPage("仪器连接");

        // 接收电源
        _grpRxPwr = new GroupBox();
        _grpRxPwr.Text = "接收电源 (RX PWR)";
        _grpRxPwr.Location = new System.Drawing.Point(12, 12);
        _grpRxPwr.Size = new System.Drawing.Size(380, 170);

        Label lblRxPwrIp = new Label();
        lblRxPwrIp.Text = "IP:";
        lblRxPwrIp.Location = new System.Drawing.Point(10, 28);
        lblRxPwrIp.AutoSize = true;

        _txtRxPwrIp = new TextBox();
        _txtRxPwrIp.Location = new System.Drawing.Point(90, 25);
        _txtRxPwrIp.Size = new System.Drawing.Size(150, 23);

        Label lblRxPwrPort = new Label();
        lblRxPwrPort.Text = "端口:";
        lblRxPwrPort.Location = new System.Drawing.Point(10, 55);
        lblRxPwrPort.AutoSize = true;

        _numRxPwrPort = new NumericUpDown();
        _numRxPwrPort.Location = new System.Drawing.Point(90, 52);
        _numRxPwrPort.Size = new System.Drawing.Size(100, 23);
        _numRxPwrPort.Minimum = 1;
        _numRxPwrPort.Maximum = 65535;
        _numRxPwrPort.Value = 2268;

        _lblRxPwrStatus = new Label();
        _lblRxPwrStatus.Text = "○ 未连接";
        _lblRxPwrStatus.ForeColor = System.Drawing.Color.Gray;
        _lblRxPwrStatus.Location = new System.Drawing.Point(10, 80);
        _lblRxPwrStatus.AutoSize = true;

        _lblRxPwrDetail = new Label();
        _lblRxPwrDetail.Text = "";
        _lblRxPwrDetail.ForeColor = System.Drawing.Color.DimGray;
        _lblRxPwrDetail.Location = new System.Drawing.Point(10, 100);
        _lblRxPwrDetail.AutoSize = true;

        _btnRxPwrConn = new Button();
        _btnRxPwrConn.Text = "连接";
        _btnRxPwrConn.Location = new System.Drawing.Point(90, 130);
        _btnRxPwrConn.Size = new System.Drawing.Size(70, 28);

        _btnRxPwrDisconn = new Button();
        _btnRxPwrDisconn.Text = "断开";
        _btnRxPwrDisconn.Location = new System.Drawing.Point(170, 130);
        _btnRxPwrDisconn.Size = new System.Drawing.Size(70, 28);
        _btnRxPwrDisconn.Enabled = false;

        _grpRxPwr.Controls.Add(lblRxPwrIp);
        _grpRxPwr.Controls.Add(_txtRxPwrIp);
        _grpRxPwr.Controls.Add(lblRxPwrPort);
        _grpRxPwr.Controls.Add(_numRxPwrPort);
        _grpRxPwr.Controls.Add(_lblRxPwrStatus);
        _grpRxPwr.Controls.Add(_lblRxPwrDetail);
        _grpRxPwr.Controls.Add(_btnRxPwrConn);
        _grpRxPwr.Controls.Add(_btnRxPwrDisconn);

        // 发射电源
        _grpTxPwr = new GroupBox();
        _grpTxPwr.Text = "发射电源 (TX PWR)";
        _grpTxPwr.Location = new System.Drawing.Point(410, 12);
        _grpTxPwr.Size = new System.Drawing.Size(380, 170);

        Label lblTxPwrIp = new Label();
        lblTxPwrIp.Text = "IP:";
        lblTxPwrIp.Location = new System.Drawing.Point(10, 28);
        lblTxPwrIp.AutoSize = true;

        _txtTxPwrIp = new TextBox();
        _txtTxPwrIp.Location = new System.Drawing.Point(90, 25);
        _txtTxPwrIp.Size = new System.Drawing.Size(150, 23);

        Label lblTxPwrPort = new Label();
        lblTxPwrPort.Text = "端口:";
        lblTxPwrPort.Location = new System.Drawing.Point(10, 55);
        lblTxPwrPort.AutoSize = true;

        _numTxPwrPort = new NumericUpDown();
        _numTxPwrPort.Location = new System.Drawing.Point(90, 52);
        _numTxPwrPort.Size = new System.Drawing.Size(100, 23);
        _numTxPwrPort.Minimum = 1;
        _numTxPwrPort.Maximum = 65535;
        _numTxPwrPort.Value = 2268;

        _lblTxPwrStatus = new Label();
        _lblTxPwrStatus.Text = "○ 未连接";
        _lblTxPwrStatus.ForeColor = System.Drawing.Color.Gray;
        _lblTxPwrStatus.Location = new System.Drawing.Point(10, 80);
        _lblTxPwrStatus.AutoSize = true;

        _lblTxPwrDetail = new Label();
        _lblTxPwrDetail.Text = "";
        _lblTxPwrDetail.ForeColor = System.Drawing.Color.DimGray;
        _lblTxPwrDetail.Location = new System.Drawing.Point(10, 100);
        _lblTxPwrDetail.AutoSize = true;

        _btnTxPwrConn = new Button();
        _btnTxPwrConn.Text = "连接";
        _btnTxPwrConn.Location = new System.Drawing.Point(90, 130);
        _btnTxPwrConn.Size = new System.Drawing.Size(70, 28);

        _btnTxPwrDisconn = new Button();
        _btnTxPwrDisconn.Text = "断开";
        _btnTxPwrDisconn.Location = new System.Drawing.Point(170, 130);
        _btnTxPwrDisconn.Size = new System.Drawing.Size(70, 28);
        _btnTxPwrDisconn.Enabled = false;

        _grpTxPwr.Controls.Add(lblTxPwrIp);
        _grpTxPwr.Controls.Add(_txtTxPwrIp);
        _grpTxPwr.Controls.Add(lblTxPwrPort);
        _grpTxPwr.Controls.Add(_numTxPwrPort);
        _grpTxPwr.Controls.Add(_lblTxPwrStatus);
        _grpTxPwr.Controls.Add(_lblTxPwrDetail);
        _grpTxPwr.Controls.Add(_btnTxPwrConn);
        _grpTxPwr.Controls.Add(_btnTxPwrDisconn);

        // 信号源
        _grpVSG = new GroupBox();
        _grpVSG.Text = "信号源 (VSG)";
        _grpVSG.Location = new System.Drawing.Point(12, 195);
        _grpVSG.Size = new System.Drawing.Size(380, 170);

        Label lblVsgIp = new Label();
        lblVsgIp.Text = "IP:";
        lblVsgIp.Location = new System.Drawing.Point(10, 28);
        lblVsgIp.AutoSize = true;

        _txtVsgIp = new TextBox();
        _txtVsgIp.Location = new System.Drawing.Point(90, 25);
        _txtVsgIp.Size = new System.Drawing.Size(150, 23);

        Label lblVsgHint = new Label();
        lblVsgHint.Text = "VISA TCPIP 直连";
        lblVsgHint.ForeColor = System.Drawing.Color.Gray;
        lblVsgHint.Location = new System.Drawing.Point(10, 55);
        lblVsgHint.AutoSize = true;

        _lblVsgStatus = new Label();
        _lblVsgStatus.Text = "○ 未连接";
        _lblVsgStatus.ForeColor = System.Drawing.Color.Gray;
        _lblVsgStatus.Location = new System.Drawing.Point(10, 80);
        _lblVsgStatus.AutoSize = true;

        _lblVsgDetail = new Label();
        _lblVsgDetail.Text = "";
        _lblVsgDetail.ForeColor = System.Drawing.Color.DimGray;
        _lblVsgDetail.Location = new System.Drawing.Point(10, 100);
        _lblVsgDetail.AutoSize = true;

        _btnVsgConn = new Button();
        _btnVsgConn.Text = "连接";
        _btnVsgConn.Location = new System.Drawing.Point(90, 130);
        _btnVsgConn.Size = new System.Drawing.Size(70, 28);

        _btnVsgDisconn = new Button();
        _btnVsgDisconn.Text = "断开";
        _btnVsgDisconn.Location = new System.Drawing.Point(170, 130);
        _btnVsgDisconn.Size = new System.Drawing.Size(70, 28);
        _btnVsgDisconn.Enabled = false;

        _grpVSG.Controls.Add(lblVsgIp);
        _grpVSG.Controls.Add(_txtVsgIp);
        _grpVSG.Controls.Add(lblVsgHint);
        _grpVSG.Controls.Add(_lblVsgStatus);
        _grpVSG.Controls.Add(_lblVsgDetail);
        _grpVSG.Controls.Add(_btnVsgConn);
        _grpVSG.Controls.Add(_btnVsgDisconn);

        // 频谱仪
        _grpSA = new GroupBox();
        _grpSA.Text = "频谱仪 (SA)";
        _grpSA.Location = new System.Drawing.Point(410, 195);
        _grpSA.Size = new System.Drawing.Size(380, 170);

        Label lblSaIp = new Label();
        lblSaIp.Text = "IP:";
        lblSaIp.Location = new System.Drawing.Point(10, 28);
        lblSaIp.AutoSize = true;

        _txtSaIp = new TextBox();
        _txtSaIp.Location = new System.Drawing.Point(90, 25);
        _txtSaIp.Size = new System.Drawing.Size(150, 23);

        Label lblSaHint = new Label();
        lblSaHint.Text = "VISA TCPIP 直连";
        lblSaHint.ForeColor = System.Drawing.Color.Gray;
        lblSaHint.Location = new System.Drawing.Point(10, 55);
        lblSaHint.AutoSize = true;

        _lblSaStatus = new Label();
        _lblSaStatus.Text = "○ 未连接";
        _lblSaStatus.ForeColor = System.Drawing.Color.Gray;
        _lblSaStatus.Location = new System.Drawing.Point(10, 80);
        _lblSaStatus.AutoSize = true;

        _lblSaDetail = new Label();
        _lblSaDetail.Text = "";
        _lblSaDetail.ForeColor = System.Drawing.Color.DimGray;
        _lblSaDetail.Location = new System.Drawing.Point(10, 100);
        _lblSaDetail.AutoSize = true;

        _btnSaConn = new Button();
        _btnSaConn.Text = "连接";
        _btnSaConn.Location = new System.Drawing.Point(90, 130);
        _btnSaConn.Size = new System.Drawing.Size(70, 28);

        _btnSaDisconn = new Button();
        _btnSaDisconn.Text = "断开";
        _btnSaDisconn.Location = new System.Drawing.Point(170, 130);
        _btnSaDisconn.Size = new System.Drawing.Size(70, 28);
        _btnSaDisconn.Enabled = false;

        _grpSA.Controls.Add(lblSaIp);
        _grpSA.Controls.Add(_txtSaIp);
        _grpSA.Controls.Add(lblSaHint);
        _grpSA.Controls.Add(_lblSaStatus);
        _grpSA.Controls.Add(_lblSaDetail);
        _grpSA.Controls.Add(_btnSaConn);
        _grpSA.Controls.Add(_btnSaDisconn);

        // 开关矩阵
        _grpSwitch = new GroupBox();
        _grpSwitch.Text = "开关矩阵 (UDC-0624F)";
        _grpSwitch.Location = new System.Drawing.Point(12, 378);
        _grpSwitch.Size = new System.Drawing.Size(778, 170);

        Label lblSwitchCom = new Label();
        lblSwitchCom.Text = "COM口:";
        lblSwitchCom.Location = new System.Drawing.Point(10, 28);
        lblSwitchCom.AutoSize = true;

        _txtSwitchCom = new TextBox();
        _txtSwitchCom.Location = new System.Drawing.Point(90, 25);
        _txtSwitchCom.Size = new System.Drawing.Size(120, 23);
        _txtSwitchCom.Text = "COM6";

        Label lblSwitchBaud = new Label();
        lblSwitchBaud.Text = "波特率:";
        lblSwitchBaud.Location = new System.Drawing.Point(10, 55);
        lblSwitchBaud.AutoSize = true;

        _numSwitchBaud = new NumericUpDown();
        _numSwitchBaud.Location = new System.Drawing.Point(90, 52);
        _numSwitchBaud.Size = new System.Drawing.Size(100, 23);
        _numSwitchBaud.Minimum = 9600;
        _numSwitchBaud.Maximum = 921600;
        _numSwitchBaud.Value = 115200;
        _numSwitchBaud.Increment = 9600;

        _lblSwitchStatus = new Label();
        _lblSwitchStatus.Text = "○ 未连接";
        _lblSwitchStatus.ForeColor = System.Drawing.Color.Gray;
        _lblSwitchStatus.Location = new System.Drawing.Point(10, 80);
        _lblSwitchStatus.AutoSize = true;

        _lblSwitchDetail = new Label();
        _lblSwitchDetail.Text = "";
        _lblSwitchDetail.ForeColor = System.Drawing.Color.DimGray;
        _lblSwitchDetail.Location = new System.Drawing.Point(10, 100);
        _lblSwitchDetail.AutoSize = true;

        _btnSwitchConn = new Button();
        _btnSwitchConn.Text = "连接";
        _btnSwitchConn.Location = new System.Drawing.Point(90, 130);
        _btnSwitchConn.Size = new System.Drawing.Size(70, 28);

        _btnSwitchDisconn = new Button();
        _btnSwitchDisconn.Text = "断开";
        _btnSwitchDisconn.Location = new System.Drawing.Point(170, 130);
        _btnSwitchDisconn.Size = new System.Drawing.Size(70, 28);
        _btnSwitchDisconn.Enabled = false;

        _grpSwitch.Controls.Add(lblSwitchCom);
        _grpSwitch.Controls.Add(_txtSwitchCom);
        _grpSwitch.Controls.Add(lblSwitchBaud);
        _grpSwitch.Controls.Add(_numSwitchBaud);
        _grpSwitch.Controls.Add(_lblSwitchStatus);
        _grpSwitch.Controls.Add(_lblSwitchDetail);
        _grpSwitch.Controls.Add(_btnSwitchConn);
        _grpSwitch.Controls.Add(_btnSwitchDisconn);

        // 底部按钮
        _btnConnectAll = new Button();
        _btnConnectAll.Text = "▶ 全部连接";
        _btnConnectAll.Location = new System.Drawing.Point(12, 560);
        _btnConnectAll.Size = new System.Drawing.Size(120, 36);

        _btnDisconnectAll = new Button();
        _btnDisconnectAll.Text = "■ 全部断开";
        _btnDisconnectAll.Location = new System.Drawing.Point(140, 560);
        _btnDisconnectAll.Size = new System.Drawing.Size(120, 36);

        _btnSaveDefaultConfig = new Button();
        _btnSaveDefaultConfig.Text = "💾 保存为默认配置";
        _btnSaveDefaultConfig.Location = new System.Drawing.Point(270, 560);
        _btnSaveDefaultConfig.Size = new System.Drawing.Size(140, 36);

        _tabConnection.Controls.Add(_grpRxPwr);
        _tabConnection.Controls.Add(_grpTxPwr);
        _tabConnection.Controls.Add(_grpVSG);
        _tabConnection.Controls.Add(_grpSA);
        _tabConnection.Controls.Add(_grpSwitch);
        _tabConnection.Controls.Add(_btnConnectAll);
        _tabConnection.Controls.Add(_btnDisconnectAll);
        _tabConnection.Controls.Add(_btnSaveDefaultConfig);

        // ========== Tab 2~5: 占位（内联创建，设计器可见） ==========
        _tabTest = new TabPage("测试执行");
        Label lblTest = new Label();
        lblTest.Text = "测试执行页面 — 后续实现";
        lblTest.Dock = DockStyle.Fill;
        lblTest.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        lblTest.ForeColor = System.Drawing.Color.Gray;
        lblTest.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
        _tabTest.Controls.Add(lblTest);

        _tabSettings = new TabPage("参数设置");
        Label lblSettings = new Label();
        lblSettings.Text = "参数设置页面 — 后续实现";
        lblSettings.Dock = DockStyle.Fill;
        lblSettings.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        lblSettings.ForeColor = System.Drawing.Color.Gray;
        lblSettings.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
        _tabSettings.Controls.Add(lblSettings);

        _tabResults = new TabPage("结果查看");
        Label lblResults = new Label();
        lblResults.Text = "结果查看页面 — 后续实现";
        lblResults.Dock = DockStyle.Fill;
        lblResults.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        lblResults.ForeColor = System.Drawing.Color.Gray;
        lblResults.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
        _tabResults.Controls.Add(lblResults);

        _tabLog = new TabPage("系统日志");
        Label lblLog = new Label();
        lblLog.Text = "系统日志页面 — 后续实现";
        lblLog.Dock = DockStyle.Fill;
        lblLog.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        lblLog.ForeColor = System.Drawing.Color.Gray;
        lblLog.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
        _tabLog.Controls.Add(lblLog);

        _tabControl.TabPages.Add(_tabConnection);
        _tabControl.TabPages.Add(_tabTest);
        _tabControl.TabPages.Add(_tabSettings);
        _tabControl.TabPages.Add(_tabResults);
        _tabControl.TabPages.Add(_tabLog);

        // ---- StatusStrip ----
        _statusStrip = new StatusStrip();
        _statusLabel = new ToolStripStatusLabel("就绪");
        _statusStrip.Items.Add(_statusLabel);

        // ---- 组装窗体 ----
        this.Controls.Add(_tabControl);
        this.Controls.Add(_statusStrip);
        this.SuspendLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion
}
