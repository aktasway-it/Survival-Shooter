using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : LivingEntity
{
    public enum State { Idle, Chasing, Attacking };
    [SerializeField]
    private float _speed = 3f;
    [SerializeField]
    private float _refreshPathRate = 1f;

    [SerializeField]
    private float _attackDistanceThreshold = 1.5f;
    [SerializeField]
    private float _timeBetweenAttacks = 1f;
    [SerializeField]
    private float _attackSpeed = 3f;
    [SerializeField]
    private Color _attackColor = Color.red;

    private float _nextAttackTime;

    private NavMeshAgent _pathfinder;
    private Transform _target;
    private Material _skinMaterial;

    private State _currentState = State.Chasing;


    protected override void Start()
    {
        base.Start();

        _pathfinder = GetComponent<NavMeshAgent>();
        _pathfinder.speed = _speed;
        _target = GameObject.FindGameObjectWithTag("Player").transform;
        _skinMaterial = GetComponent<Renderer>().material;

        StartCoroutine(UpdatePath());
    }

    private void Update()
    {
        UpdateRotation();
        if (CanAttack())
            StartCoroutine(Attack());
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private void UpdateRotation()
    {
        if (_target != null && _currentState == State.Chasing)
        {
            Vector3 lookAtDirection = Vector3.Lerp(transform.position + transform.forward, transform.position + _pathfinder.velocity, Time.deltaTime);
            transform.LookAt(lookAtDirection);
        }
    }

    private bool CanAttack()
    {
        if (_target == null || _currentState == State.Attacking)
            return false;

        if (Time.time >= _nextAttackTime)
        {
            float sqrDistance = (_target.position - transform.position).sqrMagnitude;
            return sqrDistance <= (_attackDistanceThreshold * _attackDistanceThreshold);
        }

        return false;
    }

    private IEnumerator Attack()
    {
        _currentState = State.Attacking;
        _nextAttackTime = Time.time + _timeBetweenAttacks;

        Vector3 originalPosition = transform.position;
        Vector3 directionToTarget = (_target.position - transform.position).normalized;
        Vector3 attackPosition = _target.position - directionToTarget * 0.5f;

        Color originalColor = _skinMaterial.color;

        float percent = 0f;

        while (percent <= 1f)
        {
            percent += Time.deltaTime * _attackSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4f;
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);
            transform.LookAt(_target.position);

            _skinMaterial.color = Color.Lerp(originalColor, _attackColor, interpolation);

            yield return null;
        }

        _skinMaterial.color = originalColor;
        _currentState = State.Chasing;
    }

    private IEnumerator UpdatePath()
    {
        float timer = _refreshPathRate;
        while (_target != null)
        {
            if (_currentState != State.Chasing)
                yield return null;

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
