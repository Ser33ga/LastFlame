# Scripts

Модульные C#-сборки (`.asmdef`) игры без синглтонов и прямых `FindObjectOfType` в рантайме:

- `Game.Core` — интерфейсы (`IInteractable`, `IDamageable`, `ILocalizationService`, `ISceneLoader`, `IScreenFader`), сервисы локализации и загрузки сцен.
- `Game.Settings` — хранение и применение настроек.
- `Game.Input` — обёртка над `InputSystem_Actions` (`Move`, `Attack`, `Interact`, `Dash`).
- `Game.UI` — экранный затемнитель (`ScreenFader`), HUD (`GameplayHudView`) и оверлеи Пролога/Финала (`StoryOverlayView`).
- `Game.Flame` — ресурс огня/жизни свечи (`IFlameService`, `FlameService`) и синхронизация URP `Light2D` (`CandleLightView`).
- `Game.Character` — top-down контроллер свечи (`CandleController`), камера (`TopDownCameraRig`), инвентарь (`IPlayerInventory`) и чекпоинты (`ICheckpointService`).
- `Game.World` — все сущности мира, пазлы, враги, ветры, зоны и лампа Маяка.
- `Game.Gameplay` — DI-компоновка (`GameplayLifetimeScope`, `GameplaySceneReferences`), словарь RU/EN (`LocalizationCatalog`) и оркестратор (`GameplayFlowController`).
- `Game.Boot` — точка входа игры (`BootLifetimeScope`, `BootEntryPoint`).

См. [ROADMAP.md](ROADMAP.md).
