using UnityEngine;

namespace Game.Gameplay.Heat
{
    /// <summary>
    /// Ignites once on Player contact and provides heat while the Player stays
    /// inside its trigger, for a limited burn duration.
    /// The GameObject must have a Collider2D with Is Trigger enabled.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class FlammableHeatSource : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float _burnDuration = 4f;
        [SerializeField, Min(0f)] private float _heatPerSecond = 12f;

        private float _remainingBurnTime;
        private bool _hasIgnited;
        private bool _isBurning;
        private bool _hasReportedHeatTransfer;

        public bool IsIgnited => _hasIgnited;
        public bool IsBurning => _isBurning;

        private void Update()
        {
            if (!_isBurning)
            {
                return;
            }

            _remainingBurnTime -= Time.deltaTime;
            if (_remainingBurnTime <= 0f)
            {
                _remainingBurnTime = 0f;
                _isBurning = false;
                Debug.Log("[FIRE DEBUG] Источник догорел и больше не восстанавливает огонь.", this);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out PlayerHeatBootstrap _))
            {
                return;
            }

            IgniteOnce();
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!_isBurning || !other.TryGetComponent(out PlayerHeatBootstrap playerHeat))
            {
                return;
            }

            if (!_hasReportedHeatTransfer)
            {
                _hasReportedHeatTransfer = true;
                Debug.Log($"[FIRE DEBUG] Игрок получает тепло от источника: +{_heatPerSecond:0.#}/сек.", this);
            }

            playerHeat.RestoreHeat(_heatPerSecond * Time.deltaTime);
        }

        private void IgniteOnce()
        {
            if (_hasIgnited)
            {
                return;
            }

            _hasIgnited = true;
            _isBurning = true;
            _remainingBurnTime = _burnDuration;
            Debug.Log($"[FIRE DEBUG] Источник загорелся! Время горения: {_burnDuration:0.#} сек.", this);
        }
    }
}
