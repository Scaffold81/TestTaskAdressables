using System;
using UnityEngine;

namespace Project.Core.Services.Addressable.Models
{
    /// <summary>
    /// Platform-specific settings for Addressables
    /// Платформо-специфичные настройки для Addressables
    /// </summary>
    [Serializable]
    public class PlatformSettings
    {
        [Header("WebGL Settings")]
        
        /// <summary>
        /// Maximum concurrent web requests for WebGL / Максимальное количество одновременных веб-запросов для WebGL
        /// </summary>
        [Range(1, 10)]
        public int WebGLMaxConcurrentRequests = 6;
        
        /// <summary>
        /// Catalog download timeout for WebGL in seconds / Таймаут загрузки каталога для WebGL в секундах
        /// </summary>
        [Range(10, 120)]
        public int WebGLCatalogTimeout = 30;
        
        /// <summary>
        /// Disable catalog update on start for WebGL / Отключить обновление каталога при запуске для WebGL
        /// </summary>
        public bool WebGLDisableCatalogUpdateOnStart = false;
        
        [Header("Android Settings")]
        
        /// <summary>
        /// Use Asset Database for Android development / Использовать Asset Database для разработки под Android
        /// </summary>
        public bool AndroidUseAssetDatabase = false;
        
        /// <summary>
        /// Simulate groups for Android development / Симулировать группы для разработки под Android
        /// </summary>
        public bool AndroidSimulateGroups = false;
        
        /// <summary>
        /// Enable split APKs by architecture / Включить разделение APK по архитектуре
        /// </summary>
        public bool AndroidSplitByArchitecture = true;
        
        [Header("iOS Settings")]
        
        /// <summary>
        /// Enable on-demand resources for iOS / Включить ресурсы по требованию для iOS
        /// </summary>
        public bool IOSOnDemandResources = false;
        
        /// <summary>
        /// iOS bundle identifier prefix / Префикс идентификатора bundle для iOS
        /// </summary>
        public string IOSBundlePrefix = "com.company.game";
        
        [Header("Network Settings")]
        
        /// <summary>
        /// Connection timeout in seconds / Таймаут соединения в секундах
        /// </summary>
        [Range(10, 300)]
        public int ConnectionTimeoutSeconds = 60;
        
        /// <summary>
        /// Enable retry on network failure / Включить повтор при сетевой ошибке
        /// </summary>
        public bool EnableNetworkRetry = true;
        
        /// <summary>
        /// Network retry attempts / Количество попыток при сетевой ошибке
        /// </summary>
        [Range(1, 5)]
        public int NetworkRetryAttempts = 3;
        
        /// <summary>
        /// Get settings for current platform / Получить настройки для текущей платформы
        /// </summary>
        public PlatformSpecificSettings GetCurrentPlatformSettings()
        {
#if UNITY_WEBGL
            return new PlatformSpecificSettings
            {
                MaxConcurrentRequests = WebGLMaxConcurrentRequests,
                CatalogTimeout = WebGLCatalogTimeout,
                DisableCatalogUpdateOnStart = WebGLDisableCatalogUpdateOnStart,
                UseAssetDatabase = false,
                SimulateGroups = false
            };
#elif UNITY_ANDROID
            return new PlatformSpecificSettings
            {
                MaxConcurrentRequests = 4,
                CatalogTimeout = 45,
                DisableCatalogUpdateOnStart = false,
                UseAssetDatabase = AndroidUseAssetDatabase,
                SimulateGroups = AndroidSimulateGroups
            };
#elif UNITY_IOS
            return new PlatformSpecificSettings
            {
                MaxConcurrentRequests = 4,
                CatalogTimeout = 45,
                DisableCatalogUpdateOnStart = false,
                UseAssetDatabase = false,
                SimulateGroups = false
            };
#else
            return new PlatformSpecificSettings
            {
                MaxConcurrentRequests = 6,
                CatalogTimeout = 30,
                DisableCatalogUpdateOnStart = false,
                UseAssetDatabase = true,
                SimulateGroups = true
            };
#endif
        }
        
        /// <summary>
        /// Get platform name / Получить название платформы
        /// </summary>
        public string GetCurrentPlatformName()
        {
#if UNITY_WEBGL
            return "WebGL";
#elif UNITY_ANDROID
            return "Android";
#elif UNITY_IOS
            return "iOS";
#else
            return "Editor";
#endif
        }
    }
    
    /// <summary>
    /// Platform-specific settings structure / Структура платформо-специфичных настроек
    /// </summary>
    [Serializable]
    public class PlatformSpecificSettings
    {
        public int MaxConcurrentRequests;
        public int CatalogTimeout;
        public bool DisableCatalogUpdateOnStart;
        public bool UseAssetDatabase;
        public bool SimulateGroups;
    }
}