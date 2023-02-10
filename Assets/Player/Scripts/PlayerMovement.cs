using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Quaternion TargetRotation { private set; get; }
    
    [SerializeField] private float _movementSpeed = 3.0f;
    [SerializeField] private float _rotationSpeed = 720.0f;
    [SerializeField] private float _jumpSpeed = 3.0f;
    [SerializeField] private Rigidbody _rb;
    
    private bool _isGrounded = true;
    private LayerMask _layerMask;
    private Vector3 _mouseInput;
    private Vector3 _input;
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
        _input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        _mouseInput = new Vector3(Input.GetAxisRaw("Mouse X"), 0, Input.GetAxisRaw("Mouse Y"));
        
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            HandleJumping();
        }
    }

    private void HandleJumping()
    {
        _rb.AddForce(Vector3.up * _jumpSpeed, ForceMode.Impulse);
        
        _isGrounded = false;
    }

    private void MovePlayer()
    {
        // If player is falling, reduce movement speed to 1f
        _movementSpeed = _rb.velocity.y < 0 ? 1.0f : 3.0f;
        
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
            
            if (Physics.Raycast(ray, out var hitInfo))
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
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = true;
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
