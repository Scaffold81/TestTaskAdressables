using System;
using UnityEngine;

namespace Project.Core.Services.Addressable.Models
{
    /// <summary>
    /// Platform-specific settings for Addressables
    /// Специфичные для платформы настройки Addressables
    /// </summary>
    [Serializable]
    public class PlatformSettings
    {
        /// <summary>
        /// WebGL specific settings / Настройки для WebGL
        /// </summary>
        [Serializable]
        public class WebGLSettings
        {
            public int MaxConcurrentRequests = 6;
            public int CatalogDownloadTimeout = 30;
            public bool DisableCatalogUpdateOnStart = false;
            public bool EnableBrotliCompression = true;
            public int FirstLoadBudgetMB = 30;
        }
        
        /// <summary>
        /// Android specific settings / Настройки для Android
        /// </summary>
        [Serializable]
        public class AndroidSettings
        {
            public bool UseAssetDatabase = false;
            public bool SimulateGroups = false;
            public int MaxConcurrentRequests = 4;
            public int FirstLoadBudgetMB = 15;
            public bool UseArmV7 = false;
            public bool UseArm64 = true;
        }
        
        /// <summary>
        /// iOS specific settings / Настройки для iOS
        /// </summary>
        [Serializable]
        public class IOSSettings
        {
            public int MaxConcurrentRequests = 4;
            public int FirstLoadBudgetMB = 20;
            public bool EnableOnDemandResources = false;
        }
        
        /// <summary>
        /// Standalone specific settings / Настройки для Standalone
        /// </summary>
        [Serializable]
        public class StandaloneSettings
        {
            public int MaxConcurrentRequests = 10;
            public int FirstLoadBudgetMB = 50;
        }
        
        public WebGLSettings WebGL = new WebGLSettings();
        public AndroidSettings Android = new AndroidSettings();
        public IOSSettings IOS = new IOSSettings();
        public StandaloneSettings Standalone = new StandaloneSettings();
        
        /// <summary>
        /// Get settings for current platform / Получить настройки для текущей платформы
        /// </summary>
        public object GetCurrentPlatformSettings()
        {
#if UNITY_WEBGL
            return WebGL;
#elif UNITY_ANDROID
            return Android;
#elif UNITY_IOS
            return IOS;
#else
            return Standalone;
#endif
        }
        
        /// <summary>
        /// Get current platform name / Получить имя текущей платформы
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
            return "Standalone";
#endif
        }
        
        /// <summary>
        /// Get max concurrent requests for current platform / Получить макс. одновременных запросов для текущей платформы
        /// </summary>
        public int GetMaxConcurrentRequests()
        {
#if UNITY_WEBGL
            return WebGL.MaxConcurrentRequests;
#elif UNITY_ANDROID
            return Android.MaxConcurrentRequests;
#elif UNITY_IOS
            return IOS.MaxConcurrentRequests;
#else
            return Standalone.MaxConcurrentRequests;
#endif
        }
        
        /// <summary>
        /// Get first load budget in MB for current platform / Получить бюджет первой загрузки в МБ для текущей платформы
        /// </summary>
        public int GetFirstLoadBudgetMB()
        {
#if UNITY_WEBGL
            return WebGL.FirstLoadBudgetMB;
#elif UNITY_ANDROID
            return Android.FirstLoadBudgetMB;
#elif UNITY_IOS
            return IOS.FirstLoadBudgetMB;
#else
            return Standalone.FirstLoadBudgetMB;
#endif
        }
    }
}