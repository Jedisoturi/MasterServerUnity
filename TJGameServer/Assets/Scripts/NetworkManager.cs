using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager _instance;

    public GameObject _playerPrefab;
    public GameObject _projectilePrefab;
    public InputField serverName;
    public InputField maxPlayers;
    public GameObject newMenu;
    public GameObject serverMenu;
    public ServerMenuObject serverMenuObject;
    public MenuListCreator menuListCreator;
    public TextMeshProUGUI title;
    public Button button;
    private bool serverStarted = false;

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

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 64;
    }

    public void ServerButtonClicked()
    {
        if (!serverStarted)
        {
            StartServer();
        }
        else
        {
            ModifyServer();
        }

    }

    public void StartServer()
    {
        if(!string.IsNullOrEmpty(serverName.text) && !string.IsNullOrEmpty(maxPlayers.text) && Int32.Parse(maxPlayers.text) > 0)
        {
            title.text = "Modify server";
            button.GetComponentInChildren<Text>().text = "Save Changes";
            serverStarted = true;
            //newMenu.SetActive(false);
            serverMenu.SetActive(true);
            Server.Start(serverName.text, Int32.Parse(maxPlayers.text), 26950);
            InvokeRepeating("UpdateServer", 0.1f, 1f);
        }
    }

    public void ModifyServer()
    {
        if (!string.IsNullOrEmpty(serverName.text) && !string.IsNullOrEmpty(maxPlayers.text) && Int32.Parse(maxPlayers.text) > 0)
        {
            Debug.Log(serverName.text + Int32.Parse(maxPlayers.text));
            Server.DBNameChanged(serverName.text);
            Server.DBMaxPlayersChanged(Int32.Parse(maxPlayers.text));
        }
    }

    public async void UpdateServer()
    {
        if (Server._serverId == null || Server._serverId == Guid.Empty)
            return;

        string request = $"{Constants.apiAddress}api/servers/{Server._serverId}";
        var response = await Server._httpClient.GetAsync(request);
        if (response.IsSuccessStatusCode == true)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            ServerObject serverObj = JsonConvert.DeserializeObject<ServerObject>(responseString);
            serverMenuObject.UpdateObject(0, serverObj);
            ViewServerDetails();
            Debug.Log(responseString);
        }
        else
        {
            Debug.Log("No success updating server");
        }
    }

    public async void ViewServerDetails()
    {
        var response = await Server._httpClient.GetAsync($"{Constants.apiAddress}api/servers/{Server._serverId}/players");
        var responseString = await response.Content.ReadAsStringAsync();

        PlayerObject[] playerList = JsonConvert.DeserializeObject<PlayerObject[]>(responseString);

        menuListCreator.RefreshList(playerList);
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    public Player InstantiatePlayer()
    {
        return Instantiate(_playerPrefab, Vector3.zero, Quaternion.identity).GetComponent<Player>();
    }

    public Projectile InstantiateProjectile(Transform shootOrigin)
    {
        return Instantiate(_projectilePrefab, shootOrigin.position + shootOrigin.forward * 0.7f, 
            Quaternion.identity).GetComponent<Projectile>();
    }
}
