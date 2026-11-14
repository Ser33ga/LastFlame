using Game.Character;
using Game.UI;
using PrimeTween;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Game.World
{
    /// <summary>
    /// Updates HUD zone banner and shifts Global Light 2D atmosphere when crossing act boundaries.
    /// </summary>
    public sealed class LocationZoneTrigger : MonoBehaviour
    {
        [SerializeField] private string _zoneTitleKey = "zone.forest";
        [SerializeField] private Color _ambientColor = new Color(0.08f, 0.11f, 0.22f, 1f);
        [SerializeField] private float _ambientIntensity = 0.24f;
        [SerializeField] private bool _showBannerOnEnter = true;

        private CandleController _candle;
        private GameplayHudView _hud;
        private Light2D _globalLight;
        private bool _triggeredRecently;

        public void Construct(CandleController candle, GameplayHudView hud, Light2D globalLight)
        {
            _candle = candle;
            _hud = hud;
            _globalLight = globalLight;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_candle == null || other.gameObject != _candle.gameObject)
            {
                return;
            }

            ApplyZoneAtmosphere();
        }

        public void ApplyZoneAtmosphere()
        {
            _hud?.SetZone(_zoneTitleKey, _showBannerOnEnter && !_triggeredRecently);
            _triggeredRecently = true;

            if (_globalLight != null)
            {
                Color startColor = _globalLight.color;
                float startIntensity = _globalLight.intensity;
                Tween.Custom(
                    this,
                    0f,
                    1f,
                    0.85f,
                    (target, t) =>
                    {
                        if (target._globalLight != null)
                        {
                            target._globalLight.color = Color.Lerp(startColor, target._ambientColor, t);
                            target._globalLight.intensity = Mathf.Lerp(startIntensity, target._ambientIntensity, t);
                        }
                    });
            }
        }
    }
}
