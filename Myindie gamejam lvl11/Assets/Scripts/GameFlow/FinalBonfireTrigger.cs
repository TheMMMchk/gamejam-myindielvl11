using UnityEngine;
using Game.Gameplay.Heat;

namespace Game.GameFlow
{
    /// <summary>Marks the final bonfire as the level goal. Requires a trigger Collider2D.</summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class FinalBonfireTrigger : MonoBehaviour
    {
        [SerializeField] private GameFlowBootstrap _gameFlow;

        private void Awake()
        {
            Collider2D goalCollider = GetComponent<Collider2D>();
            if (!goalCollider.isTrigger)
            {
                Debug.LogWarning("[BONFIRE] Collider2D не настроен как Trigger. Включите Is Trigger или запустите Tools > Game Flow > Setup.", this);
            }

            if (_gameFlow == null)
            {
                Debug.LogWarning("[BONFIRE] На костре нет ссылки на GameFlowBootstrap. Запустите Tools > Game Flow > Setup и назначьте этот костёр.", this);
                return;
            }

            Debug.Log("[BONFIRE] Финальный костёр активен и ожидает игрока.", this);
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

            Debug.Log($"[BONFIRE] Игрок вошёл в триггер костра: {other.name}.", this);

            if (_gameFlow == null)
            {
                Debug.LogError("[BONFIRE] Игрок найден, но GameFlowBootstrap не назначен. Запустите Tools > Game Flow > Setup.", this);
                return;
            }

            Debug.Log("[BONFIRE] Условие победы выполнено.", this);
            _gameFlow.CompleteLevel();
        }

        public void SetGameFlow(GameFlowBootstrap gameFlow) => _gameFlow = gameFlow;
    }
}
