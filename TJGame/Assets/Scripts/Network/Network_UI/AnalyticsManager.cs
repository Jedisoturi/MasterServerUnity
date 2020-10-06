using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
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
    private InputField _usernameField;
    private PlayerSort playerSort = PlayerSort.ScoreDesc;


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
        OpenViewAnalyticsMenu();
    }

    public void OpenNewAnalyticsMenu()
    {
        _newAnalyticsMenu.SetActive(true);
        _viewAnalyticsMenu.SetActive(false);
    }

    public void OpenViewAnalyticsMenu()
    {
        _newAnalyticsMenu.SetActive(false);
        _viewAnalyticsMenu.SetActive(true);
        UpdateAnalyticsList();
    }


    public void CloseMenus()
    {
        _newAnalyticsMenu.SetActive(false);
        _viewAnalyticsMenu.SetActive(false);
        _navigationMenu.SetActive(false);
    }

    public void CreateAnalytics()
    {

    }

    public void RefreshAnalyticsList()
    {
        UpdateAnalyticsList();
    }

    public void UpdateAnalyticsSort(int sorting)
    {
        if (Enum.IsDefined(typeof(PlayerSort), sorting))
        {
            playerSort = (PlayerSort)sorting;
            UpdateAnalyticsList();
        }
    }

    private async void UpdateAnalyticsList()
    {
        string request = $"{Constants.apiAddress}api/analytics/"; //?sort={playerSort}";

        var response = await Client._instance._httpClient.GetAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();

        AnalyticsEventObject[] analyticsList = JsonConvert.DeserializeObject<AnalyticsEventObject[]>(responseString);

        Debug.Log(responseString);

        Debug.Log(analyticsList[0].Message);

       _analyticsMenuCreator.RefreshList(analyticsList);
    }
}
