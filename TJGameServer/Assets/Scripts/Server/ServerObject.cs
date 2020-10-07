using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerObject : BaseObject
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string EndPoint { get; set; }
    public List<Guid> Players { get; set; }
    public int MaxPlayers { get; set; }
    public DateTime CreationDate { get; set; }
    public List<Guid> BannedPlayers { get; set; }
    public bool HasPassword { get; set; }
}
