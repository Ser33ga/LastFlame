using Cysharp.Threading.Tasks;
using Game.Character;
using Game.Core;
using Game.UI;
using PrimeTween;
using UnityEngine;

namespace Game.World
{
    /// <summary>
    /// Mine tunnel rockfall obstacle cleared with the workshop explosive to reveal daylight outside.
    /// </summary>
    public sealed class RockfallObstacle : MonoBehaviour, IInteractable
    {
        [SerializeField] private Collider2D _blockerCollider;
        [SerializeField] private Transform[] _boulderVisuals;
        [SerializeField] private SpriteRenderer _flashRenderer;
        [SerializeField] private SpriteRenderer _fuseRenderer;

        private IPlayerInventory _inventory;
        private TopDownCameraRig _cameraRig;
        private GameplayHudView _hud;
        private bool _detonating;
        private bool _cleared;

        public bool IsAvailable => !_cleared && !_detonating;
        public bool IsCleared => _cleared;
        public Vector2 WorldPosition => transform.position;

        public void Construct(IPlayerInventory inventory, TopDownCameraRig cameraRig, GameplayHudView hud)
        {
            _inventory = inventory;
            _cameraRig = cameraRig;
            _hud = hud;

            if (_flashRenderer != null)
            {
                Color c = _flashRenderer.color;
                c.a = 0f;
                _flashRenderer.color = c;
            }

            if (_fuseRenderer != null)
            {
                _fuseRenderer.gameObject.SetActive(false);
            }
        }

        public bool CanInteract()
        {
            return !_cleared && !_detonating && _inventory != null && _inventory.HasExplosive;
        }

        public string GetPromptKey(bool canInteract)
        {
            return canInteract ? "prompt.rockfall.detonate" : "prompt.rockfall.need_explosive";
        }

        public void Interact()
        {
            if (!CanInteract())
            {
                return;
            }

            if (!_inventory.TryConsumeExplosive())
            {
                return;
            }

            _detonating = true;
            DetonateSequenceAsync().Forget();
        }

        private async UniTaskVoid DetonateSequenceAsync()
        {
            if (_fuseRenderer != null)
            {
                _fuseRenderer.gameObject.SetActive(true);
            }

            for (int i = 0; i < 6; i++)
            {
                if (_fuseRenderer != null)
                {
                    _fuseRenderer.color = i % 2 == 0
                        ? new Color(1f, 0.45f, 0.1f, 1f)
                        : new Color(1f, 0.92f, 0.35f, 1f);
                }

                await UniTask.Delay(120, cancellationToken: this.GetCancellationTokenOnDestroy());
            }

            if (_fuseRenderer != null)
            {
                _fuseRenderer.gameObject.SetActive(false);
            }

            _cleared = true;
            _detonating = false;

            if (_blockerCollider != null)
            {
                _blockerCollider.enabled = false;
            }

            _cameraRig?.Shake(0.6f, 0.42f);

            if (_flashRenderer != null)
            {
                _flashRenderer.color = new Color(1f, 0.78f, 0.25f, 0.9f);
                _flashRenderer.transform.localScale = Vector3.one * 1.2f;
                _ = Tween.Scale(_flashRenderer.transform, Vector3.one * 7.5f, 0.35f, Ease.OutQuad);
                _ = Tween.Alpha(_flashRenderer, 0f, 0.38f, Ease.OutQuad);
            }

            if (_boulderVisuals != null)
            {
                for (int i = 0; i < _boulderVisuals.Length; i++)
                {
                    Transform b = _boulderVisuals[i];
                    if (b == null)
                    {
                        continue;
                    }

                    Vector3 dir = ((Vector2)b.localPosition).sqrMagnitude > 0.01f
                        ? (Vector3)((Vector2)b.localPosition).normalized * 2.5f
                        : new Vector3((i - 1) * 1.5f, 1.5f, 0f);
                    _ = Tween.LocalPosition(b, b.localPosition + dir, 0.38f, Ease.OutQuad);
                    _ = Tween.Scale(b, Vector3.zero, 0.38f, Ease.InBack);
                }
            }

            _hud?.ShowToast("toast.rockfall_cleared");
        }
    }
}
