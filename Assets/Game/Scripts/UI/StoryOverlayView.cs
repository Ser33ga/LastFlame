using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.UI
{
    /// <summary>
    /// Fullscreen prologue and lighthouse finale overlays powered by PrimeTween and UniTask.
    /// </summary>
    public sealed class StoryOverlayView : MonoBehaviour
    {
        [SerializeField] private GameObject _prologueRoot;
        [SerializeField] private CanvasGroup _prologueGroup;
        [SerializeField] private TMP_Text _prologueBodyText;
        [SerializeField] private TMP_Text _prologuePromptText;
        [SerializeField] private UnityEngine.UI.Button _prologueContinueButton;

        [SerializeField] private GameObject _endingRoot;
        [SerializeField] private CanvasGroup _endingGroup;
        [SerializeField] private TMP_Text _endingTitleText;
        [SerializeField] private TMP_Text _endingBodyText;
        [SerializeField] private UnityEngine.UI.Button _playAgainButton;
        [SerializeField] private TMP_Text _playAgainButtonText;

        private ILocalizationService _localization;
        private Action _onRestartRequested;
        private bool _prologueConfirmed;
        private Tween _activeTween;

        public void Construct(ILocalizationService localization, Action onRestartRequested)
        {
            if (_localization != null)
            {
                _localization.OnLanguageChanged -= HandleLanguageChanged;
            }

            _localization = localization;
            _onRestartRequested = onRestartRequested;

            if (_localization != null)
            {
                _localization.OnLanguageChanged += HandleLanguageChanged;
            }

            if (_playAgainButton != null)
            {
                _playAgainButton.onClick.RemoveListener(HandlePlayAgainClicked);
                _playAgainButton.onClick.AddListener(HandlePlayAgainClicked);
            }

            if (_prologueContinueButton != null)
            {
                _prologueContinueButton.onClick.RemoveListener(ConfirmPrologue);
                _prologueContinueButton.onClick.AddListener(ConfirmPrologue);
            }

            HideAllImmediate();
            RefreshTexts();
        }

        private void OnDestroy()
        {
            _activeTween.Stop();
            if (_localization != null)
            {
                _localization.OnLanguageChanged -= HandleLanguageChanged;
            }

            if (_playAgainButton != null)
            {
                _playAgainButton.onClick.RemoveListener(HandlePlayAgainClicked);
            }

            if (_prologueContinueButton != null)
            {
                _prologueContinueButton.onClick.RemoveListener(ConfirmPrologue);
            }
        }

        public void HideAllImmediate()
        {
            _activeTween.Stop();
            if (_prologueRoot != null)
            {
                _prologueRoot.SetActive(false);
            }

            if (_endingRoot != null)
            {
                _endingRoot.SetActive(false);
            }
        }

        public async UniTask ShowPrologueAsync(CancellationToken ct)
        {
            RefreshTexts();
            _prologueConfirmed = false;

            if (_endingRoot != null)
            {
                _endingRoot.SetActive(false);
            }

            if (_prologueRoot != null)
            {
                _prologueRoot.SetActive(true);
            }

            if (_prologueGroup != null)
            {
                _prologueGroup.alpha = 1f;
                _prologueGroup.blocksRaycasts = true;
            }

            // Require a brief read beat before accepting key dismissal.
            await UniTask.Delay(TimeSpan.FromSeconds(0.45), ignoreTimeScale: true, cancellationToken: ct);

            while (!_prologueConfirmed && !ct.IsCancellationRequested)
            {
                if (IsAnyContinueInputPressed())
                {
                    _prologueConfirmed = true;
                    break;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            if (ct.IsCancellationRequested)
            {
                return;
            }

            if (_prologueGroup != null)
            {
                _activeTween.Stop();
                _activeTween = Tween.Alpha(_prologueGroup, 0f, 0.75f, Ease.InOutSine, useUnscaledTime: true);
                await _activeTween;
                if (ct.IsCancellationRequested)
                {
                    return;
                }

                _prologueGroup.blocksRaycasts = false;
            }

            if (_prologueRoot != null)
            {
                _prologueRoot.SetActive(false);
            }
        }

        public async UniTask ShowEndingAsync(CancellationToken ct)
        {
            RefreshTexts();
            if (_prologueRoot != null)
            {
                _prologueRoot.SetActive(false);
            }

            if (_endingRoot != null)
            {
                _endingRoot.SetActive(true);
            }

            if (_endingGroup != null)
            {
                _endingGroup.alpha = 0f;
                _endingGroup.blocksRaycasts = true;
                _activeTween.Stop();
                _activeTween = Tween.Alpha(_endingGroup, 1f, 0.9f, Ease.OutQuad, useUnscaledTime: true);
                await _activeTween;
                if (ct.IsCancellationRequested)
                {
                    return;
                }
            }
        }

        public void ConfirmPrologue()
        {
            _prologueConfirmed = true;
        }

        private void RefreshTexts()
        {
            if (_localization == null)
            {
                return;
            }

            if (_prologueBodyText != null)
            {
                _prologueBodyText.text = _localization.Get("story.prologue.body");
            }

            if (_prologuePromptText != null)
            {
                _prologuePromptText.text = _localization.Get("story.prologue.continue");
            }

            if (_endingTitleText != null)
            {
                _endingTitleText.text = _localization.Get("story.ending.title");
            }

            if (_endingBodyText != null)
            {
                _endingBodyText.text = _localization.Get("story.ending.body");
            }

            if (_playAgainButtonText != null)
            {
                _playAgainButtonText.text = _localization.Get("ui.play_again");
            }
        }

        private static bool IsAnyContinueInputPressed()
        {
            Keyboard kb = Keyboard.current;
            if (kb != null && (kb.anyKey.wasPressedThisFrame))
            {
                return true;
            }

            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                return true;
            }

            Gamepad pad = Gamepad.current;
            if (pad != null && (pad.buttonSouth.wasPressedThisFrame || pad.startButton.wasPressedThisFrame))
            {
                return true;
            }

            return false;
        }

        private void HandlePlayAgainClicked()
        {
            _onRestartRequested?.Invoke();
        }

        private void HandleLanguageChanged(string _)
        {
            RefreshTexts();
        }
    }
}
