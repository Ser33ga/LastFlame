using System;
using System.Collections.Generic;
using Game.Core;
using Game.Flame;
using Game.Input;
using PrimeTween;
using UnityEngine;

namespace Game.Character
{
    /// <summary>
    /// Top-down candle character controller handling movement, dash, flame attack, wind response, and interaction.
    /// </summary>
    public sealed class CandleController : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private Transform _bodyVisual;
        [SerializeField] private Transform _facingIndicator;
        [SerializeField] private SpriteRenderer _attackFlashRenderer;
        [SerializeField] private CandleLightView _lightView;

        [SerializeField] private float _moveSpeed = 5.4f;
        [SerializeField] private float _dashSpeed = 13.5f;
        [SerializeField] private float _dashDuration = 0.16f;
        [SerializeField] private float _dashCooldown = 0.65f;
        [SerializeField] private float _dashFlameCost = 5f;
        [SerializeField] private float _attackCooldown = 0.32f;
        [SerializeField] private float _attackRange = 0.85f;
        [SerializeField] private float _attackRadius = 1.45f;
        [SerializeField] private float _interactRadius = 1.95f;

        private readonly Collider2D[] _attackHitsBuffer = new Collider2D[24];
        private readonly HashSet<IDamageable> _uniqueDamageables = new HashSet<IDamageable>();

        private IInputService _input;
        private IFlameService _flame;
        private TopDownCameraRig _cameraRig;
        private IReadOnlyList<IInteractable> _interactables;

        private Vector2 _facingDirection = Vector2.up;
        private Vector2 _knockbackVelocity;
        private Vector2 _accumulatedWindVelocity;
        private Vector2 _dashDirection;
        private Vector3 _baseBodyScale = Vector3.one;

        private float _dashTimer;
        private float _dashCooldownTimer;
        private float _attackCooldownTimer;
        private float _hitInvulnTimer;
        private float _stepPulseTime;
        private bool _controlsEnabled = true;

        private IInteractable _nearestInteractable;
        private string _lastPromptKey;
        private bool _lastCanInteract;

        private Tween _punchTween;
        private Tween _flashAlphaTween;
        private Tween _flashScaleTween;

        public Vector2 Position => _rb != null ? _rb.position : (Vector2)transform.position;
        public Vector2 FacingDirection => _facingDirection;
        public IInteractable NearestInteractable => _nearestInteractable;

        public event Action<string, bool> OnNearestInteractableChanged;

        public void Construct(
            IInputService input,
            IFlameService flame,
            TopDownCameraRig cameraRig,
            IReadOnlyList<IInteractable> interactables)
        {
            UnsubscribeInput();
            _input = input;
            _flame = flame;
            _cameraRig = cameraRig;
            _interactables = interactables;

            if (_bodyVisual != null)
            {
                _baseBodyScale = _bodyVisual.localScale;
            }

            if (_rb != null)
            {
                _rb.gravityScale = 0f;
                _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }

            if (_attackFlashRenderer != null)
            {
                Color c = _attackFlashRenderer.color;
                c.a = 0f;
                _attackFlashRenderer.color = c;
            }

            if (_input != null)
            {
                _input.OnAttackPerformed += HandleAttackInput;
                _input.OnInteractPerformed += HandleInteractInput;
                _input.OnDashPerformed += HandleDashInput;
            }

            UpdateFacingVisuals(Vector2.zero);
        }

        private void OnDestroy()
        {
            UnsubscribeInput();
            _punchTween.Stop();
            _flashAlphaTween.Stop();
            _flashScaleTween.Stop();
        }

        public void SetControlsEnabled(bool enabled)
        {
            _controlsEnabled = enabled;
            if (!enabled)
            {
                _dashTimer = 0f;
                _knockbackVelocity = Vector2.zero;
                _accumulatedWindVelocity = Vector2.zero;
                if (_rb != null)
                {
                    _rb.linearVelocity = Vector2.zero;
                }

                NotifyPromptChanged(null, false);
            }
        }

        public void TeleportTo(Vector2 worldPos)
        {
            _dashTimer = 0f;
            _knockbackVelocity = Vector2.zero;
            _accumulatedWindVelocity = Vector2.zero;
            _hitInvulnTimer = 0.5f;

            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.position = worldPos;
            }

