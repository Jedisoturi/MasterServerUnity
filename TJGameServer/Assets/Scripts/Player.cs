using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int _id;
    public string _username;
    public CharacterController _controller;
    public Transform _shootOrigin;
    public float _gravity = -9.81f;
    public float _moveSpeed = 5f;
    public float _jumpSpeed = 5f;
    public float _throwForce = 600f;
    public float _health;
    public float _maxHealth = 100f;
    public int _itemAmount = 0;
    public int _maxItemAmount = 3;

    private bool[] _inputs;
    private float _yVelocity = 0;

    private void Start()
    {
        _gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        _moveSpeed *= Time.fixedDeltaTime;
        _jumpSpeed *= Time.fixedDeltaTime;
    }

    public void Initialize(int id, string username)
    {
        _id = id;
        _username = username;
        _health = _maxHealth;

        _inputs = new bool[5];
    }

    public void FixedUpdate()
    {
        if (_health <= 0f) return;

        var inputDirection = Vector2.zero;
        if (_inputs[0]) inputDirection.y += 1;
        if (_inputs[1]) inputDirection.y -= 1;
        if (_inputs[2]) inputDirection.x -= 1;
        if (_inputs[3]) inputDirection.x += 1;

        Move(inputDirection);
    }

    private void Move(Vector2 inputDirection)
    {
        Vector3 moveDirection = transform.right * inputDirection.x + transform.forward * inputDirection.y;
        moveDirection *= _moveSpeed;

        if (_controller.isGrounded)
        {
            _yVelocity = 0;
            if (_inputs[4])
            {
                _yVelocity = _jumpSpeed;
            }
        }
        _yVelocity += _gravity;

        moveDirection.y = _yVelocity;
        _controller.Move(moveDirection);

        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }

    public void SetInput(bool[] inputs, Quaternion rotation)
    {
        _inputs = inputs;
        transform.rotation = rotation;
    }

    public void Shoot(Vector3 viewDirection)
    {
        if (_health <= 0f) return;

        if (Physics.Raycast(_shootOrigin.position, viewDirection, out RaycastHit hit, 25f))
        {
            if (hit.collider.CompareTag("Player"))
            {
                hit.collider.GetComponentInParent<Player>().TakeDamage(50f);
            }
        }
    }

    public void ThrowItem(Vector3 viewDirection)
    {
        if (_health <= 0f) return;

        if (0 < _itemAmount)
        {
            _itemAmount--;
            NetworkManager._instance.InstantiateProjectile(_shootOrigin).Initialize(viewDirection, _throwForce, _id);
        }
    }

    public void TakeDamage(float damage)
    {
        if (_health <= 0f) return;

        _health -= damage;
        if (_health <= 0f)
        {
            _health = 0f;
            _controller.enabled = false;
            transform.position = new Vector3(0f, 25f, 0f);
            ServerSend.PlayerPosition(this);
            StartCoroutine(Respawn());
        }

        ServerSend.PlayerHealth(this);
    }

    public IEnumerator Respawn()
    {
        yield return new WaitForSeconds(5f);

        _health = _maxHealth;
        _controller.enabled = true;
        ServerSend.PlayerRespawned(this);
    }

    public bool AttemptPickupItem()
    {
        if (_maxItemAmount <= _itemAmount) return false;

        ++_itemAmount;
        return true;
    }
}
