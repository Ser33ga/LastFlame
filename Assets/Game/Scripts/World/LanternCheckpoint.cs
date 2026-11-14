using Game.Character;
using Game.Core;
using Game.Flame;
using Game.UI;
using PrimeTween;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Game.World
{
    /// <summary>
    /// Checkpoint lantern that restores full flame, anchors respawn, and projects a safe resting halo.
    /// </summary>
    public sealed class LanternCheckpoint : MonoBehaviour, IInteractable
    {
        [SerializeField] private int _checkpointIndex;
        [SerializeField] private bool _requiresMatch;
        [SerializeField] private bool _autoActivateOnApproach;
        [SerializeField] private float _safeZoneRadius = 2.8f;
        [SerializeField] private Transform _respawnPoint;
        [SerializeField] private Light2D _lanternLight;
        [SerializeField] private GameObject _litVisualRoot;
        [SerializeField] private SpriteRenderer _bodyRenderer;

        private static readonly Color UnlitColor = new Color(0.28f, 0.30f, 0.36f, 1f);
        private static readonly Color LitColor = new Color(1f, 0.72f, 0.22f, 1f);

        private IPlayerInventory _inventory;
        private IFlameService _flame;
        private ICheckpointService _checkpoints;
        private GameplayHudView _hud;
        private CandleController _candle;

        private bool _isLit;
        private bool _playerInsideSafeZone;
        private Tween _lightPulseTween;

        public bool IsAvailable => !_isLit;
        public bool IsLit => _isLit;
        public Vector2 WorldPosition => transform.position;

        public void Construct(
            IPlayerInventory inventory,
            IFlameService flame,
            ICheckpointService checkpoints,
            GameplayHudView hud,
            CandleController candle)
        {
            _inventory = inventory;
            _flame = flame;
            _checkpoints = checkpoints;
            _hud = hud;
            _candle = candle;

            ApplyVisualState(immediate: true);
        }

        private void OnDestroy()
        {
            _lightPulseTween.Stop();
            if (_playerInsideSafeZone)
            {
                _playerInsideSafeZone = false;
                _flame?.ExitSafeZone();
            }
        }

        public bool CanInteract()
        {
            if (_isLit)
            {
                return false;
            }

            return !_requiresMatch || (_inventory != null && _inventory.HasMatch);
        }

        public string GetPromptKey(bool canInteract)
        {
            return canInteract ? "prompt.lantern.light" : "prompt.lantern.need_match";
        }

        public void Interact()
        {
            if (!CanInteract())
            {
                return;
            }

            Ignite();
        }

        public void Ignite()
        {
            if (_isLit)
            {
                return;
            }

            _isLit = true;
            Vector2 respawnPos = _respawnPoint != null
                ? (Vector2)_respawnPoint.position
                : (Vector2)transform.position + Vector2.down * 1.1f;

            _checkpoints?.RegisterCheckpoint(_checkpointIndex, respawnPos);
            _flame?.RestoreFull();
            _hud?.ShowToast("toast.checkpoint_lit");
            ApplyVisualState(immediate: false);
        }

        private void Update()
        {
            if (_candle == null)
            {
                return;
            }

            float dist = Vector2.Distance(_candle.Position, transform.position);

            if (!_isLit && _autoActivateOnApproach && dist <= _safeZoneRadius * 0.78f && CanInteract())
            {
                Ignite();
            }

            bool inside = _isLit && dist <= _safeZoneRadius;
            if (inside != _playerInsideSafeZone)
            {
                _playerInsideSafeZone = inside;
                if (inside)
                {
                    _flame?.EnterSafeZone();
                }
                else
                {
                    _flame?.ExitSafeZone();
                }
            }
        }

        private void ApplyVisualState(bool immediate)
        {
            if (_litVisualRoot != null)
            {
                _litVisualRoot.SetActive(_isLit);
            }

            if (_bodyRenderer != null)
            {
                _bodyRenderer.color = _isLit ? LitColor : UnlitColor;
            }

            if (_lanternLight != null)
            {
                _lightPulseTween.Stop();
                if (!_isLit)
                {
                    _lanternLight.intensity = 0f;
                    _lanternLight.pointLightOuterRadius = 0.2f;
                }
                else if (immediate)
                {
                    _lanternLight.intensity = 1.45f;
                    _lanternLight.pointLightOuterRadius = 6.2f;
                }
                else
                {
                    _lanternLight.intensity = 2.4f;
                    _lanternLight.pointLightOuterRadius = 7.4f;
                    _lightPulseTween = Tween.Custom(
                        this,
                        2.4f,
                        1.45f,
                        0.45f,
                        (target, val) =>
                        {
                            if (target._lanternLight != null)
                            {
                                target._lanternLight.intensity = val;
                                target._lanternLight.pointLightOuterRadius = Mathf.Lerp(6.2f, 7.4f, (val - 1.45f) / 0.95f);
                            }
                        });
                }
            }
        }
    }
}
