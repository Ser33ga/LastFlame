using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Character;
using Game.Core;
using Game.Flame;
using Game.Input;
using Game.Settings;
using Game.UI;
using Game.World;
using PrimeTween;
using UnityEngine;
using VContainer.Unity;

namespace Game.Gameplay
{
    /// <summary>
    /// Orchestrates Last Flame's full gameplay loop: prologue awakening, active burn/respawn, and lighthouse sacrifice finale.
    /// </summary>
    public sealed class GameplayFlowController : IStartable, IDisposable
    {
        private readonly ILocalizationService _localization;
        private readonly ISettingsService _settings;
        private readonly IInputService _input;
        private readonly IScreenFader _screenFader;
        private readonly ISceneLoader _sceneLoader;
        private readonly IFlameService _flame;
        private readonly IPlayerInventory _inventory;
        private readonly ICheckpointService _checkpoints;
        private readonly GameplaySceneReferences _refs;
        private readonly LifetimeScope _scope;

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private bool _isRespawning;
        private bool _isEnding;

        public GameplayFlowController(
            ILocalizationService localization,
            ISettingsService settings,
            IInputService input,
            IScreenFader screenFader,
            ISceneLoader sceneLoader,
            IFlameService flame,
            IPlayerInventory inventory,
            ICheckpointService checkpoints,
            GameplaySceneReferences refs,
            LifetimeScope scope)
        {
            _localization = localization;
            _settings = settings;
            _input = input;
            _screenFader = screenFader;
            _sceneLoader = sceneLoader;
            _flame = flame;
            _inventory = inventory;
            _checkpoints = checkpoints;
            _refs = refs;
            _scope = scope;
        }

        public void Start()
        {
            LocalizationCatalog.RegisterAll(_localization);
            _settings?.Apply(_settings.Current);
            _input?.SetCursorLock(false);
            _input?.SetPlayerInputActive(true);
            _screenFader?.SetAlpha(0f);

            _inventory.ResetInventory();
            Vector2 startPos = _refs.PlayerStartPoint != null
                ? (Vector2)_refs.PlayerStartPoint.position
                : Vector2.zero;
            _checkpoints.SetInitialSpawn(startPos);

            List<IInteractable> interactables = CollectInteractables();

            _refs.CameraRig?.Construct(_refs.Candle != null ? _refs.Candle.transform : null);
            _refs.CandleLight?.Construct(_flame);
            _refs.HudView?.Construct(_localization);
            _refs.StoryView?.Construct(_localization, HandleRestartRequested);
            _refs.Candle?.Construct(_input, _flame, _refs.CameraRig, interactables);
            _refs.Candle?.TeleportTo(startPos);

            ConstructWorldEntities();
            SubscribeEvents();

            _flame.RestoreFull();
            HandleFlameChanged(_flame.CurrentFlame, _flame.MaxFlame);
            HandleInventoryChanged();

            RunPrologueSequenceAsync(_cts.Token).Forget();
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
            UnsubscribeEvents();
        }

        private List<IInteractable> CollectInteractables()
        {
            List<IInteractable> list = new List<IInteractable>(64);
            AddRange(list, _refs.Pickups);
            AddRange(list, _refs.Lanterns);
            AddRange(list, _refs.Doors);
            AddRange(list, _refs.Levers);
            AddRange(list, _refs.Bridges);
            AddRange(list, _refs.Minecarts);
            AddRange(list, _refs.Rockfalls);
            if (_refs.LighthouseLamp != null)
            {
                list.Add(_refs.LighthouseLamp);
            }

            return list;
        }

