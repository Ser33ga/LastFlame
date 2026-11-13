using System;

namespace Game.Settings
{
    public interface ISettingsService
    {
        GameSettingsData Current { get; }
        event Action<GameSettingsData> OnSettingsChanged;
        void Apply(GameSettingsData settings);
        void Save();
        void ResetToDefaults();
    }
}
