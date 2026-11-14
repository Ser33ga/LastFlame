# Game.Core

Базовые контракты и сервисы ядра без привязки к синглтонам:

- `ILocalizationService` / `LocalizationService` — RU/EN локализация с динамической регистрацией таблиц (`RegisterTranslations`).
- `IInteractable`, `IDamageable` — нейтральные контракты взаимодействия и урона.
- `ISceneLoader` / `SceneLoader`, `IScreenFader` — асинхронная загрузка сцен через `UniTask` и экранные переходы.

См. [ROADMAP.md](ROADMAP.md).
