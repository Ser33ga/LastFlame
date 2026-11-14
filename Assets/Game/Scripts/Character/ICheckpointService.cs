using System;
using UnityEngine;

namespace Game.Character
{
    public interface ICheckpointService
    {
        Vector2 RespawnPosition { get; }
        int CheckpointIndex { get; }

        event Action<int, Vector2> OnCheckpointActivated;

        void SetInitialSpawn(Vector2 position);
        bool RegisterCheckpoint(int index, Vector2 position);
    }
}
