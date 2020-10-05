using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages current game save status
/// </summary>
public class GameStatus : MonoBehaviour
{
    public static GameStatus _status;

    // Start is called before the first frame update
    void Awake()
    {
        if (_status == null)
        {
            DontDestroyOnLoad(gameObject);
            _status = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Save feeded data as game save
    /// </summary>
    /// <param name="data"></param>
    public void SaveData(GameSaveData data)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(GameSaveFilePath());

        bf.Serialize(file, data);
        file.Close();
    }

    /// <summary>
    /// Load saved game data and return it to whom it may concern
    /// </summary>
    /// <returns></returns>
    public GameSaveData LoadData()
    {
        if (File.Exists(GameSaveFilePath()))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(GameSaveFilePath(), FileMode.Open);
            GameSaveData data = (GameSaveData)bf.Deserialize(file);
            file.Close();

            if (data._isRelevant)
                return data;
            else
                return null;
        }
        return null;
    }

    /// <summary>
    /// Overrides saved game with isRelevant = false
    /// </summary>
    public void ResetData()
    {
        GameSaveData data = new GameSaveData();
        data._isRelevant = false;
        SaveData(data);
    }

    /// <summary>
    /// Return path for saved file
    /// </summary>
    /// <returns></returns>
    private string GameSaveFilePath()
    {
        return Application.persistentDataPath + "/playerInfo.dat";
    }

    public bool HasGameSaved()
    {
        GameSaveData data = LoadData();
        if (data != null)
            if (data._isRelevant)
                return true;
        return false;
    }
}

[Serializable]
public class GameSaveData
{
    public bool _isRelevant;
    public Guid _guid;
}
