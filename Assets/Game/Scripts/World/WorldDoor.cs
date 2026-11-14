using Game.Character;
using Game.Core;
using Game.UI;
using PrimeTween;
using UnityEngine;

namespace Game.World
{
    public enum DoorUnlockMode
    {
        FreeInteract,
        MechanismControlled,
        RequiresLighthouseKey
    }

    /// <summary>
    /// Door obstacle opened directly, via lever mechanism, or with the lighthouse key.
    /// </summary>
    public sealed class WorldDoor : MonoBehaviour, IInteractable
    {
        [SerializeField] private DoorUnlockMode _unlockMode;
        [SerializeField] private Collider2D _solidCollider;
        [SerializeField] private Transform _doorLeafVisual;
        [SerializeField] private SpriteRenderer _doorRenderer;
        [SerializeField] private Vector3 _openLocalOffset = new Vector3(2.4f, 0f, 0f);
        [SerializeField] private string _openPromptKey = "prompt.door.open";
        [SerializeField] private string _lockedPromptKey = "prompt.door.locked_lever";

        private static readonly Color OpenTint = new Color(0.35f, 0.38f, 0.42f, 0.45f);

        private IPlayerInventory _inventory;
        private GameplayHudView _hud;
        private bool _isOpen;
        private Tween _slideTween;

        public bool IsAvailable => !_isOpen;
        public bool IsOpen => _isOpen;
        public Vector2 WorldPosition => transform.position;

        public void Construct(IPlayerInventory inventory, GameplayHudView hud)
        {
            _inventory = inventory;
            _hud = hud;
        }

        private void OnDestroy()
        {
            _slideTween.Stop();
        }

        public bool CanInteract()
        {
            if (_isOpen)
            {
                return false;
            }

            return _unlockMode switch
            {
                DoorUnlockMode.FreeInteract => true,
                DoorUnlockMode.MechanismControlled => false,
                DoorUnlockMode.RequiresLighthouseKey => _inventory != null && _inventory.HasLighthouseKey,
                _ => false
            };
        }

        public string GetPromptKey(bool canInteract)
        {
            return canInteract ? _openPromptKey : _lockedPromptKey;
        }

        public void Interact()
        {
            if (!CanInteract())
            {
                return;
            }

            if (_unlockMode == DoorUnlockMode.RequiresLighthouseKey && (_inventory == null || !_inventory.TryConsumeLighthouseKey()))
            {
                return;
            }

            Open();
        }

        public void Open()
        {
            if (_isOpen)
            {
                return;
            }

            _isOpen = true;
            if (_solidCollider != null)
            {
                _solidCollider.enabled = false;
            }

            if (_doorRenderer != null)
            {
                _doorRenderer.color = OpenTint;
            }

            if (_doorLeafVisual != null)
            {
                _slideTween.Stop();
                _slideTween = Tween.LocalPosition(
                    _doorLeafVisual,
                    _doorLeafVisual.localPosition + _openLocalOffset,
                    0.45f,
                    Ease.OutCubic);
            }

            _hud?.ShowToast("toast.door_opened");
        }
    }
}
