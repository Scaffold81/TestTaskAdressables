using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Build;
using System.IO;

namespace Project.Editor.Build
{
    /// <summary>
    /// Automated build script for Addressables with platform optimizations
    /// Автоматизированный скрипт сборки Addressables с оптимизациями для платформ
    /// </summary>
    public static class AddressableBuildScript
    {
        [MenuItem("Addressables/Build/Build WebGL Optimized")]
        public static void BuildWebGLOptimized()
        {
            SetWebGLOptimizations();
            BuildAddressables("Production");
            Debug.Log("[AddressableBuild] WebGL optimized build completed");
        }
        
        [MenuItem("Addressables/Build/Build Android Optimized")]
        public static void BuildAndroidOptimized()
        {
            SetAndroidOptimizations();
            BuildAddressables("Production");
            Debug.Log("[AddressableBuild] Android optimized build completed");
        }
        
        [MenuItem("Addressables/Build/Build Development")]
        public static void BuildDevelopment()
        {
            SetDevelopmentOptimizations();
            BuildAddressables("Development");
            Debug.Log("[AddressableBuild] Development build completed");
        }
        
        private static void BuildAddressables(string profileName)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("[AddressableBuild] Addressable Asset Settings not found!");
                return;
            }
            
            // Set active profile
            var profileId = settings.profileSettings.GetProfileId(profileName);
            if (!string.IsNullOrEmpty(profileId))
            {
                settings.activeProfileId = profileId;
                Debug.Log($"[AddressableBuild] Set active profile to: {profileName}");
            }
            
            // Clean previous build
            AddressableAssetSettings.CleanPlayerContent();
            
            // Build content
            AddressableAssetSettings.BuildPlayerContent();
            
            // Log build info
            var buildPath = settings.RemoteCatalogBuildPath.GetValue(settings);
            Debug.Log($"[AddressableBuild] Build completed at: {buildPath}");
            
            // Show build folder
            if (Directory.Exists(buildPath))
            {
                EditorUtility.RevealInFinder(buildPath);
            }
        }
        
        private static void SetWebGLOptimizations()
        {
            Debug.Log("[AddressableBuild] Applying WebGL optimizations...");
            
            // Player Settings
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.debugSymbols = false;
            
            // Addressables Settings
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings != null)
            {
                settings.MaxConcurrentWebRequests = 6;
                settings.CatalogRequestsTimeout = 30;
                settings.DisableCatalogUpdateOnStartup = false;
            }
        }
        
        private static void SetAndroidOptimizations()
        {
            Debug.Log("[AddressableBuild] Applying Android optimizations...");
            
            // Player Settings
            PlayerSettings.Android.splitApplicationBinary = false;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            EditorUserBuildSettings.buildAppBundle = true;
            
            // Addressables Settings
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings != null)
            {
                settings.MaxConcurrentWebRequests = 4; // Lower for mobile
                settings.CatalogRequestsTimeout = 60; // Higher for mobile networks
            }
        }
        
        private static void SetDevelopmentOptimizations()
        {
            Debug.Log("[AddressableBuild] Applying Development optimizations...");
            
            // Enable faster iteration for development
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings != null)
            {
                settings.MaxConcurrentWebRequests = 10;
                settings.CatalogRequestsTimeout = 10;
                settings.DisableCatalogUpdateOnStartup = true; // Faster startup
            }
        }
        
        [MenuItem("Addressables/Utils/Show Build Folder")]
        public static void ShowBuildFolder()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings != null)
            {
                var buildPath = settings.RemoteCatalogBuildPath.GetValue(settings);
                if (Directory.Exists(buildPath))
                {
                    EditorUtility.RevealInFinder(buildPath);
                }
                else
                {
                    Debug.LogWarning($"[AddressableBuild] Build folder not found: {buildPath}");
                }
            }
        }
        
        [MenuItem("Addressables/Utils/Clean All Builds")]
        public static void CleanAllBuilds()
        {
            AddressableAssetSettings.CleanPlayerContent();
            
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings != null)
            {
                var buildPath = settings.RemoteCatalogBuildPath.GetValue(settings);
                if (Directory.Exists(buildPath))
                {
                    Directory.Delete(buildPath, true);
                    Debug.Log($"[AddressableBuild] Cleaned build folder: {buildPath}");
                }
            }
        }
    }
}
