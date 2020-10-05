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

    [SerializeField]
    private GameObject _newPlayerMenu;
    [SerializeField]
    private GameObject _awaitServersMenu;
    [SerializeField]
    private GameObject _connectMenu;
    [SerializeField]
    private GameObject _detailsMenu;
    [SerializeField]
    private GameObject _navigationMenu;
    [SerializeField]
    private GameObject _leaderMenu;
    [SerializeField]
    private PlayerMenuListCreator _leaderMenuPlayerCreator;
    [SerializeField]
    private PlayerInServerMenuListCreator _detailsMenuPlayerCreator;
    [SerializeField]
    private ServerMenuObject _detailsMenuServer;
    [SerializeField]
    private InputField _usernameField;
    private GameSaveData playerData;
    private bool userCreated = false;
    private PlayerSort playerSort = PlayerSort.ScoreDesc;

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

    public void OpenAwaitConnectingMenu()
    {
        _newPlayerMenu.SetActive(false);
        _awaitServersMenu.SetActive(true);
    }

    public void OpenConnectMenu()
    {
        _awaitServersMenu.SetActive(false);
        _navigationMenu.SetActive(true);
        _leaderMenu.SetActive(false);
        _connectMenu.SetActive(true);
        UpdateServerList();
    }

    public void OpenLeaderMenu()
    {
        _connectMenu.SetActive(false);
        _detailsMenu.SetActive(false);
        _leaderMenu.SetActive(true);
        UpdatePlayerList();
    }

    public void CloseMenus()
    {
        _connectMenu.SetActive(false);
        _navigationMenu.SetActive(false);
        _detailsMenu.SetActive(false);
    }

    public void CreateUser()
    {
        if (_usernameField.text == "")
            return;
        if (!userCreated)
        {
            userCreated = true;
            Player.CreatePlayer(_usernameField.text);
            OpenAwaitConnectingMenu();
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
        CloseMenus();
        Client._instance.ConnectToServer();
    }

    public void RefreshServerList()
    {
        UpdateServerList();
    }

    public void RefreshPlayerList()
    {
        UpdatePlayerList();
    }

    public void UpdatePlayerSort(int sorting)
    {
        if (Enum.IsDefined(typeof(PlayerSort), sorting))
        {
            playerSort = (PlayerSort)sorting;
            UpdatePlayerList();
        }
    }

    private async void UpdatePlayerList()
    {
        string request = $"{Constants.apiAddress}api/players/?sort={playerSort}";

        var response = await Client._instance._httpClient.GetAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();

        PlayerObject[] playerList = JsonConvert.DeserializeObject<PlayerObject[]>(responseString);

        Debug.Log(responseString);

        _leaderMenuPlayerCreator.RefreshList(playerList);
    }

    private async void UpdateServerList()
    {
        var response = await Client._instance._httpClient.GetAsync($"{Constants.apiAddress}api/servers/");
        var responseString = await response.Content.ReadAsStringAsync();

        ServerObject[] serverList = JsonConvert.DeserializeObject<ServerObject[]>(responseString);

        Debug.Log(responseString);

        serverMenuList.RefreshList(serverList);
    }


}
