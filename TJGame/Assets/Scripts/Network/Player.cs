using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Assertions;

public class Player : MonoBehaviour
{
    public static Guid _guid;
    public static UIManager _uiManager;

    private void Awake()
    {
        _uiManager = GameObject.FindObjectOfType<UIManager>();
        Assert.IsNotNull(_uiManager);   
    }

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
        var response = await PostAsync($"{Constants.apiAddress}api/players/create?name={name}", null);
        var responseString = await response.Content.ReadAsStringAsync();

        var player = JsonConvert.DeserializeObject<PlayerObject>(responseString);
        _guid = player.Id;
    }

    // TODO: Not implemented
    public async static void DBRemovePlayer()
    {
    }

    #endregion

    #region HttpWrappers

    public static string Encode(string input, byte[] key)
    {
        using (var myhmacsha1 = new HMACSHA256(key))
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(input.ToLower());
            byte[] hash = myhmacsha1.ComputeHash(byteArray);
            return Convert.ToBase64String(hash);
        }
    }

    public async static Task<HttpResponseMessage> PostAsync(string url, object body)
    {
        // Convert body to bytes
        var content = JsonConvert.SerializeObject(body);
        var buffer = Encoding.UTF8.GetBytes(content);
        var byteContent = new ByteArrayContent(buffer);
        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        // Create signature
        var timeString = DateTime.UtcNow.ToString("dd/MM/yyyy H:mm:ss");
        var stringToEncrypt = url + timeString + content;
        byteContent.Headers.Add("Signature", Encode(stringToEncrypt, Constants.secret));
        byteContent.Headers.Add("TimeStamp", timeString);

        return await Client._instance._httpClient.PostAsync(url, byteContent);
    }

    #endregion
}
