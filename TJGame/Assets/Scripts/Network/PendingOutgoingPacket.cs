using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PendingOutgoingPacket
{
    public Packet _packet;
    public float _lastSent;
    public float _firstSent;
    public int _attempts;
    public int _notificationKey;

    public PendingOutgoingPacket(Packet packet, int notificationKey, float time)
    {
        _packet = packet;
        _notificationKey = notificationKey;
        _lastSent = time;
        _firstSent = time;
        _attempts = 0;
    }
}
