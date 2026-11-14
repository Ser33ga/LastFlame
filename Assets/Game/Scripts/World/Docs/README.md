# Game.World

Игровые сущности мира, враги, головоломки и опасности:

- `PickupItem` — спичка, доски, взрывчатка, ключ от Маяка, угольки воска.
- `LanternCheckpoint` — погасшие и зажигаемые фонари-чекпоинты с безопасной аурой регенерации огня.
- `WorldDoor`, `WorldLever`, `BridgeBuilderSite`, `MinecartObstacle`, `RockfallObstacle` — двери, рычаги, строительство моста, толчок вагонетки и подрыв обвала.
- `PitHazardZone`, `WindGustZone`, `BlowerSpiritEnemy` (`Дух ветра`), `ChaserEnemy` (`Тень леса` и `Ползун`) — опасности и противники, с поддержкой экранирования ветра за каменными укрытиями (Layer `Walls`).
- `LocationZoneTrigger`, `LighthouseGreatLamp` — переходы атмосферы между актами и финальное зажжение Маяка.

См. [ROADMAP.md](ROADMAP.md).
