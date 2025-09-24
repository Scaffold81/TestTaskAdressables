using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using Cysharp.Threading.Tasks;
using Zenject;

namespace Game.Services
{
    /// <summary>
    /// Сервис локализации на основе Unity Localization Package. Поддерживает динамическую смену языков.
    /// Localization service based on Unity Localization Package. Supports dynamic language switching.
    /// </summary>
    public class UnityLocalizationService : ILocalizationService, IDisposable
    {
        private bool changing;
        private bool isInitialized;
        private readonly ReactiveProperty<SystemLanguage> currentLanguage = new();
        private readonly Dictionary<SystemLanguage, LocaleIdentifier> languagesMap = new();
        private readonly List<SystemLanguage> languages = new();
        private readonly CompositeDisposable disposables = new();

        /// <summary>
        /// Конструктор сервиса локализации с инъекцией зависимостей.
        /// Localization service constructor with dependency injection.
        /// </summary>
        [Inject]
        public UnityLocalizationService()
        {
            InitializeAsync().Forget();
        }

        public ReadOnlyReactiveProperty<SystemLanguage> CurrentLanguage => currentLanguage;
        public IReadOnlyList<SystemLanguage> Languages => languages;

        /// <summary>
        /// Асинхронно инициализирует сервис локализации.
        /// Asynchronously initializes localization service.
        /// </summary>
        private async UniTask InitializeAsync()
        {
            try
            {
                // Wait for LocalizationSettings to be ready
                await WaitForLocalizationInitialization();
                
                ParseKnownLocales();
                RegisterUnityLanguageChange();
                
                // Set initial language
                var initialLanguage = GetInitialLanguage();
                if (languages.Contains(initialLanguage))
                {
                    ChangeLanguageTo(initialLanguage);
                }
                else if (languages.Count > 0)
                {
                    ChangeLanguageTo(languages[0]);
                }
                
                isInitialized = true;
            }
            catch (Exception)
            {
                // Fallback initialization with default language
                languages.Add(SystemLanguage.English);
                currentLanguage.Value = SystemLanguage.English;
                isInitialized = true;
            }
        }
        
        /// <summary>
        /// Ожидает инициализации системы локализации Unity.
        /// Waits for Unity localization system initialization.
        /// </summary>
        private async UniTask WaitForLocalizationInitialization()
        {
            const int maxAttempts = 50; // 5 seconds with 100ms intervals
            int attempts = 0;
            
            while (attempts < maxAttempts)
            {
                try
                {
                    // Try to access LocalizationSettings
                    if (LocalizationSettings.AvailableLocales?.Locales != null)
                    {
                        return;
                    }
                }
                catch (Exception)
                {
                    // Localization not ready yet, continue waiting
                }
                
                await UniTask.Delay(100); // Wait 100ms
                attempts++;
            }
            
            throw new InvalidOperationException("LocalizationSettings failed to initialize within timeout period. " +
                "Please ensure Unity Localization Package is properly configured and Addressables are built.");
        }
        
        /// <summary>
        /// Получает начальный язык для инициализации.
        /// Gets initial language for initialization.
        /// </summary>
        /// <returns>Начальный язык / Initial language</returns>
        private SystemLanguage GetInitialLanguage()
        {
            // Try to get saved language first
            // TODO: Implement with SaveService when available
            
            // Try system language
            var systemLanguage = Application.systemLanguage;
            if (languages.Contains(systemLanguage))
            {
                return systemLanguage;
            }
            
            // Fallback to English if available
            if (languages.Contains(SystemLanguage.English))
            {
                return SystemLanguage.English;
            }
            
            // Final fallback to first available
            return languages.FirstOrDefault();
        }

        /// <summary>
        /// Изменяет текущий язык локализации.
        /// Changes current localization language.
        /// </summary>
        /// <param name="language">Новый язык / New language</param>
        public void ChangeLanguageTo(SystemLanguage language)
        {
            if (!isInitialized)
            {
                return;
            }
            
            if (Languages.All(x => x != language))
            {
                return;
            }

            changing = true;
            
            try
            {
                Locale locale = LocalizationSettings.AvailableLocales.GetLocale(languagesMap[language]);
                LocalizationSettings.SelectedLocale = locale;
                currentLanguage.Value = language;
            }
            catch (Exception)
            {
                // Handle exception silently
            }
            finally
            {
                changing = false;
            }
        }

        /// <summary>
        /// Локализует строку по идентификатору.
        /// Localizes string by identifier.
        /// </summary>
        /// <param name="id">Идентификатор строки для локализации / String identifier for localization</param>
        /// <returns>Локализованная строка / Localized string</returns>
        public string Localize(string id)
        {
            if (!isInitialized)
            {
                return id;
            }
            
            if (string.IsNullOrEmpty(id))
            {
                return string.Empty;
            }

            try
            {
                return LocalizationSettings.StringDatabase.GetLocalizedString(id);
            }
            catch (Exception)
            {
                return id; // Return key as fallback
            }
        }

        /// <summary>
        /// Парсит доступные локали из Unity Localization Package.
        /// Parses available locales from Unity Localization Package.
        /// </summary>
        private void ParseKnownLocales()
        {
            if (LocalizationSettings.AvailableLocales?.Locales == null)
            {
                return;
            }

            foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
            {
                // Skip invalid locale names
                if (string.IsNullOrEmpty(locale.LocaleName) || 
                    locale.LocaleName.Contains("()") || 
                    locale.LocaleName.Contains("filler"))
                {
                    continue;
                }
                
                if (!Enum.TryParse(locale.LocaleName, true, out SystemLanguage language))
                {
                    continue;
                }

                if (!languagesMap.ContainsKey(language))
                {
                    languagesMap.Add(language, locale.Identifier);
                    languages.Add(language);
                }
            }
        }

        /// <summary>
        /// Регистрирует обработку изменений языка от Unity Localization.
        /// Registers Unity Localization language change handling.
        /// </summary>
        private void RegisterUnityLanguageChange()
        {
            try
            {
                Observable.FromEvent<Locale>(
                        method => LocalizationSettings.SelectedLocaleChanged += method,
                        method => LocalizationSettings.SelectedLocaleChanged -= method)
                    .Where(_ => !changing)
                    .Select(locale => languagesMap.Where(x => x.Value == locale.Identifier).FirstOrDefault().Key)
                    .Where(language => language != default(SystemLanguage))
                    .Subscribe(ChangeLanguageTo)
                    .AddTo(disposables);
            }
            catch (Exception)
            {
                // Handle exception silently
            }
        }

        /// <summary>
        /// Освобождает ресурсы сервиса локализации.
        /// Disposes localization service resources.
        /// </summary>
        public void Dispose()
        {
            disposables?.Dispose();
            currentLanguage?.Dispose();
        }
    }
}