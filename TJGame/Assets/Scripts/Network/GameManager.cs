using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager _instance;

    public static Dictionary<int, PlayerManager> _players = new Dictionary<int, PlayerManager>();
    public static Dictionary<int, ItemSpawner> _itemSpawners = new Dictionary<int, ItemSpawner>();
    public static Dictionary<int, ProjectileManager> _projectiles = new Dictionary<int, ProjectileManager>();

    public GameObject _localPlayerPrefab;
    public GameObject _playerPrefab;
    public GameObject _itemSpawnerPrefab;
    public GameObject _projectilePrefab;

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

    public void SpawnPlayer(int id, string username, Vector3 position, Quaternion rotation)
    {
        GameObject player;
        if (id == Client._instance._myId)
        {
            player = Instantiate(_localPlayerPrefab, position, rotation);
        }
        else
        {
            player = Instantiate(_playerPrefab, position, rotation);
        }

        player.GetComponent<PlayerManager>().Initialize(id, username);
        _players.Add(id, player.GetComponent<PlayerManager>());
    }

    public void CreateItemSpawner(int spawnerId, Vector3 position, bool hasItem)
    {
        GameObject spawner = Instantiate(_itemSpawnerPrefab, position, _itemSpawnerPrefab.transform.rotation);
        spawner.GetComponent<ItemSpawner>().Initialize(spawnerId, hasItem);
        _itemSpawners.Add(spawnerId, spawner.GetComponent<ItemSpawner>());
    }

    public void SpawnProjectile(int id, Vector3 position)
    {
        GameObject projectile = Instantiate(_projectilePrefab, position, Quaternion.identity);
        projectile.GetComponent<ProjectileManager>().Initialize(id);
        _projectiles.Add(id, projectile.GetComponent<ProjectileManager>());
    }
}
