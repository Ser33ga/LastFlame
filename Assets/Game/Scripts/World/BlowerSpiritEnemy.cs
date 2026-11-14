using Game.Character;
using Game.Core;
using PrimeTween;
using UnityEngine;

namespace Game.World
{
    /// <summary>
    /// Stationary Wind Spirit ("Blower") that telegraphs and blasts a directional wind jet blocked by covers.
    /// Can be temporarily stunned with the candle's flame strike.
    /// </summary>
    public sealed class BlowerSpiritEnemy : MonoBehaviour, IDamageable
    {
        private enum BlowerPhase
        {
            Idle,
            Telegraph,
            Blow,
            Stunned
        }

        [SerializeField] private Vector2 _aimDirection = Vector2.left;
        [SerializeField] private bool _lockAimOnTelegraph;
        [SerializeField] private float _beamLength = 6.5f;
        [SerializeField] private float _beamWidth = 2.0f;
        [SerializeField] private float _pushSpeed = 5.4f;
        [SerializeField] private float _flameDrainPerSecond = 28f;
        [SerializeField] private float _idleDuration = 1.5f;
        [SerializeField] private float _telegraphDuration = 0.8f;
        [SerializeField] private float _blowDuration = 1.75f;
        [SerializeField] private float _stunDuration = 2.5f;
        [SerializeField] private float _phaseOffset;
        [SerializeField] private LayerMask _wallMask;

        [SerializeField] private Transform _orbitRingVisual;
        [SerializeField] private Transform _coreVisual;
        [SerializeField] private Transform _beamRoot;
        [SerializeField] private SpriteRenderer _beamRenderer;
        [SerializeField] private SpriteRenderer _coreRenderer;

        private static readonly Color CoreIdleColor = new Color(0.32f, 0.78f, 0.96f, 1f);
        private static readonly Color CoreBlowColor = new Color(0.68f, 0.96f, 1f, 1f);
        private static readonly Color CoreStunColor = new Color(0.45f, 0.50f, 0.58f, 0.7f);

        private CandleController _candle;
        private BlowerPhase _phase = BlowerPhase.Idle;
        private float _phaseTimer;
        private Vector2 _currentDirection;
        private Vector3 _baseCoreScale = Vector3.one;
        private Tween _hitTween;

        public bool IsAlive => _phase != BlowerPhase.Stunned;
        public Vector2 WorldPosition => transform.position;
        public bool IsBlowing => _phase == BlowerPhase.Blow;

        public void Construct(CandleController candle, LayerMask wallMask)
        {
            _candle = candle;
            _wallMask = wallMask;
            _currentDirection = _aimDirection.sqrMagnitude > 0.001f ? _aimDirection.normalized : Vector2.left;
            _phaseTimer = _phaseOffset;

            if (_coreVisual != null)
            {
                _baseCoreScale = _coreVisual.localScale;
            }

            AlignBeamVisual();
            RefreshVisuals(0f);
        }

        private void OnDestroy()
        {
            _hitTween.Stop();
        }

        public void TakeHit(int damage, Vector2 knockbackImpulse)
        {
            if (_phase == BlowerPhase.Stunned)
            {
                return;
            }

            _phase = BlowerPhase.Stunned;
            _phaseTimer = _stunDuration;
            if (_coreVisual != null)
            {
                _hitTween.Stop();
                _coreVisual.localScale = _baseCoreScale;
                _hitTween = Tween.PunchScale(_coreVisual, new Vector3(-0.3f, -0.3f, 0f), 0.25f);
            }
        }

        private void Update()
        {
            if (_orbitRingVisual != null)
            {
                float rotSpeed = _phase switch
                {
                    BlowerPhase.Blow => 260f,
                    BlowerPhase.Telegraph => 140f,
                    BlowerPhase.Stunned => 20f,
                    _ => 75f
                };
                _orbitRingVisual.Rotate(0f, 0f, rotSpeed * Time.deltaTime);
            }

            UpdateCycle(Time.deltaTime);
            AlignBeamVisual();
            RefreshVisuals(Time.time);
        }

        private void FixedUpdate()
        {
            if (_candle == null || _phase != BlowerPhase.Blow)
            {
                return;
            }

            if (IsTargetInActiveStream(_candle.Position))
            {
                _candle.ApplyWindExposure(_flameDrainPerSecond * Time.fixedDeltaTime, _currentDirection * _pushSpeed);
            }
        }

        public bool IsTargetInActiveStream(Vector2 targetPos)
        {
            if (_phase != BlowerPhase.Blow)
            {
                return false;
            }

            Vector2 delta = targetPos - (Vector2)transform.position;
            float forwardDist = Vector2.Dot(delta, _currentDirection);
            if (forwardDist < 0f || forwardDist > _beamLength)
            {
                return false;
            }

            Vector2 perp = new Vector2(-_currentDirection.y, _currentDirection.x);
            float lateralDist = Mathf.Abs(Vector2.Dot(delta, perp));
            if (lateralDist > _beamWidth * 0.5f)
            {
                return false;
            }

            RaycastHit2D hit = Physics2D.Linecast(transform.position, targetPos, _wallMask);
            return hit.collider == null;
        }

        private void UpdateCycle(float dt)
        {
            if (_phase == BlowerPhase.Stunned)
            {
                _phaseTimer -= dt;
                if (_phaseTimer <= 0f)
                {
                    _phase = BlowerPhase.Idle;
                    _phaseTimer = _idleDuration;
                }

                return;
            }

            _phaseTimer -= dt;
            if (_phaseTimer > 0f)
            {
                return;
            }

            switch (_phase)
            {
                case BlowerPhase.Idle:
                    _phase = BlowerPhase.Telegraph;
                    _phaseTimer = _telegraphDuration;
                    if (_lockAimOnTelegraph && _candle != null)
                    {
                        Vector2 toPlayer = _candle.Position - (Vector2)transform.position;
                        if (toPlayer.sqrMagnitude > 0.01f && toPlayer.magnitude <= _beamLength + 2.5f)
                        {
                            _currentDirection = toPlayer.normalized;
                        }
                    }
                    break;
                case BlowerPhase.Telegraph:
                    _phase = BlowerPhase.Blow;
                    _phaseTimer = _blowDuration;
                    break;
                case BlowerPhase.Blow:
                    _phase = BlowerPhase.Idle;
                    _phaseTimer = _idleDuration;
                    break;
            }
        }

        private void AlignBeamVisual()
        {
            if (_beamRoot == null)
            {
                return;
            }

            float angle = Mathf.Atan2(_currentDirection.y, _currentDirection.x) * Mathf.Rad2Deg;
            _beamRoot.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void RefreshVisuals(float time)
        {
            if (_coreRenderer != null)
            {
                _coreRenderer.color = _phase switch
                {
                    BlowerPhase.Blow => CoreBlowColor,
                    BlowerPhase.Telegraph => Color.Lerp(CoreIdleColor, CoreBlowColor, 0.5f + 0.5f * Mathf.Sin(time * 18f)),
                    BlowerPhase.Stunned => CoreStunColor,
                    _ => CoreIdleColor
                };
            }

            if (_beamRenderer != null)
            {
                Color c = _phase switch
                {
                    BlowerPhase.Telegraph => new Color(0.55f, 0.92f, 1f, 0.22f + 0.1f * Mathf.Sin(time * 16f)),
                    BlowerPhase.Blow => new Color(0.65f, 0.96f, 1f, 0.62f + 0.12f * Mathf.Sin(time * 24f)),
                    _ => new Color(0.45f, 0.85f, 1f, 0.03f)
                };
                _beamRenderer.color = c;
            }
        }
    }
}
