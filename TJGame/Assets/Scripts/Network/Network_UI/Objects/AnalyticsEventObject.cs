using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnalyticsEventObject : BaseObject
{
    public EventType Type { get; set; }
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public string Message { get; set; }
    public DateTime CreationTime { get; set; }
}

