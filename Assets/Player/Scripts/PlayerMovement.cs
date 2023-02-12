using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Quaternion TargetRotation { private set; get; }
    
    [SerializeField] private Rigidbody _rb;
    
    [SerializeField] private float _movementSpeed = 3.0f;
    [SerializeField] private float _rotationSpeed = 360.0f;
    
    [SerializeField] private float _jumpHeight = 1.0f;
    [SerializeField] private float _jumpTime = 0.75f;

    [SerializeField] private float _gravityGrounded = -0.05f;
    [SerializeField] private float _gravity = -9.81f;

    private float _jumpVelocity;
    private float _distToGround;
    private bool _isJumpPressed;
    private bool _isGrounded;
    private bool _isJumping;
    
    private LayerMask _layerMask;
    private Vector3 _mouseInput;
    private Vector3 _input;
    private Camera _camera;

    private void Awake()
    {
        SetJumpValues();
    }

    private void Start()
    {
        _camera = Camera.main;
        TargetRotation = transform.rotation;
        _distToGround = GetComponent<Collider>().bounds.extents.y;
    }

    private void Update()
    {
        GatherInput();
    }

    private void FixedUpdate()
    {
        MovePlayer();
        LookDirection();
        HandleGravity();
        HandleJump();
    }

    /// <summary>
    ///     Returns horizontal and vertical player input with no smoothing filtering applied.
    /// </summary>
    private void GatherInput()
    {
        _input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        _mouseInput = new Vector3(Input.GetAxisRaw("Mouse X"), 0, Input.GetAxisRaw("Mouse Y"));

        if (Input.GetButtonDown("Jump"))
        {
            _isJumpPressed = true;
        }
    }

    private void SetJumpValues()
    {
        float timeToApex = _jumpTime / 2;
        _gravity = (-2 * _jumpHeight) / Mathf.Pow(timeToApex, 2);
        _jumpVelocity = (2 * _jumpHeight) / timeToApex;
    }
    
    private void HandleGravity()
    {
        float rbYVelocity = _rb.velocity.y;
        bool isFalling = rbYVelocity <= 0.0f || !_isJumpPressed;
        float fallMultiplier = 2.0f;

        // Apply realistic gravity with Verlet integration
        if (_isGrounded)
        {
            rbYVelocity = _gravityGrounded;
        }
        else if (isFalling)
        {
            float previousYVelocity = rbYVelocity;
            float newYVelocity = rbYVelocity + _gravity * fallMultiplier * Time.deltaTime;
            float nextYVelocity = (previousYVelocity + newYVelocity) * 0.5f;
            rbYVelocity = nextYVelocity;
        }
        else
        {
            float previousYVelocity = rbYVelocity;
            float newYVelocity = rbYVelocity + _gravity * Time.deltaTime;
            float nextYVelocity = (previousYVelocity + newYVelocity) * 0.5f;
            rbYVelocity = nextYVelocity;
        }
    }
    
    private void HandleJump()
    {
        Debug.DrawRay(transform.position - new Vector3(0f, 0.5f, 0f), Vector3.down, Color.green);
        
        // If jumping was performed and the player isn't already jumping
        if (!_isJumping && _isGrounded && _isJumpPressed)
        {
            _isJumping = true;
            _isGrounded = false;
            
            // _rb.AddForce(new Vector3(0f, _jumpVelocity, 0f), ForceMode.VelocityChange);
            _rb.AddForce(new Vector3(0.0f, _jumpVelocity * 0.5f, 0.0f), ForceMode.VelocityChange);
        }
        else if (!_isJumpPressed && _isJumping && _isGrounded)
        {
            _isJumping = false;
        }
    }
    
    private void MovePlayer()
    {
        Transform playerTransform = transform;

        _rb.MovePosition(playerTransform.position +
                         playerTransform.forward * (_input.normalized.magnitude * _movementSpeed * Time.deltaTime));
    }

    private void LookDirection()
    {
        // If no keyboard input is detected, have player stand still
        if (_input != Vector3.zero)
        {
            // Perform smooth rotation accounting for rotation speed
            Quaternion rotation = Quaternion.LookRotation(_input.ToIsometric(), Vector3.up);
            
            transform.rotation =
                Quaternion.Slerp(transform.rotation, rotation, _rotationSpeed * Time.deltaTime);
        }
        // Else if mouse input is detected, have player look in that direction
        else if (_mouseInput != Vector3.zero)
        {
            // Attempt raycast to obtain look direction
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                Vector3 lookDirection = hitInfo.point - transform.position;
                lookDirection.y = 0;
                
                Quaternion rot = Quaternion.LookRotation(lookDirection);
                
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, _rotationSpeed * Time.deltaTime);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Physics.Raycast(transform.position, Vector3.down, _distToGround + 0.1f))
        {
            _isGrounded = true;
            _isJumping = false;
            _isJumpPressed = false;
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
