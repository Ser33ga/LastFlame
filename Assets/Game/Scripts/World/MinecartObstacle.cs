using Cysharp.Threading.Tasks;
using Game.Character;
using Game.Core;
using Game.UI;
using PrimeTween;
using UnityEngine;

namespace Game.World
{
    /// <summary>
    /// Old minecart at the Mine entrance that rolls down the track to smash through a wooden barricade.
    /// </summary>
    public sealed class MinecartObstacle : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform _cartVisualRoot;
        [SerializeField] private Vector3 _rollTargetLocalPosition = new Vector3(0f, 6.2f, 0f);
        [SerializeField] private Collider2D _barricadeCollider;
        [SerializeField] private GameObject _barricadeVisual;

        private TopDownCameraRig _cameraRig;
        private GameplayHudView _hud;
        private bool _pushed;

        public bool IsAvailable => !_pushed;
        public bool IsPushed => _pushed;
        public Vector2 WorldPosition => _cartVisualRoot != null ? (Vector2)_cartVisualRoot.position : (Vector2)transform.position;

        public void Construct(TopDownCameraRig cameraRig, GameplayHudView hud)
        {
            _cameraRig = cameraRig;
            _hud = hud;
        }

        public bool CanInteract()
        {
            return !_pushed;
        }

        public string GetPromptKey(bool canInteract)
        {
            return "prompt.minecart.push";
        }

        public void Interact()
        {
            if (_pushed)
            {
                return;
            }

            _pushed = true;
            RollAndBreakAsync().Forget();
        }

        private async UniTaskVoid RollAndBreakAsync()
        {
            if (_cartVisualRoot != null)
            {
                await Tween.LocalPosition(_cartVisualRoot, _rollTargetLocalPosition, 0.7f, Ease.InQuad);
            }

            if (this == null)
            {
                return;
            }

            if (_barricadeCollider != null)
            {
                _barricadeCollider.enabled = false;
            }

            if (_barricadeVisual != null)
            {
                _barricadeVisual.SetActive(false);
            }

            _cameraRig?.Shake(0.32f, 0.25f);
            _hud?.ShowToast("toast.minecart_cleared");
        }
    }
}
