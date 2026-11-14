using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Gameplay.Player.Movement
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMovementBootstrap : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionReference _moveAction;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float _maxSpeed = 5f;
        [SerializeField, Min(0f)] private float _acceleration = 18f;
        [SerializeField, Min(0f)] private float _deceleration = 12f;

        private Rigidbody2D _rigidbody;
        private PlayerMovementController _movementController;
        private Vector2 _moveInput;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();

            _movementController = new PlayerMovementController(
                _maxSpeed,
                _acceleration,
                _deceleration);
        }

        private void OnEnable()
        {
            if (_moveAction == null)
            {
                Debug.LogError("Player Movement: Move Action is not assigned.", this);
                enabled = false;
                return;
            }

            _moveAction.action.Enable();
        }

        private void OnDisable()
        {
            if (_moveAction != null)
            {
                _moveAction.action.Disable();
            }
        }

        private void Update()
        {
            _moveInput = _moveAction.action.ReadValue<Vector2>();
        }

        private void FixedUpdate()
        {
            _rigidbody.linearVelocity = _movementController.CalculateNextVelocity(
                _rigidbody.linearVelocity,
                _moveInput,
                Time.fixedDeltaTime);
        }
    }
}
