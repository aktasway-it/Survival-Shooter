using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LivingEntity : MonoBehaviour, IDamageable
{
    public event Action OnDeath;

    public float Health { get; protected set; }
    public bool IsAlive { get { return Health > 0; } }

    [SerializeField]
    protected float _startingHealth;

    [SerializeField]
    protected ParticleSystem _deathEffectPrefab;


    protected virtual void Start()
    {
        Health = _startingHealth;
    }

    public void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        Health -= damage;
        if (Health <= 0)
        {
            if (_deathEffectPrefab != null)
            {
                ParticleSystem deathEffect = Instantiate(_deathEffectPrefab, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)) as ParticleSystem;
                Destroy(deathEffect.gameObject, deathEffect.main.startLifetime.constantMax);
            }
            Die();
        }
    }

    protected virtual void Die()
    {
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}
