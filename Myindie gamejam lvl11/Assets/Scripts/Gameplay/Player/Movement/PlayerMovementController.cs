using UnityEngine;

namespace Game.Gameplay.Player.Movement
{
    public sealed class PlayerMovementController
    {
        private readonly float _maxSpeed;
        private readonly float _acceleration;
        private readonly float _deceleration;

        public PlayerMovementController(float maxSpeed, float acceleration, float deceleration)
        {
            _maxSpeed = maxSpeed;
            _acceleration = acceleration;
            _deceleration = deceleration;
        }

        public Vector2 CalculateNextVelocity(
            Vector2 currentVelocity,
            Vector2 inputDirection,
            float deltaTime)
        {
            Vector2 normalizedInput = Vector2.ClampMagnitude(inputDirection, 1f);
            Vector2 targetVelocity = normalizedInput * _maxSpeed;

            float speedChange = normalizedInput.sqrMagnitude > 0.001f
                ? _acceleration
                : _deceleration;

            return Vector2.MoveTowards(
                currentVelocity,
                targetVelocity,
                speedChange * deltaTime);
        }
    }
}
