using System;
using UnityEngine;

namespace Game.Gameplay.Heat
{
    /// <summary>
    /// Creates the heat model and connects it to Unity's update lifecycle.
    /// Place this component on the Player GameObject.
    /// </summary>
    public sealed class PlayerHeatBootstrap : MonoBehaviour
    {
        [Header("Heat")]
        [SerializeField, Min(1f)] private float _maxHeat = 100f;
        [SerializeField, Min(0f)] private float _initialHeat = 100f;
        [SerializeField, Min(0f)] private float _drainPerSecond = 5f;
        [SerializeField, Range(0f, 1f)] private float _fadingThreshold = 0.25f;

        [Header("Debug (Editor Console)")]
        [SerializeField] private bool _enableDebugLogs = true;

        private HeatModel _heatModel;
        private bool _wasFading;

        public float CurrentHeat => _heatModel != null ? _heatModel.CurrentHeat : _initialHeat;
        public float MaxHeat => _heatModel != null ? _heatModel.MaxHeat : _maxHeat;
        public float NormalizedHeat => _heatModel != null ? _heatModel.NormalizedHeat : 1f;
        public bool IsFading => _heatModel != null && _heatModel.IsFading;
        public bool IsDepleted => _heatModel != null && _heatModel.IsDepleted;

        public event Action<float, float> HeatChanged;
        public event Action HeatDepleted;

        private void Awake()
        {
            _heatModel = new HeatModel(
                _maxHeat,
                _initialHeat,
                _drainPerSecond,
                _fadingThreshold);

            _heatModel.HeatChanged += HandleHeatChanged;
            _heatModel.HeatDepleted += HandleHeatDepleted;
            _wasFading = _heatModel.IsFading;

            Log($"Система огня запущена: {_heatModel.CurrentHeat:0.#}/{_heatModel.MaxHeat:0.#}. " +
                $"Расход: {_drainPerSecond:0.#}/сек. Порог затухания: {_fadingThreshold:P0}.");
        }

        private void Update()
        {
            _heatModel.Tick(Time.deltaTime);

            if (!_wasFading && _heatModel.IsFading)
            {
                Log($"Огонёк затухает! Осталось {_heatModel.CurrentHeat:0.#}/{_heatModel.MaxHeat:0.#}.");
            }

            _wasFading = _heatModel.IsFading;
        }

        private void OnDestroy()
        {
            if (_heatModel == null)
            {
                return;
            }

            _heatModel.HeatChanged -= HandleHeatChanged;
            _heatModel.HeatDepleted -= HandleHeatDepleted;
        }

        public void RestoreHeat(float amount)
        {
            if (_heatModel == null)
            {
                return;
            }

            float before = _heatModel.CurrentHeat;
            _heatModel.Restore(amount);
            float restored = _heatModel.CurrentHeat - before;

            if (restored > 0f)
            {
                Log($"Огонь восстановлен на +{restored:0.#}: " +
                    $"{_heatModel.CurrentHeat:0.#}/{_heatModel.MaxHeat:0.#}.");
            }
            else if (_heatModel.IsDepleted)
            {
                Log("Восстановление не сработало: огонёк уже погас.");
            }
        }

        private void HandleHeatChanged(float currentHeat, float maxHeat)
        {
            HeatChanged?.Invoke(currentHeat, maxHeat);
        }

        private void HandleHeatDepleted()
        {
            Log("Огонёк полностью погас!");
            HeatDepleted?.Invoke();
        }

        private void Log(string message)
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[FIRE DEBUG] {message}", this);
            }
        }
    }
}
