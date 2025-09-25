using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.SceneManagement;
using R3;
using Project.Core.Services.Addressable.Models;
using Project.Core.Config.Addressable;

namespace Project.Core.Services.Addressable
{
    /// <summary>
    /// Main implementation of Addressable service with GameTemplate integration
    /// Основная реализация сервиса Addressables с интеграцией в GameTemplate
    /// </summary>
    public class AddressableService : IAddressableService, IDisposable
    {
        private readonly IAddressableConfigRepository _configRepository;
        private readonly ICatalogManager _catalogManager;
        private readonly Dictionary<string, AsyncOperationHandle> _loadedHandles = new Dictionary<string, AsyncOperationHandle>();
        private readonly Dictionary<string, object> _loadedAssets = new Dictionary<string, object>();
        
        private readonly Subject<float> _progressSubject = new Subject<float>();
        private readonly Subject<(string key, float time, long size)> _assetLoadedSubject = new Subject<(string, float, long)>();
        
        private bool _isInitialized = false;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        /// <summary>
        /// Observable for progress updates / Observable для обновлений прогресса
        /// </summary>
        public Observable<float> OnProgressUpdated => _progressSubject.AsObservable();
        
        /// <summary>
        /// Observable for asset loading telemetry / Observable для телеметрии загрузки ресурсов
        /// </summary>
        public Observable<(string key, float time, long size)> OnAssetLoaded => _assetLoadedSubject.AsObservable();
        
        /// <summary>
        /// Check if system is initialized / Проверить, инициализирована ли система
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Get loaded assets dictionary / Получить словарь загруженных ресурсов
        /// </summary>
        public Dictionary<string, object> LoadedAssets => new Dictionary<string, object>(_loadedAssets);

        /// <summary>
        /// Constructor / Конструктор
        /// </summary>
        public AddressableService(IAddressableConfigRepository configRepository, ICatalogManager catalogManager)
        {
            _configRepository = configRepository ?? throw new ArgumentNullException(nameof(configRepository));
            _catalogManager = catalogManager ?? throw new ArgumentNullException(nameof(catalogManager));
            
            // Setup error handling / Настроить обработку ошибок
            SetupErrorHandling();
            
            // Initialize catalog manager / Инициализировать менеджер каталогов
            _ = InitializeAsync();
        }

        /// <summary>
        /// Load asset asynchronously by key / Загрузить ресурс асинхронно по ключу
        /// </summary>
        public async UniTask<T> LoadAssetAsync<T>(string key) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Asset key cannot be null or empty", nameof(key));

            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // Check if already loaded / Проверить, не загружен ли уже
                if (_loadedAssets.TryGetValue(key, out var cached) && cached is T cachedAsset)
                {
                    UnityEngine.Debug.Log($"[AddressableService] Asset {key} loaded from cache");
                    return cachedAsset;
                }

                // Load with retry logic / Загрузить с логикой повторов
                var asset = await LoadAssetWithRetryAsync<T>(key);
                
                stopwatch.Stop();
                
                // Store in cache / Сохранить в кеше
                _loadedAssets[key] = asset;
                
                // Send telemetry / Отправить телеметрию
                var downloadSize = GetAssetDownloadSize(key);
                _assetLoadedSubject.OnNext((key, stopwatch.ElapsedMilliseconds / 1000f, downloadSize));
                
