using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MoveProto : MessageProto {
    public string id = "";
    public float x = 0;
    public float y = 0;
    public float z = 0;
    public int score = 0;

    public MoveProto() : base("MOVE") {}
}
