using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Input
{
    /// <summary>
    /// New Input System wrapper bound cleanly without singletons or leaked event subscriptions.
    /// </summary>
    public sealed class InputService : IInputService, IDisposable
    {
        private readonly InputAction _moveAction;
        private readonly InputAction _jumpAction;
        private readonly InputAction _interactAction;
        private readonly InputAction _dashAction;
        private readonly InputAction _pauseAction;

        private readonly InputActionMap _playerMap;
        private readonly InputActionMap _uiMap;
        private readonly bool _ownsActions;

        public Vector2 Move => IsPlayerInputActive && _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;
        public bool IsJumpPressed => IsPlayerInputActive && _jumpAction != null && _jumpAction.IsPressed();
        public bool IsInteractPressed => IsPlayerInputActive && _interactAction != null && _interactAction.IsPressed();
        public bool IsDashPressed => IsPlayerInputActive && _dashAction != null && _dashAction.IsPressed();
        public bool IsPlayerInputActive { get; private set; } = true;

        public event Action OnJumpPerformed;
        public event Action OnInteractPerformed;
        public event Action OnDashPerformed;
        public event Action OnPausePerformed;

        public InputService(InputActionAsset inputActions = null)
        {
            if (inputActions != null)
            {
                _playerMap = inputActions.FindActionMap("Player", throwIfNotFound: false);
                _uiMap = inputActions.FindActionMap("UI", throwIfNotFound: false);
            }

            if (_playerMap == null)
            {
                _ownsActions = true;
                _playerMap = BuildFallbackPlayerMap();
            }

            _moveAction = _playerMap.FindAction("Move", throwIfNotFound: false);
            _jumpAction = _playerMap.FindAction("Jump", throwIfNotFound: false);
            _interactAction = _playerMap.FindAction("Interact", throwIfNotFound: false);
            _dashAction = _playerMap.FindAction("Sprint", throwIfNotFound: false)
                          ?? _playerMap.FindAction("Dash", throwIfNotFound: false);

            _pauseAction = _uiMap?.FindAction("Cancel", throwIfNotFound: false)
                           ?? _playerMap.FindAction("Pause", throwIfNotFound: false);

            if (_pauseAction == null)
            {
                _pauseAction = new InputAction("Pause", InputActionType.Button, "<Keyboard>/escape");
                _pauseAction.AddBinding("<Gamepad>/start");
                _pauseAction.Enable();
            }

            Subscribe(_jumpAction, HandleJumpPerformed);
            Subscribe(_interactAction, HandleInteractPerformed);
            Subscribe(_dashAction, HandleDashPerformed);
            Subscribe(_pauseAction, HandlePausePerformed);

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
            Unsubscribe(_jumpAction, HandleJumpPerformed);
            Unsubscribe(_interactAction, HandleInteractPerformed);
            Unsubscribe(_dashAction, HandleDashPerformed);
            Unsubscribe(_pauseAction, HandlePausePerformed);

            _playerMap?.Disable();
            _uiMap?.Disable();

            if (_ownsActions)
            {
                _playerMap?.Dispose();
                _pauseAction?.Dispose();
            }
        }

        private static InputActionMap BuildFallbackPlayerMap()
        {
            var map = new InputActionMap("Player");

            InputAction move = map.AddAction("Move", InputActionType.Value);
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            move.AddBinding("<Gamepad>/leftStick");

            InputAction jump = map.AddAction("Jump", InputActionType.Button, "<Keyboard>/space");
            jump.AddBinding("<Gamepad>/buttonSouth");

            InputAction interact = map.AddAction("Interact", InputActionType.Button, "<Keyboard>/e");
            interact.AddBinding("<Gamepad>/buttonWest");

            InputAction sprint = map.AddAction("Sprint", InputActionType.Button, "<Keyboard>/leftShift");
            sprint.AddBinding("<Gamepad>/buttonEast");

            return map;
        }

        private static void Subscribe(InputAction action, Action<InputAction.CallbackContext> callback)
        {
            if (action != null)
            {
                action.performed += callback;
            }
        }

        private static void Unsubscribe(InputAction action, Action<InputAction.CallbackContext> callback)
        {
            if (action != null)
            {
                action.performed -= callback;
            }
        }

        private void HandleJumpPerformed(InputAction.CallbackContext _)
        {
            if (IsPlayerInputActive) OnJumpPerformed?.Invoke();
        }

        private void HandleInteractPerformed(InputAction.CallbackContext _)
        {
            if (IsPlayerInputActive) OnInteractPerformed?.Invoke();
        }

        private void HandleDashPerformed(InputAction.CallbackContext _)
        {
            if (IsPlayerInputActive) OnDashPerformed?.Invoke();
        }

        private void HandlePausePerformed(InputAction.CallbackContext _)
        {
            OnPausePerformed?.Invoke();
        }
    }
}
