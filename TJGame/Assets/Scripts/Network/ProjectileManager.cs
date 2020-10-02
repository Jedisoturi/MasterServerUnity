using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public int _id;
    public GameObject _explosionPrefab;

    public void Initialize(int id)
    {
        _id = id;
    }

    public void Explode(Vector3 position)
    {
        transform.position = position;
        Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        GameManager._projectiles.Remove(_id);
        Destroy(gameObject);
    }
}
