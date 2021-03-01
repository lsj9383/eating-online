using System;

public abstract class Transport
{
    public abstract bool IsConnected();

    public abstract bool Connect(string ip, int port);
    public abstract void Disconnect();
    public abstract void Send(byte[] data);
    public abstract bool Receive(out byte[] data);
}
