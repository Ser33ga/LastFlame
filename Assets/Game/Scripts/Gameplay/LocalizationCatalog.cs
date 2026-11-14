using System.Collections.Generic;
using Game.Core;

namespace Game.Gameplay
{
    public static class LocalizationCatalog
    {
        public static void RegisterAll(ILocalizationService localization)
        {
            if (localization == null)
            {
                return;
            }

            localization.RegisterTranslations("ru", new Dictionary<string, string>
            {
                ["story.prologue.body"] =
                    "«Когда-то этот мир был наполнен светом. Но однажды огонь начал исчезать. Один за другим гасли дома, города и целые земли. Теперь осталась лишь одна маленькая искра...»",
                ["story.prologue.continue"] = "Нажмите любую клавишу или кликните, чтобы пробудить пламя",
                ["story.ending.title"] = "МАЯК ЗАЖЖЁН",
                ["story.ending.body"] =
                    "Последняя искра отдала своё пламя сердцу древнего маяка. Свет вновь разлился над забытыми землями.",
                ["ui.play_again"] = "Сыграть снова",

                ["hud.flame_label"] = "Пламя",
                ["hud.controls_hint"] = "WASD: Движение  |  Space / ЛКМ: Удар огнём  |  Shift: Рывок (5%)  |  E: Действие",
                ["hud.inventory_prefix"] = "Предметы:",
                ["hud.inventory_empty"] = "Предметы: пусто",

                ["zone.prologue"] = "Пролог — Заброшенная комната",
                ["zone.forest"] = "Локация 1 — Забытый лес",
                ["zone.mine"] = "Локация 2 — Заброшенная шахта",
                ["zone.lighthouse"] = "Локация 3 — Маяк",
                ["zone.lighthouse_base"] = "Локация 3 — Подножье Маяка",
                ["zone.lighthouse_floor1"] = "Маяк — Первый этаж",
                ["zone.lighthouse_stairs"] = "Маяк — Винтовая лестница",
                ["zone.lighthouse_summit"] = "Вершина маяка",

                ["item.match"] = "Спичка",
                ["item.planks"] = "Доски x{0}",
                ["item.explosive"] = "Взрывчатка",
                ["item.lighthouse_key"] = "Ключ от маяка",

                ["prompt.pickup.match"] = "Подобрать спичку",
                ["prompt.pickup.plank"] = "Взять доску",
                ["prompt.pickup.explosive"] = "Взять взрывчатку",
                ["prompt.pickup.key"] = "Подобрать ключ от маяка",
                ["prompt.pickup.ember"] = "Поглотить уголёк (+пламя)",
                ["prompt.pickup.generic"] = "Подобрать",

                ["prompt.lantern.light"] = "Зажечь фонарь (чекпоинт)",
                ["prompt.lantern.need_match"] = "Фонарь погас — найдите спичку рядом",

                ["prompt.door.open"] = "Открыть дверь",
                ["prompt.door.unlock_key"] = "Открыть дверь ключом",
                ["prompt.door.need_key"] = "Дверь заперта — найдите ключ возле старой тележки",
                ["prompt.door.locked_lever"] = "Металлическая дверь заперта — найдите рычаг",
                ["prompt.door.locked_stairs"] = "Проход на винтовую лестницу закрыт — найдите рычаг",

                ["prompt.lever.pull"] = "Потянуть рычаг",
                ["prompt.bridge.build"] = "Построить мост через обрыв",
                ["prompt.bridge.need_planks"] = "Разрушенный мост — соберите 2 доски поблизости",
                ["prompt.minecart.push"] = "Толкнуть старую вагонетку",
                ["prompt.rockfall.detonate"] = "Взорвать обвал",
                ["prompt.rockfall.need_explosive"] = "Завал в тоннеле — нужна взрывчатка из мастерской",
                ["prompt.lighthouse.ignite"] = "Отдать пламя лампе Маяка",

                ["toast.got_match"] = "Найдена спичка!",
                ["toast.got_plank"] = "Подобрана доска для моста",
                ["toast.got_explosive"] = "Взята взрывчатка!",
                ["toast.got_key"] = "Найден ключ от Маяка!",
                ["toast.got_ember"] = "Пламя окрепло!",
                ["toast.checkpoint_lit"] = "Фонарь зажжён — контрольная точка активна!",
                ["toast.door_opened"] = "Дверь открыта",
                ["toast.lever_pulled"] = "Механизм сработал — путь открыт!",
                ["toast.bridge_built"] = "Мост построен!",
                ["toast.minecart_cleared"] = "Вагонетка пробила преграду!",
                ["toast.rockfall_cleared"] = "Обвал расчищен! Впереди виден Маяк.",
                ["toast.pit_fall"] = "Пламя опалено падением!",
                ["toast.respawned"] = "Искра возродилась у фонаря"
            });

            localization.RegisterTranslations("en", new Dictionary<string, string>
            {
                ["story.prologue.body"] =
                    "\"Once, this world was filled with light. But one day, fire began to fade. Homes, cities, and entire lands went dark one by one. Now only a single tiny spark remains...\"",
                ["story.prologue.continue"] = "Press any key or click to awaken the flame",
                ["story.ending.title"] = "THE LIGHTHOUSE IS LIT",
                ["story.ending.body"] =
                    "The last spark gave its flame to the heart of the ancient lighthouse. Light washes across the forgotten world once more.",
                ["ui.play_again"] = "Play Again",

                ["hud.flame_label"] = "Flame",
                ["hud.controls_hint"] = "WASD: Move  |  Space / LMB: Flame Strike  |  Shift: Dash (5%)  |  E: Interact",
                ["hud.inventory_prefix"] = "Items:",
                ["hud.inventory_empty"] = "Items: none",

                ["zone.prologue"] = "Prologue — Abandoned Room",
                ["zone.forest"] = "Location 1 — Forgotten Forest",
                ["zone.mine"] = "Location 2 — Abandoned Mine",
                ["zone.lighthouse"] = "Location 3 — The Lighthouse",
                ["zone.lighthouse_base"] = "Location 3 — Lighthouse Base",
                ["zone.lighthouse_floor1"] = "Lighthouse — Ground Floor",
                ["zone.lighthouse_stairs"] = "Lighthouse — Spiral Staircase",
                ["zone.lighthouse_summit"] = "Lighthouse Summit",

                ["item.match"] = "Match",
                ["item.planks"] = "Planks x{0}",
                ["item.explosive"] = "Explosive",
                ["item.lighthouse_key"] = "Lighthouse Key",

                ["prompt.pickup.match"] = "Pick up match",
                ["prompt.pickup.plank"] = "Take plank",
                ["prompt.pickup.explosive"] = "Take explosive",
                ["prompt.pickup.key"] = "Pick up lighthouse key",
                ["prompt.pickup.ember"] = "Absorb ember (+flame)",
                ["prompt.pickup.generic"] = "Pick up",

                ["prompt.lantern.light"] = "Light lantern (Checkpoint)",
                ["prompt.lantern.need_match"] = "Lantern is dark — find a match nearby",

                ["prompt.door.open"] = "Open door",
                ["prompt.door.unlock_key"] = "Unlock door with key",
                ["prompt.door.need_key"] = "Door locked — check near the old wooden cart",
                ["prompt.door.locked_lever"] = "Metal door locked — find a lever",
                ["prompt.door.locked_stairs"] = "Spiral stair door locked — find a lever",

                ["prompt.lever.pull"] = "Pull lever",
                ["prompt.bridge.build"] = "Build bridge",
                ["prompt.bridge.need_planks"] = "Broken bridge — collect 2 planks nearby",
                ["prompt.minecart.push"] = "Push old minecart",
                ["prompt.rockfall.detonate"] = "Blast rockfall",
                ["prompt.rockfall.need_explosive"] = "Cave-in — find explosive in the workshop",
                ["prompt.lighthouse.ignite"] = "Sacrifice flame to light the Beacon",

                ["toast.got_match"] = "Picked up a match!",
                ["toast.got_plank"] = "Picked up a wood plank",
                ["toast.got_explosive"] = "Picked up explosive!",
                ["toast.got_key"] = "Found the Lighthouse Key!",
                ["toast.got_ember"] = "Flame restored!",
                ["toast.checkpoint_lit"] = "Lantern lit — Checkpoint saved!",
                ["toast.door_opened"] = "Door opened",
                ["toast.lever_pulled"] = "Mechanism activated — passage open!",
                ["toast.bridge_built"] = "Bridge built!",
                ["toast.minecart_cleared"] = "Minecart smashed the barricade!",
                ["toast.rockfall_cleared"] = "Rockfall cleared! The Lighthouse shines ahead.",
                ["toast.pit_fall"] = "Fell off the platform!",
                ["toast.respawned"] = "Spark restored at checkpoint"
            });
        }
    }
}
