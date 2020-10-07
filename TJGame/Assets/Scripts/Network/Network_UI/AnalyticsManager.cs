using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager _instance;

    [SerializeField]
    private GameObject _newAnalyticsMenu;
    [SerializeField]
    private GameObject _viewAnalyticsMenu;
    [SerializeField]
    private GameObject _navigationMenu;
    [SerializeField]
    private MenuListCreator _analyticsMenuCreator;
    [SerializeField]
    private TMP_Dropdown _playerIdDD;
    [SerializeField]
    private TMP_InputField _MessageField;
    [SerializeField]
    private TMP_InputField _LimitCountField;
    [SerializeField]
    private TMP_InputField _StartTimeField;
    [SerializeField]
    private TMP_InputField _EndTimeField;
    [SerializeField]
    private TMP_Dropdown _SortByTimeDD;
    [SerializeField]
    private TMP_Dropdown _TypeDD;
    [SerializeField]
    private TMP_Dropdown _inputTypeDD;
    [SerializeField]
    private TMP_Dropdown _inputplayerIdDD;
    [SerializeField]
    private TMP_InputField _inputMessageField;
    [SerializeField]
    private Button _inputSendButton;
    [SerializeField]
    private TextMeshProUGUI _successText;

    private AnalyticsEventObject[] analyticsList;
    private List<Guid> playerList;
    private List<Guid> analyticsPlayerList;
    


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
        UpdatePlayersList();
        UpdateAnalyticPlayersList();
        UpdateAnalyticsList();
        _TypeDD.onValueChanged.AddListener(delegate {
            TypeDDValueChanged();
        });
    }

    //Ouput the new value of the Dropdown into Text
    void TypeDDValueChanged()
    {
        UpdateAnalyticPlayersList();
    }

    public void OpenNewAnalyticsMenu()
    {
        _successText.text = "";
        _newAnalyticsMenu.SetActive(true);
        _viewAnalyticsMenu.SetActive(false);
        UpdatePlayersList();
    }

    public void OpenViewAnalyticsMenu()
    {
        _newAnalyticsMenu.SetActive(false);
        _viewAnalyticsMenu.SetActive(true);
        UpdateAnalyticPlayersList();
        UpdateAnalyticsList();
    }


    public void CloseMenus()
    {
        _newAnalyticsMenu.SetActive(false);
        _viewAnalyticsMenu.SetActive(false);
        _navigationMenu.SetActive(false);
    }

    public async void CreateAnalytics()
    {
        _inputSendButton.interactable = false;

        if (await SendAnalytics())
            _successText.text = "Successfully send analytics.";
        else
            _successText.text = "No Success sending analytics.";

        _inputSendButton.interactable = true;
    }

    public async Task<bool> SendAnalytics()
    {
        string request = $"{Constants.apiAddress}api/analytics/new/{(_inputTypeDD.value - 1)}";
        object body = new
        {
            PlayerId = _inputplayerIdDD.options[_inputplayerIdDD.value].text,
            Message = _inputMessageField.text
        };

        Debug.Log(request);

        var response = await Player.PostAsync(request, body);
        var responseString = await response.Content.ReadAsStringAsync();

        Debug.Log(responseString);
        AnalyticsEventObject analyticsEvent;
        try
        {
            analyticsEvent = JsonConvert.DeserializeObject<AnalyticsEventObject>(responseString);
        }
        catch (Exception e)
        {
            return false;
        }

        return analyticsEvent != null;
    }
    public void AddCheatValues()
    {
        _StartTimeField.text = "0001-01-01T00:00:00";
        _EndTimeField.text = "9999-12-31T23:59:59.9999999";
    }

    void PopulateDropdown(TMP_Dropdown dropdown, List<Guid> optionsList)
    {
        string currentVal = dropdown.options[dropdown.value].text;
        //Debug.Log("current " + currentVal);
        List<string> options = new List<string>() { "None" };

        int match = String.Equals("None", currentVal) ? 0 : -1;
        for (int i = 0; i < optionsList.Count; i++)
        {
            string newString = optionsList[i].ToString();
            options.Add(newString);

            if (String.Equals(newString, currentVal))
                match = i + 1;

        }

        dropdown.ClearOptions();
        dropdown.AddOptions(options);
        dropdown.value = match;
        
        if (match == -1)
            dropdown.Show();
    }

    public void RefreshAnalyticsList()
    {
        UpdateAnalyticPlayersList();
        UpdateAnalyticsList();
    }

    private async void UpdateAnalyticPlayersList()
    {
        string request = $"{Constants.apiAddress}api/analytics/allGuids";
        if (_TypeDD.value != 0)
        {
            request += "?type=" + (_TypeDD.value - 1);
        }
        Debug.Log(request);
        var response = await Client._instance._httpClient.GetAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        Debug.Log(responseString);

        try
        {
            analyticsPlayerList = JsonConvert.DeserializeObject<List<Guid>>(responseString);
            PopulateDropdown(_playerIdDD, analyticsPlayerList);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    private async void UpdatePlayersList()
    {
        string request = $"{Constants.apiAddress}api/players/allGuids";

        var response = await Client._instance._httpClient.GetAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        Debug.Log(responseString);

        try
        {
            playerList = JsonConvert.DeserializeObject<List<Guid>>(responseString);
            PopulateDropdown(_inputplayerIdDD, playerList);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }


    private async void UpdateAnalyticsList()
    {
        //(EventType? type, Guid? playerId, int? limit, bool? sortAscending, DateTime? startTime, DateTime? endTime)
        string request = $"{Constants.apiAddress}api/analytics/"; //?sort={playerSort}";
        int addCount = 0;
        if(_TypeDD.value != 0)
        {
            request += (addCount == 0 ? "?" : "&") + "type=" + (_TypeDD.value - 1);
            addCount++;
        }

        if (_playerIdDD.value != 0)
        {
            request += (addCount == 0 ? "?" : "&") + "playerId=" + _playerIdDD.options[_playerIdDD.value].text;
            addCount++;
        }

        if (!string.IsNullOrEmpty(_MessageField.text))
        {
            request += (addCount == 0 ? "?" : "&") + "message=" + _MessageField.text;
            addCount++;
        }

        if (!string.IsNullOrEmpty(_LimitCountField.text))
        {
            request += (addCount == 0 ? "?" : "&") + "limit=" + (Int32.Parse(_LimitCountField.text));
            addCount++;
        }

        if (_SortByTimeDD.value == 1)
        {
            request += (addCount == 0 ? "?" : "&") + "sortAscending=true";
            addCount++;
        }
        else if(_SortByTimeDD.value == 2)
        {
            request += (addCount == 0 ? "?" : "&") + "sortAscending=false";
            addCount++;
        }

        if (!string.IsNullOrEmpty(_StartTimeField.text))
        {
            request += (addCount == 0 ? "?" : "&") + "startTime=" + _StartTimeField.text;
            addCount++;
        }

        if (!string.IsNullOrEmpty(_EndTimeField.text))
        {
            request += (addCount == 0 ? "?" : "&") + "endTime=" + _EndTimeField.text;
            addCount++;
        }
        Debug.Log("Searching: " + request);

        var response = await Client._instance._httpClient.GetAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        Debug.Log(responseString);

        try
        {
            analyticsList = JsonConvert.DeserializeObject<AnalyticsEventObject[]>(responseString);
            _analyticsMenuCreator.RefreshList(analyticsList);
        }
        catch (Exception e)
        {
            _analyticsMenuCreator.RefreshList(new AnalyticsEventObject[0]);
            Debug.Log(e);
        }
    }
}
