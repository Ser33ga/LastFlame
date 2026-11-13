using System;
using System.Collections.Generic;

namespace Game.Core
{
    public interface ILocalizationService
    {
        string CurrentLanguage { get; }
        IReadOnlyList<string> AvailableLanguages { get; }
        string Get(string key, params object[] args);
        bool TryGet(string key, out string localizedValue);
        void SetLanguage(string languageCode);
        event Action<string> OnLanguageChanged;
    }
}
