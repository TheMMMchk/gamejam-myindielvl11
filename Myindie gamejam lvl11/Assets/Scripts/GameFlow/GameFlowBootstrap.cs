using System;
using System.Collections.Generic;
using Game.Gameplay.Heat;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.GameFlow
{
    /// <summary>
    /// Owns level progress and run completion.
    /// Required sources are assigned manually by the level author.
    /// </summary>
    public sealed class GameFlowBootstrap : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private PlayerHeatBootstrap _playerHeat;
        [SerializeField] private GameFlowUI _ui;

        [Header("Level Objective")]
        [SerializeField] private FlammableHeatSource[] _requiredSources;

        private readonly HashSet<FlammableHeatSource> _requiredSourceSet =
            new HashSet<FlammableHeatSource>();

        private readonly HashSet<FlammableHeatSource> _ignitedSources =
            new HashSet<FlammableHeatSource>();

        public GameFlowState State { get; private set; } = GameFlowState.Playing;
        public int IgnitedRequiredCount => _ignitedSources.Count;
        public int RequiredCount => _requiredSourceSet.Count;
        public int RemainingRequiredCount => Mathf.Max(0, RequiredCount - IgnitedRequiredCount);
        public bool IsGoalUnlocked => RequiredCount > 0 && RemainingRequiredCount == 0;

        public event Action<bool> GoalAvailabilityChanged;

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

            RegisterRequiredSources();

            _ui.SetHeat(_playerHeat.CurrentHeat, _playerHeat.MaxHeat);
            UpdateProgressUI();
            _ui.ShowResult(false, false);

            if (RequiredCount == 0)
            {
                Debug.LogError("[Game Flow] Не назначены обязательные горючие объекты. Добавьте их в Tools > Game Flow > Setup.", this);
            }

            GoalAvailabilityChanged?.Invoke(IsGoalUnlocked);
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

            foreach (FlammableHeatSource source in _requiredSourceSet)
            {
                if (source != null)
                    source.Ignited -= HandleRequiredSourceIgnited;
            }

            Time.timeScale = 1f;
        }

        /// <summary>Called by the final bonfire. Returns true only when the objective is complete.</summary>
        public bool TryCompleteLevel()
        {
            if (State != GameFlowState.Playing)
                return false;

            if (!IsGoalUnlocked)
            {
                int remaining = RemainingRequiredCount;
                _ui.ShowBonfireLocked(remaining);
                Debug.Log($"[BONFIRE] Костёр пока не активен. Осталось зажечь объектов: {remaining}.", this);
                return false;
            }

            State = GameFlowState.Won;
            FinishGame(true);
            return true;
        }

        private void RegisterRequiredSources()
        {
            if (_requiredSources == null)
                return;

            foreach (FlammableHeatSource source in _requiredSources)
            {
                if (source == null)
                {
                    Debug.LogWarning("[Game Flow] В списке обязательных объектов есть пустая ссылка.", this);
                    continue;
                }

                if (!_requiredSourceSet.Add(source))
                {
                    Debug.LogWarning($"[Game Flow] Источник {source.name} добавлен в цель несколько раз. Повтор пропущен.", source);
                    continue;
                }

                source.Ignited += HandleRequiredSourceIgnited;

                // A source can already be ignited before Game Flow subscribes.
                if (source.IsIgnited)
                    _ignitedSources.Add(source);
            }
        }

        private void HandleRequiredSourceIgnited(FlammableHeatSource source)
        {
            if (State != GameFlowState.Playing || !_requiredSourceSet.Contains(source))
                return;

            if (!_ignitedSources.Add(source))
                return;

            Debug.Log($"[Game Flow] Обязательный объект зажжён: {IgnitedRequiredCount}/{RequiredCount}.", source);
            UpdateProgressUI();

            if (IsGoalUnlocked)
            {
                _ui.ShowAllSourcesIgnited();
                Debug.Log("[Game Flow] Все обязательные объекты зажжены. Финальный костёр доступен.", this);
                GoalAvailabilityChanged?.Invoke(true);
            }
        }

        private void UpdateProgressUI()
        {
            _ui.SetProgress(IgnitedRequiredCount, RequiredCount);
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
