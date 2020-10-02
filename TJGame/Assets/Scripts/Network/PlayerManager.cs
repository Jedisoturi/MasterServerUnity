using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int _id;
    public string _username;
    public float _health;
    public float _maxHealth;
    public int _itemCount = 0;
    public MeshRenderer _model;

    public void Initialize(int id, string username)
    {
        _id = id;
        _username = username;
        _health = _maxHealth;
    }

    public void SetHealth(float health)
    {
        _health = health;

        if (health <= 0f)
        {
            Die();
        }

    }

    public void Die()
    {
        _model.enabled = false;
    }

    public void Respawn()
    {
        _model.enabled = true;
        SetHealth(_maxHealth);
    }
}
