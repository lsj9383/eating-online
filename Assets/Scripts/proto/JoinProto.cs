using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class JoinProto : MessageProto {
    public string id;

    public JoinProto() : base("JOIN") {}
}
