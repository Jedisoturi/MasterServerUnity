using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using TMPro;
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
    private GameObject _playerMenu;
    [SerializeField]
    private MenuListCreator _leaderMenuPlayerCreator;
    [SerializeField]
    private MenuListCreator _detailsMenuPlayerCreator;
    [SerializeField]
    private ServerMenuObject _detailsMenuServer;
    [SerializeField]
    private InputField _usernameField;
    private GameSaveData playerData;
    private bool userCreated = false;
    private PlayerSort playerSort = PlayerSort.ScoreDesc;
    [SerializeField]
    private InputField _inputName;
    [SerializeField]
    private InputField _inputScore;
    [SerializeField]
    private InputField _inputLevel;
    [SerializeField]
    private TMP_Dropdown _inputAchievement;
    [SerializeField]
    private PlayerMenuObject playerMenuObject;

    [SerializeField]
    private MenuListCreator serverMenuList;

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
        _playerMenu.SetActive(false);
        UpdateServerList();
    }

    public void OpenLeaderMenu()
    {
        _connectMenu.SetActive(false);
        _detailsMenu.SetActive(false);
        _leaderMenu.SetActive(true);
        _playerMenu.SetActive(false);
        UpdatePlayerList();
    }

    public void OpenPlayerMenu()
    {
        _connectMenu.SetActive(false);
        _detailsMenu.SetActive(false);
        _leaderMenu.SetActive(false);
        _playerMenu.SetActive(true);
        UpdatePlayerItem();
    }

    public void CloseMenus()
    {
        _connectMenu.SetActive(false);
        _navigationMenu.SetActive(false);
        _detailsMenu.SetActive(false);
        _playerMenu.SetActive(false);
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
        _detailsMenuServer.UpdateObject(0, server);
        _detailsMenu.SetActive(true);
    }

    public void ConnectToServer(ServerObject server)
    {
        Debug.Log("Connecting to: " + server.EndPoint);
        CloseMenus();

        Client._instance.ConnectToServer(server);
    }

    public void EditPlayer()
    {
        SendPlayer();
    }

    public async void SendPlayer()
    {
        string baseRequest = $"{Constants.apiAddress}api/players/{Player._guid}/";

        if (!string.IsNullOrEmpty(_inputName.text))
        {
            string nameRequest = "rename/" + _inputName.text;
            string request = baseRequest + nameRequest;
            var response = await Player.PostAsync(request, null);
            Debug.Log(request);
        }

        if(Int32.Parse(_inputScore.text) > 0)
        {
            string nameRequest = "incScore/" + Int32.Parse(_inputScore.text);
            string request = baseRequest + nameRequest;
            var response = await Player.PostAsync(request, null);
            Debug.Log(request);
        }

        if (Int32.Parse(_inputLevel.text) > 0)
        {
            string nameRequest = "incLevel/" + Int32.Parse(_inputLevel.text);
            string request = baseRequest + nameRequest;
            var response = await Player.PostAsync(request, null);
            Debug.Log(request);
        }

        if (_inputAchievement.value > 0)
        {
            string nameRequest = "addAchievement/" + (_inputAchievement.value - 1);
            string request = baseRequest + nameRequest;
            var response = await Player.PostAsync(request, null);
            Debug.Log(request);
        }
        UpdatePlayerItem();
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

    private async void UpdatePlayerItem()
    {
        string request = $"{Constants.apiAddress}api/players/{Player._guid}";

        var response = await Client._instance._httpClient.GetAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();

        PlayerObject playerObj = JsonConvert.DeserializeObject<PlayerObject>(responseString);

        Debug.Log(responseString);

        _inputName.text = playerObj.Name;
        playerMenuObject.UpdateObject(0, playerObj);
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
