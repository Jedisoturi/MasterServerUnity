using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client
{
    public static int _dataBufferSize = 4096;

    public int _id;
    public Guid _guid;
    public Player _player;
    public TCP _tcp;
    public UDP _udp;

    public Client(int clientId)
    {
        _id = clientId;
        _tcp = new TCP(_id);
        _udp = new UDP(_id);
    }

    public class TCP
    {
        public TcpClient _socket;

        private readonly int _id;
        private NetworkStream _stream;
        private Packet _receivedData;
        private byte[] _receiveBuffer;

        public TCP(int id)
        {
            _id = id;
        }

        public void Connect(TcpClient socket)
        {
            _socket = socket;
            socket.ReceiveBufferSize = _dataBufferSize;
            socket.SendBufferSize = _dataBufferSize;

            _stream = _socket.GetStream();

            _receivedData = new Packet();
            _receiveBuffer = new byte[_dataBufferSize];

            _stream.BeginRead(_receiveBuffer, 0, _dataBufferSize, ReceiveCallback, null);

            ServerSend.Welcome(_id, "Welcome to the server!");
        }

        public void SendData(Packet packet)
        {
            try
            {
                if (_socket != null) _stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
            }
            catch (Exception ex)
            {
                Debug.Log($"Error sending data to player {_id} via TCP: {ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int byteLength = _stream.EndRead(result);
                if (byteLength <= 0)
                {
                    Server._clients[_id].Disconnect();
                    return;
                }

                byte[] data = new byte[byteLength];
                Array.Copy(_receiveBuffer, data, byteLength);

                _receivedData.Reset(HandleData(data));

                _stream.BeginRead(_receiveBuffer, 0, _dataBufferSize, ReceiveCallback, null);
            }
            catch (Exception ex)
            {
                Debug.Log($"Error receiving TCP data: {ex}");
                Server._clients[_id].Disconnect();
            }

        }

        private bool HandleData(byte[] data)
        {
            int packetLength = 0;

            _receivedData.SetBytes(data);

            if (4 <= _receivedData.UnreadLength())
            {
                packetLength = _receivedData.ReadInt();
                if (packetLength <= 0) return true;
            }

            while (0 < packetLength && packetLength <= _receivedData.UnreadLength())
            {
                byte[] packetBytes = _receivedData.ReadBytes(packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (var packet = new Packet(packetBytes))
                    {
                        int packetId = packet.ReadInt();
                        Server._packetHandlers[packetId](_id, packet);
                    }
                });

                packetLength = 0;
                if (4 <= _receivedData.UnreadLength())
                {
                    packetLength = _receivedData.ReadInt();
                    if (packetLength <= 0) return true;
                }
            }

            if (packetLength <= 1)
            {
                return true;
            }
            return false;
        }

        public void Disconnect()
        {
            _socket.Close();
            _stream = null;
            _receivedData = null;
            _receiveBuffer = null;
            _socket = null;
        }
    }

    public class UDP
    {
        public IPEndPoint _endPoint;

        private int _id;

        public UDP(int id)
        {
            _id = id;
        }

        public void Connect(IPEndPoint endPoint)
        {
            _endPoint = endPoint;
            ServerSend.UdpTest(_id);
        }

        public void SendData(Packet packet)
        {
            Server.SendUdpData(_endPoint, packet);
        }

        public void HandleData(Packet packetToHandle)
        {
            int packetLength = packetToHandle.ReadInt();
            byte[] packetBytes = packetToHandle.ReadBytes(packetLength);

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (var packet = new Packet(packetBytes))
                {
                    int packetId = packet.ReadInt();
                    Server._packetHandlers[packetId](_id, packet);
                }
            });
        }

        public void Disconnect()
        {
            _endPoint = null;
        }
    }

    public void SendIntoGame(string playerName)
    {
        _player = NetworkManager._instance.InstantiatePlayer();
        _player.Initialize(_id, playerName);

        // Spawn other players for this client
        foreach (Client client in Server._clients.Values)
        {
            if (client._player != null && client._id != _id) ServerSend.SpawnPlayer(_id, client._player);
        }

        // Spawn this client for all clients (including himself)
        foreach (Client client in Server._clients.Values)
        {
            if (client._player != null) ServerSend.SpawnPlayer(client._id, _player);
        }

        foreach (ItemSpawner itemSpawner in ItemSpawner._spawners.Values)
        {
            ServerSend.CreateItemSpawner(_id, itemSpawner._spawnerId, itemSpawner.transform.position, itemSpawner._hasItem);
        }
    }

    public void Disconnect()
    {
        Debug.Log($"{_tcp._socket.Client.RemoteEndPoint} has disconnected.");

        if (_player != null)
        {
            ThreadManager.ExecuteOnMainThread(() =>
            {
                UnityEngine.Object.Destroy(_player.gameObject);
                _player = null;
            });

            ServerSend.PlayerDisconnected(_id);

            Server.DBPlayerDisconnected(_id);
        }

        _tcp.Disconnect();
        _udp.Disconnect();
    }
}
