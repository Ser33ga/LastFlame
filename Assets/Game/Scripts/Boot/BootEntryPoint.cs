using Cysharp.Threading.Tasks;
using Game.Core;
using Game.Input;
using Game.Settings;
using Game.UI;
using UnityEngine;
using VContainer.Unity;

namespace Game.Boot
{
    /// <summary>
    /// Entry point verifying core DI services on startup and revealing the lit baseline scene.
    /// </summary>
    public sealed class BootEntryPoint : IStartable
    {
        private readonly ISettingsService _settings;
        private readonly IScreenFader _screenFader;
        private readonly IAudioService _audio;
        private readonly IInputService _input;
        private readonly GameShellView _shellView;

        public BootEntryPoint(
            ISettingsService settings,
            IScreenFader screenFader,
            IAudioService audio,
            IInputService input,
            GameShellView shellView)
        {
            _settings = settings;
            _screenFader = screenFader;
            _audio = audio;
            _input = input;
            _shellView = shellView;
        }

        public void Start()
        {
            Application.targetFrameRate = 60;
            _input.SetCursorLock(false);
            _settings.Apply(_settings.Current);
            _shellView.SetFlameStatus(100f, 0, 3);
            _screenFader.SetAlpha(1f);

            RevealSceneAsync().Forget();
        }

        private async UniTaskVoid RevealSceneAsync()
        {
            await _screenFader.FadeInAsync(0.45f);
            _audio.StartAmbience();
        }
    }
}
