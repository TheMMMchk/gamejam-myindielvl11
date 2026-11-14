using System;

namespace Game.Gameplay.Heat
{
    /// <summary>
    /// Stores the player's heat and applies its gameplay rules.
    /// This class does not depend on Unity components or lifecycle.
    /// </summary>
    public sealed class HeatModel
    {
        private readonly float _maxHeat;
        private readonly float _drainPerSecond;
        private readonly float _fadingThreshold;

        public float CurrentHeat { get; private set; }
        public float MaxHeat => _maxHeat;
        public float NormalizedHeat => _maxHeat <= 0f ? 0f : CurrentHeat / _maxHeat;
        public bool IsFading => NormalizedHeat <= _fadingThreshold;
        public bool IsDepleted => CurrentHeat <= 0f;

        public event Action<float, float> HeatChanged;
        public event Action HeatDepleted;

        public HeatModel(
            float maxHeat,
            float initialHeat,
            float drainPerSecond,
            float fadingThreshold)
        {
            _maxHeat = Math.Max(0f, maxHeat);
            _drainPerSecond = Math.Max(0f, drainPerSecond);
            _fadingThreshold = Clamp01(fadingThreshold);
            CurrentHeat = Clamp(initialHeat, 0f, _maxHeat);
        }

        public void Tick(float deltaTime)
        {
            if (IsDepleted || deltaTime <= 0f)
            {
                return;
            }

            ChangeHeat(CurrentHeat - _drainPerSecond * deltaTime);
        }

        public void Restore(float amount)
        {
            if (IsDepleted || amount <= 0f)
            {
                return;
            }

            ChangeHeat(CurrentHeat + amount);
        }

        private void ChangeHeat(float value)
        {
            float nextHeat = Clamp(value, 0f, _maxHeat);
            if (Math.Abs(nextHeat - CurrentHeat) < 0.0001f)
            {
                return;
            }

            bool wasDepleted = IsDepleted;
            CurrentHeat = nextHeat;
            HeatChanged?.Invoke(CurrentHeat, _maxHeat);

            if (!wasDepleted && IsDepleted)
            {
                HeatDepleted?.Invoke();
            }
        }

        private static float Clamp01(float value)
        {
            return Clamp(value, 0f, 1f);
        }

        private static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }
}
