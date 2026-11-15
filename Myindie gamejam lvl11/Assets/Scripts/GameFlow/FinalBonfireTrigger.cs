using Game.Gameplay.Heat;
using UnityEngine;

namespace Game.GameFlow
{
    /// <summary>Checks the level objective when the Player reaches the final bonfire.</summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class FinalBonfireTrigger : MonoBehaviour
    {
        [Header("Game Flow")]
        [SerializeField] private GameFlowBootstrap _gameFlow;

        [Header("Optional visuals")]
        [SerializeField] private GameObject _lockedVisual;
        [SerializeField] private GameObject _readyVisual;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color _lockedColor = new Color(0.25f, 0.25f, 0.3f, 1f);
        [SerializeField] private Color _readyColor = Color.white;

        private void Awake()
        {
            Collider2D goalCollider = GetComponent<Collider2D>();
            if (!goalCollider.isTrigger)
                Debug.LogWarning("[BONFIRE] Collider2D не настроен как Trigger. Включите Is Trigger или запустите Tools > Game Flow > Setup.", this);

            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            if (_gameFlow == null)
            {
                Debug.LogError("[BONFIRE] Не назначен GameFlowBootstrap. Запустите Tools > Game Flow > Setup.", this);
                return;
            }

            _gameFlow.GoalAvailabilityChanged += HandleGoalAvailabilityChanged;
            HandleGoalAvailabilityChanged(_gameFlow.IsGoalUnlocked);

            Debug.Log("[BONFIRE] Финальный костёр готов. Он станет активным после зажигания всех обязательных объектов.", this);
        }

        private void OnDestroy()
        {
            if (_gameFlow != null)
                _gameFlow.GoalAvailabilityChanged -= HandleGoalAvailabilityChanged;
        }

        private void Reset()
        {
            Collider2D goalCollider = GetComponent<Collider2D>();
            goalCollider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerHeatBootstrap playerHeat = other.GetComponentInParent<PlayerHeatBootstrap>();
            if (playerHeat == null)
                return;

            Debug.Log($"[BONFIRE] Игрок коснулся финального костра: {other.name}.", this);

            if (_gameFlow == null)
            {
                Debug.LogError("[BONFIRE] GameFlowBootstrap не назначен. Запустите Tools > Game Flow > Setup.", this);
                return;
            }

            _gameFlow.TryCompleteLevel();
        }

        private void HandleGoalAvailabilityChanged(bool isUnlocked)
        {
            if (_lockedVisual != null)
                _lockedVisual.SetActive(!isUnlocked);

            if (_readyVisual != null)
                _readyVisual.SetActive(isUnlocked);

            if (_lockedVisual == null && _readyVisual == null && _spriteRenderer != null)
                _spriteRenderer.color = isUnlocked ? _readyColor : _lockedColor;

            Debug.Log(isUnlocked
                ? "[BONFIRE] Финальный костёр активирован."
                : "[BONFIRE] Финальный костёр пока погашен.", this);
        }

        public void SetGameFlow(GameFlowBootstrap gameFlow) => _gameFlow = gameFlow;
    }
}
