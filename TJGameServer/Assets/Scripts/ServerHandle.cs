using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerHandle
{
    public static void WelcomeReceived(int id, Packet packet)
    {
        int clientIdCheck = packet.ReadInt();
        string username = packet.ReadString();

        Debug.Log($"{Server._clients[id]._tcp._socket.Client.RemoteEndPoint} connected successfully and " +
            $"is now player {id}");

        if (id != clientIdCheck)
        {
            Debug.Log($"Player \" {username}\" (ID: {id}) has assumed the" +
                $"wrong client id ({clientIdCheck})!");
        }
        Server._clients[id].SendIntoGame(username);
    }

    public static void UdpTestReceived(int id, Packet packet)
    {
        string msg = packet.ReadString();

        Debug.Log($"Received packet via UDP. Contains message: {msg}");
    }

    public static void PlayerMovement(int id, Packet packet)
    {
        var inputs = new bool[packet.ReadInt()];
        for (int i = 0; i < inputs.Length; ++i)
        {
            inputs[i] = packet.ReadBool();
        }

        Quaternion rotation = packet.ReadQuaternion();

        Server._clients[id]._player.SetInput(inputs, rotation);
    }

    public static void PlayerShoot(int id, Packet packet)
    {
        Vector3 shootDirection = packet.ReadVector3();

        Server._clients[id]._player.Shoot(shootDirection);
    }

    public static void PlayerThrowItem(int fromClient, Packet packet)
    {
        Vector3 throwDirection = packet.ReadVector3();

        Server._clients[fromClient]._player.ThrowItem(throwDirection);
    }
}
