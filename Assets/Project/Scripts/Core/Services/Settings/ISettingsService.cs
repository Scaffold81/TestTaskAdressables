using R3;
using UnityEngine;

namespace Game.Services
{
    /// <summary>
    /// Интерфейс сервиса настроек для централизованного управления пользовательскими предпочтениями.
    /// Settings service interface for centralized management of user preferences.
    /// </summary>
    public interface ISettingsService
    {
        // Audio Settings
        ReactiveProperty<float> MasterVolume { get; }
        ReactiveProperty<float> MusicVolume { get; }
        ReactiveProperty<float> SFXVolume { get; }
        ReactiveProperty<float> UIVolume { get; }
        ReactiveProperty<bool> IsMuted { get; }
        
        // Graphics Settings
        ReactiveProperty<int> GraphicsQuality { get; }
        ReactiveProperty<int> TargetFrameRate { get; }
        ReactiveProperty<bool> VSync { get; }
        ReactiveProperty<bool> Fullscreen { get; }
        
        // Gameplay Settings
        ReactiveProperty<int> GameDifficulty { get; }
        ReactiveProperty<bool> AutoSave { get; }
        ReactiveProperty<bool> ShowTutorial { get; }
        ReactiveProperty<float> InputSensitivity { get; }
        
        // System Settings
        ReactiveProperty<SystemLanguage> Language { get; }
        ReactiveProperty<bool> AnalyticsEnabled { get; }
        ReactiveProperty<bool> CloudSaveEnabled { get; }
        
        // Core Operations
        void ResetToDefaults();
        void ResetCategory(string categoryName);
        void SaveSettings();
        void LoadSettings();
        
        // Generic get/set operations
        T GetSetting<T>(string key, T defaultValue = default);
        void SetSetting<T>(string key, T value);
        bool HasSetting(string key);
        
        // Events
        Observable<string> OnSettingChanged { get; }
        Observable<string> OnCategoryChanged { get; }
    }
}
