using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Flame
{
    public interface IFlameService
    {
        float CurrentFlame { get; }
        float MaxFlame { get; }
        float NormalizedFlame { get; }
        bool IsExtinguished { get; }
        bool IsInSafeZone { get; }
        bool IsDrainPaused { get; }

        event Action<float, float> OnFlameChanged;
        event Action OnFlameExtinguished;

        void SetDrainPaused(bool paused);
        void EnterSafeZone();
        void ExitSafeZone();
        void ResetSafeZones();
        void AddFlame(float amount);
        bool TryConsumeFlame(float amount);
        void DrainFlame(float amount);
        void RestoreFull();
        UniTask SacrificeDrainAsync(float duration, CancellationToken ct = default);
    }
}
