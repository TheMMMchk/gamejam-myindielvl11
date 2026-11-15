using System;
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
        [Header("Heat")]
        [SerializeField, Min(0.1f)] private float _burnDuration = 4f;
        [SerializeField, Min(0f)] private float _heatPerSecond = 12f;

        [Header("Optional visuals")]
        [SerializeField] private GameObject _unlitVisual;
        [SerializeField] private GameObject _litVisual;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color _unlitColor = new Color(0.3f, 0.3f, 0.35f, 1f);
        [SerializeField] private Color _litColor = Color.white;

        private float _remainingBurnTime;
        private bool _hasIgnited;
        private bool _isBurning;
        private bool _hasReportedHeatTransfer;

        public bool IsIgnited => _hasIgnited;
        public bool IsBurning => _isBurning;

        /// <summary>Raised exactly once when this source is ignited for the first time.</summary>
        public event Action<FlammableHeatSource> Ignited;

        private void Awake()
        {
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();

            UpdateIgnitionVisual();
        }

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
            UpdateIgnitionVisual();
            Debug.Log($"[FIRE DEBUG] Источник загорелся! Время горения: {_burnDuration:0.#} сек.", this);
            Ignited?.Invoke(this);
        }

        private void UpdateIgnitionVisual()
        {
            if (_unlitVisual != null)
                _unlitVisual.SetActive(!_hasIgnited);

            if (_litVisual != null)
                _litVisual.SetActive(_hasIgnited);

            if (_unlitVisual == null && _litVisual == null && _spriteRenderer != null)
                _spriteRenderer.color = _hasIgnited ? _litColor : _unlitColor;
        }
    }
}
