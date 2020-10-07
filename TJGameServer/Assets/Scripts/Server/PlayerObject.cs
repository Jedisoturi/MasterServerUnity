using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObject : BaseObject
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Score { get; set; }
    public int Level { get; set; }
    public DateTime CreationTime { get; set; }
    public bool[] Achievements { get; set; }
}
