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
        /// Profile names / Названия профилей
        /// </summary>
        public static class Profiles
        {
            public const string Development = "Development";
            public const string Staging = "Staging";
            public const string Production = "Production";
            public const string Default = "Default";
        }
        
        /// <summary>
        /// Get current active profile name
        /// Получить имя текущего активного профиля
        /// </summary>
        public static string GetCurrentProfile()
        {
#if UNITY_EDITOR
            return GetEditorProfile();
#else
            return GetRuntimeProfile();
#endif
        }
        
        /// <summary>
        /// Get profile for editor
        /// Получить профиль для редактора
        /// </summary>
        private static string GetEditorProfile()
        {
#if UNITY_EDITOR
            // Check for custom define symbols / Проверить кастомные символы
            var buildTargetGroup = UnityEditor.EditorUserBuildSettings.selectedBuildTargetGroup;
            var namedBuildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
            
            UnityEditor.PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget, out string[] symbols);
            var symbolsString = string.Join(";", symbols);
            
            if (symbolsString.Contains("USE_PRODUCTION_PROFILE"))
                return Profiles.Production;
            if (symbolsString.Contains("USE_STAGING_PROFILE"))
                return Profiles.Staging;
            
            return Profiles.Development;
#else
            return Profiles.Development;
#endif
        }
        
        /// <summary>
        /// Get profile for runtime
        /// Получить профиль для рантайма
        /// </summary>
        private static string GetRuntimeProfile()
        {
            // Check environment variable / Проверить переменную окружения
            var envProfile = System.Environment.GetEnvironmentVariable("ADDRESSABLES_PROFILE");
            if (!string.IsNullOrEmpty(envProfile))
            {
                Debug.Log($"[AddressableProfile] Using profile from environment: {envProfile}");
                return envProfile;
            }
            
            // Check build configuration / Проверить конфигурацию билда
#if DEVELOPMENT_BUILD
            return Profiles.Development;
#elif STAGING_BUILD
            return Profiles.Staging;
#else
            return Profiles.Production;
#endif
        }
        
        /// <summary>
        /// Check if current profile is development
        /// Проверить, является ли текущий профиль разработческим
        /// </summary>
        public static bool IsDevelopmentProfile()
        {
            return GetCurrentProfile() == Profiles.Development;
        }
        
        /// <summary>
        /// Check if current profile is production
        /// Проверить, является ли текущий профиль продакшеном
        /// </summary>
        public static bool IsProductionProfile()
        {
            return GetCurrentProfile() == Profiles.Production;
        }
        
        /// <summary>
        /// Check if current profile is staging
        /// Проверить, является ли текущий профиль стейджингом
        /// </summary>
        public static bool IsStagingProfile()
        {
            return GetCurrentProfile() == Profiles.Staging;
        }
        
        /// <summary>
        /// Get remote load path for current profile
        /// Получить путь удаленной загрузки для текущего профиля
        /// </summary>
        public static string GetRemoteLoadPath()
        {
            var profile = GetCurrentProfile();
            
            return profile switch
            {
                Profiles.Development => "http://localhost:8080/[BuildTarget]",
                Profiles.Staging => "https://staging.yourcdn.com/[BuildTarget]",
                Profiles.Production => "https://cdn.yourprod.com/[BuildTarget]",
                _ => "ServerData/[BuildTarget]"
            };
        }
        
        /// <summary>
        /// Get build path for current profile
        /// Получить путь сборки для текущего профиля
        /// </summary>
        public static string GetBuildPath()
        {
            return "ServerData/[BuildTarget]";
        }
        
        /// <summary>
        /// Log current profile information
        /// Залогировать информацию о текущем профиле
        /// </summary>
        public static void LogProfileInfo()
        {
            var profile = GetCurrentProfile();
            var loadPath = GetRemoteLoadPath();
            var buildPath = GetBuildPath();
            
            Debug.Log($"[AddressableProfile] Active Profile: {profile}");
            Debug.Log($"[AddressableProfile] Remote Load Path: {loadPath}");
            Debug.Log($"[AddressableProfile] Build Path: {buildPath}");
        }
    }
}