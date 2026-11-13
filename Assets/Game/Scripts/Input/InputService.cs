using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Input
{
    /// <summary>
    /// Neutral New Input System wrapper bound cleanly without singletons or leaked subscriptions.
    /// Gameplay actions are extended after mechanics and controls are specified.
    /// </summary>
    public sealed class InputService : IInputService, IDisposable
    {
        private readonly InputAction _moveAction;
        private readonly InputAction _lookAction;
        private readonly InputAction _pauseAction;
        private readonly InputActionMap _playerMap;
        private readonly InputActionMap _uiMap;

        public Vector2 Move => IsPlayerInputActive && _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;
        public Vector2 Look => IsPlayerInputActive && _lookAction != null ? _lookAction.ReadValue<Vector2>() : Vector2.zero;
        public bool IsPlayerInputActive { get; private set; } = true;

        public event Action OnPausePerformed;

        public InputService(InputActionAsset inputActions = null)
        {
            if (inputActions != null)
            {
                _playerMap = inputActions.FindActionMap("Player", throwIfNotFound: false);
                _uiMap = inputActions.FindActionMap("UI", throwIfNotFound: false);
            }

            _moveAction = _playerMap?.FindAction("Move", throwIfNotFound: false);
            _lookAction = _playerMap?.FindAction("Look", throwIfNotFound: false);
            _pauseAction = _uiMap?.FindAction("Cancel", throwIfNotFound: false)
                           ?? _playerMap?.FindAction("Pause", throwIfNotFound: false);

            if (_pauseAction != null)
            {
                _pauseAction.performed += HandlePausePerformed;
            }

            _uiMap?.Enable();
            SetPlayerInputActive(true);
        }

        public void SetPlayerInputActive(bool active)
        {
            IsPlayerInputActive = active;
            if (_playerMap == null)
            {
                return;
            }

            if (active)
            {
                _playerMap.Enable();
            }
            else
            {
                _playerMap.Disable();
            }
        }

        public void SetCursorLock(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        public void Dispose()
        {
            if (_pauseAction != null)
            {
                _pauseAction.performed -= HandlePausePerformed;
            }

            _playerMap?.Disable();
            _uiMap?.Disable();
        }

        private void HandlePausePerformed(InputAction.CallbackContext _)
        {
            OnPausePerformed?.Invoke();
        }
    }
}
