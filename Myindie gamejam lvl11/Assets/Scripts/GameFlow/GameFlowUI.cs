using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameFlow
{
    /// <summary>Updates the generated heat HUD and result overlay.</summary>
    public sealed class GameFlowUI : MonoBehaviour
    {
        [SerializeField] private Slider _heatSlider;
        [SerializeField] private Text _heatLabel;
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private Text _resultLabel;
        [SerializeField] private Button _restartButton;

        public event Action RestartRequested;

        private void Awake()
        {
            if (_heatSlider == null || _heatLabel == null || _resultPanel == null ||
                _resultLabel == null || _restartButton == null)
            {
                Debug.LogError("[Game Flow UI] Не все ссылки UI назначены. Перезапустите Tools > Game Flow > Setup.", this);
                enabled = false;
                return;
            }

            _restartButton.onClick.AddListener(HandleRestartClicked);
        }

        private void OnDestroy()
        {
            if (_restartButton != null)
                _restartButton.onClick.RemoveListener(HandleRestartClicked);
        }

        public void SetHeat(float current, float max)
        {
            if (_heatSlider == null || _heatLabel == null)
                return;

            _heatSlider.minValue = 0f;
            _heatSlider.maxValue = Mathf.Max(1f, max);
            _heatSlider.value = current;
            _heatLabel.text = $"Тепло: {current:0}/{max:0}";
        }

        public void ShowResult(bool visible, bool won)
        {
            if (_resultPanel == null)
                return;

            _resultPanel.SetActive(visible);
            if (visible && _resultLabel != null)
                _resultLabel.text = won ? "КОСТЁР НАЙДЕН" : "ОГОНЁК ПОГАС";
        }

        private void HandleRestartClicked() => RestartRequested?.Invoke();
    }
}
