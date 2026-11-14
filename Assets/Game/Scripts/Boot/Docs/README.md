# Game.Boot

Стартовый DI-контейнер сцены `Boot.unity`:

- `BootLifetimeScope` — регистрирует `LocalizationService`, `SettingsService`, `InputService`, `ScreenFader`, `SceneLoader` и `BootEntryPoint` (при наличии `Parent == null` для сохранения `Boot` между сценами).
- `BootEntryPoint` — применяет настройки и переходит в `Gameplay.unity` через `LifetimeScope.EnqueueParent(_bootScope)`.

См. [ROADMAP.md](ROADMAP.md).
