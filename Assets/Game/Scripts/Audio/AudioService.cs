using System;
using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game.Audio
{
    /// <summary>
    /// DI audio service playing synthesized SFX and candle ambience through dedicated host sources.
    /// </summary>
    public sealed class AudioService : IAudioService, IDisposable
    {
        private readonly Dictionary<GameSoundId, AudioClip> _clips = new Dictionary<GameSoundId, AudioClip>();
        private readonly GameObject _hostObject;
        private readonly AudioSource _sfxSource;
        private readonly AudioSource _ambienceSource;
        private AudioClip _ambienceClip;

        private float _masterVolume = 0.85f;
        private float _musicVolume = 0.70f;
        private float _sfxVolume = 0.90f;

        public AudioService()
        {
            _hostObject = new GameObject("[AudioServiceHost]");
            UnityEngine.Object.DontDestroyOnLoad(_hostObject);

            _sfxSource = _hostObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _sfxSource.spatialBlend = 0f;

            _ambienceSource = _hostObject.AddComponent<AudioSource>();
            _ambienceSource.playOnAwake = false;
            _ambienceSource.loop = true;
            _ambienceSource.spatialBlend = 0f;

            ApplyVolumes(_masterVolume, _musicVolume, _sfxVolume);
        }

        public void PlaySfx(GameSoundId soundId, float volumeScale = 1f, float pitch = 1f)
        {
            if (_sfxSource == null)
            {
                return;
            }

            if (!_clips.TryGetValue(soundId, out AudioClip clip) || clip == null)
            {
                clip = ProceduralSfxFactory.CreateClip(soundId);
                _clips[soundId] = clip;
            }

            _sfxSource.pitch = Mathf.Clamp(pitch, 0.5f, 2.0f);
            _sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale) * _masterVolume * _sfxVolume);
        }

        public void StartAmbience()
        {
            if (_ambienceSource == null)
            {
                return;
            }

            if (_ambienceClip == null)
            {
                _ambienceClip = ProceduralSfxFactory.CreateFireAmbienceLoop();
                _ambienceSource.clip = _ambienceClip;
            }

            if (!_ambienceSource.isPlaying)
            {
                _ambienceSource.Play();
            }
        }

        public void StopAmbience()
        {
            if (_ambienceSource != null && _ambienceSource.isPlaying)
            {
                _ambienceSource.Stop();
            }
        }

        public void ApplyVolumes(float masterVolume, float musicVolume, float sfxVolume)
        {
            _masterVolume = Mathf.Clamp01(masterVolume);
            _musicVolume = Mathf.Clamp01(musicVolume);
            _sfxVolume = Mathf.Clamp01(sfxVolume);

            if (_ambienceSource != null)
            {
                _ambienceSource.volume = _masterVolume * _musicVolume * 0.45f;
            }
        }

        public void Dispose()
        {
            foreach (AudioClip clip in _clips.Values)
            {
                if (clip != null)
                {
                    UnityEngine.Object.Destroy(clip);
                }
            }

            _clips.Clear();

            if (_ambienceClip != null)
            {
                UnityEngine.Object.Destroy(_ambienceClip);
                _ambienceClip = null;
            }

            if (_hostObject != null)
            {
                UnityEngine.Object.Destroy(_hostObject);
            }
        }
    }
}
