using System;
using System.Collections.Generic;
using System.Net.Sockets;

using UnityEngine;

public class TCPTransport : Transport
{
    private Socket mSocket;
    private readonly Queue<byte[]> queue = new Queue<byte[]>();
    private bool connecting = false;
    private byte[] mBuffer;
    private int bytesNeed;
    private int bytesRead;
    private bool isReadDataSize;

    public override bool IsConnected() {
        if (connecting && mSocket != null && mSocket.Connected)
        {
            // check current state of the connection
            // https://docs.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.connected?view=net-5.0
            bool blockingState = mSocket.Blocking;
            try
            {
                byte[] tmp = new byte[1];

                mSocket.Blocking = false;
                mSocket.Send(tmp, 0, 0);
                return true;
            }
            catch (SocketException e)
            {
                // 10035 == WSAEWOULDBLOCK
                if (e.NativeErrorCode.Equals(10035))
                {
                    // Still Connected, but the Send would block
                    return true;
                }
            }
            finally
            {
                mSocket.Blocking = blockingState;
            }
        }
        connecting = false;
        return false;
    }

    public override bool Connect(string ip, int port) {
        try
        {
            connecting = false;
            mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            mSocket.Connect(ip, port);
            connecting = true;
            // receive data size at first
            bytesNeed = sizeof(int);
            bytesRead = 0;
            isReadDataSize = true;
            mBuffer = new byte[bytesNeed];
            mSocket.BeginReceive(mBuffer, bytesRead, bytesNeed, 0, new AsyncCallback(OnReceive), mSocket);
        }
        catch (Exception e)
        {
            Debug.Log("connect failed: " + e.Message);
            connecting = false;
        }

        // clear message queue
        lock (queue)
        {
            queue.Clear();
        }

        return connecting;
    }

    void OnReceive(IAsyncResult ar)
    {
        try
        {
            int count = mSocket.EndReceive(ar);
            if (count <= 0) {
                return;
            }

            bytesRead += count;
            if (bytesRead == bytesNeed) {
                ProcessReceiveData();
            }

            mSocket.BeginReceive(mBuffer, bytesRead, bytesNeed - bytesRead, 0, new AsyncCallback(OnReceive), mSocket);              
        }
        catch (Exception)
        {
            connecting = false;
        }
    }

    void ProcessReceiveData() {
        if (isReadDataSize)
        {
            // converter big-endian to littleEndian
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(mBuffer);
            }
            bytesNeed = BitConverter.ToInt32(mBuffer, 0);
            if (bytesNeed <= 0) {
                return;
            }
            isReadDataSize = false;
            bytesRead = 0;
            mBuffer = new byte[bytesNeed];
        } else {
            lock (queue)
            {
                queue.Enqueue(mBuffer);
            }

            isReadDataSize = true;
            bytesNeed = sizeof(int);
            bytesRead = 0;
            mBuffer = new byte[bytesNeed];
        }
    }

    public override void Disconnect() {
        if (mSocket != null)
        {
            try
            {
                mSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception)
            {
                // log error
            }
            finally
            {
                mSocket.Close();
            }
        }

        connecting = false;
    }

    public override void Send(byte[] data) {
        if (data.Length <= 0 || !IsConnected())
        {
            return;
        }
        // send data with <size,content> structure
        // size == len(content)
        int payloadSize = sizeof(int) + data.Length;
        byte[] payload = new byte[payloadSize];
        byte[] payloadSizeBytes = BitConverter.GetBytes(data.Length);
        // converter big-endian
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(payloadSizeBytes);
        }
        Array.Copy(payloadSizeBytes, 0, payload, 0, payloadSizeBytes.Length);
        Array.Copy(data, 0, payload, payloadSizeBytes.Length, data.Length);

        // Begin sending the data to the remote device.
        mSocket.BeginSend(payload, 0, payload.Length, 0, new AsyncCallback(OnSend), mSocket);
    }

    void OnSend(IAsyncResult result)
    {
        try
        {
            // Retrieve the socket from the state object.  
            Socket socket = (Socket)result.AsyncState;

            // Complete sending the data to the remote device.  
            int bytesSent = socket.EndSend(result);
        }
        catch (Exception)
        {
            // log error
        }
    }

    public override bool Receive(out byte[] data) {
        lock (queue)
        {
            data = default;
            if (queue.Count > 0)
            {
                data = queue.Dequeue();
                return true;
            }
        }
        return false;
    }

}
