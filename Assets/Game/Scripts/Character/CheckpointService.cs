using System;
using UnityEngine;

namespace Game.Character
{
    public sealed class CheckpointService : ICheckpointService
    {
        public Vector2 RespawnPosition { get; private set; }
        public int CheckpointIndex { get; private set; } = -1;

        public event Action<int, Vector2> OnCheckpointActivated;

        public void SetInitialSpawn(Vector2 position)
        {
            RespawnPosition = position;
            CheckpointIndex = -1;
        }

        public bool RegisterCheckpoint(int index, Vector2 position)
        {
            if (index <= CheckpointIndex && Vector2.Distance(RespawnPosition, position) < 0.2f)
            {
                return false;
            }

            CheckpointIndex = Mathf.Max(CheckpointIndex, index);
            RespawnPosition = position;
            OnCheckpointActivated?.Invoke(CheckpointIndex, RespawnPosition);
            return true;
        }
    }
}
