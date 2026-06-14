# CbandAutoTest 项目学习指南

面向读者：有 C/C++/Python/C# 基础，了解 WinForms 设计器，写过简易计算器，想系统学习本项目。

---

## 一、从计算器到本项目——你已掌握的部分

如果你写过 WinForms 计算器，你已经会了：

| 你会的东西 | 在本项目的体现 |
|---|---|
| 从工具箱拖 Button / TextBox 到窗体 | `MainForm.Designer.cs` 里所有 `new Button()` / `new TextBox()` 都是你在设计器拖一个控件时 VS 自动生成的代码 |
| 双击按钮写点击事件 | 本项目是手动绑定的，但本质一样：`_btn.Click += (_, _) => 方法名()` |
| 修改控件属性（Text / Size / Location） | 设计器属性面板改一个属性 → VS 在 `InitializeComponent()` 里加一行 `_btn.Text = "连接"` |
| `int.TryParse` 处理输入 | 本项目用 `NumericUpDown` 替代，无需自己解析 |

**核心区别**：计算器是一个窗体 + 几个控件全部在设计器里拖拽完成。本项目是一个窗体里嵌套了 5 个 Tab 页，每个 Tab 页里又有 GroupBox ➜ Label / TextBox / Button。控件数量多，所以部分页面（Tab 2~5）用了**代码生成**而非纯拖拽。

---

## 二、逐文件阅读攻略（按推荐顺序）

### 第 1 站：`Program.cs` （5 分钟）

```csharp
static void Main()
{
    ApplicationConfiguration.Initialize();
    // 读配置文件 → 启动主窗口
    var config = new ConfigManager(defaultsPath);
    config.LoadDefaults();
    Application.Run(new MainForm(config));
}
```

**你要理解的**：
- `static void Main()` — 程序入口，相当于 C 的 `main()`、Python 的 `if __name__ == "__main__"`
- `new ConfigManager(...)` — 创建一个配置管理器对象
- `new MainForm(config)` — 创建主窗口，把配置传进去
- `Application.Run(...)` — 启动窗口消息循环（相当于一个 while 循环不断检查鼠标/键盘事件）

### 第 2 站：`Config/InstrumentConfig.cs` （5 分钟）

```csharp
public class InstrumentConfig
{
    public DeviceConfig RxPowerSupply { get; set; } = new() { Ip = "192.168.1.11", Port = 2268 };
}

public class DeviceConfig
{
    public string Ip { get; set; } = "";
    public int Port { get; set; } = 2268;
}
```

**你要理解的**：
- `class` — 定义数据类型，就像 C 的 `struct` 或 Python 的 `class`
- `{ get; set; }` — 自动属性，相当于 `private string _ip; public string Ip { get { return _ip; } set { _ip = value; } }` 的简写。Python 里用 `@property` 做类似的事
- `[JsonPropertyName("rx_power_supply")]` — 特性（Attribute），告诉 JSON 解析器这个属性对应 JSON 里的哪个字段名
- `= new() { Ip = "...", Port = 2268 }` — 对象初始化器，等价于 `var c = new DeviceConfig(); c.Ip = "..."; c.Port = 2268;`

**和 Python 对比**：
```python
# Python 版（用 dict 模拟）
config = {"rx_power_supply": {"ip": "192.168.1.11", "port": 2268}}
# 访问方式（不安全，拼错了运行时才报错）
ip = config["rx_power_supply"]["ip"]
```
```csharp
// C# 版（强类型，写 . 之后 VS 会提示可用的属性）
string ip = config.Instruments.RxPowerSupply.Ip;  // 拼错了编译不过
```

### 第 3 站：`Instruments/Abstractions/IInstrument.cs` 及其接口文件（5 分钟）

```csharp
public interface IInstrument : IDisposable
{
    string Connect();
    void Disconnect();
    bool IsConnected { get; }
    string Idn { get; }
}
```

