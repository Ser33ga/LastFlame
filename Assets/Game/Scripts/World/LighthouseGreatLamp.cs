using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core;
using PrimeTween;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Game.World
{
    /// <summary>
    /// Great extinguished beacon at the Lighthouse summit where the candle sacrifices its flame to relight the world.
    /// </summary>
    public sealed class LighthouseGreatLamp : MonoBehaviour, IInteractable
    {
        [SerializeField] private Light2D _lampLight;
        [SerializeField] private SpriteRenderer _lampCoreRenderer;
        [SerializeField] private Transform _beaconBeamsRoot;
        [SerializeField] private SpriteRenderer[] _beamRenderers;

        private static readonly Color ExtinguishedCoreColor = new Color(0.24f, 0.25f, 0.30f, 1f);
        private static readonly Color IgnitedCoreColor = new Color(1f, 0.92f, 0.56f, 1f);

        private bool _activated;

        public bool IsAvailable => !_activated;
        public bool IsActivated => _activated;
        public Vector2 WorldPosition => transform.position;

        public event Action OnSacrificeRequested;

        public void Construct()
        {
            _activated = false;
            if (_lampCoreRenderer != null)
            {
                _lampCoreRenderer.color = ExtinguishedCoreColor;
            }

            if (_lampLight != null)
            {
                _lampLight.intensity = 0.25f;
                _lampLight.pointLightOuterRadius = 3.2f;
            }

            SetBeamAlpha(0f);
        }

        private void Update()
        {
            if (_activated && _beaconBeamsRoot != null)
            {
                _beaconBeamsRoot.Rotate(0f, 0f, 28f * Time.deltaTime);
            }
        }

        public bool CanInteract()
        {
            return !_activated;
        }

        public string GetPromptKey(bool canInteract)
        {
            return "prompt.lighthouse.ignite";
        }

        public void Interact()
        {
            if (_activated)
            {
                return;
            }

            _activated = true;
            OnSacrificeRequested?.Invoke();
        }

        public async UniTask IgniteBeaconAsync(Light2D globalLight, CancellationToken ct)
        {
            if (_lampCoreRenderer != null)
            {
                _ = Tween.Color(_lampCoreRenderer, IgnitedCoreColor, 1.1f, Ease.OutQuad);
            }

            if (_lampLight != null)
            {
                float startInt = _lampLight.intensity;
                float startRad = _lampLight.pointLightOuterRadius;
                _ = Tween.Custom(
                    this,
                    0f,
                    1f,
                    1.6f,
                    (target, t) =>
                    {
                        if (target._lampLight != null)
                        {
                            target._lampLight.intensity = Mathf.Lerp(startInt, 3.6f, t);
                            target._lampLight.pointLightOuterRadius = Mathf.Lerp(startRad, 48f, t);
                        }
                    });
            }

            if (globalLight != null)
            {
                Color startGlobalColor = globalLight.color;
                float startGlobalInt = globalLight.intensity;
                Color warmDay = new Color(1f, 0.91f, 0.72f, 1f);
                _ = Tween.Custom(
                    this,
                    0f,
                    1f,
                    1.8f,
                    (target, t) =>
                    {
                        if (globalLight != null)
                        {
                            globalLight.color = Color.Lerp(startGlobalColor, warmDay, t);
                            globalLight.intensity = Mathf.Lerp(startGlobalInt, 1.05f, t);
                        }
                    });
            }

            _ = Tween.Custom(
                this,
                0f,
                0.38f,
                1.2f,
                (target, alpha) => target.SetBeamAlpha(alpha));

            await UniTask.Delay(TimeSpan.FromSeconds(1.8), ignoreTimeScale: true, cancellationToken: ct);
        }

        private void SetBeamAlpha(float alpha)
        {
            if (_beamRenderers == null)
            {
                return;
            }

            for (int i = 0; i < _beamRenderers.Length; i++)
            {
                if (_beamRenderers[i] != null)
                {
                    Color c = _beamRenderers[i].color;
                    c.a = alpha;
                    _beamRenderers[i].color = c;
                }
            }
        }
    }
}
