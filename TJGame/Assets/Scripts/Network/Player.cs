using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Guid _guid;

    /*private void Start()
    {
        if (!GetPlayerFromFiles())
        {
            // ask for player name...
        }
    }*/

    public static bool GetPlayerFromFiles()
    {
        if (GameStatus._status.HasGameSaved())
        {
            _guid = GameStatus._status.LoadData()._guid;
            return true;
        }
        return false;
    }

    public static void SavePlayerToFiles()
    {
        var saveData = new GameSaveData();
        saveData._isRelevant = true;
        saveData._guid = _guid;
        GameStatus._status.SaveData(saveData);
    }

    public async static void CreatePlayer(string name)
    {
        await DBCreatePlayer(name);
        SavePlayerToFiles();
    }

    public static void RemovePlayer()
    {
        GameStatus._status.ResetData();
    }

    #region DB

    public async static Task DBCreatePlayer(string name)
    {
        var response = await Client._instance._httpClient.PostAsync($"{Constants.apiAddress}api/players/create?name={name}", null);
        var responseString = await response.Content.ReadAsStringAsync();

        var player = JsonConvert.DeserializeObject<PlayerObject>(responseString);
        _guid = player.Id;
    }

    // TODO: Not implemented
    public async static void DBRemovePlayer()
    {
    }

    #endregion
}