**你要理解的**：
- `interface` — 约定一个「能干啥」的清单。好比一张合同，写着：凡是实现了 `IInstrument` 的类，**必须**提供 `Connect()`、`Disconnect()`、`IsConnected` 等方法
- `IDisposable` — .NET 的内置接口，表示「这个类需要清理资源（比如关闭网络连接）」。实现了它就可以用 `using` 语法自动释放
- **为什么要有接口**：因为将来可能换不同品牌的仪器。只要新仪器也实现 `IPowerSupply`，测试代码一行都不用改

### 第 4 站：`Instruments/PowerSupply.cs`（15 分钟）

这是最简单的仪器驱动，没有 VISA，只有 TCP 发 SCPI 命令。

```csharp
public class PowerSupply : IPowerSupply    // ← 这个类承诺实现 IPowerSupply
{
    private TcpClient? _tcp;               // ← 字段（成员变量），? 表示可以为 null

    public string Connect()
    {
        _tcp = new TcpClient();
        _tcp.Connect(_ip, _port);          // ← TCP 连接，和 Python socket.connect() 一样
        _stream = _tcp.GetStream();
        _idn = Query("*IDN?");             // ← 发 SCPI 命令 "*IDN?\n" 并读回响应
        return _idn;
    }

    private string Query(string cmd)       // ← 私有方法，只有这个类自己能调
    {
        Send(cmd);
        Thread.Sleep(50);                  // ← 等 50ms 让仪器准备好回复
        return Receive();
    }
```

**你要理解的**：
- `public` / `private` — 访问修饰符。`public` 谁都能调，`private` 只有我自己能调
- `_tcp` — 下划线前缀是 C# 的命名惯例，表示「这是私有字段」。不是语法强制，但大家都这么写
- `TcpClient` — .NET 自带的 TCP 客户端，和 Python 的 `socket` 一一对应
- `Send(cmd)` → `Thread.Sleep(50)` → `Receive()` — SCPI 通信的经典三步
- `double.TryParse(resp, out var v)` — 尝试把字符串转成 double，成功则 v 有值，失败返回 false。这是 C# 的「TryParse 模式」

**和你写过的计算器对比**：
```
计算器：btn1_Click → _textBox.Text += "1"
这里：  btnConn_Click → new PowerSupply() → .Connect()
都是「按钮点了 → 执行一段代码」——只不过计算器是改文本框，这里是发网络请求
```

### 第 5 站：`InstrumentManager.cs`（5 分钟）

```csharp
public class InstrumentManager : IDisposable
{
    public IPowerSupply RxPower { get; }     // ← 自动属性，外部可读
    public IPowerSupply TxPower { get; }

    public InstrumentManager(InstrumentConfig config, Logger logger)
    {
        // 构造函数：根据配置创建 5 台仪器实例
        RxPower = new PowerSupply(config.RxPowerSupply.Ip, ...);
        TxPower = new PowerSupply(config.TxPowerSupply.Ip, ...);
        ...
    }

    public void ConnectAll()
    {
        // 依次连接 5 台仪器
        ConnectOne(RxPower, "接收电源");
        ConnectOne(TxPower, "发射电源");
        ...
    }
}
```

**你要理解的**：
- **构造函数** `public InstrumentManager(...)` — 和类同名，`new` 时自动执行。相当于 Python 的 `__init__`
- `IPowerSupply RxPower { get; }` — 「外部只能读，不能写」，安全
- 这个类不干具体活，只是把 5 台仪器**组织在一起**，方便统一管理

### 第 6 站：`MainForm.Designer.cs`（20 分钟）

