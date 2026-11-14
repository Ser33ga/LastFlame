using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace Game.Flame
{
    /// <summary>
    /// Tracks candle flame life/light resource, passive burn, lantern safe zones, and finale sacrifice.
    /// </summary>
    public sealed class FlameService : IFlameService, ITickable
    {
        private const float DefaultMaxFlame = 100f;
        private const float PassiveDrainPerSecond = 1.55f;
        private const float SafeZoneRegenPerSecond = 35f;

        private int _safeZoneCount;
        private bool _isSacrificing;

        public float CurrentFlame { get; private set; } = DefaultMaxFlame;
        public float MaxFlame => DefaultMaxFlame;
        public float NormalizedFlame => MaxFlame > 0.001f ? Mathf.Clamp01(CurrentFlame / MaxFlame) : 0f;
        public bool IsExtinguished { get; private set; }
        public bool IsInSafeZone => _safeZoneCount > 0;
        public bool IsDrainPaused { get; private set; }

        public event Action<float, float> OnFlameChanged;
        public event Action OnFlameExtinguished;

        public void Tick()
        {
            if (_isSacrificing || IsDrainPaused || IsExtinguished)
            {
                return;
            }

            if (IsInSafeZone)
            {
                if (CurrentFlame < MaxFlame)
                {
                    SetFlameInternal(CurrentFlame + SafeZoneRegenPerSecond * Time.deltaTime, triggerExtinguish: false);
                }

                return;
            }

            SetFlameInternal(CurrentFlame - PassiveDrainPerSecond * Time.deltaTime, triggerExtinguish: true);
        }

        public void SetDrainPaused(bool paused)
        {
            IsDrainPaused = paused;
        }

        public void EnterSafeZone()
        {
            _safeZoneCount++;
        }

        public void ExitSafeZone()
        {
            _safeZoneCount = Mathf.Max(0, _safeZoneCount - 1);
        }

        public void ResetSafeZones()
        {
            _safeZoneCount = 0;
        }

        public void AddFlame(float amount)
        {
            if (amount <= 0f || _isSacrificing)
            {
                return;
            }

            IsExtinguished = false;
            SetFlameInternal(CurrentFlame + amount, triggerExtinguish: false);
        }

        public bool TryConsumeFlame(float amount)
        {
            if (amount <= 0f || _isSacrificing || IsExtinguished || CurrentFlame <= amount)
            {
                return false;
            }

            SetFlameInternal(CurrentFlame - amount, triggerExtinguish: false);
            return true;
        }

        public void DrainFlame(float amount)
        {
            if (amount <= 0f || _isSacrificing || IsExtinguished)
            {
                return;
            }

            SetFlameInternal(CurrentFlame - amount, triggerExtinguish: true);
        }

        public void RestoreFull()
        {
            _isSacrificing = false;
            IsExtinguished = false;
            SetFlameInternal(MaxFlame, triggerExtinguish: false);
        }

        public async UniTask SacrificeDrainAsync(float duration, CancellationToken ct = default)
        {
            _isSacrificing = true;
            IsDrainPaused = true;
            float startFlame = CurrentFlame;
            float elapsed = 0f;
            float safeDuration = Mathf.Max(0.1f, duration);

            while (elapsed < safeDuration && !ct.IsCancellationRequested)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / safeDuration);
                SetFlameInternal(Mathf.Lerp(startFlame, 0f, t), triggerExtinguish: false);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            if (!ct.IsCancellationRequested)
            {
                SetFlameInternal(0f, triggerExtinguish: false);
            }
        }

        private void SetFlameInternal(float targetValue, bool triggerExtinguish)
        {
            float clamped = Mathf.Clamp(targetValue, 0f, MaxFlame);
            bool changed = !Mathf.Approximately(CurrentFlame, clamped);
            CurrentFlame = clamped;

            if (changed)
            {
                OnFlameChanged?.Invoke(CurrentFlame, MaxFlame);
            }

            if (triggerExtinguish && CurrentFlame <= 0.001f && !IsExtinguished)
            {
                CurrentFlame = 0f;
                IsExtinguished = true;
                OnFlameChanged?.Invoke(CurrentFlame, MaxFlame);
                OnFlameExtinguished?.Invoke();
            }
        }
    }
}
