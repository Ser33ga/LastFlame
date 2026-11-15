using Game.Character;
using Game.Core;
using Game.Flame;
using PrimeTween;
using UnityEngine;

namespace Game.World
{
    public enum ChaserKind
    {
        ForestShadow,
        Crawler
    }

    /// <summary>
    /// Mobile shadow or crawler enemy built from colored primitives. Chases nearby candle and extinguishes flame on hit.
    /// </summary>
    public sealed class ChaserEnemy : MonoBehaviour, IDamageable
    {
        [SerializeField] private ChaserKind _kind = ChaserKind.ForestShadow;
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private SpriteRenderer _bodyRenderer;
        [SerializeField] private Collider2D _bodyCollider;

        [SerializeField] private int _maxHealth = 3;
        [SerializeField] private float _moveSpeed = 1.75f;
        [SerializeField] private float _aggroRadius = 6.5f;
        [SerializeField] private float _contactRadius = 0.85f;
        [SerializeField] private float _contactFlameLoss = 22f;
        [SerializeField] private float _rewardFlameOnKill = 10f;
        [SerializeField] private Vector2 _patrolOffset = new Vector2(2.6f, 0f);

        private CandleController _candle;
        private IFlameService _flame;

        private Vector2 _homePosition;
        private Vector2 _knockbackVelocity;
        private Color _baseColor;
        private int _currentHealth;
        private bool _patrolToOffset = true;
        private float _hitFlashTimer;
        private Tween _deathTween;

        public bool IsAlive => _currentHealth > 0;
        public Vector2 WorldPosition => _rb != null ? _rb.position : (Vector2)transform.position;
        public ChaserKind Kind => _kind;

        public void Construct(CandleController candle, IFlameService flame)
        {
            _candle = candle;
            _flame = flame;
            _homePosition = transform.position;
            _currentHealth = Mathf.Max(1, _maxHealth);

            if (_bodyRenderer != null)
            {
                _baseColor = _bodyRenderer.color;
            }

            if (_rb != null)
            {
                _rb.gravityScale = 0f;
                _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }

        private void OnDestroy()
        {
            _deathTween.Stop();
        }

        public void TakeHit(int damage, Vector2 knockbackImpulse)
        {
            if (!IsAlive)
            {
                return;
            }

            _currentHealth -= Mathf.Max(1, damage);
            _knockbackVelocity += knockbackImpulse;
            _hitFlashTimer = 0.14f;

            if (_bodyRenderer != null)
            {
                _bodyRenderer.color = Color.white;
            }

            if (_visualRoot != null)
            {
                Tween.PunchScale(_visualRoot, new Vector3(0.28f, -0.22f, 0f), 0.18f);
            }

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        private void Update()
        {
            if (_hitFlashTimer > 0f)
            {
                _hitFlashTimer -= Time.deltaTime;
                if (_hitFlashTimer <= 0f && _bodyRenderer != null)
                {
                    _bodyRenderer.color = _baseColor;
                }
            }
        }

        private void FixedUpdate()
        {
            if (!IsAlive || _rb == null)
            {
                return;
            }

            Vector2 pos = _rb.position;
            Vector2 moveVelocity = Vector2.zero;

            if (_candle != null)
            {
                Vector2 toCandle = _candle.Position - pos;
                float dist = toCandle.magnitude;

                if (dist <= _contactRadius)
                {
                    Vector2 pushDir = dist > 0.001f ? toCandle / dist : Vector2.up;
                    _candle.ApplyHazardHit(_contactFlameLoss, pushDir * 7.2f);
                    _knockbackVelocity -= pushDir * 3.5f;
                }
                else if (dist <= _aggroRadius)
                {
                    moveVelocity = (toCandle / dist) * _moveSpeed;
                }
                else
                {
                    moveVelocity = ComputePatrolVelocity(pos);
                }
            }
            else
            {
                moveVelocity = ComputePatrolVelocity(pos);
            }

            _rb.linearVelocity = moveVelocity + _knockbackVelocity;
            _knockbackVelocity = Vector2.MoveTowards(_knockbackVelocity, Vector2.zero, 20f * Time.fixedDeltaTime);
        }

        private Vector2 ComputePatrolVelocity(Vector2 currentPos)
        {
            if (_patrolOffset.sqrMagnitude < 0.05f)
            {
                return Vector2.zero;
            }

            Vector2 target = _patrolToOffset ? _homePosition + _patrolOffset : _homePosition;
            Vector2 toTarget = target - currentPos;
            if (toTarget.magnitude <= 0.2f)
            {
                _patrolToOffset = !_patrolToOffset;
                return Vector2.zero;
            }

            return toTarget.normalized * (_moveSpeed * 0.6f);
        }

        private void Die()
        {
            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.simulated = false;
            }

            if (_bodyCollider != null)
            {
                _bodyCollider.enabled = false;
            }

            _flame?.AddFlame(_rewardFlameOnKill);

            if (_visualRoot != null)
            {
                _deathTween.Stop();
                _deathTween = Tween.Scale(_visualRoot, Vector3.zero, 0.22f, Ease.InBack)
                    .OnComplete(this, target => target.gameObject.SetActive(false));
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