        private void ConstructWorldEntities()
        {
            LayerMask wallMask = _refs.WallMask.value != 0
                ? _refs.WallMask
                : LayerMask.GetMask("Walls");

            if (_refs.Pickups != null)
            {
                foreach (PickupItem item in _refs.Pickups)
                {
                    item?.Construct(_inventory, _flame, _refs.HudView, _refs.Candle);
                }
            }

            if (_refs.Lanterns != null)
            {
                foreach (LanternCheckpoint lantern in _refs.Lanterns)
                {
                    lantern?.Construct(_inventory, _flame, _checkpoints, _refs.HudView, _refs.Candle);
                }
            }

            if (_refs.Doors != null)
            {
                foreach (WorldDoor door in _refs.Doors)
                {
                    door?.Construct(_inventory, _refs.HudView);
                }
            }

            if (_refs.Levers != null)
            {
                foreach (WorldLever lever in _refs.Levers)
                {
                    lever?.Construct(_refs.HudView);
                }
            }

            if (_refs.Bridges != null)
            {
                foreach (BridgeBuilderSite bridge in _refs.Bridges)
                {
                    bridge?.Construct(_inventory, _refs.HudView);
                }
            }

            if (_refs.Minecarts != null)
            {
                foreach (MinecartObstacle cart in _refs.Minecarts)
                {
                    cart?.Construct(_refs.CameraRig, _refs.HudView);
                }
            }

            if (_refs.PitHazards != null)
            {
                foreach (PitHazardZone pit in _refs.PitHazards)
                {
                    pit?.Construct(_refs.Candle, _refs.HudView);
                }
            }

            if (_refs.WindZones != null)
            {
                foreach (WindGustZone wind in _refs.WindZones)
                {
                    wind?.Construct(_refs.Candle, wallMask);
                }
            }

            if (_refs.Blowers != null)
            {
                foreach (BlowerSpiritEnemy blower in _refs.Blowers)
                {
                    blower?.Construct(_refs.Candle, wallMask);
                }
            }

            if (_refs.Chasers != null)
            {
                foreach (ChaserEnemy chaser in _refs.Chasers)
                {
                    chaser?.Construct(_refs.Candle, _flame);
                }
            }

            if (_refs.Rockfalls != null)
            {
                foreach (RockfallObstacle rockfall in _refs.Rockfalls)
                {
                    rockfall?.Construct(_inventory, _refs.CameraRig, _refs.HudView);
                }
            }

            if (_refs.ZoneTriggers != null)
            {
                foreach (LocationZoneTrigger trigger in _refs.ZoneTriggers)
                {
                    trigger?.Construct(_refs.Candle, _refs.HudView, _refs.GlobalLight);
                }
            }

            _refs.LighthouseLamp?.Construct();
        }

        private void SubscribeEvents()
        {
            _flame.OnFlameChanged += HandleFlameChanged;
            _flame.OnFlameExtinguished += HandleFlameExtinguished;
            _inventory.OnInventoryChanged += HandleInventoryChanged;

            if (_refs.Candle != null)
            {
                _refs.Candle.OnNearestInteractableChanged += HandleNearestInteractableChanged;
            }

            if (_refs.LighthouseLamp != null)
            {
                _refs.LighthouseLamp.OnSacrificeRequested += HandleSacrificeRequested;
            }
        }

        private void UnsubscribeEvents()
        {
            _flame.OnFlameChanged -= HandleFlameChanged;
            _flame.OnFlameExtinguished -= HandleFlameExtinguished;
            _inventory.OnInventoryChanged -= HandleInventoryChanged;

            if (_refs.Candle != null)
            {
                _refs.Candle.OnNearestInteractableChanged -= HandleNearestInteractableChanged;
            }

            if (_refs.LighthouseLamp != null)
            {
                _refs.LighthouseLamp.OnSacrificeRequested -= HandleSacrificeRequested;
            }
        }

        private async UniTaskVoid RunPrologueSequenceAsync(CancellationToken ct)
        {
            _flame.SetDrainPaused(true);
            _refs.Candle?.SetControlsEnabled(false);
            _refs.HudView?.SetHudVisible(false);

            if (_refs.StoryView != null)
            {
                await _refs.StoryView.ShowPrologueAsync(ct);
            }

            if (ct.IsCancellationRequested)
            {
                return;
            }

            _refs.HudView?.SetHudVisible(true);
            _refs.HudView?.SetZone("zone.prologue", showBanner: true);
            _flame.SetDrainPaused(false);
            _refs.Candle?.SetControlsEnabled(true);
        }

