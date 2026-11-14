using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Contract for entities that can take flame melee damage and knockback.
    /// </summary>
    public interface IDamageable
    {
        bool IsAlive { get; }
        Vector2 WorldPosition { get; }
        void TakeHit(int damage, Vector2 knockbackImpulse);
    }
}
