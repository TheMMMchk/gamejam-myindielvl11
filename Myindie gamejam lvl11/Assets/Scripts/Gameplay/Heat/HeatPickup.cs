using UnityEngine;

namespace Game.Gameplay.Heat
{
    /// <summary>
    /// Restores heat once when the Player enters this trigger, then disappears.
    /// The GameObject must have a Collider2D with Is Trigger enabled.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class HeatPickup : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _heatAmount = 35f;

        private bool _collected;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected || !other.TryGetComponent(out PlayerHeatBootstrap playerHeat))
            {
                return;
            }

            _collected = true;
            Debug.Log($"[FIRE DEBUG] Подобран источник тепла: +{_heatAmount:0.#}.", this);
            playerHeat.RestoreHeat(_heatAmount);
            Destroy(gameObject);
        }
    }
}
