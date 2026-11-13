using System;
using System.Collections.Generic;

namespace Game.Core
{
    /// <summary>
    /// Stateless-per-instance DI localization service holding built-in RU/EN string tables without singletons.
    /// </summary>
    public sealed class LocalizationService : ILocalizationService
    {
        private static readonly string[] SupportedLanguages = { "ru", "en" };

        private readonly Dictionary<string, Dictionary<string, string>> _tables =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        public string CurrentLanguage { get; private set; } = "ru";
        public IReadOnlyList<string> AvailableLanguages => SupportedLanguages;

        public event Action<string> OnLanguageChanged;

        public LocalizationService()
        {
            SeedDefaultTables();
        }

        public string Get(string key, params object[] args)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            if (!TryGet(key, out string template))
            {
                return key;
            }

            if (args == null || args.Length == 0)
            {
                return template;
            }

            try
            {
                return string.Format(template, args);
            }
            catch (FormatException)
            {
                return template;
            }
        }

        public bool TryGet(string key, out string localizedValue)
        {
            if (!string.IsNullOrEmpty(key))
            {
                if (_tables.TryGetValue(CurrentLanguage, out Dictionary<string, string> activeTable)
                    && activeTable.TryGetValue(key, out localizedValue))
                {
                    return true;
                }

                if (_tables.TryGetValue("en", out Dictionary<string, string> fallbackTable)
                    && fallbackTable.TryGetValue(key, out localizedValue))
                {
                    return true;
                }
            }

            localizedValue = key;
            return false;
        }

        public void SetLanguage(string languageCode)
        {
            string normalized = NormalizeLanguageCode(languageCode);
            if (string.Equals(CurrentLanguage, normalized, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            CurrentLanguage = normalized;
            OnLanguageChanged?.Invoke(CurrentLanguage);
        }

        public void RegisterTranslations(string languageCode, IReadOnlyDictionary<string, string> entries)
        {
            if (entries == null)
            {
                return;
            }

            string normalized = NormalizeLanguageCode(languageCode);
            if (!_tables.TryGetValue(normalized, out Dictionary<string, string> table))
            {
                table = new Dictionary<string, string>(StringComparer.Ordinal);
                _tables[normalized] = table;
            }

            foreach (KeyValuePair<string, string> pair in entries)
            {
                table[pair.Key] = pair.Value;
            }
        }

        private static string NormalizeLanguageCode(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
            {
                return "ru";
            }

            string lower = languageCode.Trim().ToLowerInvariant();
            return lower.StartsWith("en", StringComparison.Ordinal) ? "en" : "ru";
        }

        private void SeedDefaultTables()
        {
            _tables["ru"] = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["game.title"] = "Последний огонёк",
                ["game.subtitle"] = "Донеси пламя до маяка во тьме",
                ["ui.play"] = "Зажечь путь",
                ["ui.resume"] = "Продолжить",
                ["ui.restart"] = "От последнего факела",
                ["ui.settings"] = "Настройки",
                ["ui.language"] = "Язык: RU",
                ["ui.pause"] = "Пауза",
                ["ui.flame_label"] = "Пламя: {0}%",
                ["ui.checkpoints_label"] = "Факелы: {0}/{1}",
                ["ui.controls_hint"] = "WASD — движение / Пробел — прыжок / Shift — рывок / E — факел / Esc — пауза",
                ["ui.status.baseline"] = "Огонёк тлеет — мир ждёт света",
                ["ui.gameover.title"] = "Огонь угас",
                ["ui.gameover.subtitle"] = "Ветер одолел пламя. Возрождение у последнего факела.",
                ["ui.victory.title"] = "Маяк зажжён",
                ["ui.victory.subtitle"] = "Твоё пламя вернуло свет заброшенному миру."
            };

            _tables["en"] = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["game.title"] = "Last Flame",
                ["game.subtitle"] = "Carry the last spark to the lighthouse",
                ["ui.play"] = "Ignite Journey",
                ["ui.resume"] = "Resume",
                ["ui.restart"] = "Retry Checkpoint",
                ["ui.settings"] = "Settings",
                ["ui.language"] = "Language: EN",
                ["ui.pause"] = "Paused",
                ["ui.flame_label"] = "Flame: {0}%",
                ["ui.checkpoints_label"] = "Torches: {0}/{1}",
                ["ui.controls_hint"] = "WASD Move / Space Jump / Shift Flare Dash / E Ignite / Esc Pause",
                ["ui.status.baseline"] = "The wick glows — awaiting perspective confirmation",
                ["ui.gameover.title"] = "Flame Extinguished",
                ["ui.gameover.subtitle"] = "The gale snuffed the candle. Rise at the last lit torch.",
                ["ui.victory.title"] = "Beacon Lit",
                ["ui.victory.subtitle"] = "Your final sacrifice returned light to the ruins."
            };
        }
    }
}
