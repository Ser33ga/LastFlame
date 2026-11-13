using Game.Core;
using Game.Input;
using Game.Settings;
using VContainer.Unity;

namespace Game.Boot
{
    /// <summary>
    /// Neutral entry point initializing base services cleanly on startup.
    /// </summary>
    public sealed class BootEntryPoint : IStartable
    {
        private readonly ISettingsService _settings;
        private readonly IScreenFader _screenFader;
        private readonly IInputService _input;

        public BootEntryPoint(
            ISettingsService settings,
            IScreenFader screenFader,
            IInputService input)
        {
            _settings = settings;
            _screenFader = screenFader;
            _input = input;
        }

        public void Start()
        {
            _input.SetCursorLock(false);
            _settings.Apply(_settings.Current);
            _screenFader.SetAlpha(0f);
        }
    }
}
