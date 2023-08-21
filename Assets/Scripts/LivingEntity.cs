using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LivingEntity : MonoBehaviour, IDamageable
{
    public float Health { get; protected set; }
    public bool IsAlive { get { return Health > 0; } }
    
    [SerializeField]
    protected float _startingHealth;
    

    protected virtual void Start()
    {
        Health = _startingHealth;
    }

    public void TakeHit(float damage, RaycastHit hit)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}
