using R3;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Services
{
    /// <summary>
    /// Интерфейс сервиса локализации для работы с многоязычным контентом.
    /// Localization service interface for working with multi-language content.
    /// </summary>
    public interface ILocalizationService
    {
        ReadOnlyReactiveProperty<SystemLanguage> CurrentLanguage { get; }
        IReadOnlyList<SystemLanguage> Languages { get; }
        void ChangeLanguageTo(SystemLanguage language);
        string Localize(string id);
    }
}