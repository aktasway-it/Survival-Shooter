using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float Speed { get => _speed; set => _speed = value;}

    [SerializeField]
    private float _damage = 1f;

    [SerializeField]
    private float _speed = 10f;

    [SerializeField]
    private LayerMask _collisionMask;

    private Coroutine _deactivateCoroutine;
    private TrailRenderer _trailRenderer;

    private void Start()
    {
        _trailRenderer = GetComponent<TrailRenderer>();
    }

    private void OnEnable()
    {
        _deactivateCoroutine = StartCoroutine(DeactivateAfterSeconds(3f));
    }

    private void Update()
    {
        float moveDistance = _speed * Time.deltaTime;
        CheckForCollision(moveDistance);
        transform.Translate(moveDistance * Vector3.forward);
    }

    private void CheckForCollisionsOnSpawn()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.1f, _collisionMask);
        if (colliders.Length > 0)
        {
            OnHit(colliders[0], transform.position);
        }
    }

    private void CheckForCollision(float moveDistance)
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, moveDistance, _collisionMask, QueryTriggerInteraction.Collide))
        {
            OnHit(hit.collider, hit.point);
        }
    }

    private void OnHit(Collider collider, Vector3 hitPoint)
    {
        IDamageable damageableObject = collider.GetComponent<IDamageable>();
        if (damageableObject != null)
        {
            damageableObject.TakeHit(_damage, hitPoint, transform.forward);
        }

        Dispose();
    }

    private void Dispose()
    {
        StopCoroutine(_deactivateCoroutine);
        BulletSpawner.Instance.Return(this);
        _trailRenderer.Clear();
    }

    private IEnumerator DeactivateAfterSeconds(float seconds)
    {
        yield return null;
        CheckForCollisionsOnSpawn();

        float timer = 0f;
        while (timer < seconds)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        Dispose();
    }
}
