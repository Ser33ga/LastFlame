using Game.Audio;
using Game.Core;
using Game.Input;
using Game.Settings;
using Game.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace Game.Boot
{
    /// <summary>
    /// Root DI container registering global game services without static singletons.
    /// </summary>
    public sealed class BootLifetimeScope : LifetimeScope
    {
        [SerializeField] private InputActionAsset _inputActions;
        [SerializeField] private ScreenFader _screenFader;
        [SerializeField] private GameShellView _gameShellView;

        protected override void Awake()
        {
            base.Awake();
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<LocalizationService>(Lifetime.Singleton).As<ILocalizationService>();
            builder.Register<AudioService>(Lifetime.Singleton).As<IAudioService>();
            builder.Register<SettingsService>(Lifetime.Singleton).As<ISettingsService>();
            builder.Register<InputService>(Lifetime.Singleton)
                .WithParameter(_inputActions)
                .As<IInputService>();

            ScreenFader fader = EnsureScreenFader();
            builder.RegisterComponent(fader).As<IScreenFader>().AsSelf();

            GameShellView shellView = EnsureGameShellView();
            builder.RegisterComponent(shellView).As<IGameShellController>().AsSelf();

            builder.Register<SceneLoader>(Lifetime.Singleton).As<ISceneLoader>();
            builder.RegisterEntryPoint<BootEntryPoint>();
        }

        private ScreenFader EnsureScreenFader()
        {
            if (_screenFader != null)
            {
                return _screenFader;
            }

            _screenFader = GetComponentInChildren<ScreenFader>(true);
            if (_screenFader != null)
            {
                return _screenFader;
            }

            GameObject go = new GameObject("ScreenFader");
            go.transform.SetParent(transform, false);
            _screenFader = go.AddComponent<ScreenFader>();
            return _screenFader;
        }

        private GameShellView EnsureGameShellView()
        {
            if (_gameShellView != null)
            {
                return _gameShellView;
            }

            _gameShellView = GetComponentInChildren<GameShellView>(true);
            if (_gameShellView != null)
            {
                return _gameShellView;
            }

            GameObject go = new GameObject("GameShellView");
            go.transform.SetParent(transform, false);
            _gameShellView = go.AddComponent<GameShellView>();
            return _gameShellView;
        }
    }
}
