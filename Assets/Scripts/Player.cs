using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity
{
    private PlayerController _controller;
    private GunController _gunController;

    private Camera _mainCamera;
    private bool _isShooting;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
        _gunController = GetComponent<GunController>();
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        UpdateRotation();
        if (_isShooting)
            _gunController.Shoot();
    }

    private void UpdateRotation()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            _controller.LookAt(new Vector3(point.x, transform.position.y, point.z));
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
            _isShooting = true;
        }
        else if (context.canceled)
        {
            _isShooting = false;
        }
    }
}
