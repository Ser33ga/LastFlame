namespace Game.Core
{
    public enum GameSoundId
    {
        UiClick = 0,
        CandleIgnite = 1,
        TorchCheckpoint = 2,
        GaleHit = 3,
        FlareDash = 4,
        FlameExtinguish = 5,
        BeaconLit = 6
    }

    public interface IAudioService
    {
        void PlaySfx(GameSoundId soundId, float volumeScale = 1f, float pitch = 1f);
        void StartAmbience();
        void StopAmbience();
        void ApplyVolumes(float masterVolume, float musicVolume, float sfxVolume);
    }
}
