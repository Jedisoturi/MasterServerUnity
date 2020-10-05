using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using Priority_Queue;
using System.Net.Http;

public class Client : MonoBehaviour
{

    public static Client _instance;
    public static int _dataBufferSize = 4096;

    public string _ip = "127.0.0.1";
    public int _port = 26950;
    public int _myId = 0;
    public TCP _tcp;
    public UDP _udp;
    public HttpClient _httpClient = new HttpClient();

    private bool _isConnected = false;
    private delegate void PacketHandler(Packet packet);
    private static Dictionary<int, PacketHandler> _packetHandlers;


    // RUDP
    private Dictionary<int, PendingOutgoingPacket> _pendingPackets;  // key is the notificationKey
    private const int _maxPending = 500;  // this can easily be overrun in the case of a disconnect
    private const float _basicTimeout = 1f;
    private SimplePriorityQueue<PendingOutgoingPacket, float> _timeoutQueue;  // priority is determined by next timeout
    private readonly object _rudpLock = new object();
    

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        } 
        else if (_instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    public void ConnectToServer()
    {
        _tcp = new TCP();
        _udp = new UDP();

        InitializeClientData();

        _isConnected = true;
        _tcp.Connect();
    }

    public class TCP
    {
        public TcpClient _socket;

        private NetworkStream _stream;
        private Packet _receivedData;
        private byte[] _receiveBuffer;

        public void Connect()
        {
            _socket = new TcpClient
            {
                ReceiveBufferSize = _dataBufferSize,
                SendBufferSize = _dataBufferSize
            };

            _receiveBuffer = new byte[_dataBufferSize];
            _socket.BeginConnect(_instance._ip, _instance._port, ConnectCallback, _socket);
        }

        private void ConnectCallback(IAsyncResult result)
        {
            _socket.EndConnect(result);

            if (!_socket.Connected) return;

            _stream = _socket.GetStream();

            _receivedData = new Packet();

            _stream.BeginRead(_receiveBuffer, 0, _dataBufferSize, ReceiveCallback, null);
        }

        public void SendData(Packet packet)
        {
            try
            {
                if (_socket != null)
                {
                    _stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                }
            } 
            catch (Exception ex)
            {
                Debug.Log($"Error sending data to server via TCP: {ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int byteLength = _stream.EndRead(result);
                if (byteLength <= 0)
                {
                    _instance.Disconnect();
                    return;
                }

                byte[] data = new byte[byteLength];
                Array.Copy(_receiveBuffer, data, byteLength);

                _receivedData.Reset(HandleData(data));

                _stream.BeginRead(_receiveBuffer, 0, _dataBufferSize, ReceiveCallback, null);
            }
            catch
            {
                Disconnect();
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
                        _packetHandlers[packetId](packet);
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

        private void Disconnect()
        {
            _instance.Disconnect();

            _stream = null;
            _receivedData = null;
            _receiveBuffer = null;
            _socket = null;
        }
    }


    public class UDP
    {
        public UdpClient _socket;
        public IPEndPoint _endPoint;

        public UDP()
        {
            _endPoint = new IPEndPoint(IPAddress.Parse(_instance._ip), _instance._port);
        }

        public void Connect(int localPort)
        {
            _socket = new UdpClient(localPort);

            _socket.Connect(_endPoint);
            _socket.BeginReceive(ReceiveCallback, null);

            using (var packet = new Packet())
            {
                SendData(packet);
            }
        }

        public void SendReliableData(Packet packet)
        {
            try
            {
                lock (_instance._rudpLock)
                {
                    packet.InsertInt(_instance._myId);
                    int notificationKey = _instance.GetNotificationKey();
                    packet.InsertInt(notificationKey);
                    packet.InsertInt((int)TransportType.rudp);
                    _instance._pendingPackets.Add(notificationKey, new PendingOutgoingPacket(packet, notificationKey, Time.fixedTime));
                    _instance._timeoutQueue.Enqueue(_instance._pendingPackets[notificationKey], Time.fixedTime + _basicTimeout);
                }
                
                if (_socket != null) _socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
            }
            catch (Exception ex)
            {
                Debug.Log($"Error sending data to server via RUDP: {ex}");
            }
        }

        public void SendData(Packet packet)
        {
            try
            {
                packet.InsertInt(_instance._myId);
                packet.InsertInt((int)TransportType.udp);
                if (_socket != null) _socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
            }
            catch (Exception ex)
            {
                Debug.Log($"Error sending data to server via UDP: {ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                byte[] data = _socket.EndReceive(result, ref _endPoint);
                _socket.BeginReceive(ReceiveCallback, null);

                if (data.Length < 4)
                {
                    _instance.Disconnect();
                    return;
                }

                HandleData(data);
            }
            catch
            {
                Disconnect();
            }
        }

        private void HandleData(byte[] data)
        {
            using (var packet = new Packet(data))
            {
                int packetLength = packet.ReadInt();
                data = packet.ReadBytes(packetLength);
            }

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (var packet = new Packet(data))
                {
                    int packetId = packet.ReadInt();
                    _packetHandlers[packetId](packet);
                }
            });
        }

        private void Disconnect()
        {
            _instance.Disconnect();

            _endPoint = null;
            _socket = null;
        }
    }

    private int GetNotificationKey()
    {
        foreach (KeyValuePair<int, PendingOutgoingPacket> entry in _pendingPackets)
        {
            if (entry.Value == null)
            {
                return entry.Key;
            }
        }

        throw new Exception("No available keys!");
    }

    private void InitializeClientData()
    {
        _pendingPackets = new Dictionary<int, PendingOutgoingPacket>();

        for (int i = 0; i < _maxPending; ++i)
        {
            _pendingPackets.Add(i, null);
        }

        _timeoutQueue = new SimplePriorityQueue<PendingOutgoingPacket, float>();

        _packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ServerPackets.welcome, ClientHandle.Welcome },
            { (int)ServerPackets.udpTest, ClientHandle.UdpTest },
            { (int)ServerPackets.spawnPlayer, ClientHandle.SpawnPlayer },
            { (int)ServerPackets.playerPosition, ClientHandle.PlayerPosition },
            { (int)ServerPackets.playerRotation, ClientHandle.PlayerRotation },
            { (int)ServerPackets.playerDisconnected, ClientHandle.PlayerDisconnected },
            { (int)ServerPackets.playerHealth, ClientHandle.PlayerHealth },
            { (int)ServerPackets.playerRespawned, ClientHandle.PlayerRespawned },
            { (int)ServerPackets.createItemSpawner, ClientHandle.CreateItemSpawner },
            { (int)ServerPackets.itemSpawned, ClientHandle.ItemSpawned },
            { (int)ServerPackets.itemPickedUp, ClientHandle.ItemPickedUp },
            { (int)ServerPackets.spawnProjectile, ClientHandle.SpawnProjectile },
            { (int)ServerPackets.projectilePosition, ClientHandle.ProjectilePosition },
            { (int)ServerPackets.projectileExploded, ClientHandle.ProjectileExploded }
        };
        Debug.Log("Initilized packet handlers!");
    }

    private void Disconnect()
    {
        if (!_isConnected) return;

        _isConnected = false;
        _tcp._socket.Close();
        _udp._socket.Close();

        Debug.Log("Disconnected from server.");
    }

}
