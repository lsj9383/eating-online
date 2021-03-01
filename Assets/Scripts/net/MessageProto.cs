using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MessageProto {
    public string type;

    public MessageProto(string _type) {
        type = _type;
    }
}
