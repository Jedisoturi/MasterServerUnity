using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnalyticsEventObject : BaseObject
{
    public EventType Type { get; private set; }
    public Guid Id { get; private set; }
    public Guid PlayerId { get; private set; }
    public string Message { get; private set; }
    public DateTime CreationTime { get; private set; }
}

