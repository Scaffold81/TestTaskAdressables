using System;
using UnityEngine;

namespace Project.Core.Services.Addressable.Models
{
    /// <summary>
    /// Settings for Content Update workflow
    /// Настройки для процесса Content Update
    /// </summary>
    [Serializable]
    public class ContentUpdateSettings
    {
        [Header("Update Settings / Настройки обновлений")]
        
        /// <summary>
        /// Enable automatic content updates / Включить автоматические обновления контента
        /// </summary>
        public bool EnableAutoUpdates = true;
        
        /// <summary>
        /// Check for updates on startup / Проверять обновления при запуске
        /// </summary>
        public bool CheckUpdatesOnStartup = true;
        
        /// <summary>
        /// Update check interval in minutes / Интервал проверки обновлений в минутах
        /// </summary>
        [Range(5, 1440)]
        public int UpdateCheckIntervalMinutes = 60;
        
        [Header("Catalog Settings / Настройки каталогов")]
        
        /// <summary>
        /// Main catalog configuration / Конфигурация основного каталога
        /// </summary>
        public CatalogConfig MainCatalog = new CatalogConfig("main", false);
        
        /// <summary>
        /// Levels catalog configuration / Конфигурация каталога уровней
        /// </summary>
        public CatalogConfig LevelsCatalog = new CatalogConfig("catalog_levels", true);
        
        [Header("Fallback Settings / Настройки fallback")]
        
        /// <summary>
        /// Enable fallback to local assets / Включить fallback на локальные ресурсы
        /// </summary>
        public bool EnableFallback = true;
        
        /// <summary>
        /// Fallback timeout in seconds / Таймаут fallback в секундах
        /// </summary>
        [Range(10, 120)]
        public float FallbackTimeoutSeconds = 30f;
        
        /// <summary>
        /// Show user notification on fallback / Показывать уведомление пользователю при fallback
        /// </summary>
        public bool ShowFallbackNotification = true;
    }
    
    /// <summary>
    /// Configuration for individual catalog
    /// Конфигурация для отдельного каталога
    /// </summary>
    [Serializable]
    public class CatalogConfig
    {
        /// <summary>
        /// Catalog file name / Имя файла каталога
        /// </summary>
        public string FileName;
        
        /// <summary>
        /// Is separate catalog / Является ли отдельным каталогом
        /// </summary>
        public bool IsSeparate;
        
        /// <summary>
        /// Priority for loading (lower = higher priority) / Приоритет загрузки (меньше = выше приоритет)
        /// </summary>
        public int LoadPriority;
        
        /// <summary>
        /// Enable independent updates / Включить независимые обновления
        /// </summary>
        public bool EnableIndependentUpdates;
        
        /// <summary>
        /// Constructor / Конструктор
        /// </summary>
        public CatalogConfig(string fileName, bool isSeparate, int loadPriority = 0, bool enableIndependentUpdates = true)
        {
            FileName = fileName;
            IsSeparate = isSeparate;
            LoadPriority = loadPriority;
            EnableIndependentUpdates = enableIndependentUpdates;
        }
    }
}