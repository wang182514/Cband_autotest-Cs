namespace CbandAutoTest.Instruments.Abstractions;

/// <summary>
/// 所有仪器的基接口 —— 定义了每台仪器"至少能干什么"
/// 
/// 【概念】interface（接口） = 一份"能力清单" / "合同"
///   任何实现了 IInstrument 的类，都必须提供 Connect、Disconnect、IsConnected 等
///   和 C++ 的纯虚类、Python 的 ABC 抽象基类是同一个思想
/// 
/// 【概念】: IDisposable 表示这个接口也继承了 IDisposable
///   意味着所有仪器都必须支持资源释放（using 语法糖依赖它）
/// </summary>
public interface IInstrument : IDisposable
{
    /// <summary>连接仪器，返回 IDN 标识字符串（如 "Keysight N9020A..."）</summary>
    string Connect();
    /// <summary>断开连接，释放网络/串口资源</summary>
    void Disconnect();
    /// <summary>当前是否已连接（属性，外部只能读）</summary>
    bool IsConnected { get; }
    /// <summary>仪器 IDN 标识字符串（属性，外部只能读）</summary>
    string Idn { get; }
    /// <summary>最后一次错误信息（属性，外部只能读）</summary>
    string LastError { get; }
}
