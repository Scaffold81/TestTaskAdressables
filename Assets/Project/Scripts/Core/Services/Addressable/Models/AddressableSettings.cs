using System;
using UnityEngine;

namespace Project.Core.Services.Addressable.Models
{
    /// <summary>
    /// Settings for Addressable system behavior
    /// Настройки поведения системы Addressables
    /// </summary>
    [Serializable]
    public class AddressableSettings
    {
        [Header("Profile Settings / Настройки профилей")]
        
        /// <summary>
        /// Default profile name / Имя профиля по умолчанию
        /// </summary>
        public string DefaultProfile = "Default";
        
        /// <summary>
        /// Development profile name / Имя профиля разработки
        /// </summary>
        public string DevelopmentProfile = "Development";
        
        /// <summary>
        /// Production profile name / Имя профиля продакшена
        /// </summary>
        public string ProductionProfile = "Production";
        
        [Header("Retry Settings / Настройки повторных попыток")]
        
        /// <summary>
        /// Enable retry mechanism / Включить механизм повторных попыток
        /// </summary>
        public bool EnableRetry = true;
        
        /// <summary>
        /// Maximum number of retries / Максимальное количество повторных попыток
        /// </summary>
        [Range(1, 5)]
        public int MaxRetries = 3;
        
        /// <summary>
        /// Base delay between retries in seconds / Базовая задержка между попытками в секундах
        /// </summary>
        [Range(0.5f, 5f)]
        public float RetryDelay = 1.0f;
        
        [Header("Cache Settings / Настройки кеша")]
        
        /// <summary>
        /// Enable caching / Включить кеширование
        /// </summary>
        public bool EnableCaching = true;
        
        /// <summary>
        /// Maximum cache size in MB / Максимальный размер кеша в МБ
        /// </summary>
        [Range(50, 500)]
        public int MaxCacheSizeMB = 100;
        
        [Header("Content Update Settings / Настройки обновления контента")]
        
        /// <summary>
        /// Content update workflow settings / Настройки процесса обновления контента
        /// </summary>
        public ContentUpdateSettings ContentUpdate = new ContentUpdateSettings();
        
        [Header("Performance Settings / Настройки производительности")]
        
        /// <summary>
        /// First load budget in MB for WebGL / Бюджет первой загрузки в МБ для WebGL
        /// </summary>
        [Range(10, 50)]
        public int FirstLoadBudgetWebGL = 30;
        
        /// <summary>
        /// First load budget in MB for Mobile / Бюджет первой загрузки в МБ для мобильных
        /// </summary>
        [Range(5, 30)]
        public int FirstLoadBudgetMobile = 15;
        
        /// <summary>
        /// Enable diagnostic logging / Включить диагностические логи
        /// </summary>
        public bool EnableDiagnosticLogging = true;
        
        [Header("Platform Settings / Платформенные настройки")]
        
        /// <summary>
        /// Platform-specific settings / Платформо-специфичные настройки
        /// </summary>
        public PlatformSettings Platform = new PlatformSettings();
        
        /// <summary>
        /// Get current platform budget / Получить бюджет для текущей платформы
        /// </summary>
        public int GetCurrentPlatformBudget()
        {
#if UNITY_WEBGL
            return FirstLoadBudgetWebGL;
#elif UNITY_ANDROID || UNITY_IOS
            return FirstLoadBudgetMobile;
#else
            return FirstLoadBudgetWebGL;
#endif
        }
        
        /// <summary>
        /// Get exponential backoff delay / Получить задержку экспоненциального отката
        /// </summary>
        public float GetExponentialBackoffDelay(int attemptNumber)
        {
            return RetryDelay * Mathf.Pow(2, attemptNumber);
        }
    }
}