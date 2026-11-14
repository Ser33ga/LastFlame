using System;
using UnityEngine;

namespace Game.Input
{
    public interface IInputService
    {
        Vector2 Move { get; }
        Vector2 Look { get; }
        bool IsPlayerInputActive { get; }

        event Action OnPausePerformed;
        event Action OnAttackPerformed;
        event Action OnInteractPerformed;
        event Action OnDashPerformed;

        void SetPlayerInputActive(bool active);
        void SetCursorLock(bool locked);
    }
}
