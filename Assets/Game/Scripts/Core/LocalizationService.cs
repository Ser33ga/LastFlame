using System;
using System.Collections.Generic;

namespace Game.Core
{
    /// <summary>
    /// Empty DI localization service supporting runtime RU/EN switching without singletons.
    /// Keys are registered per feature when gameplay/UI modules are added.
    /// </summary>
    public sealed class LocalizationService : ILocalizationService
    {
        private static readonly string[] SupportedLanguages = { "ru", "en" };

        private readonly Dictionary<string, Dictionary<string, string>> _tables =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["ru"] = new Dictionary<string, string>(StringComparer.Ordinal),
                ["en"] = new Dictionary<string, string>(StringComparer.Ordinal)
            };

        public string CurrentLanguage { get; private set; } = "ru";
        public IReadOnlyList<string> AvailableLanguages => SupportedLanguages;

        public event Action<string> OnLanguageChanged;

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
    }
}
