using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Events
{
    public delegate void Callback(Codecs cs, byte[] data);
    public Dictionary<string, Callback> callbacks = new Dictionary<string, Callback>();

    public void AddCallback(string name, Callback cb) {
        if (!callbacks.ContainsKey(name)) {
            callbacks[name] = null;
        }

        callbacks[name] += cb;
    }

    public void Invoke(string type, Codecs cs, byte[] data)
    {
        if (callbacks.ContainsKey(type)) {
            callbacks[type](cs, data);
        } else {
            Debug.Log("event type invalid: " + type);
        }
    }
}
