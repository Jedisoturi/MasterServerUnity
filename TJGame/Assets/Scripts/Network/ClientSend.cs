using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void SendTcpData(Packet packet)
    {
        packet.WriteLength();
        Client._instance._tcp.SendData(packet);
    }

    private static void SendUdpData(Packet packet)
    {
        packet.WriteLength();
        Client._instance._udp.SendData(packet);
    }

    private static void SendRudpData(Packet packet)
    {
        packet.WriteLength();
        Client._instance._udp.SendReliableData(packet);
    }

    #region Packets
    public static void WelcomeReceived()
    {
        using (Packet packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            packet.Write(Client._instance._myId);
            packet.Write(UIManager._instance._usernameField.text);
            // TODO: Take Guid from client instance
            packet.Write(Guid.NewGuid().ToString());

            SendTcpData(packet);
        }
    }

    public static void UdpTestReceived()
    {
        using (var packet = new Packet((int)ClientPackets.udpTestReceived))
        {
            packet.Write("Received a UDP packet.");

            SendUdpData(packet);
        }
    }

    public static void PlayerMovement(bool[] inputs)
    {
        using (var packet = new Packet((int)ClientPackets.playerMovement))
        {
            packet.Write(inputs.Length);
            foreach (bool input in inputs)
            {
                packet.Write(input);
            }
            packet.Write(GameManager._players[Client._instance._myId].transform.rotation);

            SendUdpData(packet);
        }
    }

    public static void PlayerShoot(Vector3 facing)
    {
        using (var packet = new Packet((int)ClientPackets.playerShoot))
        {
            packet.Write(facing);

            SendTcpData(packet);
        }
    }

    public static void PlayerThrowItem(Vector3 facing)
    {
        using (var packet = new Packet((int)ClientPackets.playerThrowItem))
        {
            packet.Write(facing);

            SendTcpData(packet);
        }
    }
    #endregion
}
