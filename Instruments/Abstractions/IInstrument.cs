namespace CbandAutoTest.Instruments.Abstractions;

public interface IInstrument : IDisposable
{
    string Connect();
    void Disconnect();
    bool IsConnected { get; }
    string Idn { get; }
    string LastError { get; }
}
