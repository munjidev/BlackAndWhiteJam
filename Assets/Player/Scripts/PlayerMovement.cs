using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Quaternion TargetRotation { private set; get; }
    public int deaths;
    
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
    private bool _canPlay;
    
    private GameObject _gameOverScreen;
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
        _gameOverScreen = GameObject.Find("GameOverScreen");
        _gameOverScreen.SetActive(false);
        _camera = Camera.main;
        TargetRotation = transform.rotation;
        _distToGround = GetComponent<Collider>().bounds.extents.y;
        _canPlay = true;
    }

    private void Update()
    {
        if (!_canPlay) return;
        
        GatherInput();
    }

    private void FixedUpdate()
    {
        if (!_canPlay) return;
        
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
        // Create ray from transform position to ground
        Debug.DrawRay(transform.position, Vector3.down, Color.magenta);

        // If jumping was performed and the player isn't already jumping
        if (!_isJumping && _isGrounded && _isJumpPressed)
        {
            _isJumping = true;
            _isGrounded = false;

            // Lerp movementSpeed to 1.5f
            _movementSpeed = Mathf.Lerp(_movementSpeed, 1.5f, 0.5f);
            
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
        bool raycast = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo,
            _distToGround + 0.1f);
        
        if (raycast)
        {
            _isGrounded = true;
            _isJumping = false;
            _isJumpPressed = false;
            _movementSpeed = 3.0f;
        }

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            transform.position = new Vector3(1.5f, -6.35f, 8.5f);
            // Automatically press spacebar
            _isJumpPressed = true;
            deaths++;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "EndTrigger")
        {
            // Find GameOverScreen and set active
            _gameOverScreen.SetActive(true);
            _canPlay = false;
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
