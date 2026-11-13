using System;
using Game.Core;
using Game.Input;
using Game.Settings;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Game.UI
{
    /// <summary>
    /// Four-state game shell (Title, Playing, Paused, Ended) with localized text, PrimeTween juice,
    /// and a live flame gauge without hardcoded strings or static singletons.
    /// </summary>
    public sealed class GameShellView : MonoBehaviour, IGameShellController
    {
        [Header("Canvas & Panels")]
        [SerializeField] private Canvas _rootCanvas;
        [SerializeField] private GameObject _titlePanel;
        [SerializeField] private GameObject _hudPanel;
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private GameObject _endedPanel;

        [Header("Title Elements")]
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _subtitleText;
        [SerializeField] private TMP_Text _playButtonLabel;
        [SerializeField] private TMP_Text _languageButtonLabel;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _languageButton;

        [Header("HUD Elements")]
        [SerializeField] private TMP_Text _flameLabelText;
        [SerializeField] private TMP_Text _checkpointsText;
        [SerializeField] private TMP_Text _controlsHintText;
        [SerializeField] private Image _flameFillImage;
        [SerializeField] private Button _pauseHudButton;

        [Header("Pause & End Elements")]
        [SerializeField] private TMP_Text _pauseTitleText;
        [SerializeField] private TMP_Text _resumeButtonLabel;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private TMP_Text _endedTitleText;
        [SerializeField] private TMP_Text _endedSubtitleText;
        [SerializeField] private TMP_Text _restartButtonLabel;
        [SerializeField] private Button _restartButton;

        private ILocalizationService _localization;
        private IAudioService _audio;
        private IInputService _input;
        private ISettingsService _settings;

        private float _flamePercent = 100f;
        private int _litTorches;
        private int _totalTorches = 3;
        private bool _uiBuilt;
        private bool _listenersBound;

        public GameShellState CurrentState { get; private set; } = GameShellState.Title;
        public GameOutcome CurrentOutcome { get; private set; } = GameOutcome.None;

        public event Action<GameShellState, GameOutcome> OnStateChanged;

        [Inject]
        public void Construct(
            ILocalizationService localization,
            IAudioService audio,
            IInputService input,
            ISettingsService settings)
        {
            if (_localization != null)
            {
                _localization.OnLanguageChanged -= HandleLanguageChanged;
            }

            if (_input != null)
            {
                _input.OnPausePerformed -= HandlePauseInput;
            }

            _localization = localization;
            _audio = audio;
            _input = input;
            _settings = settings;

            EnsureShellVisuals();
            BindButtonListeners();

            if (_localization != null)
            {
                _localization.OnLanguageChanged += HandleLanguageChanged;
            }

            if (_input != null)
            {
                _input.OnPausePerformed += HandlePauseInput;
            }

            RefreshLocalizedTexts();
            ApplyState(CurrentState, CurrentOutcome, notify: false);
        }

        private void Awake()
        {
            EnsureShellVisuals();
            BindButtonListeners();
        }

        private void OnDestroy()
        {
            if (_localization != null)
            {
                _localization.OnLanguageChanged -= HandleLanguageChanged;
            }

            if (_input != null)
            {
                _input.OnPausePerformed -= HandlePauseInput;
            }

            Time.timeScale = 1f;
        }

        public void SetFlameStatus(float flamePercent, int litTorches, int totalTorches)
        {
            _flamePercent = Mathf.Clamp(flamePercent, 0f, 100f);
            _litTorches = Mathf.Max(0, litTorches);
            _totalTorches = Mathf.Max(1, totalTorches);

            if (_flameFillImage != null)
            {
                float normalized = _flamePercent / 100f;
                _flameFillImage.fillAmount = normalized;
                _flameFillImage.color = Palette.GetFlameColor(normalized);
            }

            RefreshHudCounters();
        }

        public void StartPlay()
        {
            _audio?.PlaySfx(GameSoundId.CandleIgnite);
            _audio?.StartAmbience();
            ApplyState(GameShellState.Playing, GameOutcome.None, notify: true);
        }

        public void TogglePause()
        {
            if (CurrentState == GameShellState.Playing)
            {
                _audio?.PlaySfx(GameSoundId.UiClick);
                ApplyState(GameShellState.Paused, GameOutcome.None, notify: true);
            }
            else if (CurrentState == GameShellState.Paused)
            {
                _audio?.PlaySfx(GameSoundId.UiClick);
                ApplyState(GameShellState.Playing, GameOutcome.None, notify: true);
            }
        }

        public void RestartFromCheckpoint()
        {
            _audio?.PlaySfx(GameSoundId.CandleIgnite);
            SetFlameStatus(100f, _litTorches, _totalTorches);
            ApplyState(GameShellState.Playing, GameOutcome.None, notify: true);
        }

        public void FinishRun(GameOutcome outcome)
        {
            _audio?.PlaySfx(outcome == GameOutcome.BeaconLit ? GameSoundId.BeaconLit : GameSoundId.FlameExtinguish);
            ApplyState(GameShellState.Ended, outcome, notify: true);
        }

        public void ReturnToTitle()
        {
            _audio?.PlaySfx(GameSoundId.UiClick);
            ApplyState(GameShellState.Title, GameOutcome.None, notify: true);
        }

        private void ApplyState(GameShellState nextState, GameOutcome outcome, bool notify)
        {
            CurrentState = nextState;
            CurrentOutcome = outcome;

            if (_titlePanel != null) _titlePanel.SetActive(nextState == GameShellState.Title);
            if (_hudPanel != null) _hudPanel.SetActive(nextState == GameShellState.Playing || nextState == GameShellState.Paused);
            if (_pausePanel != null) _pausePanel.SetActive(nextState == GameShellState.Paused);
            if (_endedPanel != null) _endedPanel.SetActive(nextState == GameShellState.Ended);

            Time.timeScale = nextState == GameShellState.Paused ? 0f : 1f;
            _input?.SetPlayerInputActive(nextState == GameShellState.Playing);

            RefreshLocalizedTexts();
            AnimateActivePanel(nextState);

            if (notify)
            {
                OnStateChanged?.Invoke(CurrentState, CurrentOutcome);
            }
        }

        private void AnimateActivePanel(GameShellState state)
        {
            Transform panel = state switch
            {
                GameShellState.Title => _titlePanel != null ? _titlePanel.transform : null,
                GameShellState.Paused => _pausePanel != null ? _pausePanel.transform : null,
                GameShellState.Ended => _endedPanel != null ? _endedPanel.transform : null,
                _ => null
            };

            if (panel != null && panel.gameObject.activeInHierarchy)
            {
                panel.localScale = Vector3.one * 0.96f;
                Tween.Scale(panel, Vector3.one, 0.22f, Ease.OutBack, useUnscaledTime: true);
            }
        }

        private void ToggleLanguage()
        {
            _audio?.PlaySfx(GameSoundId.UiClick);
            if (_localization == null)
            {
                return;
            }

            string nextLang = string.Equals(_localization.CurrentLanguage, "ru", StringComparison.OrdinalIgnoreCase)
                ? "en"
                : "ru";

            if (_settings != null)
            {
                GameSettingsData updated = _settings.Current.Clone();
                updated.Language = nextLang;
                _settings.Apply(updated);
            }
            else
            {
                _localization.SetLanguage(nextLang);
            }
        }

        private void HandlePauseInput()
        {
            if (CurrentState == GameShellState.Playing || CurrentState == GameShellState.Paused)
            {
                TogglePause();
            }
        }

        private void HandleLanguageChanged(string _)
        {
            RefreshLocalizedTexts();
        }

        private void RefreshLocalizedTexts()
        {
            string Get(string key) => _localization != null ? _localization.Get(key) : key;

            if (_titleText != null) _titleText.text = Get("game.title");
            if (_subtitleText != null) _subtitleText.text = Get("game.subtitle");
            if (_playButtonLabel != null) _playButtonLabel.text = Get("ui.play");
            if (_languageButtonLabel != null) _languageButtonLabel.text = Get("ui.language");
            if (_controlsHintText != null) _controlsHintText.text = Get("ui.controls_hint");
            if (_pauseTitleText != null) _pauseTitleText.text = Get("ui.pause");
            if (_resumeButtonLabel != null) _resumeButtonLabel.text = Get("ui.resume");
            if (_restartButtonLabel != null) _restartButtonLabel.text = Get("ui.restart");

            if (CurrentOutcome == GameOutcome.BeaconLit)
            {
                if (_endedTitleText != null) _endedTitleText.text = Get("ui.victory.title");
                if (_endedSubtitleText != null) _endedSubtitleText.text = Get("ui.victory.subtitle");
            }
            else
            {
                if (_endedTitleText != null) _endedTitleText.text = Get("ui.gameover.title");
                if (_endedSubtitleText != null) _endedSubtitleText.text = Get("ui.gameover.subtitle");
            }

            RefreshHudCounters();
        }

        private void RefreshHudCounters()
        {
            if (_flameLabelText != null)
            {
                int rounded = Mathf.RoundToInt(_flamePercent);
                _flameLabelText.text = _localization != null
                    ? _localization.Get("ui.flame_label", rounded)
                    : $"{rounded}%";
            }

            if (_checkpointsText != null)
            {
                _checkpointsText.text = _localization != null
                    ? _localization.Get("ui.checkpoints_label", _litTorches, _totalTorches)
                    : $"{_litTorches}/{_totalTorches}";
            }
        }

        private void BindButtonListeners()
        {
            if (_listenersBound)
            {
                return;
            }

            _listenersBound = true;
            _playButton?.onClick.AddListener(StartPlay);
            _languageButton?.onClick.AddListener(ToggleLanguage);
            _pauseHudButton?.onClick.AddListener(TogglePause);
            _resumeButton?.onClick.AddListener(TogglePause);
            _restartButton?.onClick.AddListener(RestartFromCheckpoint);
        }

        private void EnsureShellVisuals()
        {
            if (_uiBuilt || _rootCanvas != null)
            {
                return;
            }

            _uiBuilt = true;

            GameObject canvasGo = new GameObject("GameShellCanvas");
            canvasGo.transform.SetParent(transform, false);
            _rootCanvas = canvasGo.AddComponent<Canvas>();
            _rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _rootCanvas.sortingOrder = 50;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            BuildTitlePanel(canvasGo.transform);
            BuildHudPanel(canvasGo.transform);
            BuildPausePanel(canvasGo.transform);
            BuildEndedPanel(canvasGo.transform);
        }

        private void BuildTitlePanel(Transform parent)
        {
            _titlePanel = CreatePanel("TitlePanel", parent, new Vector2(0.05f, 0.56f), new Vector2(0.46f, 0.90f), Palette.PanelBackdrop);

            _titleText = CreateText("TitleText", _titlePanel.transform, 52f, FontStyles.Bold, Palette.FlameCore, TextAlignmentOptions.TopLeft,
                new Vector2(0.06f, 0.62f), new Vector2(0.94f, 0.92f));
            _subtitleText = CreateText("SubtitleText", _titlePanel.transform, 24f, FontStyles.Normal, Palette.CandleWax, TextAlignmentOptions.TopLeft,
                new Vector2(0.06f, 0.42f), new Vector2(0.94f, 0.62f));

            (_playButton, _playButtonLabel) = CreateButton("PlayButton", _titlePanel.transform,
                new Vector2(0.06f, 0.12f), new Vector2(0.56f, 0.34f), Palette.FlameAccent, Palette.VoidNight);
            (_languageButton, _languageButtonLabel) = CreateButton("LanguageButton", _titlePanel.transform,
                new Vector2(0.60f, 0.12f), new Vector2(0.94f, 0.34f), Palette.RuinsStone, Palette.CandleWax);
        }

        private void BuildHudPanel(Transform parent)
        {
            _hudPanel = CreatePanel("HudPanel", parent, Vector2.zero, Vector2.one, Color.clear);

            GameObject flameBox = CreatePanel("FlameBox", _hudPanel.transform, new Vector2(0.03f, 0.87f), new Vector2(0.30f, 0.96f), Palette.PanelBackdrop);
            _flameLabelText = CreateText("FlameLabel", flameBox.transform, 24f, FontStyles.Bold, Palette.FlameCore, TextAlignmentOptions.MidlineLeft,
                new Vector2(0.06f, 0.48f), new Vector2(0.60f, 0.92f));
            _checkpointsText = CreateText("CheckpointsLabel", flameBox.transform, 20f, FontStyles.Normal, Palette.CandleWax, TextAlignmentOptions.MidlineRight,
                new Vector2(0.58f, 0.48f), new Vector2(0.94f, 0.92f));

            GameObject barBg = CreatePanel("FlameBarBg", flameBox.transform, new Vector2(0.06f, 0.16f), new Vector2(0.94f, 0.38f), Palette.ObsidianSilhouette);
            GameObject fillGo = new GameObject("FlameBarFill");
            fillGo.transform.SetParent(barBg.transform, false);
            RectTransform fillRect = fillGo.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            _flameFillImage = fillGo.AddComponent<Image>();
            _flameFillImage.type = Image.Type.Filled;
            _flameFillImage.fillMethod = Image.FillMethod.Horizontal;
            _flameFillImage.color = Palette.FlameCore;

            _controlsHintText = CreateText("ControlsHint", _hudPanel.transform, 18f, FontStyles.Normal, Palette.TextMuted, TextAlignmentOptions.Bottom,
                new Vector2(0.15f, 0.02f), new Vector2(0.85f, 0.08f));

            (Button pauseBtn, TMP_Text pauseBtnText) = CreateButton("PauseButton", _hudPanel.transform,
                new Vector2(0.91f, 0.89f), new Vector2(0.97f, 0.96f), Palette.RuinsStone, Palette.CandleWax);
            _pauseHudButton = pauseBtn;
            pauseBtnText.text = "||";
        }

        private void BuildPausePanel(Transform parent)
        {
            _pausePanel = CreatePanel("PausePanel", parent, new Vector2(0.32f, 0.34f), new Vector2(0.68f, 0.66f), Palette.PanelBackdrop);
            _pauseTitleText = CreateText("PauseTitle", _pausePanel.transform, 42f, FontStyles.Bold, Palette.CandleWax, TextAlignmentOptions.Center,
                new Vector2(0.10f, 0.56f), new Vector2(0.90f, 0.88f));
            (_resumeButton, _resumeButtonLabel) = CreateButton("ResumeButton", _pausePanel.transform,
                new Vector2(0.22f, 0.18f), new Vector2(0.78f, 0.44f), Palette.FlameAccent, Palette.VoidNight);
        }

        private void BuildEndedPanel(Transform parent)
        {
            _endedPanel = CreatePanel("EndedPanel", parent, new Vector2(0.28f, 0.30f), new Vector2(0.72f, 0.70f), Palette.PanelBackdrop);
            _endedTitleText = CreateText("EndedTitle", _endedPanel.transform, 44f, FontStyles.Bold, Palette.FlameCore, TextAlignmentOptions.Center,
                new Vector2(0.08f, 0.62f), new Vector2(0.92f, 0.90f));
            _endedSubtitleText = CreateText("EndedSubtitle", _endedPanel.transform, 22f, FontStyles.Normal, Palette.CandleWax, TextAlignmentOptions.Center,
                new Vector2(0.08f, 0.38f), new Vector2(0.92f, 0.60f));
            (_restartButton, _restartButtonLabel) = CreateButton("RestartButton", _endedPanel.transform,
                new Vector2(0.24f, 0.12f), new Vector2(0.76f, 0.32f), Palette.FlameAccent, Palette.VoidNight);
        }

        private static GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            if (color.a > 0f)
            {
                Image img = go.AddComponent<Image>();
                img.color = color;
            }

            return go;
        }

        private static TMP_Text CreateText(
            string name,
            Transform parent,
            float fontSize,
            FontStyles fontStyle,
            Color color,
            TextAlignmentOptions alignment,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = fontSize;
            tmp.fontStyle = fontStyle;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            if (TMP_Settings.instance != null && TMP_Settings.defaultFontAsset != null)
            {
                tmp.font = TMP_Settings.defaultFontAsset;
            }

            return tmp;
        }

        private static (Button button, TMP_Text label) CreateButton(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Color backgroundColor,
            Color textColor)
        {
            GameObject go = CreatePanel(name, parent, anchorMin, anchorMax, backgroundColor);
            Button button = go.AddComponent<Button>();

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            button.colors = colors;

            TMP_Text label = CreateText("Label", go.transform, 24f, FontStyles.Bold, textColor, TextAlignmentOptions.Center,
                new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.95f));

            return (button, label);
        }
    }
}
