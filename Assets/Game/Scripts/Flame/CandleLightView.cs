using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Game.Flame
{
    /// <summary>
    /// Synchronizes the candle's 2D Light2D radius/intensity and primitive flame circle visuals with flame level.
    /// </summary>
    public sealed class CandleLightView : MonoBehaviour
    {
        [SerializeField] private Light2D _pointLight;
        [SerializeField] private Transform _flameOuterVisual;
        [SerializeField] private Transform _flameInnerVisual;
        [SerializeField] private SpriteRenderer _flameOuterRenderer;

        [SerializeField] private float _minOuterRadius = 2.2f;
        [SerializeField] private float _maxOuterRadius = 9.0f;
        [SerializeField] private float _minIntensity = 0.6f;
        [SerializeField] private float _maxIntensity = 1.6f;

        private IFlameService _flameService;
        private Vector3 _baseOuterScale = new Vector3(0.45f, 0.45f, 1f);
        private Vector3 _baseInnerScale = new Vector3(0.22f, 0.22f, 1f);
        private Vector2 _tiltOffset;
        private float _targetNormalized = 1f;
        private float _smoothedNormalized = 1f;

        public void Construct(IFlameService flameService)
        {
            if (_flameService != null)
            {
                _flameService.OnFlameChanged -= HandleFlameChanged;
            }

            _flameService = flameService;
            if (_flameOuterVisual != null)
            {
                _baseOuterScale = _flameOuterVisual.localScale;
            }

            if (_flameInnerVisual != null)
            {
                _baseInnerScale = _flameInnerVisual.localScale;
            }

            if (_flameService != null)
            {
                _flameService.OnFlameChanged += HandleFlameChanged;
                _targetNormalized = _flameService.NormalizedFlame;
                _smoothedNormalized = _targetNormalized;
            }

            ApplyVisuals(0f);
        }

        private void OnDestroy()
        {
            if (_flameService != null)
            {
                _flameService.OnFlameChanged -= HandleFlameChanged;
            }
        }

        public void SetTiltOffset(Vector2 offset)
        {
            _tiltOffset = Vector2.ClampMagnitude(offset, 0.16f);
        }

        private void Update()
        {
            _smoothedNormalized = Mathf.Lerp(_smoothedNormalized, _targetNormalized, Time.deltaTime * 10f);
            float pulse = Mathf.Sin(Time.time * 9.5f) * 0.04f + Mathf.Cos(Time.time * 15.2f) * 0.025f;
            ApplyVisuals(pulse);
        }

        private void HandleFlameChanged(float current, float max)
        {
            _targetNormalized = max > 0.001f ? Mathf.Clamp01(current / max) : 0f;
        }

        private void ApplyVisuals(float flicker)
        {
            float n = Mathf.Clamp01(_smoothedNormalized);
            bool alive = n > 0.002f;

            if (_pointLight != null)
            {
                if (!alive)
                {
                    _pointLight.intensity = 0f;
                    _pointLight.pointLightOuterRadius = 0.1f;
                    _pointLight.pointLightInnerRadius = 0f;
                }
                else
                {
                    float curve = Mathf.Pow(n, 0.78f);
                    _pointLight.pointLightOuterRadius = Mathf.Lerp(_minOuterRadius, _maxOuterRadius, curve) + flicker * 2.5f;
                    _pointLight.pointLightInnerRadius = Mathf.Lerp(0.45f, 2.2f, curve);
                    _pointLight.intensity = Mathf.Lerp(_minIntensity, _maxIntensity, curve) + flicker * 1.1f;
                }
            }

            float visualScale = alive ? Mathf.Lerp(0.35f, 1.05f, n) + flicker * 0.8f : 0f;
            if (_flameOuterVisual != null)
            {
                _flameOuterVisual.localScale = _baseOuterScale * Mathf.Max(0f, visualScale);
                _flameOuterVisual.localPosition = new Vector3(_tiltOffset.x, _tiltOffset.y, 0f);
            }

            if (_flameInnerVisual != null)
            {
                _flameInnerVisual.localScale = _baseInnerScale * Mathf.Max(0f, visualScale);
                _flameInnerVisual.localPosition = new Vector3(_tiltOffset.x * 1.25f, _tiltOffset.y * 1.25f, 0f);
            }

            if (_flameOuterRenderer != null)
            {
                Color low = new Color(1f, 0.36f, 0.12f, 1f);
                Color high = new Color(1f, 0.72f, 0.18f, 1f);
                _flameOuterRenderer.color = Color.Lerp(low, high, n);
            }
        }
    }
}
