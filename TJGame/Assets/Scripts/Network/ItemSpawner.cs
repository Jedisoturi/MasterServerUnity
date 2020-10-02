using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public int _spawnerId;
    public bool _hasItem;
    public MeshRenderer _itemModel;

    private Vector3 _basePosition;

    public void Initialize(int spawnerId, bool hasItem)
    {
        _spawnerId = spawnerId;
        _hasItem = hasItem;
        _itemModel.enabled = _hasItem;

        _basePosition = transform.position;
    }

    public void ItemSpawned()
    {
        _hasItem = true;
        _itemModel.enabled = true;
    }

    public void ItemPickedUp()
    {
        _hasItem = false;
        _itemModel.enabled = false;
    }
}
