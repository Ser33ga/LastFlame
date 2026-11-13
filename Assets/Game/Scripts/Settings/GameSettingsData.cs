using System;
using UnityEngine;

namespace Game.Settings
{
    [Serializable]
    public sealed class GameSettingsData
    {
        public float MasterVolume = 1.0f;
        public float MusicVolume = 0.8f;
        public float SfxVolume = 1.0f;
        public string Language = "ru";

        public GameSettingsData Clone()
        {
            return new GameSettingsData
            {
                MasterVolume = MasterVolume,
                MusicVolume = MusicVolume,
                SfxVolume = SfxVolume,
                Language = Language
            };
        }

        public void Clamp()
        {
            MasterVolume = Mathf.Clamp01(MasterVolume);
            MusicVolume = Mathf.Clamp01(MusicVolume);
            SfxVolume = Mathf.Clamp01(SfxVolume);
            Language = string.IsNullOrWhiteSpace(Language) ? "ru" : Language;
        }
    }
}
