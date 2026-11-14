# Game.Gameplay

Сборка и оркестрация игрового процесса сцены `Gameplay.unity`:

- `GameplayLifetimeScope` — DI-контейнер сцены с локальным фолбэком при прямом запуске без `Boot`.
- `GameplaySceneReferences` — явная привязка объектов сцены без `FindObjectOfType`.
- `LocalizationCatalog` — полная таблица русских и английских текстов интерфейса, подсказок и истории.
- `GameplayFlowController` — управление циклом: Пролог -> активный геймплей -> возрождение на чекпоинте при угасании -> жертва огня Маяку -> Финал и перезапуск.

См. [ROADMAP.md](ROADMAP.md).
