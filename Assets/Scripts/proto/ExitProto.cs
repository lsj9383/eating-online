using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ExitProto : MessageProto {
    public string id;

    public ExitProto() : base("EXIT") {}
}