```csharp
partial class MainForm
{
    // 第 1 部分：控件字段声明
    private TabControl _tabControl;
    private GroupBox _grpRxPwr;
    private TextBox _txtRxPwrIp;
    private Button _btnRxPwrConn;
    ...

    // 第 2 部分：InitializeComponent() — 创建所有控件并设置位置
    private void InitializeComponent()
    {
        _grpRxPwr = new GroupBox();                  // 创建一个 GroupBox
        _grpRxPwr.Text = "接收电源 (RX PWR)";        // 设置标题
        _grpRxPwr.Location = new Point(12, 12);      // 设置位置（距离左边12px，上边12px）
        _grpRxPwr.Size = new Size(380, 170);          // 设置尺寸（宽380px，高170px）

        _btnRxPwrConn = new Button();                // 创建一个按钮
        _btnRxPwrConn.Text = "连接";
        _btnRxPwrConn.Location = new Point(90, 130);

        _grpRxPwr.Controls.Add(_btnRxPwrConn);      // 把按钮放进 GroupBox
        _tabConnection.Controls.Add(_grpRxPwr);      // 把 GroupBox 放进 Tab 页
        _tabControl.Controls.Add(_tabConnection);    // 把 Tab 页放进 TabControl
        this.Controls.Add(_tabControl);              // 把 TabControl 放进窗体
    }
}
```

**你要理解的**：
- `partial class` — 同一个类分散在多个文件中。`MainForm.Designer.cs` 和 `MainForm.cs` 共同组成 `MainForm` 类。设计器只读写 `.Designer.cs`，你只编辑 `.cs`
- `Location` / `Size` — 所有 WinForms 控件都有这两个属性。单位是**像素**
- `Controls.Add()` — 把子控件添加到父控件。这是一个**嵌套树**：
  ```
  Form
   └── TabControl
        └── TabPage "仪器连接"
             ├── GroupBox "接收电源"
             │    ├── Label "IP:"
             │    ├── TextBox
             │    ├── Label "端口:"
             │    ├── NumericUpDown
             │    ├── Label "○ 未连接"
             │    ├── Button "连接"
             │    └── Button "断开"
             ├── GroupBox "发射电源"
             │    └── ...
             └── Button "全部连接"
  ```

**动手实验1**：在设计器中选中「接收电源」的 IP 输入框 → 在属性面板（F4）找到 `Text` → 改成 `192.168.1.99` → 运行看看是不是变了

### 第 7 站：`MainForm.cs`（30 分钟）

```csharp
public partial class MainForm : Form
{
    public MainForm(ConfigManager config, Logger? logger = null)
    {
        InitializeComponent();        // ← 来自 Designer.cs，创建所有控件
        LoadConfigToUI();             // ← 把配置里的 IP 填入输入框

        // 绑定按钮事件
        _btnRxPwrConn.Click += (_, _) => ConnectInstrument(...);
        _btnConnectAll.Click += (_, _) => ConnectAll();
    }
```

**你要理解的**：

**a) 事件绑定** — 相当于 C 语言里的函数指针：
```csharp
// 写法 1：传统方法
_btnRxPwrConn.Click += BtnRxPwrConn_Click;
void BtnRxPwrConn_Click(object? sender, EventArgs e) { ... }

// 写法 2：Lambda 表达式（本项目用这种）
_btnRxPwrConn.Click += (_, _) => ConnectInstrument(...);
// 这里的 (_, _) 是两个参数，用 _ 表示「我不关心它们」
```

**b) async/await** — 异步编程：
```csharp
private void ConnectInstrument(...)
{
    // 1. 先禁用按钮
    connBtn.Enabled = false;

    // 2. 启动后台任务（不卡 UI）
    Task.Run(() =>
    {
        // 这里在后台线程执行
        var idn = instr.Connect();        // 可能等几秒

        // 3. 回到 UI 线程更新界面
        BeginInvoke(() =>
        {
            status.Text = "● 已连接";
        });
    });
}
```

**关键理解**：
- `Task.Run(() => { ... })` — 启动一个后台线程。相当于 `new Thread(() => { ... }).Start()`
- `BeginInvoke(() => { ... })` — 回到 UI 线程。因为 Windows 规定：**只有创建控件的那个线程才能修改它**
- 如果不这样做（不用 `Task.Run`），点击「连接」按钮后程序会**卡住几秒**，窗口不能拖动、不能关闭，像死了一样

