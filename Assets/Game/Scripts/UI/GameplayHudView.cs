using System.Text;
using Game.Core;
using PrimeTween;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// Top-down HUD displaying flame level, current location, collected quest items, and interaction prompts.
    /// </summary>
    public sealed class GameplayHudView : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Image _flameFillImage;
        [SerializeField] private TMP_Text _flamePercentText;
        [SerializeField] private TMP_Text _locationPersistentText;
        [SerializeField] private CanvasGroup _locationBannerGroup;
        [SerializeField] private TMP_Text _locationBannerText;
        [SerializeField] private TMP_Text _inventoryText;
        [SerializeField] private GameObject _promptRoot;
        [SerializeField] private TMP_Text _promptText;
        [SerializeField] private CanvasGroup _toastGroup;
        [SerializeField] private TMP_Text _toastText;
        [SerializeField] private TMP_Text _controlsHintText;
        [SerializeField] private GameObject _hudRoot;

        private static readonly Color FlameHealthyColor = new Color(1f, 0.67f, 0.16f, 1f);
        private static readonly Color FlameCriticalColor = new Color(0.95f, 0.2f, 0.14f, 1f);
        private static readonly Color PromptReadyColor = new Color(1f, 0.94f, 0.68f, 1f);
        private static readonly Color PromptBlockedColor = new Color(1f, 0.5f, 0.42f, 1f);

        private ILocalizationService _localization;
        private Sequence _bannerSequence;
        private Sequence _toastSequence;
        private string _currentZoneKey = "zone.prologue";
        private string _currentPromptKey;
        private bool _currentPromptCanInteract;
        private bool _hasMatch;
        private int _woodPlanks;
        private bool _hasMineKey;
        private bool _hasExplosive;
        private bool _hasLighthouseKey;

        public void Construct(ILocalizationService localization)
        {
            if (_localization != null)
            {
                _localization.OnLanguageChanged -= HandleLanguageChanged;
            }

            _localization = localization;
            if (_localization != null)
            {
                _localization.OnLanguageChanged += HandleLanguageChanged;
            }

            if (_locationBannerGroup != null)
            {
                _locationBannerGroup.alpha = 0f;
            }

            if (_toastGroup != null)
            {
                _toastGroup.alpha = 0f;
            }

            SetInteractionPrompt(null, false);
            RefreshStaticLabels();
        }

        private void OnDestroy()
        {
            _bannerSequence.Stop();
            _toastSequence.Stop();
            if (_localization != null)
            {
                _localization.OnLanguageChanged -= HandleLanguageChanged;
            }
        }

        public void SetHudVisible(bool visible)
        {
            if (_hudRoot != null)
            {
                _hudRoot.SetActive(visible);
            }
        }

        public void SetFlame(float current, float max)
        {
            float normalized = max > 0.001f ? Mathf.Clamp01(current / max) : 0f;
            if (_flameFillImage != null)
            {
                if (_flameFillImage.type == UnityEngine.UI.Image.Type.Filled)
                {
                    _flameFillImage.fillAmount = normalized;
                }
                else
                {
                    RectTransform rt = _flameFillImage.rectTransform;
                    rt.anchorMax = new Vector2(normalized, rt.anchorMax.y);
                }

                float dangerT = Mathf.Clamp01(normalized / 0.35f);
                _flameFillImage.color = Color.Lerp(FlameCriticalColor, FlameHealthyColor, dangerT);
            }

            if (_flamePercentText != null)
            {
                int percent = Mathf.CeilToInt(Mathf.Clamp(current, 0f, max));
                string label = _localization != null ? _localization.Get("hud.flame_label") : "Flame";
                _flamePercentText.text = $"{label}: {percent}%";
            }
        }

        public void SetZone(string zoneTitleKey, bool showBanner = true)
        {
            if (string.IsNullOrWhiteSpace(zoneTitleKey))
            {
                return;
            }

            _currentZoneKey = zoneTitleKey;
            string localizedZone = _localization != null ? _localization.Get(zoneTitleKey) : zoneTitleKey;

            if (_locationPersistentText != null)
            {
                _locationPersistentText.text = localizedZone;
            }

            if (showBanner && _locationBannerGroup != null && _locationBannerText != null)
            {
                _locationBannerText.text = localizedZone;
                _bannerSequence.Stop();
                _locationBannerGroup.alpha = 0f;
                _bannerSequence = Sequence.Create(useUnscaledTime: true)
                    .Chain(Tween.Alpha(_locationBannerGroup, 1f, 0.35f, Ease.OutQuad, useUnscaledTime: true))
                    .ChainDelay(1.7f)
                    .Chain(Tween.Alpha(_locationBannerGroup, 0f, 0.55f, Ease.InQuad, useUnscaledTime: true));
            }
        }

        public void SetInventoryState(bool hasMatch, int woodPlanks, bool hasExplosive, bool hasLighthouseKey, bool hasMineKey = false)
        {
            _hasMatch = hasMatch;
            _woodPlanks = woodPlanks;
            _hasMineKey = hasMineKey;
            _hasExplosive = hasExplosive;
            _hasLighthouseKey = hasLighthouseKey;
            RefreshInventoryText();
        }

        public void SetInteractionPrompt(string promptKey, bool canInteract)
        {
            _currentPromptKey = promptKey;
            _currentPromptCanInteract = canInteract;

            bool hasPrompt = !string.IsNullOrEmpty(promptKey);
            if (_promptRoot != null)
            {
                _promptRoot.SetActive(hasPrompt);
            }

            if (hasPrompt && _promptText != null)
            {
                string message = _localization != null ? _localization.Get(promptKey) : promptKey;
                _promptText.text = canInteract ? $"[E] {message}" : message;
                _promptText.color = canInteract ? PromptReadyColor : PromptBlockedColor;
            }
        }

        public void ShowToast(string toastKey)
        {
            if (string.IsNullOrEmpty(toastKey) || _toastGroup == null || _toastText == null)
            {
                return;
            }

            _toastText.text = _localization != null ? _localization.Get(toastKey) : toastKey;
            _toastSequence.Stop();
            _toastGroup.alpha = 0f;
            _toastSequence = Sequence.Create(useUnscaledTime: true)
                .Chain(Tween.Alpha(_toastGroup, 1f, 0.22f, Ease.OutQuad, useUnscaledTime: true))
                .ChainDelay(1.8f)
                .Chain(Tween.Alpha(_toastGroup, 0f, 0.45f, Ease.InQuad, useUnscaledTime: true));
        }

        public void RefreshStaticLabels()
        {
            if (_controlsHintText != null && _localization != null)
            {
                _controlsHintText.text = _localization.Get("hud.controls_hint");
            }

            SetZone(_currentZoneKey, showBanner: false);
            RefreshInventoryText();
            SetInteractionPrompt(_currentPromptKey, _currentPromptCanInteract);
        }

        private void RefreshInventoryText()
        {
            if (_inventoryText == null)
            {
                return;
            }

            if (_localization == null)
            {
                _inventoryText.text = string.Empty;
                return;
            }

            StringBuilder sb = new StringBuilder();
            if (_hasMatch)
            {
                sb.Append(_localization.Get("item.match"));
            }

            if (_woodPlanks > 0)
            {
                if (sb.Length > 0) sb.Append("  |  ");
                sb.Append(_localization.Get("item.planks", _woodPlanks));
            }

            if (_hasMineKey)
            {
                if (sb.Length > 0) sb.Append("  |  ");
                sb.Append(_localization.Get("item.mine_key"));
            }

            if (_hasExplosive)
            {
                if (sb.Length > 0) sb.Append("  |  ");
                sb.Append(_localization.Get("item.explosive"));
            }

            if (_hasLighthouseKey)
            {
                if (sb.Length > 0) sb.Append("  |  ");
                sb.Append(_localization.Get("item.lighthouse_key"));
            }

            _inventoryText.text = sb.Length > 0
                ? $"{_localization.Get("hud.inventory_prefix")} {sb}"
                : _localization.Get("hud.inventory_empty");
        }

        private void HandleLanguageChanged(string _)
        {
            RefreshStaticLabels();
        }
    }
}
