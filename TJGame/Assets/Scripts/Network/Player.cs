using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public static Guid _guid;

    public static void CreatePlayer(string name)
    {
        DBCreatePlayer(name);
    }

    #region DB

    public async static void DBCreatePlayer(string name)
    {
        var response = await Client._instance._httpClient.PostAsync($"{Constants.apiAddress}api/players/create?name={name}", null);
        var responseString = await response.Content.ReadAsStringAsync();

        var player = JsonConvert.DeserializeObject<PlayerObject>(responseString);
        _guid = player.Id;
    }

    #endregion
}
