using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Project.Core.Services.Addressable;
using Project.Core.Services.Loading;

namespace Project.Core.Services.Addressable
{
    /// <summary>
    /// Extension methods for AddressableService integration with LoadingService
    /// Методы расширения для интеграции AddressableService с LoadingService
    /// </summary>
    public static class AddressableServiceExtensions
    {
        /// <summary>
        /// Download dependencies with progress tracking through LoadingService
        /// Загрузить зависимости с отслеживанием прогресса через LoadingService
        /// </summary>
        public static async UniTask DownloadDependenciesWithProgressAsync(
            this IAddressableService addressableService,
            IEnumerable<string> keys,
            ILoadingService loadingService,
            string loadingTitle = "Downloading Content")
        {
            if (addressableService == null || loadingService == null) return;
            
            var keyArray = keys?.ToArray();
            if (keyArray == null || keyArray.Length == 0) return;

            try
            {
                // Show loading screen
                loadingService.ShowProgress(loadingTitle, "Preparing download...", 0f);
                
                // Get total download size first
                var totalSize = await addressableService.GetDownloadSizeAsync(keyArray);
                var sizeText = totalSize > 0 ? $"({FormatBytes(totalSize)})" : "";
                
                if (totalSize == 0)
                {
                    loadingService.UpdateProgress("Content already cached", 1f);
                    await UniTask.Delay(TimeSpan.FromSeconds(0.5f)); // Brief pause to show message
                    return;
                }

                // Create progress tracker
                var progress = new Progress<float>(progressValue =>
                {
                    var statusText = $"Downloading content... {sizeText}";
                    loadingService.UpdateProgress(statusText, progressValue);
                });

                // Download with progress
                await addressableService.DownloadDependenciesAsync(keyArray, progress);
                
                // Final update
                loadingService.UpdateProgress("Download complete!", 1f);
                
                Debug.Log($"[AddressableServiceExtensions] Downloaded {keyArray.Length} dependencies successfully");
            }
            catch (Exception ex)
            {
                loadingService.UpdateProgress($"Download failed: {ex.Message}", 1f);
                Debug.LogError($"[AddressableServiceExtensions] Download failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Load asset with loading progress display
        /// Загрузить ресурс с отображением прогресса загрузки
        /// </summary>
        public static async UniTask<T> LoadAssetWithProgressAsync<T>(
            this IAddressableService addressableService,
            string key,
            ILoadingService loadingService,
            string loadingTitle = null) where T : UnityEngine.Object
        {
            if (addressableService == null) return null;
            
            var title = loadingTitle ?? $"Loading {typeof(T).Name}";
            
            try
            {
                if (loadingService != null)
                {
                    loadingService.ShowProgress(title, $"Loading {key}...", 0f);
                }
                
                var asset = await addressableService.LoadAssetAsync<T>(key);
                
                if (loadingService != null)
                {
                    loadingService.UpdateProgress("Asset loaded successfully!", 1f);
                }
                
                return asset;
            }
            catch (Exception ex)
            {
                if (loadingService != null)
                {
                    loadingService.UpdateProgress($"Failed to load {key}: {ex.Message}", 1f);
                }
                
                Debug.LogError($"[AddressableServiceExtensions] Failed to load {key}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Load scene with progress display
        /// Загрузить сцену с отображением прогресса
        /// </summary>
        public static async UniTask LoadSceneWithProgressAsync(
            this IAddressableService addressableService,
            string sceneKey,
            ILoadingService loadingService,
            string loadingTitle = null)
        {
            if (addressableService == null) return;
            
            var title = loadingTitle ?? "Loading Scene";
            
            try
            {
                if (loadingService != null)
                {
                    loadingService.ShowProgress(title, $"Loading {sceneKey}...", 0f);
                }

                await addressableService.LoadSceneAsync(sceneKey);
                
                if (loadingService != null)
                {
                    loadingService.UpdateProgress("Scene loaded successfully!", 1f);
                }
            }
            catch (Exception ex)
            {
                if (loadingService != null)
                {
                    loadingService.UpdateProgress($"Failed to load scene: {ex.Message}", 1f);
                }
                
                Debug.LogError($"[AddressableServiceExtensions] Failed to load scene {sceneKey}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Batch load multiple assets with progress
        /// Пакетная загрузка нескольких ресурсов с прогрессом
        /// </summary>
        public static async UniTask<Dictionary<string, T>> LoadMultipleAssetsWithProgressAsync<T>(
            this IAddressableService addressableService,
            IEnumerable<string> keys,
            ILoadingService loadingService,
            string loadingTitle = null) where T : UnityEngine.Object
        {
            if (addressableService == null) return new Dictionary<string, T>();
            
            var keyArray = keys?.ToArray();
            if (keyArray == null || keyArray.Length == 0) return new Dictionary<string, T>();
            
            var title = loadingTitle ?? $"Loading {keyArray.Length} {typeof(T).Name}s";
            var results = new Dictionary<string, T>();
            
            try
            {
                if (loadingService != null)
                {
                    loadingService.ShowProgress(title, "Preparing to load assets...", 0f);
                }
                
                for (int i = 0; i < keyArray.Length; i++)
                {
                    var key = keyArray[i];
                    var progress = (float)i / keyArray.Length;
                    
                    if (loadingService != null)
                    {
                        loadingService.UpdateProgress($"Loading {key}... ({i + 1}/{keyArray.Length})", progress);
                    }
                    
                    try
                    {
                        var asset = await addressableService.LoadAssetAsync<T>(key);
                        if (asset != null)
                        {
                            results[key] = asset;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[AddressableServiceExtensions] Failed to load {key}: {ex.Message}");
                        // Continue loading other assets
                    }
                }
                
                if (loadingService != null)
                {
                    loadingService.UpdateProgress($"Loaded {results.Count}/{keyArray.Length} assets", 1f);
                }
                
                return results;
            }
            catch (Exception ex)
            {
                if (loadingService != null)
                {
                    loadingService.UpdateProgress($"Batch load failed: {ex.Message}", 1f);
                }
                
                Debug.LogError($"[AddressableServiceExtensions] Batch load failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Preload assets by category with progress
        /// Предварительная загрузка ресурсов по категории с прогрессом
        /// </summary>
        public static async UniTask PreloadAssetsByCategoryAsync(
            this IAddressableService addressableService,
            string category,
            ILoadingService loadingService,
            string loadingTitle = null)
        {
            if (addressableService == null || string.IsNullOrEmpty(category)) return;
            
            var title = loadingTitle ?? $"Preloading {category}";
            
            try
            {
                if (loadingService != null)
                {
                    loadingService.ShowProgress(title, $"Checking {category} content size...", 0f);
                }
                
                // Get download size for category
                var downloadSize = await addressableService.GetDownloadSizeAsync(category);
                
                if (downloadSize > 0)
                {
                    // Download dependencies first
                    await addressableService.DownloadDependenciesWithProgressAsync(
                        new[] { category }, loadingService, title);
                }
                else
                {
                    if (loadingService != null)
                    {
                        loadingService.UpdateProgress($"{category} content already available", 1f);
                    }
                }
            }
            catch (Exception ex)
            {
                if (loadingService != null)
                {
                    loadingService.UpdateProgress($"Preload failed: {ex.Message}", 1f);
                }
                
                Debug.LogError($"[AddressableServiceExtensions] Preload category {category} failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Format bytes to human readable string
        /// Форматировать байты в читаемую строку
        /// </summary>
        private static string FormatBytes(long bytes)
        {
            const long KB = 1024;
            const long MB = KB * 1024;
            const long GB = MB * 1024;

            if (bytes >= GB)
                return $"{bytes / (float)GB:F2} GB";
            if (bytes >= MB)
                return $"{bytes / (float)MB:F2} MB";
            if (bytes >= KB)
                return $"{bytes / (float)KB:F2} KB";
            
            return $"{bytes} B";
        }
    }
}
