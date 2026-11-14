using UnityEngine;

namespace Game.GameFlow
{
    /// <summary>Marks the final bonfire as the level goal. Requires a trigger Collider2D.</summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class FinalBonfireTrigger : MonoBehaviour
    {
        [SerializeField] private GameFlowBootstrap _gameFlow;

        private void Reset()
        {
            Collider2D goalCollider = GetComponent<Collider2D>();
            goalCollider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_gameFlow == null || !other.TryGetComponent(out Game.Gameplay.Heat.PlayerHeatBootstrap _))
                return;

            _gameFlow.CompleteLevel();
        }

        public void SetGameFlow(GameFlowBootstrap gameFlow) => _gameFlow = gameFlow;
    }
}
