using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager _instance;

    public GameObject _newPlayerMenu;
    public GameObject _connectingMenu;
    public GameObject _connectMenu;
    public GameObject _detailsMenu;
    public PlayerMenuListCreator _detailsMenuPlayerCreator;
    public ServerMenuObject _detailsMenuServer;
    public InputField _usernameField;
    private GameSaveData playerData;
    private bool userCreated = false;
    private ServerObject[] serverList;
    [SerializeField]
    private ServerMenuListCreator serverMenuList;

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
        InitMenu();
    }

    private void InitMenu()
    {
        if (Player.GetPlayerFromFiles())
        {
            Debug.Log("Player found: " + Player._guid);
            OpenConnectMenu();
        }
        else
        {
            OpenNewPlayerMenu();
        }
    }

    private bool ValidatePlayer()
    {
        if (Player._guid != null && Player._guid != Guid.Empty)
            return true;

        return false;
    }

    public void OpenNewPlayerMenu()
    {
        _newPlayerMenu.SetActive(true);
    }

    public void OpenConnectingMenu()
    {
        _newPlayerMenu.SetActive(false);
        _connectingMenu.SetActive(true);
    }

    public void OpenConnectMenu()
    {
        UpdateServerList();
        _connectingMenu.SetActive(false);
        _connectMenu.SetActive(true);
    }

    public void CreateUser()
    {
        if (_usernameField.text == "")
            return;
        if (!userCreated)
        {
            userCreated = true;
            Player.CreatePlayer(_usernameField.text);
            OpenConnectingMenu();
            StartCoroutine("WaitForPlayerDB");
        }
    }

    IEnumerator WaitForPlayerDB()
    {
        while (!ValidatePlayer())
        {
            yield return 0;
        }
        Debug.Log("Player created: " + Player._guid);
        OpenConnectMenu();
    }

    public async void ViewServerDetails(ServerObject server)
    {
        var response = await Client._instance._httpClient.GetAsync($"{Constants.apiAddress}api/servers/{server.Id}/players");
        var responseString = await response.Content.ReadAsStringAsync();

        PlayerObject[] playerList = JsonConvert.DeserializeObject<PlayerObject[]>(responseString);

        _detailsMenuPlayerCreator.RefreshList(playerList);
        _detailsMenuServer.UpdateServer(server);
        _detailsMenu.SetActive(true);
    }

    public void ConnectToServer(ServerObject server)
    {
        Debug.Log("Connecting to: " + server.EndPoint);
        _connectMenu.SetActive(false);
        Client._instance.ConnectToServer(server);
    }

    public void RefreshServerList()
    {
        UpdateServerList();
    }

    private async void UpdateServerList()
    {
        var response = await Client._instance._httpClient.GetAsync($"{Constants.apiAddress}api/servers/");
        var responseString = await response.Content.ReadAsStringAsync();

        serverList = JsonConvert.DeserializeObject<ServerObject[]>(responseString);

        Debug.Log(responseString);

        serverMenuList.RefreshList(serverList);
    }


}
