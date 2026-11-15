using System;

namespace Game.Character
{
    public interface IPlayerInventory
    {
        bool HasMatch { get; }
        int WoodPlanks { get; }
        bool HasMineKey { get; }
        bool HasExplosive { get; }
        bool HasLighthouseKey { get; }

        event Action OnInventoryChanged;

        void GiveMatch();
        void AddWoodPlanks(int count);
        bool TryConsumeWoodPlanks(int count);
        void GiveMineKey();
        bool TryConsumeMineKey();
        void GiveExplosive();
        bool TryConsumeExplosive();
        void GiveLighthouseKey();
        bool TryConsumeLighthouseKey();
        void ResetInventory();
    }
}
