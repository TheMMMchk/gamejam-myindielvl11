using Game.Gameplay.Heat;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.GameFlow
{
    /// <summary>Connects the existing heat gameplay contract to run completion.</summary>
    public sealed class GameFlowBootstrap : MonoBehaviour
    {
        [SerializeField] private PlayerHeatBootstrap _playerHeat;
        [SerializeField] private GameFlowUI _ui;

        public GameFlowState State { get; private set; } = GameFlowState.Playing;

        private void Awake()
        {
            if (_playerHeat == null || _ui == null)
            {
                Debug.LogError("[Game Flow] Не назначены PlayerHeat или UI. Запустите Tools > Game Flow > Setup.", this);
                enabled = false;
                return;
            }

            Time.timeScale = 1f;
            _playerHeat.HeatChanged += HandleHeatChanged;
            _playerHeat.HeatDepleted += HandleHeatDepleted;
            _ui.RestartRequested += RestartLevel;
            _ui.SetHeat(_playerHeat.CurrentHeat, _playerHeat.MaxHeat);
            _ui.ShowResult(false, false);
        }

        private void OnDestroy()
        {
            if (_playerHeat != null)
            {
                _playerHeat.HeatChanged -= HandleHeatChanged;
                _playerHeat.HeatDepleted -= HandleHeatDepleted;
            }

            if (_ui != null)
                _ui.RestartRequested -= RestartLevel;

            Time.timeScale = 1f;
        }

        public void CompleteLevel()
        {
            if (State != GameFlowState.Playing)
                return;

            State = GameFlowState.Won;
            FinishGame(true);
        }

        private void HandleHeatChanged(float current, float max)
        {
            if (State == GameFlowState.Playing)
                _ui.SetHeat(current, max);
        }

        private void HandleHeatDepleted()
        {
            if (State != GameFlowState.Playing)
                return;

            State = GameFlowState.Lost;
            FinishGame(false);
        }

        private void FinishGame(bool won)
        {
            _ui.ShowResult(true, won);
            Time.timeScale = 0f;
        }

        private void RestartLevel()
        {
            Time.timeScale = 1f;
            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }
    }
}
