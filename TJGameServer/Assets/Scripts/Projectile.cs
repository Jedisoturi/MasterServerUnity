using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public static Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>();
    private static int _nextProjectileId = 1;

    public int _id;
    public Rigidbody _rigidbody;
    public int _throwByPlayer;
    public Vector3 _initialForce;
    public float _explosionRadius = 1.5f;
    public float _explosionDamage = 75f;

    private void Start()
    {
        _id = _nextProjectileId;
        _nextProjectileId++;
        _projectiles.Add(_id, this);

        ServerSend.SpawnProjectile(this, _throwByPlayer);

        _rigidbody.AddForce(_initialForce);
        StartCoroutine(ExplodeAfterTime());
    }

    private void FixedUpdate()
    {
        ServerSend.ProjectilePosition(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    public void Initialize(Vector3 initialMovementDirection, float initialForceStrength, int thrownByPlayer)
    {
        _initialForce = initialMovementDirection * initialForceStrength;
        _throwByPlayer = thrownByPlayer;
    }

    private void Explode()
    {
        ServerSend.ProjectileExploded(this);

        Collider[] colliders = Physics.OverlapSphere(transform.position, _explosionRadius);

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.CompareTag("Player"))
            {
                collider.GetComponentInParent<Player>().TakeDamage(_explosionDamage);
            }
        }

        _projectiles.Remove(_id);
        Destroy(gameObject);
    }

    private IEnumerator ExplodeAfterTime()
    {
        yield return new WaitForSeconds(10f);

        Explode();
    }
}
