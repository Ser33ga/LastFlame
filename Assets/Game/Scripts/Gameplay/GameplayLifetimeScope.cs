using Game.Character;
using Game.Core;
using Game.Flame;
using Game.Input;
using Game.Settings;
using Game.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay
{
    /// <summary>
    /// Scene DI container for Gameplay. Registers core services locally when run standalone without Boot.
    /// </summary>
    public sealed class GameplayLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameplaySceneReferences _sceneRefs;
        [SerializeField] private InputActionAsset _fallbackInputActions;
        [SerializeField] private ScreenFader _fallbackScreenFader;

        protected override void Configure(IContainerBuilder builder)
        {
            if (Parent == null)
            {
                builder.Register<LocalizationService>(Lifetime.Singleton).As<ILocalizationService>();
                builder.Register<SettingsService>(Lifetime.Singleton).As<ISettingsService>();
                builder.Register<InputService>(Lifetime.Singleton)
                    .WithParameter(_fallbackInputActions)
                    .As<IInputService>();

                ScreenFader fader = EnsureFallbackScreenFader();
                builder.RegisterComponent(fader).As<IScreenFader>().AsSelf();

                builder.Register<SceneLoader>(Lifetime.Singleton).As<ISceneLoader>();
            }

            builder.Register<FlameService>(Lifetime.Singleton).As<IFlameService, ITickable>();
            builder.Register<PlayerInventory>(Lifetime.Singleton).As<IPlayerInventory>();
            builder.Register<CheckpointService>(Lifetime.Singleton).As<ICheckpointService>();

            if (_sceneRefs != null)
            {
                builder.RegisterComponent(_sceneRefs);
            }

            builder.RegisterEntryPoint<GameplayFlowController>();
        }

        private ScreenFader EnsureFallbackScreenFader()
        {
            if (_fallbackScreenFader != null)
            {
                return _fallbackScreenFader;
            }

            _fallbackScreenFader = GetComponentInChildren<ScreenFader>(true);
            if (_fallbackScreenFader != null)
            {
                return _fallbackScreenFader;
            }

            GameObject go = new GameObject("ScreenFader");
            go.transform.SetParent(transform, false);
            _fallbackScreenFader = go.AddComponent<ScreenFader>();
            return _fallbackScreenFader;
        }
    }
}
