using System;

namespace Game.Core
{
    public enum GameShellState
    {
        Title = 0,
        Playing = 1,
        Paused = 2,
        Ended = 3
    }

    public enum GameOutcome
    {
        None = 0,
        Extinguished = 1,
        BeaconLit = 2
    }

    public interface IGameShellController
    {
        GameShellState CurrentState { get; }
        GameOutcome CurrentOutcome { get; }
        event Action<GameShellState, GameOutcome> OnStateChanged;
        void StartPlay();
        void TogglePause();
        void RestartFromCheckpoint();
        void FinishRun(GameOutcome outcome);
        void ReturnToTitle();
    }
}
