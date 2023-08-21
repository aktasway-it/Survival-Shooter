using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float Speed { get => _speed; set => _speed = value;}

    [SerializeField]
    private float _speed = 10f;

    private Rigidbody _rigidbody;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        StartCoroutine(DeactivateAfterSeconds(3f));
    }

    private void FixedUpdate()
    {
        _rigidbody.MovePosition(_rigidbody.position + transform.forward * _speed * Time.deltaTime);
    }

    private IEnumerator DeactivateAfterSeconds(float seconds)
    {
        float timer = 0f;
        while (timer < seconds)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        BulletSpawner.Instance.Return(this);
    }
}
