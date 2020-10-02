using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet packet)
    {
        string msg = packet.ReadString();
        int myId = packet.ReadInt();

        Debug.Log($"Message from server: {msg}");
        Client._instance._myId = myId;
        ClientSend.WelcomeReceived();

        Client._instance._udp.Connect(((IPEndPoint)Client._instance._tcp._socket.Client.LocalEndPoint).Port);
    }

    public static void UdpTest(Packet packet)
    {
        string msg = packet.ReadString();

        Debug.Log($"Received packet via UDP. Contains message: {msg}");
        ClientSend.UdpTestReceived();
    }

    public static void SpawnPlayer(Packet packet)
    {
        int id = packet.ReadInt();
        string username = packet.ReadString();
        Vector3 position = packet.ReadVector3();
        Quaternion rotation = packet.ReadQuaternion();

        GameManager._instance.SpawnPlayer(id, username, position, rotation);
    }

    public static void PlayerPosition(Packet packet)
    {
        int id = packet.ReadInt();
        Vector3 position = packet.ReadVector3();

        if (GameManager._players.ContainsKey(id)) GameManager._players[id].transform.position = position;

    }

    public static void PlayerRotation(Packet packet)
    {
        int id = packet.ReadInt();
        Quaternion rotation = packet.ReadQuaternion();

        if (GameManager._players.ContainsKey(id)) GameManager._players[id].transform.rotation = rotation;
    }

    public static void PlayerDisconnected(Packet packet)
    {
        int id = packet.ReadInt();

        Destroy(GameManager._players[id].gameObject);
        GameManager._players.Remove(id);
    }

    public static void PlayerHealth(Packet packet)
    {
        int id = packet.ReadInt();
        float health = packet.ReadFloat();

        GameManager._players[id].SetHealth(health);
    }

    public static void PlayerRespawned(Packet packet)
    {
        int id = packet.ReadInt();

        GameManager._players[id].Respawn();
    }

    public static void CreateItemSpawner(Packet packet)
    {
        int spawnerId = packet.ReadInt();
        Vector3 spawnerPosition = packet.ReadVector3();
        bool hasItem = packet.ReadBool();

        GameManager._instance.CreateItemSpawner(spawnerId, spawnerPosition, hasItem);
    }

    public static void ItemSpawned(Packet packet)
    {
        int spawnerId = packet.ReadInt();

        GameManager._itemSpawners[spawnerId].ItemSpawned();
    }

    public static void ItemPickedUp(Packet packet)
    {
        int spawnerId = packet.ReadInt();
        int byPlayer = packet.ReadInt();

        GameManager._itemSpawners[spawnerId].ItemPickedUp();
        GameManager._players[byPlayer]._itemCount++;
    }

    public static void SpawnProjectile(Packet packet)
    {
        int projectileId = packet.ReadInt();
        Vector3 position = packet.ReadVector3();
        int thrownByPlayer = packet.ReadInt();

        GameManager._instance.SpawnProjectile(projectileId, position);
        GameManager._players[thrownByPlayer]._itemCount--;
    }

    public static void ProjectilePosition(Packet packet)
    {
        int projectileId = packet.ReadInt();
        Vector3 position = packet.ReadVector3();

        GameManager._projectiles[projectileId].transform.position = position;
    }

    public static void ProjectileExploded(Packet packet)
    {
        int projectileId = packet.ReadInt();
        Vector3 position = packet.ReadVector3();

        GameManager._projectiles[projectileId].Explode(position);
    }
}
