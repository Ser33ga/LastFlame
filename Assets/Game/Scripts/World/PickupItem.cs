using Game.Character;
using Game.Core;
using Game.Flame;
using Game.UI;
using PrimeTween;
using UnityEngine;

namespace Game.World
{
    public enum PickupItemKind
    {
        Match,
        WoodPlank,
        Explosive,
        LighthouseKey,
        WaxEmber,
        MineKey
    }

    /// <summary>
    /// Collectible primitive pickup supporting both [E] interaction and trigger touch collection.
    /// </summary>
    public sealed class PickupItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private PickupItemKind _kind;
        [SerializeField] private int _amount = 1;
        [SerializeField] private float _emberFlameAmount = 22f;
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private Collider2D _triggerCollider;

        private IPlayerInventory _inventory;
        private IFlameService _flame;
        private GameplayHudView _hud;
        private CandleController _candle;
        private bool _collected;
        private Tween _pulseTween;

        public bool IsAvailable => !_collected;
        public Vector2 WorldPosition => transform.position;
        public PickupItemKind Kind => _kind;

        public void Construct(
            IPlayerInventory inventory,
            IFlameService flame,
            GameplayHudView hud,
            CandleController candle)
        {
            _inventory = inventory;
            _flame = flame;
            _hud = hud;
            _candle = candle;

            if (_visualRoot != null && !_collected)
            {
                _pulseTween.Stop();
                _pulseTween = Tween.Scale(
                    _visualRoot,
                    _visualRoot.localScale * 1.12f,
                    0.9f,
                    Ease.InOutSine,
                    cycles: -1,
                    cycleMode: CycleMode.Yoyo);
            }
        }

        private void OnDestroy()
        {
            _pulseTween.Stop();
        }

        public bool CanInteract()
        {
            return !_collected;
        }

        public string GetPromptKey(bool canInteract)
        {
            return _kind switch
            {
                PickupItemKind.Match => "prompt.pickup.match",
                PickupItemKind.WoodPlank => "prompt.pickup.plank",
                PickupItemKind.Explosive => "prompt.pickup.explosive",
                PickupItemKind.LighthouseKey => "prompt.pickup.key",
                PickupItemKind.WaxEmber => "prompt.pickup.ember",
                PickupItemKind.MineKey => "prompt.pickup.mine_key",
                _ => "prompt.pickup.generic"
            };
        }

        public void Interact()
        {
            Collect();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected || _candle == null)
            {
                return;
            }

            if (other.gameObject == _candle.gameObject)
            {
                Collect();
            }
        }

        public void Collect()
        {
            if (_collected)
            {
                return;
            }

            _collected = true;
            _pulseTween.Stop();

            if (_triggerCollider != null)
            {
                _triggerCollider.enabled = false;
            }

            switch (_kind)
            {
                case PickupItemKind.Match:
                    _inventory?.GiveMatch();
                    _hud?.ShowToast("toast.got_match");
                    break;
                case PickupItemKind.WoodPlank:
                    _inventory?.AddWoodPlanks(Mathf.Max(1, _amount));
                    _hud?.ShowToast("toast.got_plank");
                    break;
                case PickupItemKind.Explosive:
                    _inventory?.GiveExplosive();
                    _hud?.ShowToast("toast.got_explosive");
                    break;
                case PickupItemKind.LighthouseKey:
                    _inventory?.GiveLighthouseKey();
                    _hud?.ShowToast("toast.got_key");
                    break;
                case PickupItemKind.WaxEmber:
                    _flame?.AddFlame(_emberFlameAmount);
                    _hud?.ShowToast("toast.got_ember");
                    break;
                case PickupItemKind.MineKey:
                    _inventory?.GiveMineKey();
                    _hud?.ShowToast("toast.got_mine_key");
                    break;
            }

            if (_visualRoot != null)
            {
                Tween.Scale(_visualRoot, Vector3.zero, 0.16f, Ease.InBack)
                    .OnComplete(this, target => target.gameObject.SetActive(false));
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
