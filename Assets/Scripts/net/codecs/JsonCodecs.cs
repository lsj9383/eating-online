using System;
using System.Collections.Generic;

using UnityEngine;

public class JsonCodecs : Codecs
{

    public override byte[] Encode<T>(T message)
    {
        string json = JsonUtility.ToJson(message);
        return System.Text.Encoding.UTF8.GetBytes(json);
    }

    public override T Decode<T>(byte[] data)
    {
        string json = System.Text.Encoding.UTF8.GetString(data);
        return JsonUtility.FromJson<T>(json);
    }
}