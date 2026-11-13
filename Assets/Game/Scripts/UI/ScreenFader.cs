using Cysharp.Threading.Tasks;
using Game.Core;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Full-screen void-night transition fader powered by PrimeTween and UniTask.
    /// </summary>
    public sealed class ScreenFader : MonoBehaviour, IScreenFader
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _overlayImage;

        private Tween _activeTween;

        public float CurrentAlpha => _canvasGroup != null ? _canvasGroup.alpha : 0f;

        private void Awake()
        {
            EnsureUiComponents();
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void OnDestroy()
        {
            _activeTween.Stop();
        }

        public async UniTask FadeInAsync(float duration = 0.4f)
        {
            EnsureUiComponents();
            await AnimateAlphaAsync(0f, duration);
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = false;
            }
        }

        public async UniTask FadeOutAsync(float duration = 0.4f)
        {
            EnsureUiComponents();
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = true;
            }

            await AnimateAlphaAsync(1f, duration);
        }

        public void SetAlpha(float alpha)
        {
            EnsureUiComponents();
            _activeTween.Stop();
            float clamped = Mathf.Clamp01(alpha);
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = clamped;
                _canvasGroup.blocksRaycasts = clamped > 0.01f;
            }
        }

        private async UniTask AnimateAlphaAsync(float targetAlpha, float duration)
        {
            if (_canvasGroup == null)
            {
                return;
            }

            _activeTween.Stop();
            if (duration <= 0.001f)
            {
                _canvasGroup.alpha = targetAlpha;
                return;
            }

            _activeTween = Tween.Alpha(_canvasGroup, targetAlpha, duration, Ease.InOutSine, useUnscaledTime: true);
            await _activeTween;
        }

        private void EnsureUiComponents()
        {
            if (_canvasGroup != null && _overlayImage != null)
            {
                return;
            }

            Canvas canvas = GetComponentInChildren<Canvas>(true);
            if (canvas == null)
            {
                GameObject canvasGo = new GameObject("FadeCanvas");
                canvasGo.transform.SetParent(transform, false);
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 999;
                canvasGo.AddComponent<CanvasScaler>();
            }

            if (_canvasGroup == null)
            {
                _canvasGroup = canvas.GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
                }
            }

            if (_overlayImage == null)
            {
                _overlayImage = canvas.GetComponentInChildren<Image>(true);
                if (_overlayImage == null)
                {
                    GameObject imgGo = new GameObject("Backdrop");
                    imgGo.transform.SetParent(canvas.transform, false);
                    RectTransform rect = imgGo.AddComponent<RectTransform>();
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;
                    _overlayImage = imgGo.AddComponent<Image>();
                }
            }

            _overlayImage.color = Color.black;
            _overlayImage.raycastTarget = true;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = _canvasGroup.alpha > 0.01f;
        }
    }
}
