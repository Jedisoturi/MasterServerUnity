using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServerMenuObject : MonoBehaviour
{
    public TextMeshProUGUI name;
    public TextMeshProUGUI endpoint;
    public TextMeshProUGUI players;
    public TextMeshProUGUI hasPass;
    public Image background;
    private ServerObject serverObj;

    public void Init(ServerObject server)
    {
        serverObj = server;
        name.text = server.Name;
        endpoint.text = server.EndPoint;
        players.text = server.Players.Count + " / " + server.MaxPlayers;
        hasPass.text = server.HasPassword ? "On" : "Off";
    }



    public void ViewDetails()
    {
        Debug.Log("View details");
    }

    public void ConnectServer()
    {
        UIManager._instance.ConnectToServer(serverObj);
    }
}