**c) `Func<InstrumentManager, IInstrument>`** — 把方法当成参数传：
```csharp
// 方法签名：第一个参数是「一个方法，接受 InstrumentManager 返回 IInstrument」
private void ConnectInstrument(Func<InstrumentManager, IInstrument> selector, ...)

// 调用时：
ConnectInstrument(m => m.RxPower, ...);   // 传「从 mgr 取 RxPower」这个操作
ConnectInstrument(m => m.TxPower, ...);   // 传「从 mgr 取 TxPower」这个操作
// 上面两行用**同一个** ConnectInstrument 方法，只是取的仪器不同
```

这就像 C 语言的函数指针：

```c
// C 语言版本：
void connect_instrument(IInstrument* (*selector)(InstrumentManager*), ...);
connect_instrument(get_rx_power, ...);
connect_instrument(get_tx_power, ...);
```

### 第 8 站：回顾 `Instruments/SignalGenerator.cs` 和 `SpectrumAnalyzer.cs`（可选）

这两个文件的结构和 `PowerSupply.cs` 一样，只是通信方式从 TCP 换成了 VISA。多了一行：

```csharp
var rmType = Type.GetType("Ivi.Visa.ResourceManager, Ivi.Visa");
```

这是**晚期绑定**——编译时不检查 VISA 是否安装，运行时才检测。没装 VISA 的用户不会编译报错，只是运行时报错。

---

## 三、关键 C# 概念速查（对比你已知的语言）

### 3.1 类 vs 结构体 vs 接口

| 概念 | 类比 C | 类比 Python | 本项目举例 |
|---|---|---|---|
| `class` | `struct` + 函数指针表 | `class` | `PowerSupply` 电源类 |
| `interface` | 纯虚函数类 | `ABC` + `@abstractmethod` | `IPowerSupply` |
| `record` | — | `@dataclass` | 本项目未用，但类似 `TestResult` |
| `enum` | `enum` | `Enum` | `SAnalyzerMode` (SA/NF/PN) |

### 3.2 访问修饰符

| 修饰符 | 谁可以访问 | 类比 Python |
|---|---|---|
| `public` | 谁都行 | 不加 `_` 前缀 |
| `private` | 只有本类 | `__` 双下划线（但 Python 只是改名） |
| `internal` | 同一项目内 | — |
| `protected` | 本类 + 子类 | 单 `_` 前缀 |

### 3.3 属性 vs 字段

```csharp
// 字段（field）—— 老老实实存值
private string _name;

// 属性（property）—— 像字段一样用，但背后可以加逻辑
public string Name
{
    get { return _name; }
    set { _name = value; }
}

// 自动属性（auto-property）—— 编译器自动生成背后的字段
public string Name { get; set; }  // 最简形式，本项目大量使用
```

### 3.4 `?` 的含义

```csharp
string? x;           // x 可以是 null
TcpClient? _tcp;     // _tcp 可以是 null
Logger? logger = null; // logger 参数的默认值是 null
```

加了 `?` 的类型叫**可空类型**。编译器会提醒你「这个变量可能是 null，用之前检查一下」。

### 3.5 `=>` 表达式体

```csharp
// 普通写法
public string Idn { get { return _idn; } }

// 箭头写法（Lambda 表达式体）
public string Idn => _idn;

// 方法也可以
public void RfOn() => Write("OUTP ON");  // 单行方法不用写 {}
```

---

## 四、WinForms 控件树（本项目用到的）

