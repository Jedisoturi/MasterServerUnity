using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using UnityEngine;

public class Server
{
    public static int MaxPlayers { get; private set; }
    public static int Port { get; private set; }

    public static Dictionary<int, Client> _clients = new Dictionary<int, Client>();
    public delegate void PacketHandler(int id, Packet packet);
    public static Dictionary<int, PacketHandler> _packetHandlers;

    private static TcpListener _tcpListener;
    private static UdpClient _udpClient;
    private static readonly HttpClient _httpClient = new HttpClient();

    private static Guid _serverId;
    private static Guid _adminKey;

    public static List<Guid> _bannedPlayers = new List<Guid>();

    public static void Start(int maxPlayers, int port)
    {
        MaxPlayers = maxPlayers;
        Port = port;

        Debug.Log("Starting server...");
        InitializeServerData();

        _tcpListener = new TcpListener(IPAddress.Any, Port);
        _tcpListener.Start();
        _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallback), null);

        _udpClient = new UdpClient(Port);
        _udpClient.BeginReceive(UdpReceiveCallback, null);

        Debug.Log($"Server started on {Port}.");

        DBCreateServer();
    }

    #region DB

    public async static void DBCreateServer()
    {
        // TODO: IP, name, bannedplayers should be given by the server creator
        string ip = "127.0.0.1";

        object data = new
        {
            Name = "Foo",
            EndPoint = $"{ip}:{Port}",
            Players = new List<Guid>(),
            MaxPlayers = MaxPlayers,
            BannedPlayers = new List<Guid>(),
            HasPassword = false
        };

        var response = await PostAsync($"{Constants.apiAddress}api/servers/create", data);

        // TODO: Add error handling for failed requests

        var responseString = await response.Content.ReadAsStringAsync();

        var serverAndKey = JsonConvert.DeserializeObject<ServerAndKeyObject>(responseString);
        _adminKey = serverAndKey.AdminKey;
        _serverId = serverAndKey.Server.Id;
    }

    public async static void DBPlayerConnected(int id)
    {
        await PostAsync($"{Constants.apiAddress}api/servers/{_serverId}/playerConnected/{_clients[id]._guid}?adminKey={_adminKey}", null);
    }

    public async static void DBPlayerDisconnected(int id)
    {
        await PostAsync($"{Constants.apiAddress}api/servers/{_serverId}/playerDisconnected/{_clients[id]._guid}?adminKey={_adminKey}", null);
    }

    public async static void DBBanPlayer(Guid guid)
    {
        await PostAsync($"{Constants.apiAddress}api/servers/{_serverId}/banPlayer/{guid}?adminKey={_adminKey}", null);
    }

    public async static void DBUnbanPlayer(Guid guid)
    {
        await PostAsync($"{Constants.apiAddress}api/servers/{_serverId}/unbanPlayer/{guid}?adminKey={_adminKey}", null);
    }

    public async static void DBDeleteServer()
    {
        await DeleteAsync($"{Constants.apiAddress}api/servers/{_serverId}?adminKey={_adminKey}");
    }

    public async static Task<PlayerObject> DBGetPlayer(Guid guid)
    {
        var response = await _httpClient.GetAsync($"{Constants.apiAddress}api/players/{guid}");
        if (response.IsSuccessStatusCode == true)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<PlayerObject>(responseString);
        }
        return null;
    }
    
    #endregion

    #region Misc

    public static void BanPlayer(int id)
    {
        DBBanPlayer(_clients[id]._guid);
        _bannedPlayers.Add(_clients[id]._guid);
        _clients[id].Disconnect();
    }

    public static void BanPlayer(Guid guid)
    {
        // TODO: Try to disconnect
        DBBanPlayer(guid);
        _bannedPlayers.Add(guid);
    }

    public static void UnbanPlayer(Guid guid)
    {
        DBUnbanPlayer(guid);
        if (_bannedPlayers.Contains(guid))
        {
            _bannedPlayers.Remove(guid);
        }
    }

    #endregion

    public static void Stop()
    {
        _tcpListener.Stop();
        _udpClient.Close();

        DBDeleteServer();
    }

    private static void TcpConnectCallback(IAsyncResult result)
    {
        TcpClient client = _tcpListener.EndAcceptTcpClient(result);
        _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TcpConnectCallback), null);
        Debug.Log($"Incoming connection from {((IPEndPoint)client.Client.RemoteEndPoint).Address} " +
            $"with port {((IPEndPoint)client.Client.RemoteEndPoint).Port} and endpoint {client.Client.RemoteEndPoint}...");

        for (int i = 1; i <= MaxPlayers; ++i)
        {
            if (_clients[i]._tcp._socket == null)
            {
                _clients[i]._tcp.Connect(client);
                return;
            }
        }
        Debug.Log($"{client.Client.RemoteEndPoint} failed to connect: Server full!");
    }


    private static void UdpReceiveCallback(IAsyncResult result)
    {
        try
        {
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = _udpClient.EndReceive(result, ref clientEndPoint);
            _udpClient.BeginReceive(UdpReceiveCallback, null);

            if (data.Length < Constants.intLengthInBytes)
            {
                return;
            }

            using (var packet = new Packet(data))
            {
                int transportId = packet.ReadInt();
                int clientId = packet.ReadInt();

                if (clientId == 0) return;

                if (_clients[clientId]._udp._endPoint == null)
                {
                    _clients[clientId]._udp.Connect(clientEndPoint);
                    return;
                }

                if (_clients[clientId]._udp._endPoint.ToString() == clientEndPoint.ToString())
                {
                    if (transportId == (int)TransportType.udp)
                    {
                        _clients[clientId]._udp.HandleData(packet);
                    }
                    else
                    {
                        // handle RUDP
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"Error receiving UDP data: {ex}");
        }
    }

    public static void SendUdpData(IPEndPoint clientEndPoint, Packet packet)
    {
        try
        {
            if (clientEndPoint == null) return;
            _udpClient.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
        }
        catch (Exception ex)
        {
            Debug.Log($"Error sending data to {clientEndPoint} via UDP: {ex}");
        }
    }

    private static void InitializeServerData()
    {
        for (int i = 1; i <= MaxPlayers; ++i)
        {
            _clients.Add(i, new Client(i));
        }

        _packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                { (int)ClientPackets.udpTestReceived, ServerHandle.UdpTestReceived },
                { (int)ClientPackets.playerMovement, ServerHandle.PlayerMovement },
                { (int)ClientPackets.playerShoot, ServerHandle.PlayerShoot },
                { (int)ClientPackets.playerThrowItem, ServerHandle.PlayerThrowItem }
            };

        Debug.Log("Initialized packets.");
    }

    #region HttpWrappers

    public static string Encode(string input, byte[] key)
    {
        using (var myhmacsha1 = new HMACSHA256(key))
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(input.ToLower());
            byte[] hash = myhmacsha1.ComputeHash(byteArray);
            return Convert.ToBase64String(hash);
        }
    }

    public static string GenerateURL(string path, string body)
    {
        var url = path;
        var stringToEncrypt = url + body;
        var encrypted = Encode(stringToEncrypt, Constants.secret);
        encrypted = HttpUtility.UrlEncode(encrypted);
        Debug.Log(encrypted);
        Debug.Log(HttpUtility.UrlDecode(encrypted));
        url += (path.IndexOf("?") < 0 ? "?" : "&") + "signature=" + encrypted;

        return url;
    }

    public async static Task<HttpResponseMessage> PostAsync(string url, object body)
    {
        // Convert body to bytes
        var content = JsonConvert.SerializeObject(body);
        var buffer = Encoding.UTF8.GetBytes(content);
        var byteContent = new ByteArrayContent(buffer);
        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        url = GenerateURL(url, content);
        return await _httpClient.PostAsync(url, byteContent);
    }

    public async static Task<HttpResponseMessage> DeleteAsync(string url)
    {
        url = GenerateURL(url, null);
        return await _httpClient.DeleteAsync(url);
    }

    #endregion
}
