using System;
using UnityEngine;

namespace Game.Input
{
    public interface IInputService
    {
        Vector2 Move { get; }
        bool IsJumpPressed { get; }
        bool IsInteractPressed { get; }
        bool IsDashPressed { get; }
        bool IsPlayerInputActive { get; }

        event Action OnJumpPerformed;
        event Action OnInteractPerformed;
        event Action OnDashPerformed;
        event Action OnPausePerformed;

        void SetPlayerInputActive(bool active);
        void SetCursorLock(bool locked);
    }
}
