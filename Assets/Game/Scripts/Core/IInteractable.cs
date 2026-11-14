using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Contract for world objects the candle can inspect and interact with.
    /// </summary>
    public interface IInteractable
    {
        bool IsAvailable { get; }
        Vector2 WorldPosition { get; }
        bool CanInteract();
        string GetPromptKey(bool canInteract);
        void Interact();
    }
}
