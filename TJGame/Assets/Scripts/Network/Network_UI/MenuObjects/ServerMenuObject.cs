using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServerMenuObject : BaseMenuObject
{
    public TextMeshProUGUI endpointText;
    public TextMeshProUGUI playersText;
    public TextMeshProUGUI hasPassText;
    public bool isDetail = false;
    private ServerObject serverObj;

    public override void UpdateObject(int index, BaseObject inObj)
    {
        ServerObject server = (ServerObject)inObj;
        if (server == null)
            return;
        if (isDetail)
        {
            serverObj = (ServerObject)server;
            nameText.text = server.Name;
            endpointText.text = "Address: " + server.EndPoint;
            playersText.text = "Player Count: " + server.Players.Count + " / " + server.MaxPlayers;
            hasPassText.text = "Password Protection: " + (server.HasPassword ? "On" : "Off");
        }
        else
        {
            serverObj = server;
            nameText.text = server.Name;
            endpointText.text = server.EndPoint;
            playersText.text = server.Players.Count + " / " + server.MaxPlayers;
            hasPassText.text = (server.HasPassword ? "On" : "Off");
        }

    }

    public void ViewDetails()
    {
       Player._uiManager.ViewServerDetails(serverObj);
    }

    public void ConnectServer()
    {
        Player._uiManager.ConnectToServer(serverObj);
    }


}
