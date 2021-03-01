using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Network
{
    bool connected = false;
    int _process_message_once = 128;
    string _ip;
    int _port;
    Codecs _codecs;

    Transport _transport = new TCPTransport();
    Events _events = new Events();

    public Network(Codecs cs) {
        _codecs = cs;
    }

    public void AddCallback(string name, Events.Callback cb) {
        _events.AddCallback(name, cb);
    }

    public bool IsConnected() {
        return connected;
    }

    public bool Connect(string ip, int port) {
        if (connected) {
            return ip == _ip && port == _port ? true : false;
        }

        try
        {
            _transport.Connect(ip, port);
            if (!_transport.IsConnected())
            {
                return false;
            }

            _ip = ip;
            _port = port;
            connected = true;
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }

        return connected;
    }

    public void Disconnect() {
        _transport.Disconnect();
    }

    public bool Send(byte[] data) {
        _transport.Send(data);
        return true;
    }

    public bool Send<T>(T t) {
        _transport.Send(_codecs.Encode<T>(t));
        return true;
    }

    public void Update() {
        int process_count = 0;
        byte[] bytes;

        while (process_count < _process_message_once)
        {
            process_count++;

            if (!_transport.Receive(out bytes))
            {
                continue;
            }

            MessageProto proto = _codecs.Decode<MessageProto>(bytes);
            _events.Invoke(proto.type, _codecs, bytes);
        }
    }
}
