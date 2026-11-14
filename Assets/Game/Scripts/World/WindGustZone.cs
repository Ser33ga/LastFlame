using Game.Character;
using UnityEngine;

namespace Game.World
{
    /// <summary>
    /// Cyclic wind gust corridor or broken-window draft. Stone covers on WallMask block the stream via Linecast.
    /// </summary>
    public sealed class WindGustZone : MonoBehaviour
    {
        public enum GustPhase
        {
            Calm,
            Telegraph,
            Active
        }

        [SerializeField] private BoxCollider2D _zoneCollider;
        [SerializeField] private Vector2 _windDirection = Vector2.left;
        [SerializeField] private float _pushSpeed = 4.6f;
        [SerializeField] private float _flameDrainPerSecond = 26f;
        [SerializeField] private float _calmDuration = 1.6f;
        [SerializeField] private float _telegraphDuration = 0.85f;
        [SerializeField] private float _activeDuration = 1.9f;
        [SerializeField] private float _phaseOffset;
        [SerializeField] private LayerMask _wallMask;
        [SerializeField] private SpriteRenderer[] _streamRenderers;

        private static readonly Color CalmColor = new Color(0.45f, 0.85f, 1f, 0.05f);
        private static readonly Color TelegraphColor = new Color(0.55f, 0.90f, 1f, 0.28f);
        private static readonly Color ActiveColor = new Color(0.65f, 0.95f, 1f, 0.62f);

        private CandleController _candle;
        private float _timer;
        private GustPhase _phase = GustPhase.Calm;

        public GustPhase CurrentPhase => _phase;
        public Vector2 WindDirection => _windDirection.sqrMagnitude > 0.001f ? _windDirection.normalized : Vector2.left;

        public void Construct(CandleController candle, LayerMask wallMask)
        {
            _candle = candle;
            _wallMask = wallMask;
            _timer = _phaseOffset;
            AdvanceStateByTime(0f);
            UpdateVisuals(0f);
        }

        public bool IsPlayerInsideAndExposed()
        {
            if (_candle == null || _zoneCollider == null || _phase != GustPhase.Active)
            {
                return false;
            }

            Vector2 candlePos = _candle.Position;
            if (!_zoneCollider.bounds.Contains(new Vector3(candlePos.x, candlePos.y, _zoneCollider.bounds.center.z)))
            {
                return false;
            }

            Vector2 dir = WindDirection;
            Vector2 upwindOrigin = GetUpwindOrigin(candlePos, dir);
            RaycastHit2D wallHit = Physics2D.Linecast(upwindOrigin, candlePos, _wallMask);
            return wallHit.collider == null;
        }

        private void Update()
        {
            AdvanceStateByTime(Time.deltaTime);
            UpdateVisuals(Time.time);
        }

        private void FixedUpdate()
        {
            if (_candle == null || _phase != GustPhase.Active)
            {
                return;
            }

            if (IsPlayerInsideAndExposed())
            {
                _candle.ApplyWindExposure(_flameDrainPerSecond * Time.fixedDeltaTime, WindDirection * _pushSpeed);
            }
        }

        private void AdvanceStateByTime(float dt)
        {
            _timer += dt;
            float cycle = Mathf.Max(0.2f, _calmDuration + _telegraphDuration + _activeDuration);
            float t = Mathf.Repeat(_timer, cycle);

            if (t < _calmDuration)
            {
                _phase = GustPhase.Calm;
            }
            else if (t < _calmDuration + _telegraphDuration)
            {
                _phase = GustPhase.Telegraph;
            }
            else
            {
                _phase = GustPhase.Active;
            }
        }

        private Vector2 GetUpwindOrigin(Vector2 targetPos, Vector2 dir)
        {
            Bounds b = _zoneCollider.bounds;
            float tMax = 100f;
            Vector2 invRay = -dir;

            if (Mathf.Abs(invRay.x) > 0.0001f)
            {
                float tx = ((invRay.x > 0f ? b.max.x : b.min.x) - targetPos.x) / invRay.x;
                tMax = Mathf.Min(tMax, Mathf.Max(0f, tx));
            }

            if (Mathf.Abs(invRay.y) > 0.0001f)
            {
                float ty = ((invRay.y > 0f ? b.max.y : b.min.y) - targetPos.y) / invRay.y;
                tMax = Mathf.Min(tMax, Mathf.Max(0f, ty));
            }

            return targetPos + invRay * Mathf.Max(0.2f, tMax);
        }

        private void UpdateVisuals(float time)
        {
            if (_streamRenderers == null || _streamRenderers.Length == 0)
            {
                return;
            }

            Color targetColor = _phase switch
            {
                GustPhase.Calm => CalmColor,
                GustPhase.Telegraph => Color.Lerp(CalmColor, TelegraphColor, 0.5f + 0.5f * Mathf.Sin(time * 16f)),
                GustPhase.Active => Color.Lerp(TelegraphColor, ActiveColor, 0.65f + 0.35f * Mathf.Sin(time * 24f)),
                _ => CalmColor
            };

            for (int i = 0; i < _streamRenderers.Length; i++)
            {
                if (_streamRenderers[i] != null)
                {
                    _streamRenderers[i].color = targetColor;
                }
            }
        }
    }
}
