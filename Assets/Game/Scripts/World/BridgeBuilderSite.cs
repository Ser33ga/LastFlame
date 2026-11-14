using Game.Character;
using Game.Core;
using Game.UI;
using PrimeTween;
using UnityEngine;

namespace Game.World
{
    /// <summary>
    /// Abandoned cliff crossing in Forgotten Forest requiring collected wood planks to assemble.
    /// </summary>
    public sealed class BridgeBuilderSite : MonoBehaviour, IInteractable
    {
        [SerializeField] private int _requiredPlanks = 2;
        [SerializeField] private Collider2D _gapBlockerCollider;
        [SerializeField] private Transform _builtBridgeRoot;
        [SerializeField] private GameObject _unbuiltMarker;

        private IPlayerInventory _inventory;
        private GameplayHudView _hud;
        private bool _isBuilt;

        public bool IsAvailable => !_isBuilt;
        public bool IsBuilt => _isBuilt;
        public Vector2 WorldPosition => transform.position;

        public void Construct(IPlayerInventory inventory, GameplayHudView hud)
        {
            _inventory = inventory;
            _hud = hud;

            if (_builtBridgeRoot != null)
            {
                _builtBridgeRoot.gameObject.SetActive(false);
            }
        }

        public bool CanInteract()
        {
            return !_isBuilt && _inventory != null && _inventory.WoodPlanks >= _requiredPlanks;
        }

        public string GetPromptKey(bool canInteract)
        {
            return canInteract ? "prompt.bridge.build" : "prompt.bridge.need_planks";
        }

        public void Interact()
        {
            if (!CanInteract())
            {
                return;
            }

            if (!_inventory.TryConsumeWoodPlanks(_requiredPlanks))
            {
                return;
            }

            _isBuilt = true;
            if (_gapBlockerCollider != null)
            {
                _gapBlockerCollider.enabled = false;
            }

            if (_unbuiltMarker != null)
            {
                _unbuiltMarker.SetActive(false);
            }

            if (_builtBridgeRoot != null)
            {
                _builtBridgeRoot.gameObject.SetActive(true);
                for (int i = 0; i < _builtBridgeRoot.childCount; i++)
                {
                    Transform plank = _builtBridgeRoot.GetChild(i);
                    Vector3 targetScale = plank.localScale;
                    plank.localScale = new Vector3(0.01f, targetScale.y, targetScale.z);
                    Tween.Scale(plank, targetScale, 0.25f, Ease.OutBack, startDelay: i * 0.09f);
                }
            }

            _hud?.ShowToast("toast.bridge_built");
        }
    }
}
