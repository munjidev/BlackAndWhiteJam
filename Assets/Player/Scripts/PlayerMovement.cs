using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder.MeshOperations;

public class PlayerMovement : MonoBehaviour
{
    public Quaternion TargetRotation { private set; get; }
    private PlayerInput _playerInput;
    private CharacterController _characterController;
    
    // Store player input values
    private Vector2 _currentMovementInput;
    private Vector3 _currentMovement;
    private bool _isMovementPressed;
    
    // Store mouse position
    private Vector2 _mousePositionInput;
    private Vector3 _mousePosition;
    private bool _isMouseActive;
    
    private Camera _camera;

    [SerializeField] private float rotationPerFrame = 360.0f;

    private void Awake()
    {
        // Access action maps
        _playerInput = new PlayerInput();

        _characterController = GetComponent<CharacterController>();
        
        _camera = Camera.main;
        TargetRotation = transform.rotation;
        
        // Set callbacks to listen for mapped actions
        
        // Track and update movement input
        _playerInput.CharacterControls.Move.started += OnMovementInput;
        _playerInput.CharacterControls.Move.canceled += OnMovementInput;
        _playerInput.CharacterControls.Move.performed += OnMovementInput;
        
        // Track and update mouse input
        _playerInput.CharacterControls.Move.started += OnMouseInput;
        _playerInput.CharacterControls.Move.canceled += OnMouseInput;
        _playerInput.CharacterControls.Mouse.performed += OnMouseInput;
    }

    private void Update()
    {
        HandleGravity();
        HandleRotation();
        _characterController.Move(_currentMovement * Time.deltaTime);
    }

    private void OnEnable()
    {
        _playerInput.CharacterControls.Enable();
    }

    private void OnDisable()
    {
        _playerInput.CharacterControls.Disable();
    }

    private void OnMovementInput(InputAction.CallbackContext ctx)
    {
        _currentMovementInput = ctx.ReadValue<Vector2>();
        _currentMovement.x = _currentMovementInput.x;
        _currentMovement.z = _currentMovementInput.y;
        _isMovementPressed = _currentMovementInput.x != 0 || _currentMovementInput.y != 0;
    }
    
    private void OnMouseInput(InputAction.CallbackContext ctx)
    {
        // Convert mouse input from screen space to world space
        _mousePositionInput = ctx.ReadValue<Vector2>();
        _mousePosition.x = _mousePositionInput.x;
        _mousePosition.z = _mousePositionInput.y;

        if (ctx.started || ctx.performed)
        {
            _isMouseActive = true;
        }
        else if (ctx.canceled)
        {
            _isMouseActive = false;
        }
    }

    private void HandleRotation()
    {
        Vector3 lookDirection;
        Quaternion rotation = transform.rotation;
        
        // Rotate the player towards look direction
        if (_isMovementPressed)
        {
            // Change towards position the player should point towards
            lookDirection.x = _currentMovement.x;
            lookDirection.y = 0.0f;
            lookDirection.z = _currentMovement.z;
            
            // Perform spherical interpolation between current rotation and target rotation
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(rotation, targetRotation, rotationPerFrame * Time.deltaTime);
        }
        else if (_isMouseActive)
        {
            Ray ray = _camera.ScreenPointToRay(_mousePosition);
            
            if(Physics.Raycast(ray, out RaycastHit hit))
            {
                // Change in the direction the player is pointing towards
                lookDirection = hit.point - transform.position;
                lookDirection.y = 0.0f;
            
                // Perform spherical interpolation between current rotation and target rotation
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(rotation, targetRotation, rotationPerFrame * Time.deltaTime);
            }
        }
    }

    private void HandleGravity()
    {
        if (_characterController.isGrounded)
        {
            float groundedGravity = -0.05f;
            _currentMovement.y = groundedGravity;
        }
        else
        {
            float gravity = -9.81f * Time.deltaTime;
            _currentMovement.y += gravity;
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