            transform.position = new Vector3(worldPos.x, worldPos.y, transform.position.z);
            _cameraRig?.SnapToTarget();
        }

        public void ApplyWindExposure(float flameDrainDelta, Vector2 windPushVelocity)
        {
            if (!_controlsEnabled)
            {
                return;
            }

            if (_dashTimer <= 0f)
            {
                _accumulatedWindVelocity += windPushVelocity;
            }

            if (flameDrainDelta > 0f)
            {
                _flame?.DrainFlame(flameDrainDelta);
            }
        }

        public void ApplyHazardHit(float flameLoss, Vector2 knockbackImpulse, bool bypassInvulnerability = false)
        {
            if (!bypassInvulnerability && _hitInvulnTimer > 0f)
            {
                return;
            }

            _hitInvulnTimer = 0.45f;
            _knockbackVelocity += knockbackImpulse;

            if (flameLoss > 0f)
            {
                _flame?.DrainFlame(flameLoss);
            }

            if (_bodyVisual != null)
            {
                _punchTween.Stop();
                _bodyVisual.localScale = _baseBodyScale;
                _punchTween = Tween.PunchScale(_bodyVisual, new Vector3(0.28f, -0.26f, 0f), 0.25f);
            }

            _cameraRig?.Shake(0.28f, 0.22f);
        }

        public void TriggerAttack()
        {
            if (!_controlsEnabled || _attackCooldownTimer > 0f)
            {
                return;
            }

            _attackCooldownTimer = _attackCooldown;
            Vector2 origin = Position + _facingDirection * _attackRange;

            if (_attackFlashRenderer != null)
            {
                Transform flashTf = _attackFlashRenderer.transform;
                flashTf.position = new Vector3(origin.x, origin.y, flashTf.position.z);
                flashTf.localScale = Vector3.one * 0.6f;

                Color startColor = new Color(1f, 0.72f, 0.22f, 0.85f);
                _attackFlashRenderer.color = startColor;

                _flashAlphaTween.Stop();
                _flashScaleTween.Stop();
                _flashScaleTween = Tween.Scale(flashTf, Vector3.one * 1.9f, 0.16f, Ease.OutQuad);
                _flashAlphaTween = Tween.Alpha(_attackFlashRenderer, 0f, 0.18f, Ease.OutQuad);
            }

            if (_bodyVisual != null)
            {
                _punchTween.Stop();
                _bodyVisual.localScale = _baseBodyScale;
                _punchTween = Tween.PunchScale(_bodyVisual, new Vector3(0.2f, 0.2f, 0f), 0.18f);
            }

            int count = Physics2D.OverlapCircle(origin, _attackRadius, ContactFilter2D.noFilter, _attackHitsBuffer);
            _uniqueDamageables.Clear();

            for (int i = 0; i < count; i++)
            {
                Collider2D hit = _attackHitsBuffer[i];
                if (hit == null)
                {
                    continue;
                }

                IDamageable damageable = hit.GetComponent<IDamageable>() ?? hit.GetComponentInParent<IDamageable>();
                if (damageable != null && damageable.IsAlive && _uniqueDamageables.Add(damageable))
                {
                    Vector2 pushDir = (damageable.WorldPosition - Position).sqrMagnitude > 0.001f
                        ? (damageable.WorldPosition - Position).normalized
                        : _facingDirection;
                    damageable.TakeHit(1, pushDir * 7.5f);
                    _cameraRig?.Shake(0.14f, 0.12f);
                }
            }
        }

        public void TriggerDash()
        {
            if (!_controlsEnabled || _dashCooldownTimer > 0f || _dashTimer > 0f)
            {
                return;
            }

            if (_flame == null || _flame.CurrentFlame <= _dashFlameCost + 2f)
            {
                return;
            }

            if (!_flame.TryConsumeFlame(_dashFlameCost))
            {
                return;
            }

            Vector2 move = _input != null ? _input.Move : Vector2.zero;
            _dashDirection = move.sqrMagnitude > 0.01f ? move.normalized : _facingDirection;
            _dashTimer = _dashDuration;
            _dashCooldownTimer = _dashCooldown;

            if (_bodyVisual != null)
            {
                _punchTween.Stop();
                _bodyVisual.localScale = _baseBodyScale;
                _punchTween = Tween.PunchScale(_bodyVisual, new Vector3(0.26f, -0.22f, 0f), 0.2f);
            }
        }

        public void TriggerInteract()
        {
            if (!_controlsEnabled || _nearestInteractable == null)
            {
                return;
            }

            if (_nearestInteractable.IsAvailable && _nearestInteractable.CanInteract())
            {
                _nearestInteractable.Interact();
                RefreshNearestInteractable();
            }
        }

        private void Update()
        {
            if (_dashCooldownTimer > 0f) _dashCooldownTimer -= Time.deltaTime;
            if (_attackCooldownTimer > 0f) _attackCooldownTimer -= Time.deltaTime;
            if (_hitInvulnTimer > 0f) _hitInvulnTimer -= Time.deltaTime;

            if (!_controlsEnabled)
            {
                return;
            }

            Vector2 move = _input != null ? _input.Move : Vector2.zero;
            if (move.sqrMagnitude > 0.01f)
            {
                _facingDirection = move.normalized;
                _stepPulseTime += Time.deltaTime * 14f;
                if (_bodyVisual != null && !_punchTween.isAlive)
                {
                    float squash = Mathf.Sin(_stepPulseTime) * 0.06f;
                    _bodyVisual.localScale = new Vector3(
                        _baseBodyScale.x * (1f - squash),
                        _baseBodyScale.y * (1f + squash),
                        _baseBodyScale.z);
                }
            }
            else if (_bodyVisual != null && !_punchTween.isAlive)
            {
                _bodyVisual.localScale = Vector3.Lerp(_bodyVisual.localScale, _baseBodyScale, Time.deltaTime * 12f);
            }

            UpdateFacingVisuals(_accumulatedWindVelocity);
            RefreshNearestInteractable();
        }

        private void FixedUpdate()
        {
            if (_rb == null)
            {
                return;
            }

            if (!_controlsEnabled)
            {
                _rb.linearVelocity = Vector2.zero;
                _accumulatedWindVelocity = Vector2.zero;
                return;
            }

            Vector2 velocity;
            if (_dashTimer > 0f)
            {
                _dashTimer -= Time.fixedDeltaTime;
                velocity = _dashDirection * _dashSpeed;
            }
            else
            {
                Vector2 move = _input != null ? _input.Move : Vector2.zero;
                if (move.sqrMagnitude > 1f)
                {
                    move.Normalize();
                }

                velocity = move * _moveSpeed;
            }

            velocity += _knockbackVelocity + _accumulatedWindVelocity;
            _rb.linearVelocity = velocity;

            _knockbackVelocity = Vector2.MoveTowards(_knockbackVelocity, Vector2.zero, 22f * Time.fixedDeltaTime);
            _accumulatedWindVelocity = Vector2.zero;
        }

        private void RefreshNearestInteractable()
        {
            if (_interactables == null)
            {
                _nearestInteractable = null;
                NotifyPromptChanged(null, false);
                return;
            }

            Vector2 pos = Position;
            IInteractable best = null;
            float bestScore = float.MaxValue;

            for (int i = 0; i < _interactables.Count; i++)
            {
                IInteractable candidate = _interactables[i];
                if (candidate == null || !candidate.IsAvailable)
                {
                    continue;
                }

                float dist = Vector2.Distance(pos, candidate.WorldPosition);
                if (dist > _interactRadius)
                {
                    continue;
                }

                bool canUse = candidate.CanInteract();
                float score = dist + (canUse ? 0f : 10f);
                if (score < bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }

            _nearestInteractable = best;
            if (best == null)
            {
                NotifyPromptChanged(null, false);
            }
            else
            {
                bool canUse = best.CanInteract();
                NotifyPromptChanged(best.GetPromptKey(canUse), canUse);
            }
        }

        private void NotifyPromptChanged(string promptKey, bool canInteract)
        {
            if (string.Equals(_lastPromptKey, promptKey, StringComparison.Ordinal) && _lastCanInteract == canInteract)
            {
                return;
            }

            _lastPromptKey = promptKey;
            _lastCanInteract = canInteract;
            OnNearestInteractableChanged?.Invoke(promptKey, canInteract);
        }

        private void UpdateFacingVisuals(Vector2 windPush)
        {
            if (_facingIndicator != null)
            {
                _facingIndicator.localPosition = new Vector3(_facingDirection.x * 0.55f, _facingDirection.y * 0.55f, 0f);
            }

            if (_lightView != null)
            {
                Vector2 moveTilt = -_facingDirection * (_input != null && _input.Move.sqrMagnitude > 0.01f ? 0.05f : 0f);
                Vector2 windTilt = windPush.sqrMagnitude > 0.1f ? windPush.normalized * 0.11f : Vector2.zero;
                _lightView.SetTiltOffset(moveTilt + windTilt);
            }
        }

        private void HandleAttackInput() => TriggerAttack();
        private void HandleInteractInput() => TriggerInteract();
        private void HandleDashInput() => TriggerDash();

        private void UnsubscribeInput()
        {
            if (_input == null)
            {
                return;
            }

            _input.OnAttackPerformed -= HandleAttackInput;
            _input.OnInteractPerformed -= HandleInteractInput;
            _input.OnDashPerformed -= HandleDashInput;
        }
    }
}
