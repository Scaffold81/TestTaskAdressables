using R3;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Services
{
    /// <summary>
    /// Сервис настроек для централизованного управления пользовательскими предпочтениями.
    /// Интегрируется с SaveService для персистентности и другими сервисами для синхронизации.
    /// Settings service for centralized management of user preferences.
    /// Integrates with SaveService for persistence and other services for synchronization.
    /// </summary>
    public class SettingsService : ISettingsService, IDisposable
    {
        // Dependencies
        private readonly ISaveService saveService;
        private readonly CompositeDisposable disposables = new();
        
        // Audio Settings
        public ReactiveProperty<float> MasterVolume { get; private set; } = new(1f);
        public ReactiveProperty<float> MusicVolume { get; private set; } = new(0.8f);
        public ReactiveProperty<float> SFXVolume { get; private set; } = new(1f);
        public ReactiveProperty<float> UIVolume { get; private set; } = new(1f);
        public ReactiveProperty<bool> IsMuted { get; private set; } = new(false);
        
        // Graphics Settings
        public ReactiveProperty<int> GraphicsQuality { get; private set; } = new(2); // Medium
        public ReactiveProperty<int> TargetFrameRate { get; private set; } = new(60);
        public ReactiveProperty<bool> VSync { get; private set; } = new(true);
        public ReactiveProperty<bool> Fullscreen { get; private set; } = new(true);
        
        // Gameplay Settings
        public ReactiveProperty<int> GameDifficulty { get; private set; } = new(1); // Normal
        public ReactiveProperty<bool> AutoSave { get; private set; } = new(true);
        public ReactiveProperty<bool> ShowTutorial { get; private set; } = new(true);
        public ReactiveProperty<float> InputSensitivity { get; private set; } = new(1f);
        
        // System Settings
        public ReactiveProperty<SystemLanguage> Language { get; private set; } = new(SystemLanguage.English);
        public ReactiveProperty<bool> AnalyticsEnabled { get; private set; } = new(true);
        public ReactiveProperty<bool> CloudSaveEnabled { get; private set; } = new(false);
        
        // Events
        private readonly Subject<string> onSettingChanged = new();
        private readonly Subject<string> onCategoryChanged = new();
        public Observable<string> OnSettingChanged => onSettingChanged.AsObservable();
        public Observable<string> OnCategoryChanged => onCategoryChanged.AsObservable();
        
        // Internal state
        private readonly Dictionary<string, object> customSettings = new();
        private bool isInitialized = false;
        
        // Save keys
        private const string SETTINGS_SAVE_KEY = "GameSettings";
        
        /// <summary>
        /// Конструктор сервиса настроек с инъекцией зависимостей.
        /// Settings service constructor with dependency injection.
        /// </summary>
        /// <param name="saveService">Сервис сохранения / Save service</param>
        [Inject]
        public SettingsService(ISaveService saveService)
        {
            this.saveService = saveService;
            
            Initialize();
        }
        
        /// <summary>
        /// Инициализирует сервис настроек.
        /// Initializes settings service.
        /// </summary>
        private void Initialize()
        {
            LoadSettings();
            SubscribeToSettingsChanges();
            
            isInitialized = true;
        }
        
        /// <summary>
        /// Подписывается на изменения настроек для автоматического сохранения.
        /// Subscribes to settings changes for automatic saving.
        /// </summary>
        private void SubscribeToSettingsChanges()
        {
            // Audio settings subscriptions
            MasterVolume.Subscribe(_ => { SaveSettings(); onSettingChanged.OnNext("MasterVolume"); }).AddTo(disposables);
            MusicVolume.Subscribe(_ => { SaveSettings(); onSettingChanged.OnNext("MusicVolume"); }).AddTo(disposables);
            SFXVolume.Subscribe(_ => { SaveSettings(); onSettingChanged.OnNext("SFXVolume"); }).AddTo(disposables);
            UIVolume.Subscribe(_ => { SaveSettings(); onSettingChanged.OnNext("UIVolume"); }).AddTo(disposables);
            IsMuted.Subscribe(_ => { SaveSettings(); onSettingChanged.OnNext("IsMuted"); }).AddTo(disposables);
            
            // Graphics settings subscriptions
            GraphicsQuality.Subscribe(_ => { SaveSettings(); onSettingChanged.OnNext("GraphicsQuality"); }).AddTo(disposables);
            TargetFrameRate.Subscribe(_ => { SaveSettings(); onSettingChanged.OnNext("TargetFrameRate"); }).AddTo(disposables);
            VSync.Subscribe(_ => { SaveSettings(); onSettingChanged.OnNext("VSync"); }).AddTo(disposables);
            Fullscreen.Subscribe(_ => { SaveSettings(); onSettingChanged.OnNext("Fullscreen"); }).AddTo(disposables);
            
            // Gameplay settings subscriptions
            GameDifficulty.Subscribe(_ => { SaveSettings(); onSettingChanged.OnNext("GameDifficulty"); }).AddTo(disposables);
            AutoSave.Subscribe(_ => { SaveSettings(); onSettingChanged.OnNext("AutoSave"); }).AddTo(disposables);
            ShowTutorial.Subscribe(_ => { SaveSettings(); onSettingChanged.OnNext("ShowTutorial"); }).AddTo(disposables);
            InputSensitivity.Subscribe(_ => { SaveSettings(); onSettingChanged.OnNext("InputSensitivity"); }).AddTo(disposables);
            
            // System settings subscriptions
            Language.Subscribe(_ => { SaveSettings(); onSettingChanged.OnNext("Language"); }).AddTo(disposables);
            AnalyticsEnabled.Subscribe(_ => { SaveSettings(); onSettingChanged.OnNext("AnalyticsEnabled"); }).AddTo(disposables);
            CloudSaveEnabled.Subscribe(_ => { SaveSettings(); onSettingChanged.OnNext("CloudSaveEnabled"); }).AddTo(disposables);
        }
        
        /// <summary>
        /// Сбрасывает все настройки к значениям по умолчанию.
        /// Resets all settings to default values.
        /// </summary>
        public void ResetToDefaults()
        {
            // Audio defaults
            MasterVolume.Value = 1f;
            MusicVolume.Value = 0.8f;
            SFXVolume.Value = 1f;
            UIVolume.Value = 1f;
            IsMuted.Value = false;
            
            // Graphics defaults
            GraphicsQuality.Value = 2; // Medium
            TargetFrameRate.Value = 60;
            VSync.Value = true;
            Fullscreen.Value = true;
            
            // Gameplay defaults
            GameDifficulty.Value = 1; // Normal
            AutoSave.Value = true;
            ShowTutorial.Value = true;
            InputSensitivity.Value = 1f;
            
            // System defaults
            Language.Value = Application.systemLanguage;
            AnalyticsEnabled.Value = true;
            CloudSaveEnabled.Value = false;
            
            // Clear custom settings
            customSettings.Clear();
            
            SaveSettings();
        }
        
        /// <summary>
        /// Сбрасывает настройки определенной категории к значениям по умолчанию.
        /// Resets settings of specific category to default values.
        /// </summary>
        /// <param name="categoryName">Название категории / Category name</param>
        public void ResetCategory(string categoryName)
        {
            switch (categoryName.ToLower())
            {
                case "audio":
                    MasterVolume.Value = 1f;
                    MusicVolume.Value = 0.8f;
                    SFXVolume.Value = 1f;
                    UIVolume.Value = 1f;
                    IsMuted.Value = false;
                    break;
                    
                case "graphics":
                    GraphicsQuality.Value = 2;
                    TargetFrameRate.Value = 60;
                    VSync.Value = true;
                    Fullscreen.Value = true;
                    break;
                    
                case "gameplay":
                    GameDifficulty.Value = 1;
                    AutoSave.Value = true;
                    ShowTutorial.Value = true;
                    InputSensitivity.Value = 1f;
                    break;
                    
                case "system":
                    Language.Value = Application.systemLanguage;
                    AnalyticsEnabled.Value = true;
                    CloudSaveEnabled.Value = false;
                    break;
            }
            
            onCategoryChanged.OnNext(categoryName);
        }
        
        /// <summary>
        /// Сохраняет все настройки через SaveService.
        /// Saves all settings through SaveService.
        /// </summary>
        public void SaveSettings()
        {
            if (!isInitialized) return;
            
            var settingsData = new SettingsData
            {
                // Audio
                masterVolume = MasterVolume.Value,
                musicVolume = MusicVolume.Value,
                sfxVolume = SFXVolume.Value,
                uiVolume = UIVolume.Value,
                isMuted = IsMuted.Value,
                
                // Graphics
                graphicsQuality = GraphicsQuality.Value,
                targetFrameRate = TargetFrameRate.Value,
                vSync = VSync.Value,
                fullscreen = Fullscreen.Value,
                
                // Gameplay
                gameDifficulty = GameDifficulty.Value,
                autoSave = AutoSave.Value,
                showTutorial = ShowTutorial.Value,
                inputSensitivity = InputSensitivity.Value,
                
                // System
                language = (int)Language.Value,
                analyticsEnabled = AnalyticsEnabled.Value,
                cloudSaveEnabled = CloudSaveEnabled.Value,
                
                // Custom settings
                customSettings = this.customSettings
            };
            
            saveService.SaveJson(SETTINGS_SAVE_KEY, settingsData);
        }
        
        /// <summary>
        /// Загружает все настройки через SaveService.
        /// Loads all settings through SaveService.
        /// </summary>
        public void LoadSettings()
        {
            var settingsData = saveService.LoadJson<SettingsData>(SETTINGS_SAVE_KEY);
            
            if (settingsData == null)
            {
                // First launch - use defaults
                return;
            }
            
            // Audio
            MasterVolume.Value = settingsData.masterVolume;
            MusicVolume.Value = settingsData.musicVolume;
            SFXVolume.Value = settingsData.sfxVolume;
            UIVolume.Value = settingsData.uiVolume;
            IsMuted.Value = settingsData.isMuted;
            
            // Graphics
            GraphicsQuality.Value = settingsData.graphicsQuality;
            TargetFrameRate.Value = settingsData.targetFrameRate;
            VSync.Value = settingsData.vSync;
            Fullscreen.Value = settingsData.fullscreen;
            
            // Gameplay
            GameDifficulty.Value = settingsData.gameDifficulty;
            AutoSave.Value = settingsData.autoSave;
            ShowTutorial.Value = settingsData.showTutorial;
            InputSensitivity.Value = settingsData.inputSensitivity;
            
            // System
            Language.Value = (SystemLanguage)settingsData.language;
            AnalyticsEnabled.Value = settingsData.analyticsEnabled;
            CloudSaveEnabled.Value = settingsData.cloudSaveEnabled;
            
            // Custom settings
            if (settingsData.customSettings != null)
            {
                customSettings.Clear();
                foreach (var kvp in settingsData.customSettings)
                {
                    customSettings[kvp.Key] = kvp.Value;
                }
            }
        }
        
        /// <summary>
        /// Получает значение настройки по ключу.
        /// Gets setting value by key.
        /// </summary>
        /// <typeparam name="T">Тип значения / Value type</typeparam>
        /// <param name="key">Ключ настройки / Setting key</param>
        /// <param name="defaultValue">Значение по умолчанию / Default value</param>
        /// <returns>Значение настройки / Setting value</returns>
        public T GetSetting<T>(string key, T defaultValue = default)
        {
            if (customSettings.TryGetValue(key, out var value))
            {
                try
                {
                    return (T)value;
                }
                catch
                {
                    return defaultValue;
                }
            }
            
            return defaultValue;
        }
        
        /// <summary>
        /// Устанавливает значение настройки по ключу.
        /// Sets setting value by key.
        /// </summary>
        /// <typeparam name="T">Тип значения / Value type</typeparam>
        /// <param name="key">Ключ настройки / Setting key</param>
        /// <param name="value">Значение настройки / Setting value</param>
        public void SetSetting<T>(string key, T value)
        {
            customSettings[key] = value;
            SaveSettings();
            onSettingChanged.OnNext(key);
        }
        
        /// <summary>
        /// Проверяет наличие настройки по ключу.
        /// Checks if setting exists by key.
        /// </summary>
        /// <param name="key">Ключ настройки / Setting key</param>
        /// <returns>True если настройка существует / True if setting exists</returns>
        public bool HasSetting(string key)
        {
            return customSettings.ContainsKey(key);
        }
        
        /// <summary>
        /// Освобождает ресурсы сервиса настроек.
        /// Disposes settings service resources.
        /// </summary>
        public void Dispose()
        {
            disposables?.Dispose();
            onSettingChanged?.Dispose();
            onCategoryChanged?.Dispose();
            
            // Dispose reactive properties
            MasterVolume?.Dispose();
            MusicVolume?.Dispose();
            SFXVolume?.Dispose();
            UIVolume?.Dispose();
            IsMuted?.Dispose();
            
            GraphicsQuality?.Dispose();
            TargetFrameRate?.Dispose();
            VSync?.Dispose();
            Fullscreen?.Dispose();
            
            GameDifficulty?.Dispose();
            AutoSave?.Dispose();
            ShowTutorial?.Dispose();
            InputSensitivity?.Dispose();
            
            Language?.Dispose();
            AnalyticsEnabled?.Dispose();
            CloudSaveEnabled?.Dispose();
        }
    }
    
    /// <summary>
    /// Структура данных для сериализации настроек.
    /// Data structure for settings serialization.
    /// </summary>
    [System.Serializable]
    public class SettingsData
    {
        // Audio
        public float masterVolume = 1f;
        public float musicVolume = 0.8f;
        public float sfxVolume = 1f;
        public float uiVolume = 1f;
        public bool isMuted = false;
        
        // Graphics
        public int graphicsQuality = 2;
        public int targetFrameRate = 60;
        public bool vSync = true;
        public bool fullscreen = true;
        
        // Gameplay
        public int gameDifficulty = 1;
        public bool autoSave = true;
        public bool showTutorial = true;
        public float inputSensitivity = 1f;
        
        // System
        public int language = (int)SystemLanguage.English;
        public bool analyticsEnabled = true;
        public bool cloudSaveEnabled = false;
        
        // Custom settings
        public Dictionary<string, object> customSettings = new();
    }
}
