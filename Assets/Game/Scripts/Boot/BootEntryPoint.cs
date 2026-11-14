using System;
using Cysharp.Threading.Tasks;
using Game.Core;
using Game.Input;
using Game.Settings;
using VContainer.Unity;

namespace Game.Boot
{
    /// <summary>
    /// Root entry point initializing core services and loading the Gameplay scene from Boot.
    /// </summary>
    public sealed class BootEntryPoint : IStartable
    {
        private readonly ISettingsService _settings;
        private readonly IScreenFader _screenFader;
        private readonly IInputService _input;
        private readonly ISceneLoader _sceneLoader;
        private readonly BootLifetimeScope _bootScope;

        public BootEntryPoint(
            ISettingsService settings,
            IScreenFader screenFader,
            IInputService input,
            ISceneLoader sceneLoader,
            BootLifetimeScope bootScope)
        {
            _settings = settings;
            _screenFader = screenFader;
            _input = input;
            _sceneLoader = sceneLoader;
            _bootScope = bootScope;
        }

        public void Start()
        {
            _input.SetCursorLock(false);
            _settings.Apply(_settings.Current);
            _screenFader.SetAlpha(0f);

            if (string.Equals(_sceneLoader.GetActiveSceneName(), "Boot", StringComparison.Ordinal))
            {
                LoadGameplayAsync().Forget();
            }
        }

        private async UniTaskVoid LoadGameplayAsync()
        {
            using (LifetimeScope.EnqueueParent(_bootScope))
            {
                await _sceneLoader.LoadSceneAsync("Gameplay", useFader: true, fadeDuration: 0.25f);
            }
        }
    }
}
