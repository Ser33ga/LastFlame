using System;
using UnityEngine;

namespace Game.Settings
{
    [Serializable]
    public sealed class GameSettingsData
    {
        public float MasterVolume = 0.85f;
        public float MusicVolume = 0.70f;
        public float SfxVolume = 0.90f;
        public float ScreenShakeIntensity = 1.00f;
        public string Language = "ru";

        public GameSettingsData Clone()
        {
            return new GameSettingsData
            {
                MasterVolume = MasterVolume,
                MusicVolume = MusicVolume,
                SfxVolume = SfxVolume,
                ScreenShakeIntensity = ScreenShakeIntensity,
                Language = Language
            };
        }

        public void Clamp()
        {
            MasterVolume = Mathf.Clamp01(MasterVolume);
            MusicVolume = Mathf.Clamp01(MusicVolume);
            SfxVolume = Mathf.Clamp01(SfxVolume);
            ScreenShakeIntensity = Mathf.Clamp(ScreenShakeIntensity, 0f, 1.5f);
            Language = string.IsNullOrWhiteSpace(Language) ? "ru" : Language;
        }
    }
}
