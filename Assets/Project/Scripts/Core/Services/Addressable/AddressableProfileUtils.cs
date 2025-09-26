using UnityEngine;

namespace Project.Core.Services.Addressable
{
    /// <summary>
    /// Utility class for managing Addressable profiles
    /// Утилитарный класс для управления профилями Addressables
    /// </summary>
    public static class AddressableProfileUtils
    {
        /// <summary>
        /// Profile names constants / Константы имен профилей
        /// </summary>
        public static class ProfileNames
        {
            public const string DEFAULT = "Default";
            public const string DEVELOPMENT = "Development";
            public const string STAGING = "Staging";
            public const string PRODUCTION = "Production";
        }
        
        /// <summary>
        /// Get current active profile name / Получить имя текущего активного профиля
        /// </summary>
        public static string GetCurrentProfileName()
        {
#if UNITY_EDITOR
            return GetEditorProfileName();
#elif DEVELOPMENT_BUILD
            return ProfileNames.DEVELOPMENT;
#elif STAGING
            return ProfileNames.STAGING;
#else
            return ProfileNames.PRODUCTION;
#endif
        }
        
        /// <summary>
        /// Get profile name for editor / Получить имя профиля для редактора
        /// </summary>
        private static string GetEditorProfileName()
        {
#if UNITY_EDITOR
            // Check for custom define symbols / Проверить пользовательские символы
            var namedBuildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(
                UnityEditor.BuildTargetGroup.Standalone);
            var defines = UnityEditor.PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
            
            if (defines.Contains("FORCE_PRODUCTION"))
                return ProfileNames.PRODUCTION;
            if (defines.Contains("FORCE_STAGING"))
                return ProfileNames.STAGING;
            
            return ProfileNames.DEVELOPMENT;
#else
            return ProfileNames.DEFAULT;
#endif
        }
        
        /// <summary>
        /// Get remote load path for profile / Получить путь удаленной загрузки для профиля
        /// </summary>
        public static string GetRemoteLoadPath(string profileName)
        {
            return profileName switch
            {
                ProfileNames.DEVELOPMENT => "http://localhost:8080/[BuildTarget]",
                ProfileNames.STAGING => "https://staging.yourcdn.com/[BuildTarget]",
                ProfileNames.PRODUCTION => "https://cdn.yourprod.com/[BuildTarget]",
                _ => "http://localhost:8080/[BuildTarget]"
            };
        }
        
        /// <summary>
        /// Get build path for profile / Получить путь сборки для профиля
        /// </summary>
        public static string GetBuildPath(string profileName)
        {
            return "ServerData/[BuildTarget]";
        }
        
        /// <summary>
        /// Check if profile supports content updates / Проверить, поддерживает ли профиль обновления контента
        /// </summary>
        public static bool SupportsContentUpdates(string profileName)
        {
            return profileName != ProfileNames.DEFAULT;
        }
        
        /// <summary>
        /// Get profile description / Получить описание профиля
        /// </summary>
        public static string GetProfileDescription(string profileName)
        {
            return profileName switch
            {
                ProfileNames.DEFAULT => "Default Unity profile with local assets",
                ProfileNames.DEVELOPMENT => "Development profile with localhost server",
                ProfileNames.STAGING => "Staging profile with staging CDN",
                ProfileNames.PRODUCTION => "Production profile with production CDN",
                _ => "Unknown profile"
            };
        }
        
        /// <summary>
        /// Log current profile information / Записать информацию о текущем профиле в лог
        /// </summary>
        public static void LogCurrentProfile()
        {
            var currentProfile = GetCurrentProfileName();
            var description = GetProfileDescription(currentProfile);
            var remotePath = GetRemoteLoadPath(currentProfile);
            var buildPath = GetBuildPath(currentProfile);
            var supportsUpdates = SupportsContentUpdates(currentProfile);
            
            Debug.Log($"[AddressableProfile] Current: {currentProfile}\n" +
                     $"Description: {description}\n" +
                     $"Remote Path: {remotePath}\n" +
                     $"Build Path: {buildPath}\n" +
                     $"Content Updates: {(supportsUpdates ? "Enabled" : "Disabled")}");
        }
    }
}