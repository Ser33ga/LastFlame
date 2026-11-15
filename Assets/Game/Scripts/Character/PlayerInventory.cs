using System;

namespace Game.Character
{
    public sealed class PlayerInventory : IPlayerInventory
    {
        public bool HasMatch { get; private set; }
        public int WoodPlanks { get; private set; }
        public bool HasMineKey { get; private set; }
        public bool HasExplosive { get; private set; }
        public bool HasLighthouseKey { get; private set; }

        public event Action OnInventoryChanged;

        public void GiveMatch()
        {
            if (HasMatch)
            {
                return;
            }

            HasMatch = true;
            OnInventoryChanged?.Invoke();
        }

        public void AddWoodPlanks(int count)
        {
            if (count <= 0)
            {
                return;
            }

            WoodPlanks += count;
            OnInventoryChanged?.Invoke();
        }

        public bool TryConsumeWoodPlanks(int count)
        {
            if (count <= 0 || WoodPlanks < count)
            {
                return false;
            }

            WoodPlanks -= count;
            OnInventoryChanged?.Invoke();
            return true;
        }

        public void GiveMineKey()
        {
            if (HasMineKey)
            {
                return;
            }

            HasMineKey = true;
            OnInventoryChanged?.Invoke();
        }

        public bool TryConsumeMineKey()
        {
            if (!HasMineKey)
            {
                return false;
            }

            HasMineKey = false;
            OnInventoryChanged?.Invoke();
            return true;
        }

        public void GiveExplosive()
        {
            if (HasExplosive)
            {
                return;
            }

            HasExplosive = true;
            OnInventoryChanged?.Invoke();
        }

        public bool TryConsumeExplosive()
        {
            if (!HasExplosive)
            {
                return false;
            }

            HasExplosive = false;
            OnInventoryChanged?.Invoke();
            return true;
        }

        public void GiveLighthouseKey()
        {
            if (HasLighthouseKey)
            {
                return;
            }

            HasLighthouseKey = true;
            OnInventoryChanged?.Invoke();
        }

        public bool TryConsumeLighthouseKey()
        {
            if (!HasLighthouseKey)
            {
                return false;
            }

            HasLighthouseKey = false;
            OnInventoryChanged?.Invoke();
            return true;
        }

        public void ResetInventory()
        {
            HasMatch = false;
            WoodPlanks = 0;
            HasMineKey = false;
            HasExplosive = false;
            HasLighthouseKey = false;
            OnInventoryChanged?.Invoke();
        }
    }
}
