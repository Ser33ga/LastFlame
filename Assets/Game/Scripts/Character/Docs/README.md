# Game.Character

Модуль управления персонажем-свечой, камерой, инвентарём и точками возрождения:

- `CandleController` — top-down движение (`Rigidbody2D.linearVelocity`), рывок (`Shift`, трата пламени), атака волной огня (`Space`/`ЛКМ` по `IDamageable`), взаимодействие `[E]` с ближайшим `IInteractable`, отдача и сдувание ветром.
- `TopDownCameraRig` — плавное ортографическое слежение и импульсная тряска (`PrimeTween.Tween.ShakeLocalPosition`).
- `PlayerInventory` (`IPlayerInventory`) — хранение спички, досок для моста, взрывчатки и ключа от Маяка.
- `CheckpointService` (`ICheckpointService`) — регистрация зажжённых фонарей для возрождения.

См. [ROADMAP.md](ROADMAP.md).
