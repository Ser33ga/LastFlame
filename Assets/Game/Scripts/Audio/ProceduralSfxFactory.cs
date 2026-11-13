using System;
using Game.Core;
using UnityEngine;

namespace Game.Audio
{
    /// <summary>
    /// Procedural PCM synthesizer creating lightweight placeholder SFX and warm fire ambience.
    /// </summary>
    internal static class ProceduralSfxFactory
    {
        private const int SampleRate = 22050;

        public static AudioClip CreateClip(GameSoundId soundId)
        {
            return soundId switch
            {
                GameSoundId.UiClick => SynthesizeTone("SFX_UiClick", 0.06f, 640f, 920f, 0.25f, addNoise: false),
                GameSoundId.CandleIgnite => SynthesizeFilteredNoise("SFX_CandleIgnite", 0.28f, 420f, 1180f, 0.35f),
                GameSoundId.TorchCheckpoint => SynthesizeChord("SFX_TorchCheckpoint", 0.55f, new[] { 329.63f, 440f, 659.25f }, 0.30f),
                GameSoundId.GaleHit => SynthesizeFilteredNoise("SFX_GaleHit", 0.36f, 180f, 90f, 0.45f),
                GameSoundId.FlareDash => SynthesizeFilteredNoise("SFX_FlareDash", 0.22f, 880f, 360f, 0.32f),
                GameSoundId.FlameExtinguish => SynthesizeFilteredNoise("SFX_FlameExtinguish", 0.45f, 950f, 140f, 0.40f),
                GameSoundId.BeaconLit => SynthesizeChord("SFX_BeaconLit", 1.15f, new[] { 261.63f, 329.63f, 392.00f, 523.25f }, 0.35f),
                _ => SynthesizeTone("SFX_Default", 0.08f, 520f, 420f, 0.2f, addNoise: false)
            };
        }

        public static AudioClip CreateFireAmbienceLoop()
        {
            const float durationSeconds = 2.5f;
            int samples = Mathf.RoundToInt(SampleRate * durationSeconds);
            float[] data = new float[samples];
            var rng = new System.Random(1337);
            float lowPass = 0f;

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                float seamFade = Mathf.Sin(t * Mathf.PI);
                float white = (float)(rng.NextDouble() * 2.0 - 1.0);
                lowPass = Mathf.Lerp(lowPass, white, 0.06f);
                float rumble = Mathf.Sin(2f * Mathf.PI * 58f * i / SampleRate) * 0.08f;
                float crackle = rng.NextDouble() > 0.996 ? (float)(rng.NextDouble() * 0.45) : 0f;
                data[i] = Mathf.Clamp((lowPass * 0.22f + rumble + crackle) * (0.65f + 0.35f * seamFade), -0.9f, 0.9f);
            }

            AudioClip clip = AudioClip.Create("AMB_CandleCrackle", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip SynthesizeTone(
            string clipName,
            float duration,
            float startFreq,
            float endFreq,
            float gain,
            bool addNoise)
        {
            int samples = Mathf.Max(64, Mathf.RoundToInt(SampleRate * duration));
            float[] data = new float[samples];
            var rng = new System.Random(clipName.GetHashCode());
            float phase = 0f;

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                float env = Mathf.Pow(1f - t, 1.8f);
                float freq = Mathf.Lerp(startFreq, endFreq, t);
                phase += 2f * Mathf.PI * freq / SampleRate;
                float sample = Mathf.Sin(phase);
                if (addNoise)
                {
                    sample = sample * 0.7f + (float)(rng.NextDouble() * 2.0 - 1.0) * 0.3f;
                }

                data[i] = sample * env * gain;
            }

            AudioClip clip = AudioClip.Create(clipName, samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip SynthesizeFilteredNoise(
            string clipName,
            float duration,
            float startFreq,
            float endFreq,
            float gain)
        {
            int samples = Mathf.Max(64, Mathf.RoundToInt(SampleRate * duration));
            float[] data = new float[samples];
            var rng = new System.Random(clipName.GetHashCode());
            float phase = 0f;
            float smoothed = 0f;

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                float attack = Mathf.Clamp01(t / 0.12f);
                float env = attack * Mathf.Pow(1f - t, 1.4f);
                float freq = Mathf.Lerp(startFreq, endFreq, t);
                phase += 2f * Mathf.PI * freq / SampleRate;

                float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
                smoothed = Mathf.Lerp(smoothed, noise, 0.16f);
                float tone = Mathf.Sin(phase) * 0.35f;
                data[i] = (smoothed * 0.75f + tone) * env * gain;
            }

            AudioClip clip = AudioClip.Create(clipName, samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip SynthesizeChord(
            string clipName,
            float duration,
            float[] frequencies,
            float gain)
        {
            int samples = Mathf.Max(64, Mathf.RoundToInt(SampleRate * duration));
            float[] data = new float[samples];
            float invCount = 1f / Mathf.Max(1, frequencies.Length);

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                float attack = Mathf.Clamp01(t / 0.04f);
                float env = attack * Mathf.Pow(1f - t, 2.0f);
                float sum = 0f;

                for (int f = 0; f < frequencies.Length; f++)
                {
                    float phase = 2f * Mathf.PI * frequencies[f] * i / SampleRate;
                    sum += Mathf.Sin(phase);
                }

                data[i] = sum * invCount * env * gain;
            }

            AudioClip clip = AudioClip.Create(clipName, samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
