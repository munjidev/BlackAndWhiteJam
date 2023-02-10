using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private float _movementSpeed = 5.0f;
    [SerializeField] private float _rotationSpeed = 720.0f;
    [SerializeField] private float _jumpSpeed = 5.0f;
    public bool isGrounded = true;

    public Quaternion TargetRotation { private set; get; }

    private LayerMask layerMask;

    private Vector3 _input;
    private Vector3 _mouseInput;

    private Camera _camera;

    private void Start()
    {
        _camera = Camera.main;
        TargetRotation = transform.rotation;
    }

    private void Update()
    {
        GatherInput();
    }

    private void FixedUpdate()
    {
        MovePlayer();
        LookDirection();
    }

    /// <summary>
    ///     Returns horizontal and vertical player input with no smoothing filtering applied.
    /// </summary>
    private void GatherInput()
    {
        // Gather movement
        _input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        // Gather mouse movement
        _mouseInput = new Vector3(Input.GetAxisRaw("Mouse X"), 0, Input.GetAxisRaw("Mouse Y"));
        // Gather jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            HandleJumping();
        }
    }

    private void HandleJumping()
    {
        _rb.AddForce(Vector3.up * _jumpSpeed, ForceMode.Impulse);
        isGrounded = false;
    }

    private void MovePlayer()
    {
        _rb.MovePosition(transform.position +
                         transform.forward * (_input.normalized.magnitude * _movementSpeed * Time.deltaTime));
    }

    private void LookDirection()
    {
        // If no keyboard input is detected, have player stand still
        if (_input != Vector3.zero)
        {
            // Perform smooth rotation accounting for rotation speed
            var rotation = Quaternion.LookRotation(_input.ToIsometric(), Vector3.up);
            transform.rotation =
                Quaternion.Slerp(transform.rotation, rotation, _rotationSpeed * Time.deltaTime);
        }
        // Else if mouse input is detected, have player look in that direction
        else if (_mouseInput != Vector3.zero)
        {
            // Attempt raycast to obtain look direction
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hitInfo))
            {
                var lookDirection = hitInfo.point - transform.position;
                lookDirection.y = 0;
                var rot = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, _rotationSpeed * Time.deltaTime);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    /// <summary>
    ///     Resets transform's rotation after warp.
    /// </summary>
    public void ResetTargetRotation()
    {
        TargetRotation = Quaternion.LookRotation(transform.forward, Vector3.up);
    }
}