```
Form
 ├── TabControl (多页签容器)
 │    ├── TabPage "仪器连接"
 │    │    ├── GroupBox (带标题的分组框)
 │    │    │    ├── Label (文字标签)
 │    │    │    ├── TextBox (单行输入框)
 │    │    │    ├── NumericUpDown (数字调节框)
 │    │    │    ├── Button (按钮)
 │    │    │    └── Button
 │    │    ├── GroupBox
 │    │    │    └── ...
 │    │    └── Button "全部连接"
 │    ├── TabPage "测试执行"
 │    ├── TabPage "参数设置"
 │    ├── TabPage "结果查看"
 │    └── TabPage "系统日志"
 └── StatusStrip (底部状态栏)
      └── ToolStripStatusLabel "就绪"
```

**常见属性**：
- `Text` — 显示的文本
- `Location` — 位置 `(x, y)` 像素
- `Size` — 尺寸 `(宽, 高)` 像素
- `Dock` — 停靠方式（`Fill` 填满父容器，`Top` 贴顶部...）
- `Enabled` — 是否可用（`false` 变灰不能点）
- `ForeColor` / `BackColor` — 前景色/背景色

**常见事件**：
- `Click` — 点击
- `TextChanged` — 文本改变
- `ValueChanged` — 数值改变（NumericUpDown）

---

## 五、学习路线图

### 阶段一：读代码（今天）

```
1. Program.cs     ─── 5min ─── 入口
2. InstrumentConfig.cs ─ 5min ─── 配置数据长什么样
3. IInstrument.cs ─── 5min ─── 接口：约定仪器能干什么
4. PowerSupply.cs ─── 15min ─── 一个具体的仪器驱动
5. MainForm.Designer.cs ─ 20min ─── 控件是怎么摆放的
6. MainForm.cs    ─── 30min ─── 按钮点了之后发生了什么
```

### 阶段二：动手改（1~2 天）

**练习 1**：改一个控件的属性
- 在设计器里选中「全部连接」按钮
- 在属性窗格（F4）找 `ForeColor` → 改成 `Blue`
- 运行看看

**练习 2**：加一个 Label
- 打开 `MainForm.Designer.cs`
- 在 `_btnSaveDefaultConfig` 的后面加：
  ```csharp
  Label lblVersion = new Label();
  lblVersion.Text = "版本 1.0";
  lblVersion.Location = new System.Drawing.Point(12, 600);
  lblVersion.AutoSize = true;
  _tabConnection.Controls.Add(lblVersion);
  ```
- 编译运行

**练习 3**：理解事件绑定
- 在 `MainForm.cs` 中找到 `_btnConnectAll.Click += (_, _) => ConnectAll();`
- 改成 `_btnConnectAll.Click += (_, _) => { MessageBox.Show("全部连接"); ConnectAll(); };`
- 运行→点按钮→先弹对话框再连接

**练习 4**：读懂异步
- 在 `ConnectInstrument` 方法里的 `Task.Run` 前加一行 `Console.WriteLine("UI 线程");`
- 在 `Task.Run` 内部加一行 `Console.WriteLine("后台线程");`
- 观察输出顺序

### 阶段三：加一个小功能（3~5 天）

**任务**：在「仪器连接」Tab 右下角加一个「清空日志」按钮

```
你需要：

1. 在 MainForm.Designer.cs 中：
   - 声明字段：private Button _btnClearLog;
   - 在 InitializeComponent 里创建它，设置位置

2. 在 MainForm.cs 中：
   - 绑定事件
   - 事件处理里清空日志文件（调用 Logger 的方法）
   
提示：
   Logger 目前没有「清空」方法，你可以：
   a) 在 Logger 里加一个 Clear() 方法
   b) 或者直接 File.WriteAllText(logFile, "")
```

### 阶段四：深入原理（1~2 周）

- **TCP/IP 通信**：看懂 `PowerSupply.cs` 里的 `Send` / `Receive`，对比 Python 版
- **VISA 晚期绑定**：看懂 `SignalGenerator.cs` 里的 `Type.GetType` + `Activator.CreateInstance`
- **事件和委托**：弄懂 `event EventHandler<LogEventArgs> OnLog` 和 `+=` 绑定
- **异步编程**：研究 `Task.Run` / `BeginInvoke` 模式，为什么不能直接调 UI 控件

