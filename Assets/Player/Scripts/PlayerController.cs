using UnityEngine;

namespace Player.Scripts
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private float _movementSpeed = 5f;
        [SerializeField] private float _rotationSpeed = 360f;
        private Vector3 _input;

        private void Update()
        {
            GatherInput();
            LookDirection();
        }

        private void FixedUpdate()
        {
            MovePlayer();
        }

        /// <summary>
        /// Returns horizontal and vertical player input with no smoothing filtering applied,
        /// which will always be either -1, 0 or 1. 
        /// </summary>
        private void GatherInput()
        {
            _input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        }

        private void MovePlayer()
        {
            _rb.MovePosition(transform.position +
                             _input.ToIsometric() * (_input.normalized.magnitude * _movementSpeed * Time.deltaTime));
        }

        private void LookDirection()
        {
            // Stop rotation if no input is gathered
            if (_input == Vector3.zero) return;
            // Find relative angle between player and input direction
            var relative = (transform.position + _input.ToIsometric()) - transform.position;
            // Rotate around the up axis
            var rot = Quaternion.LookRotation(relative, Vector3.up);
            // Perform smooth accounting for rotation speed
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, _rotationSpeed * Time.deltaTime);
        }
    }
}