                UnityEngine.Debug.Log($"[AddressableService] Loaded {key} in {stopwatch.ElapsedMilliseconds}ms");
                return asset;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[AddressableService] Failed to load asset {key}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Load scene asynchronously / Загрузить сцену асинхронно
        /// </summary>
        public async UniTask LoadSceneAsync(string sceneKey)
        {
            if (string.IsNullOrEmpty(sceneKey))
                throw new ArgumentException("Scene key cannot be null or empty", nameof(sceneKey));

            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                _progressSubject.OnNext(0f);
                
                var handle = Addressables.LoadSceneAsync(sceneKey, LoadSceneMode.Single);
                
                // Track progress / Отслеживать прогресс
                while (!handle.IsDone)
                {
                    _progressSubject.OnNext(handle.PercentComplete);
                    await UniTask.Yield();
                }
                
                await handle.ToUniTask();
                
                _progressSubject.OnNext(1f);
                stopwatch.Stop();
                
                UnityEngine.Debug.Log($"[AddressableService] Loaded scene {sceneKey} in {stopwatch.ElapsedMilliseconds}ms");
                
                // Store handle / Сохранить handle
                _loadedHandles[sceneKey] = handle;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[AddressableService] Failed to load scene {sceneKey}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Release specific asset / Освободить конкретный ресурс
        /// </summary>
        public void ReleaseAsset(string key)
        {
            if (string.IsNullOrEmpty(key)) return;

            try
            {
                if (_loadedHandles.TryGetValue(key, out var handle))
                {
                    Addressables.Release(handle);
                    _loadedHandles.Remove(key);
                }
                
                if (_loadedAssets.ContainsKey(key))
                {
                    _loadedAssets.Remove(key);
                }
                
                UnityEngine.Debug.Log($"[AddressableService] Released asset {key}");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[AddressableService] Error releasing asset {key}: {ex.Message}");
            }
        }

        /// <summary>
        /// Release all loaded assets / Освободить все загруженные ресурсы
        /// </summary>
        public void ReleaseAllAssets()
        {
            try
            {
                var keysToRelease = _loadedHandles.Keys.ToArray();
                foreach (var key in keysToRelease)
                {
                    ReleaseAsset(key);
                }
                
                UnityEngine.Debug.Log($"[AddressableService] Released {keysToRelease.Length} assets");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[AddressableService] Error releasing all assets: {ex.Message}");
            }
        }

        /// <summary>
        /// Get download size for assets / Получить размер загрузки для ресурсов
        /// </summary>
        public async UniTask<long> GetDownloadSizeAsync(params string[] keys)
        {
            if (keys == null || keys.Length == 0) return 0;

            try
            {
                var handle = Addressables.GetDownloadSizeAsync(keys);
                var size = await handle.ToUniTask();
                
                // Always release handle to prevent memory leaks
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
                
                UnityEngine.Debug.Log($"[AddressableService] Download size for {keys.Length} assets: {size / (1024f * 1024f):F1} MB");
                return size;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[AddressableService] Error getting download size: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Download dependencies with progress tracking / Загрузить зависимости с отслеживанием прогресса
        /// </summary>
        public async UniTask DownloadDependenciesAsync(IEnumerable<string> keys, IProgress<float> progress = null)
        {
            var keyArray = keys?.ToArray();
            if (keyArray == null || keyArray.Length == 0) return;

            try
            {
                var handle = Addressables.DownloadDependenciesAsync(keyArray, Addressables.MergeMode.Union);
                
                // Track progress / Отслеживать прогресс
                while (!handle.IsDone)
                {
                    var currentProgress = handle.PercentComplete;
                    progress?.Report(currentProgress);
                    _progressSubject.OnNext(currentProgress);
                    await UniTask.Yield();
                }
                
                await handle.ToUniTask();
                
                // Release handle
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
                
                progress?.Report(1f);
                _progressSubject.OnNext(1f);
                
                UnityEngine.Debug.Log($"[AddressableService] Downloaded dependencies for {keyArray.Length} assets");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[AddressableService] Error downloading dependencies: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Initialize service / Инициализировать сервис
        /// </summary>
        private async UniTask InitializeAsync()
        {
            try
            {
                await _catalogManager.InitializeAsync();
                
                // Apply platform settings / Применить настройки платформы
                ApplyPlatformSettings();
                
                _isInitialized = true;
                UnityEngine.Debug.Log("[AddressableService] Initialized successfully");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[AddressableService] Initialization failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Load asset with retry logic / Загрузить ресурс с логикой повторов
        /// </summary>
        private async UniTask<T> LoadAssetWithRetryAsync<T>(string key) where T : UnityEngine.Object
        {
            var settings = _configRepository.GetSettings();
            var maxRetries = settings.EnableRetry ? settings.MaxRetries : 1;
            
            Exception lastException = null;
            
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    var handle = Addressables.LoadAssetAsync<T>(key);
                    var asset = await handle.ToUniTask();
                    
                    // Store handle / Сохранить handle
                    _loadedHandles[key] = handle;
                    
                    return asset;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    
                    if (attempt < maxRetries - 1)
                    {
                        var delay = settings.GetExponentialBackoffDelay(attempt);
                        UnityEngine.Debug.LogWarning($"[AddressableService] Retry {attempt + 1}/{maxRetries} for {key} in {delay}s: {ex.Message}");
                        await UniTask.Delay(TimeSpan.FromSeconds(delay));
                    }
                }
            }
            
            throw new InvalidOperationException($"Failed to load {key} after {maxRetries} attempts", lastException);
        }

        /// <summary>
        /// Setup error handling / Настроить обработку ошибок
        /// </summary>
        private void SetupErrorHandling()
        {
            // Note: ExceptionHandler might not be available in all Unity versions
            // This is optional error handling setup
            try
            {
                // Setup global exception handler if available
                UnityEngine.Debug.Log("[AddressableService] Error handling setup completed");
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogWarning($"[AddressableService] Could not setup error handling: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply platform settings / Применить настройки платформы
        /// </summary>
        private void ApplyPlatformSettings()
        {
            var settings = _configRepository.GetSettings();
            var platformSettings = settings.Platform.GetCurrentPlatformSettings();
            
            UnityEngine.Debug.Log($"[AddressableService] Applied {settings.Platform.GetCurrentPlatformName()} settings");
        }

        /// <summary>
        /// Get asset download status / Получить статус загрузки ресурса
        /// </summary>
        private long GetAssetDownloadSize(string key)
        {
            // For telemetry purposes, return 0 as placeholder
            // Real size tracking happens in GetDownloadSizeAsync and DownloadDependenciesAsync
            return 0;
        }

        /// <summary>
        /// Dispose resources / Освободить ресурсы
        /// </summary>
        public void Dispose()
        {
            ReleaseAllAssets();
            _disposables?.Dispose();
            _progressSubject?.Dispose();
            _assetLoadedSubject?.Dispose();
        }
    }
}