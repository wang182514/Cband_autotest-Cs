# C波段射频模块自动化测试系统 (C# + WinForms)

基于原有 Python (PySide6) 版本重构，采用 **.NET 8.0 WinForms** 重新实现。

## 环境要求

- **Visual Studio 2022**（17.8+）
- **.NET 8.0 SDK**（[下载](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0)）
- **Windows**（VISA 通信 + 串口依赖 Windows 平台）
- **硬件**：Keysight N9020A、R&S SMU200A、TDK-Lambda 电源 ×2、UDC-0624F 开关矩阵
- **可选**：NI-VISA 或 Keysight VISA 运行时（信号源/频谱仪通信需要）

## 快速开始

```bash
# 克隆
git clone https://github.com/wang182514/Cband_autotest-Cs.git
cd CbandAutoTest

# 还原并运行
dotnet restore
dotnet run
```

或者在 VS2022 中直接打开 `CbandAutoTest.sln`，按 `F5` 运行。

> **注意**：首次运行从 `Config/default_settings.json` 加载默认仪器 IP / COM 口配置。在「仪器连接」页修改后点击「保存为默认配置」即可持久化。

## 项目结构

```
CbandAutoTest/
├── Program.cs                       # 入口：加载配置 → 启动主窗口
├── MainForm.cs                      # 主窗体业务逻辑
├── MainForm.Designer.cs             # 主窗体控件布局（设计器可见）
├── InstrumentManager.cs             # 仪器管理器
│
├── Config/
│   ├── InstrumentConfig.cs          # 配置数据模型（JSON 序列化）
│   ├── ConfigManager.cs             # JSON 加载/保存/深度合并
│   └── default_settings.json        # 默认配置
│
├── Instruments/
│   ├── Abstractions/                # 仪器抽象接口层
│   │   ├── IInstrument.cs           #   基接口
│   │   ├── IPowerSupply.cs          #   直流电源
│   │   ├── ISignalGenerator.cs      #   信号发生器
│   │   ├── ISpectrumAnalyzer.cs     #   频谱仪（SA / NF / PN）
│   │   └── ISwitchMatrix.cs         #   开关矩阵
│   ├── PowerSupply.cs               # TCP/IP SCPI 实现
│   ├── SignalGenerator.cs           # VISA 实现（R&S SMU200A）
│   ├── SpectrumAnalyzer.cs          # VISA 实现（Keysight N9020A）
│   └── SwitchMatrix.cs              # UART 串口实现（UDC-0624F）
│
└── Utils/
    └── Logger.cs                    # 日志器（文件 + 事件）
```

## 界面布局

主窗体采用 **TabControl** 分页设计：

| Tab 页 | 状态 | 功能 |
|---|---|---|
| **仪器连接** | ✅ 已实现 | 5 台仪器独立连接/断开控制，状态指示灯，IP/COM 配置 |
| **测试执行** | 🟡 待实现 | 选择测试项、设定参数、实时测量数据表 |
| **参数设置** | 🟡 待实现 | 测试限值、射频链路参数、报告选项 |
| **结果查看** | 🟡 待实现 | 测试结果汇总、历史记录、报告导出 |
| **系统日志** | 🟡 待实现 | 全量日志、级别过滤、搜索 |

## 测试项

| 测试 | 对应原 MATLAB 文件 | 说明 |
|---|---|---|
| RX 噪声系数 + 增益 | `SubProcess1_TestRXNF.m` | 噪声系数、增益、平坦度 |
| RX 相位噪声 | `SubProcess2_TestRXPN.m` | 4 个偏置点的相位噪声 |
| TX 增益 + 输出功率 | `SubProcess4_TestTXGain_Pout.m` | 3 个频点的 Pout / Gain |
| TX 平坦度 + 相位噪声 | `SubProcess3_TestTXFlatness_PN.m` | 扫频平坦度 + PN |
| 收发干扰 | `SubProcess5_TestRXInfluence.m` | TX 对 RX 噪底的影响 |

## VISA 说明

信号源和频谱仪使用 **NI-VISA**（晚期绑定加载），编译时无需安装 VISA SDK。运行时若检测不到 VISA 会提示安装。可从以下地址获取：

- [NI-VISA 下载](https://www.ni.com/zh-cn/support/downloads/drivers/download.ni-visa.html)

## 从源码构建

```bash
dotnet restore
dotnet build
dotnet publish -c Release -o publish   # 单文件发布
```
