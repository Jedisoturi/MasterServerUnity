using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServerMenuObject : MonoBehaviour
{
    public bool isDetail = false;
    public TextMeshProUGUI name;
    public TextMeshProUGUI endpoint;
    public TextMeshProUGUI players;
    public TextMeshProUGUI hasPass;
    public Image background;
    private ServerObject serverObj;
    private UIManager boss;

    public void Init(UIManager manager, ServerObject server)
    {
        boss = manager;
        UpdateServer(server);
    }

    public void UpdateServer(ServerObject server)
    {
        if (isDetail)
        {
            serverObj = server;
            name.text = server.Name;
            endpoint.text = "Address: " + server.EndPoint;
            players.text = "Player Count: " + server.Players.Count + " / " + server.MaxPlayers;
            hasPass.text = "Password Protection: " + (server.HasPassword ? "On" : "Off");
        }
        else
        {
            serverObj = server;
            name.text = server.Name;
            endpoint.text = server.EndPoint;
            players.text = server.Players.Count + " / " + server.MaxPlayers;
            hasPass.text = (server.HasPassword ? "On" : "Off");
        }

    }

    public void ViewDetails()
    {
       boss.ViewServerDetails(serverObj);
    }

    public void ConnectServer()
    {
        boss.ConnectToServer(serverObj);
    }
}
