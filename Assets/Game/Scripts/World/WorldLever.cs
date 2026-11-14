using Game.Core;
using Game.UI;
using PrimeTween;
using UnityEngine;

namespace Game.World
{
    /// <summary>
    /// Interactive primitive lever that flips visually and opens a linked WorldDoor.
    /// </summary>
    public sealed class WorldLever : MonoBehaviour, IInteractable
    {
        [SerializeField] private WorldDoor _targetDoor;
        [SerializeField] private Transform _handleVisual;
        [SerializeField] private SpriteRenderer _indicatorRenderer;

        private static readonly Color ActiveColor = new Color(0.32f, 0.95f, 0.45f, 1f);

        private GameplayHudView _hud;
        private bool _activated;
        private Tween _flipTween;

        public bool IsAvailable => !_activated;
        public bool IsActivated => _activated;
        public Vector2 WorldPosition => transform.position;

        public void Construct(GameplayHudView hud)
        {
            _hud = hud;
        }

        private void OnDestroy()
        {
            _flipTween.Stop();
        }

        public bool CanInteract()
        {
            return !_activated;
        }

        public string GetPromptKey(bool canInteract)
        {
            return "prompt.lever.pull";
        }

        public void Interact()
        {
            if (_activated)
            {
                return;
            }

            _activated = true;
            if (_indicatorRenderer != null)
            {
                _indicatorRenderer.color = ActiveColor;
            }

            if (_handleVisual != null)
            {
                _flipTween.Stop();
                _flipTween = Tween.LocalRotation(_handleVisual, Quaternion.Euler(0f, 0f, -45f), 0.25f, Ease.OutBack);
            }

            _targetDoor?.Open();
            _hud?.ShowToast("toast.lever_pulled");
        }
    }
}
