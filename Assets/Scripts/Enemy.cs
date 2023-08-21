using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : LivingEntity
{
    [SerializeField]
    private float _speed = 3f;
    [SerializeField]
    private float _refreshPathRate = 1f;

    private NavMeshAgent _pathfinder;
    private Transform _target;

    protected override void Start()
    {
        base.Start();

        _pathfinder = GetComponent<NavMeshAgent>();
        _pathfinder.speed = _speed;
        _target = GameObject.FindGameObjectWithTag("Player").transform;

        StartCoroutine(UpdatePath());
    }

    private void Update()
    {
        if (_target != null)
        {
            Vector3 lookAtDirection = Vector3.Lerp(transform.position + transform.forward, transform.position + _pathfinder.velocity, Time.deltaTime);
            transform.LookAt(lookAtDirection);
        }
    }

    private IEnumerator UpdatePath()
    {
        float timer = _refreshPathRate;
        while (_target != null)
        {
            while (timer < _refreshPathRate)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            if (!IsAlive)
                yield break;

            _pathfinder.SetDestination(_target.position);
            timer = 0f;
        }
    }
}
