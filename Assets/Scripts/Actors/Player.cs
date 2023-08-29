using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity
{
    public bool IsCamping { get; private set; }

    [SerializeField]
    private Crosshair _crosshair;

    private PlayerController _controller;
    private GunController _gunController;

    private Camera _mainCamera;
    private float _campingCheckTimer;
    private Vector3 _lastCampingCheckPosition;

    protected override void Start()
    {
        base.Start();

        _controller = GetComponent<PlayerController>();
        _gunController = GetComponent<GunController>();
        _mainCamera = Camera.main;

        _lastCampingCheckPosition = transform.position;

        EnemySpawner.OnNewWave += OnNewWave;
    }

    private void OnDestroy()
    {
        EnemySpawner.OnNewWave -= OnNewWave;
    }

    private void OnNewWave(int waveNumber)
    {
        Health = _startingHealth;
        _lastCampingCheckPosition = transform.position;
        _campingCheckTimer = 0;
    }

    private void Update()
    {
        CheckCamping();
        UpdateRotation();
    }

    private void CheckCamping()
    {
        _campingCheckTimer += Time.deltaTime;
        if (_campingCheckTimer > 3f)
        {
            _campingCheckTimer = 0;
            IsCamping = (_lastCampingCheckPosition - transform.position).sqrMagnitude < 2;
            _lastCampingCheckPosition = transform.position;
        }
    }

    private void UpdateRotation()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            Vector3 lookAtPoint = new Vector3(point.x, transform.position.y, point.z);
            _controller.LookAt(lookAtPoint);
            _crosshair.transform.position = lookAtPoint;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 inputDirection = Vector2.zero;
        if (context.started || context.performed)
            inputDirection = context.ReadValue<Vector2>();
        else if (context.canceled)
            inputDirection = Vector2.zero;

        Vector3 moveDirection = new Vector3(inputDirection.x, 0, inputDirection.y);
        _controller.Move(moveDirection);
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _gunController.OnTriggerHold();
        }
        else if (context.canceled)
        {
            _gunController.OnTriggerRelease();
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _gunController.Reload();
        }
    }
}
