using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float _speed = 5f;

    private Rigidbody _rigidbody;
    private Vector3 _moveDirection;
    private Camera _mainCamera;

    private void Start()
    {
        _mainCamera = Camera.main;
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void Move(Vector3 direction)
    {
        _moveDirection = direction;
    }

    public void LookAt(Vector3 point)
    {
        transform.LookAt(new Vector3(point.x, transform.position.y, point.z));
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        _rigidbody.MovePosition(_rigidbody.position + _moveDirection * _speed * Time.deltaTime);
    }    
}
