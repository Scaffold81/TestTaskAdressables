using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Project.Core.Services.Addressable
{
    /// <summary>
    /// Extension methods for IAddressableService
    /// Методы расширения для IAddressableService
    /// </summary>
    public static class AddressableServiceExtensions
    {
        /// <summary>
        /// Load multiple assets by keys
        /// Загрузить несколько ресурсов по ключам
        /// </summary>
        public static async UniTask<T[]> LoadAssetsAsync<T>(this IAddressableService service, params string[] keys) 
            where T : UnityEngine.Object
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));
            
            if (keys == null || keys.Length == 0)
                return Array.Empty<T>();
            
            var results = new List<T>();
            
            foreach (var key in keys)
            {
                try
                {
                    var asset = await service.LoadAssetAsync<T>(key);
                    results.Add(asset);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[AddressableServiceExtensions] Failed to load {key}: {ex.Message}");
                }
            }
            
            return results.ToArray();
        }
        
        /// <summary>
        /// Load asset with timeout
        /// Загрузить ресурс с таймаутом
        /// </summary>
        public static async UniTask<T> LoadAssetWithTimeoutAsync<T>(
            this IAddressableService service, 
            string key, 
            TimeSpan timeout) where T : UnityEngine.Object
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));
            
            var loadTask = service.LoadAssetAsync<T>(key);
            var timeoutTask = UniTask.Delay(timeout);
            
            var (hasResultLeft, result) = await UniTask.WhenAny(loadTask, timeoutTask);
            
            if (hasResultLeft)
            {
                return result;
            }
            else
            {
                throw new TimeoutException($"Loading {key} timed out after {timeout.TotalSeconds}s");
            }
        }
        
        /// <summary>
        /// Preload assets for specific group
        /// Предзагрузить ресурсы для конкретной группы
        /// </summary>
        public static async UniTask PreloadGroupAsync(
            this IAddressableService service, 
            string[] keys, 
            IProgress<float> progress = null)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));
            
            if (keys == null || keys.Length == 0)
                return;
            
            // Download dependencies first / Сначала загрузить зависимости
            await service.DownloadDependenciesAsync(keys, progress);
            
            Debug.Log($"[AddressableServiceExtensions] Preloaded {keys.Length} assets");
        }
        
        /// <summary>
        /// Check if asset is loaded
        /// Проверить, загружен ли ресурс
        /// </summary>
        public static bool IsAssetLoaded(this IAddressableService service, string key)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));
            
            return service.LoadedAssets.ContainsKey(key);
        }
        
        /// <summary>
        /// Get loaded asset without loading
        /// Получить загруженный ресурс без загрузки
        /// </summary>
        public static T GetLoadedAsset<T>(this IAddressableService service, string key) where T : UnityEngine.Object
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));
            
            if (service.LoadedAssets.TryGetValue(key, out var asset))
            {
                return asset as T;
            }
            
            return null;
        }
        
        /// <summary>
        /// Release multiple assets
        /// Освободить несколько ресурсов
        /// </summary>
        public static void ReleaseAssets(this IAddressableService service, params string[] keys)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));
            
            if (keys == null || keys.Length == 0)
                return;
            
            foreach (var key in keys)
            {
                service.ReleaseAsset(key);
            }
        }
        
        /// <summary>
        /// Get total download size for multiple keys
        /// Получить общий размер загрузки для нескольких ключей
        /// </summary>
        public static async UniTask<long> GetTotalDownloadSizeAsync(
            this IAddressableService service, 
            params string[] keys)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));
            
            return await service.GetDownloadSizeAsync(keys);
        }
        
        /// <summary>
        /// Get formatted download size string
        /// Получить отформатированную строку размера загрузки
        /// </summary>
        public static async UniTask<string> GetFormattedDownloadSizeAsync(
            this IAddressableService service, 
            params string[] keys)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));
            
            var sizeBytes = await service.GetDownloadSizeAsync(keys);
            return FormatBytes(sizeBytes);
        }
        
        /// <summary>
        /// Load asset or get cached
        /// Загрузить ресурс или получить из кеша
        /// </summary>
        public static async UniTask<T> LoadOrGetCachedAsync<T>(
            this IAddressableService service, 
            string key) where T : UnityEngine.Object
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));
            
            // Check if already loaded / Проверить, загружен ли уже
            var cached = service.GetLoadedAsset<T>(key);
            if (cached != null)
            {
                Debug.Log($"[AddressableServiceExtensions] Using cached asset: {key}");
                return cached;
            }
            
            // Load new / Загрузить новый
            return await service.LoadAssetAsync<T>(key);
        }
        
        /// <summary>
        /// Format bytes to human readable string
        /// Форматировать байты в читаемую строку
        /// </summary>
        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024)
                return $"{bytes} B";
            if (bytes < 1024 * 1024)
                return $"{bytes / 1024f:F1} KB";
            if (bytes < 1024 * 1024 * 1024)
                return $"{bytes / (1024f * 1024f):F1} MB";
            return $"{bytes / (1024f * 1024f * 1024f):F2} GB";
        }
        
        /// <summary>
        /// Wait until service is initialized
        /// Ждать пока сервис инициализируется
        /// </summary>
        public static async UniTask WaitForInitializationAsync(
            this IAddressableService service, 
            int maxWaitTimeSeconds = 30)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));
            
            var startTime = Time.time;
            
            while (!service.IsInitialized)
            {
                if (Time.time - startTime > maxWaitTimeSeconds)
                {
                    throw new TimeoutException($"Service initialization timeout after {maxWaitTimeSeconds}s");
                }
                
                await UniTask.Yield();
            }
        }
    }
}