using System;
using Game.Core;
using UnityEngine;

namespace Game.Settings
{
    /// <summary>
    /// Persists user audio, shake, and language preferences and synchronizes them with injected services.
    /// </summary>
    public sealed class SettingsService : ISettingsService
    {
        private const string PlayerPrefsKey = "LastFlame.Settings.v1";

        private readonly ILocalizationService _localizationService;
        private readonly IAudioService _audioService;

        public GameSettingsData Current { get; private set; }

        public event Action<GameSettingsData> OnSettingsChanged;

        public SettingsService(ILocalizationService localizationService, IAudioService audioService)
        {
            _localizationService = localizationService;
            _audioService = audioService;
            Current = LoadOrDefault();
            ApplyInternal(Current, persist: false);
        }

        public void Apply(GameSettingsData settings)
        {
            if (settings == null)
            {
                return;
            }

            ApplyInternal(settings.Clone(), persist: true);
        }

        public void Save()
        {
            if (Current == null)
            {
                return;
            }

            PlayerPrefs.SetString(PlayerPrefsKey, JsonUtility.ToJson(Current));
            PlayerPrefs.Save();
        }

        public void ResetToDefaults()
        {
            ApplyInternal(new GameSettingsData(), persist: true);
        }

        private GameSettingsData LoadOrDefault()
        {
            if (!PlayerPrefs.HasKey(PlayerPrefsKey))
            {
                return new GameSettingsData();
            }

            string json = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new GameSettingsData();
            }

            try
            {
                GameSettingsData loaded = JsonUtility.FromJson<GameSettingsData>(json);
                if (loaded == null)
                {
                    return new GameSettingsData();
                }

                loaded.Clamp();
                return loaded;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SettingsService] Failed to parse saved settings, falling back to defaults: {ex.Message}");
                return new GameSettingsData();
            }
        }

        private void ApplyInternal(GameSettingsData data, bool persist)
        {
            data.Clamp();
            Current = data;

            _audioService?.ApplyVolumes(data.MasterVolume, data.MusicVolume, data.SfxVolume);
            _localizationService?.SetLanguage(data.Language);

            if (persist)
            {
                Save();
            }

            OnSettingsChanged?.Invoke(Current.Clone());
        }
    }
}
