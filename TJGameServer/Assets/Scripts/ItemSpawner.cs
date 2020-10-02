using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public static Dictionary<int, ItemSpawner> _spawners = new Dictionary<int, ItemSpawner>();
    private static int _nextSpawnerId = 1;

    public int _spawnerId;
    public bool _hasItem = false;

    private void Start()
    {
        _hasItem = false;
        _spawnerId = _nextSpawnerId;
        ++_nextSpawnerId;
        _spawners.Add(_spawnerId, this);

        StartCoroutine(SpawnItem());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasItem && other.CompareTag("Player"))
        {
            Debug.Log("Collision");
            Player player = other.GetComponentInParent<Player>();
            if (player.AttemptPickupItem())
            {
                ItemPickedUp(player._id);
            }
        }
    }

    private IEnumerator SpawnItem()
    {
        yield return new WaitForSeconds(10f);

        _hasItem = true;
        ServerSend.ItemSpawned(_spawnerId);
    }

    private void ItemPickedUp(int byPlayer)
    {
        _hasItem = false;
        ServerSend.ItemPickedUp(_spawnerId, byPlayer);

        StartCoroutine(SpawnItem());
    }
}
