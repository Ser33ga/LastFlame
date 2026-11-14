using Game.Character;
using Game.UI;
using UnityEngine;

namespace Game.World
{
    /// <summary>
    /// Abyss hazard beside narrow mine platforms that damages flame and returns the candle to safe footing.
    /// </summary>
    public sealed class PitHazardZone : MonoBehaviour
    {
        [SerializeField] private Transform _safeReturnPoint;
        [SerializeField] private float _flameLoss = 25f;

        private CandleController _candle;
        private GameplayHudView _hud;

        public void Construct(CandleController candle, GameplayHudView hud)
        {
            _candle = candle;
            _hud = hud;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_candle == null || other.gameObject != _candle.gameObject)
            {
                return;
            }

            _candle.ApplyHazardHit(_flameLoss, Vector2.zero, bypassInvulnerability: true);
            if (_safeReturnPoint != null)
            {
                _candle.TeleportTo(_safeReturnPoint.position);
            }

            _hud?.ShowToast("toast.pit_fall");
        }
    }
}
