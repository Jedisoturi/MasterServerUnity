using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend
{
    private static void SendTcpData(int id, Packet packet)
    {
        packet.WriteLength();
        Server._clients[id]._tcp.SendData(packet);
    }

    private static void SendUdpData(int id, Packet packet)
    {
        packet.WriteLength();
        Server._clients[id]._udp.SendData(packet);
    }

    private static void SendTcpDataToAll(Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; ++i)
        {
            Server._clients[i]._tcp.SendData(packet);
        }
    }
    private static void SendTcpDataToAll(int exceptClient, Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; ++i)
        {
            if (i == exceptClient) continue;
            Server._clients[i]._tcp.SendData(packet);
        }
    }

    private static void SendUdpDataToAll(Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; ++i)
        {
            Server._clients[i]._udp.SendData(packet);
        }
    }
    private static void SendUdpDataToAll(int exceptClient, Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; ++i)
        {
            if (i == exceptClient) continue;
            Server._clients[i]._udp.SendData(packet);
        }
    }

    #region Packets
    public static void Welcome(int id, string msg)
    {
        using (var packet = new Packet((int)ServerPackets.welcome))
        {
            packet.Write(msg);
            packet.Write(id);

            SendTcpData(id, packet);
        }
    }

    public static void UdpTest(int id)
    {
        using (var packet = new Packet((int)ServerPackets.udpTest))
        {
            packet.Write("A test packet for UDP.");

            SendUdpData(id, packet);
        }
    }

    public static void SpawnPlayer(int id, Player player)
    {
        Debug.Log($"Spawning player");
        using (var packet = new Packet((int)ServerPackets.spawnPlayer))
        {
            packet.Write(player._id);
            packet.Write(player._username);
            packet.Write(player.transform.position);
            packet.Write(player.transform.rotation);

            SendTcpData(id, packet);
        }
    }

    public static void PlayerPosition(Player player)
    {
        using (var packet = new Packet((int)ServerPackets.playerPosition))
        {
            packet.Write(player._id);
            packet.Write(player.transform.position);

            SendUdpDataToAll(packet);
        }
    }

    public static void PlayerRotation(Player player)
    {
        using (var packet = new Packet((int)ServerPackets.playerRotation))
        {
            packet.Write(player._id);
            packet.Write(player.transform.rotation);

            SendUdpDataToAll(player._id, packet);
        }
    }

    public static void PlayerDisconnected(int playerId)
    {
        using (var packet = new Packet((int)ServerPackets.playerDisconnected))
        {
            packet.Write(playerId);

            SendTcpDataToAll(packet);
        }
    }

    public static void PlayerHealth(Player player)
    {
        using (var packet = new Packet((int)ServerPackets.playerHealth))
        {
            packet.Write(player._id);
            packet.Write(player._health);

            SendTcpDataToAll(packet);
        }
    }

    public static void PlayerRespawned(Player player)
    {
        using (var packet = new Packet((int)ServerPackets.playerRespawned))
        {
            packet.Write(player._id);

            SendTcpDataToAll(packet);
        }
    }

    public static void CreateItemSpawner(int toClient, int spawnerId, Vector3 spawnerPosition, bool hasItem)
    {
        using (var packet = new Packet((int)ServerPackets.createItemSpawner))
        {
            packet.Write(spawnerId);
            packet.Write(spawnerPosition);
            packet.Write(hasItem);

            SendTcpData(toClient, packet);

        }
    }

    public static void ItemSpawned(int spawnerId)
    {
        using (var packet = new Packet((int)ServerPackets.itemSpawned))
        {
            packet.Write(spawnerId);

            SendTcpDataToAll(packet);
        }
    }

    public static void ItemPickedUp(int spawnerId, int byPlayer)
    {
        using (var packet = new Packet((int)ServerPackets.itemPickedUp))
        {
            packet.Write(spawnerId);
            packet.Write(byPlayer);

            SendTcpDataToAll(packet);
        }
    }

    public static void SpawnProjectile(Projectile projectile, int thrownByPlayer)
    {
        using (var packet = new Packet((int)ServerPackets.spawnProjectile))
        {
            packet.Write(projectile._id);
            packet.Write(projectile.transform.position);
            packet.Write(thrownByPlayer);

            SendTcpDataToAll(packet);
        }
    }

    public static void ProjectilePosition(Projectile projectile)
    {
        using (var packet = new Packet((int)ServerPackets.projectilePosition))
        {
            packet.Write(projectile._id);
            packet.Write(projectile.transform.position);

            SendTcpDataToAll(packet);
        }
    }

    public static void ProjectileExploded(Projectile projectile)
    {
        using (var packet = new Packet((int)ServerPackets.projectileExploded))
        {
            packet.Write(projectile._id);
            packet.Write(projectile.transform.position);

            SendTcpDataToAll(packet);
        }
    }
    #endregion
}