        private async UniTaskVoid RespawnSequenceAsync(CancellationToken ct)
        {
            if (_isRespawning || _isEnding)
            {
                return;
            }

            _isRespawning = true;
            _flame.SetDrainPaused(true);
            _refs.Candle?.SetControlsEnabled(false);

            if (_screenFader != null)
            {
                await _screenFader.FadeOutAsync(0.1f);
            }

            if (ct.IsCancellationRequested)
            {
                return;
            }

            _flame.ResetSafeZones();
            _flame.RestoreFull();
            _refs.Candle?.TeleportTo(_checkpoints.RespawnPosition);

            if (_screenFader != null)
            {
                await _screenFader.FadeInAsync(0.1f);
            }

            if (ct.IsCancellationRequested)
            {
                return;
            }

            _refs.HudView?.ShowToast("toast.respawned");
            _flame.SetDrainPaused(false);
            _refs.Candle?.SetControlsEnabled(true);
            _isRespawning = false;
        }

        private async UniTaskVoid EndingSequenceAsync(CancellationToken ct)
        {
            if (_isEnding)
            {
                return;
            }

            _isEnding = true;
            _flame.SetDrainPaused(true);
            _refs.Candle?.SetControlsEnabled(false);
            _refs.HudView?.SetInteractionPrompt(null, false);
            _refs.HudView?.SetHudVisible(false);

            if (_refs.Candle != null && _refs.LighthouseLamp != null)
            {
                Vector3 targetPos = _refs.LighthouseLamp.transform.position + new Vector3(0f, -1.6f, 0f);
                await Tween.Position(_refs.Candle.transform, targetPos, 0.55f, Ease.InOutSine);
            }

            if (ct.IsCancellationRequested)
            {
                return;
            }

            await _flame.SacrificeDrainAsync(1.4f, ct);
            if (ct.IsCancellationRequested)
            {
                return;
            }

            _refs.CameraRig?.AnimateOrthographicSize(10.5f, 1.5f);
            if (_refs.LighthouseLamp != null)
            {
                await _refs.LighthouseLamp.IgniteBeaconAsync(_refs.GlobalLight, ct);
            }

            if (ct.IsCancellationRequested)
            {
                return;
            }

            if (_refs.StoryView != null)
            {
                await _refs.StoryView.ShowEndingAsync(ct);
            }
        }

        private void HandleFlameChanged(float current, float max)
        {
            _refs.HudView?.SetFlame(current, max);
        }

        private void HandleFlameExtinguished()
        {
            RespawnSequenceAsync(_cts.Token).Forget();
        }

        private void HandleInventoryChanged()
        {
            _refs.HudView?.SetInventoryState(
                _inventory.HasMatch,
                _inventory.WoodPlanks,
                _inventory.HasExplosive,
                _inventory.HasLighthouseKey,
                _inventory.HasMineKey);
        }

        private void HandleNearestInteractableChanged(string promptKey, bool canInteract)
        {
            _refs.HudView?.SetInteractionPrompt(promptKey, canInteract);
        }

        private void HandleSacrificeRequested()
        {
            EndingSequenceAsync(_cts.Token).Forget();
        }

        private void HandleRestartRequested()
        {
            RestartAsync().Forget();
        }

        private async UniTaskVoid RestartAsync()
        {
            if (_sceneLoader == null)
            {
                return;
            }

            if (_scope != null && _scope.Parent != null)
            {
                using (LifetimeScope.EnqueueParent(_scope.Parent))
                {
                    await _sceneLoader.LoadSceneAsync("Gameplay");
                }
            }
            else
            {
                await _sceneLoader.LoadSceneAsync("Gameplay");
            }
        }

        private static void AddRange<T>(List<IInteractable> target, T[] items) where T : class, IInteractable
        {
            if (items == null)
            {
                return;
            }

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] != null)
                {
                    target.Add(items[i]);
                }
            }
        }
    }
}
