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
    /// Root DI container registering neutral core services without singletons.
    /// </summary>
    public sealed class BootLifetimeScope : LifetimeScope
    {
        [SerializeField] private InputActionAsset _inputActions;
        [SerializeField] private ScreenFader _screenFader;

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
            builder.Register<SettingsService>(Lifetime.Singleton).As<ISettingsService>();
            builder.Register<InputService>(Lifetime.Singleton)
                .WithParameter(_inputActions)
                .As<IInputService>();

            ScreenFader fader = EnsureScreenFader();
            builder.RegisterComponent(fader).As<IScreenFader>().AsSelf();

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
    }
}
