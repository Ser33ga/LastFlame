using Game.Character;
using Game.Flame;
using Game.UI;
using Game.World;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Game.Gameplay
{
    /// <summary>
    /// Explicit serialized scene reference registry avoiding FindObjectOfType.
    /// </summary>
    public sealed class GameplaySceneReferences : MonoBehaviour
    {
        [Header("Player & Camera")]
        public Transform PlayerStartPoint;
        public CandleController Candle;
        public CandleLightView CandleLight;
        public TopDownCameraRig CameraRig;
        public Light2D GlobalLight;

        [Header("UI")]
        public GameplayHudView HudView;
        public StoryOverlayView StoryView;

        [Header("World Entities")]
        public PickupItem[] Pickups;
        public LanternCheckpoint[] Lanterns;
        public WorldDoor[] Doors;
        public WorldLever[] Levers;
        public BridgeBuilderSite[] Bridges;
        public MinecartObstacle[] Minecarts;
        public PitHazardZone[] PitHazards;
        public WindGustZone[] WindZones;
        public BlowerSpiritEnemy[] Blowers;
        public ChaserEnemy[] Chasers;
        public RockfallObstacle[] Rockfalls;
        public LocationZoneTrigger[] ZoneTriggers;
        public LighthouseGreatLamp LighthouseLamp;

        [Header("Physics Masks")]
        public LayerMask WallMask;
    }
}