### 阶段五：独立扩展（2~4 周）

- **实现「测试执行」Tab**：加一个 `CheckedListBox` 列出测试项，加一个 `DataGridView` 显示实时数据
- **实现「参数设置」Tab**：用嵌套 TabControl，把 `default_settings.json` 的所有字段做成编辑界面
- **连接真实仪器**：在生产环境运行，验证连接逻辑是否正确

---

## 六、调试技巧

### 6.1 断点调试

```
1. 在 MainForm.cs 的 _btnConnectAll.Click += ... 这行左侧灰色边栏点一下 → 出现红点
2. 按 F5 运行
3. 在程序里点「全部连接」按钮
4. 程序会自动停在红点那行，F10 逐行执行，F11 进入方法内部
5. 鼠标悬停在变量上查看当前值
6. 按 F5 继续运行
```

### 6.2 查看调用栈

程序停在断点时，按 `Ctrl+Alt+C` 打开调用栈窗口。你会看到：
```
MainForm.ConnectAll()         ← 你当前在这
MainForm.<>c.<.ctor>b__()     ← 编译器生成的匿名方法
Control.OnClick()             ← WinForms 内部
...
```

### 6.3 即时窗口

断点暂停时，打开「即时窗口」（`Ctrl+Alt+I`），可以输入表达式立即执行：
```
> _config.Instruments.RxPowerSupply.Ip
"192.168.1.11"

> _lblRxPwrStatus.Text
"○ 未连接"

> _instrumentManager?.RxPower.IsConnected
false
```

---

## 七、常见疑问

### Q：为什么控件不全拖到设计器上，而要写代码？

本项目「仪器连接」Tab 的控件是在设计器中可见的（全部写在 `InitializeComponent()` 里），但后续 Tab 2~5 的内容要在下一阶段实现。到时候你可以选择：

- **设计器拖拽**：打开 DesignerView，直接从工具箱拖控件到 Tab 页上
- **代码生成**：在 `InitializeComponent()` 里写 `new Button()` 代码

选择依据：**控件多且固定布局**→拖拽；**控件数量动态变化**→代码。

### Q：`dynamic? _session` 是什么？

`dynamic` 告诉编译器「别检查这个变量的类型，运行时再说」。因为 VISA 的 .NET 类型 (`Ivi.Visa.ResourceManager`) 不一定在编译时安装，用 `dynamic` 可以避免编译时依赖。

### Q：为什么有些文件有 `?` 有些没有？

```csharp
private TcpClient? _tcp;          // 可空：创建对象前是 null
private readonly Logger _logger;   // 不可空：构造函数里一定会赋值
```

C# 8.0 引入了**可空引用类型**。项目 `.csproj` 里有 `<Nullable>enable</Nullable>` 表示启用这个特性。加了 `?` 的变量使用前必须检查 null。

---

## 八、本项目文件与你的知识对照

| 文件 | 你已有的知识 | 新知识点 |
|---|---|---|
| `Program.cs` | 认识 `Main()` | `Application.Run()` |
| `InstrumentConfig.cs` | C 的 struct, Python class | `{ get; set; }` 自动属性, `[JsonPropertyName]` |
| `IInstrument.cs` | C++ 纯虚函数 | `interface` + `IDisposable` |
| `PowerSupply.cs` | TCP socket | `TcpClient`, `TryParse` |
| `SignalGenerator.cs` | — | `dynamic`, `Type.GetType` 晚期绑定 |
| `MainForm.Designer.cs` | 设计器拖控件 | `partial class`, `InitializeComponent()` |
| `MainForm.cs` | 按钮事件 | `Task.Run`, `BeginInvoke`, `Func<>` 委托 |
| `InstrumentManager.cs` | — | 构造函数注入, 方法封装 |
| `Logger.cs` | `StreamWriter` | `event EventHandler` |
| `ConfigManager.cs` | JSON | `System.Text.Json`, 深度合并 |
