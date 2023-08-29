using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : LivingEntity
{
    private bool HasTarget { get { return _target != null; } }

    public enum State { Idle, Chasing, Attacking };
    [SerializeField]
    private float _speed = 3f;
    
    [SerializeField]
    private float _damage = 1f;
    
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

    [SerializeField]
    private AudioClip _attackSfx;

    [SerializeField]
    private AudioClip[] _hitSfx;

    private float _nextAttackTime;

    private NavMeshAgent _pathfinder;
    private Transform _target;
    private LivingEntity _targetEntity;

    private State _currentState = State.Chasing;


    protected override void Start()
    {
        base.Start();

        _pathfinder = GetComponent<NavMeshAgent>();
        _pathfinder.speed = _speed;

        GameObject targetObject = GameObject.FindGameObjectWithTag("Player");
        if (targetObject != null)
        {
            _target = GameObject.FindGameObjectWithTag("Player").transform;
            _targetEntity = _target.GetComponent<LivingEntity>();
            _targetEntity.OnDeath += OnTargetDeath;

            StartCoroutine(UpdatePath());
        }
    }

    private void Update()
    {
        if (!HasTarget)
            return;

        UpdateRotation();
        if (CanAttack())
            StartCoroutine(Attack());
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private void OnTargetDeath()
    {
        _targetEntity.OnDeath -= OnTargetDeath;
        _target = null;
        _targetEntity = null;
        _currentState = State.Idle;
    }

    private void UpdateRotation()
    {
        if (!HasTarget && _currentState == State.Chasing)
        {
            Vector3 lookAtDirection = Vector3.Lerp(transform.position + transform.forward, transform.position + _pathfinder.velocity, Time.deltaTime);
            transform.LookAt(lookAtDirection);
        }
    }

    private bool CanAttack()
    {
        if (!HasTarget || _currentState == State.Attacking)
            return false;

        if (Time.time >= _nextAttackTime)
        {
            float sqrDistance = (_target.position - transform.position).sqrMagnitude;
            return sqrDistance <= (_attackDistanceThreshold * _attackDistanceThreshold);
        }

        return false;
    }

    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        base.TakeHit(damage, hitPoint, hitDirection);

        if (IsAlive)
            AudioManager.Instance.Play(_hitSfx[Random.Range(0, _hitSfx.Length)], audioMixerGroup: _audioMixerGroup, position: hitPoint, maxDistance: 20f);
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
        bool hasAppliedDamage = false;

        AudioManager.Instance.Play(_attackSfx, audioMixerGroup: _audioMixerGroup, position: transform.position);

        while (percent <= 1f)
        {
            if (percent >= 0.5f && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                if (HasTarget)
                    _targetEntity.TakeHit(_damage, _targetEntity.transform.position, transform.forward);
            }

            percent += Time.deltaTime * _attackSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4f;
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);

            if (HasTarget)
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
        while (HasTarget)
        {
            if (_currentState == State.Chasing)
            {
                while (timer < _refreshPathRate)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }

                if (!IsAlive)
                    yield break;

                if (HasTarget)
                    _pathfinder.SetDestination(_target.position);
                    
                timer = 0f;
            }
            else
            {
                yield return null;
            }
        }
    }
}
