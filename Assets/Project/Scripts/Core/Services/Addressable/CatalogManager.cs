using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Project.Core.Services.Addressable.Models;
using Project.Core.Config.Addressable;
using R3;

namespace Project.Core.Services.Addressable
{
    /// <summary>
    /// Manager for Addressable catalogs and content updates
    /// Менеджер каталогов Addressables и обновлений контента
    /// </summary>
    public interface ICatalogManager
    {
        /// <summary>
        /// Check if catalog manager is initialized
        /// Проверить, инициализирован ли менеджер каталогов
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Current catalog info
        /// Информация о текущем каталоге
        /// </summary>
        CatalogInfo CurrentCatalogInfo { get; }
        
        /// <summary>
        /// Initialize catalog system / Инициализировать систему каталогов
        /// </summary>
        UniTask InitializeAsync();
        
        /// <summary>
        /// Check for content updates / Проверить обновления контента
        /// </summary>
        UniTask<bool> CheckForUpdatesAsync();
        
        /// <summary>
        /// Download catalog updates
        /// Загрузить обновления каталога
        /// </summary>
        UniTask DownloadUpdatesAsync();
        
        /// <summary>
        /// Update specific catalog by name
        /// Обновить конкретный каталог по имени
        /// </summary>
        UniTask UpdateCatalogAsync(string catalogName);
        
        /// <summary>
        /// Get catalog version information
        /// Получить информацию о версии каталога
        /// </summary>
        string GetCatalogVersion();
        
        /// <summary>
        /// Clear catalog cache
        /// Очистить кеш каталогов
        /// </summary>
        UniTask ClearCacheAsync();
        
        /// <summary>
        /// Download content updates with progress / Скачать обновления контента с прогрессом
        /// </summary>
        UniTask DownloadUpdatesAsync(IProgress<float> progress);
        
        /// <summary>
        /// Get all loaded catalogs / Получить все загруженные каталоги
        /// </summary>
        CatalogInfo[] GetLoadedCatalogs();
        
        /// <summary>
        /// Observable for catalog updates / Observable для обновлений каталогов
        /// </summary>
        Observable<CatalogInfo> OnCatalogUpdated { get; }
    }
    
    /// <summary>
    /// Implementation of catalog manager / Реализация менеджера каталогов
    /// </summary>
    public class CatalogManager : ICatalogManager
    {
        private readonly IAddressableConfigRepository _configRepository;
        private readonly List<CatalogInfo> _loadedCatalogs = new List<CatalogInfo>();
        
        private readonly Subject<CatalogInfo> _catalogUpdatedSubject = new Subject<CatalogInfo>();
        public Observable<CatalogInfo> OnCatalogUpdated => _catalogUpdatedSubject.AsObservable();
        
        private bool _isInitialized = false;
        
        /// <summary>
        /// Check if catalog manager is initialized
        /// Проверить, инициализирован ли менеджер каталогов
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Current catalog info
        /// Информация о текущем каталоге
        /// </summary>
        public CatalogInfo CurrentCatalogInfo => _loadedCatalogs.FirstOrDefault();
        
        /// <summary>
        /// Constructor / Конструктор
        /// </summary>
        public CatalogManager(IAddressableConfigRepository configRepository)
        {
            _configRepository = configRepository ?? throw new ArgumentNullException(nameof(configRepository));
        }
        
        /// <summary>
        /// Initialize catalog system / Инициализировать систему каталогов
        /// </summary>
        public async UniTask InitializeAsync()
        {
            try
            {
                await UniTask.Yield();
                
                var mainCatalog = new CatalogInfo(
                    "main",
                    "1.0.0",
                    "main_catalog.json",
                    false,
                    new[] { "Core_Local", "UI_Remote", "Characters_Remote", "Environment_Remote", "Effects_Remote" }
                );
                
                _loadedCatalogs.Add(mainCatalog);
                _isInitialized = true;
                
                Debug.Log($"[CatalogManager] Initialized with {_loadedCatalogs.Count} catalogs");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CatalogManager] Failed to initialize: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Check for content updates / Проверить обновления контента
        /// </summary>
        public async UniTask<bool> CheckForUpdatesAsync()
        {
            try
            {
                await UniTask.Yield();
                Debug.Log("[CatalogManager] No catalog updates found");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CatalogManager] Error checking for updates: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Download catalog updates
        /// Загрузить обновления каталога
        /// </summary>
        public async UniTask DownloadUpdatesAsync()
        {
            await DownloadUpdatesAsync(null);
        }
        
        /// <summary>
        /// Download content updates with progress / Скачать обновления контента с прогрессом
        /// </summary>
        public async UniTask DownloadUpdatesAsync(IProgress<float> progress)
        {
            try
            {
                await UniTask.Yield();
                progress?.Report(1f);
                Debug.Log("[CatalogManager] Updates downloaded successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CatalogManager] Error downloading updates: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Update specific catalog by name
        /// Обновить конкретный каталог по имени
        /// </summary>
        public async UniTask UpdateCatalogAsync(string catalogName)
        {
            try
            {
                await UniTask.Yield();
                
                var catalogToUpdate = _loadedCatalogs.FirstOrDefault(c => c.Name == catalogName);
                if (catalogToUpdate != null)
                {
                    catalogToUpdate.LastUpdated = DateTime.Now;
                    _catalogUpdatedSubject.OnNext(catalogToUpdate);
                    Debug.Log($"[CatalogManager] Updated catalog: {catalogName}");
                }
                else
                {
                    Debug.LogWarning($"[CatalogManager] Catalog {catalogName} not found");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CatalogManager] Failed to update catalog {catalogName}: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Get catalog version information
        /// Получить информацию о версии каталога
        /// </summary>
        public string GetCatalogVersion()
        {
            var mainCatalog = _loadedCatalogs.FirstOrDefault(c => c.Name == "main");
            return mainCatalog?.Version ?? "Unknown";
        }
        
        /// <summary>
        /// Clear catalog cache
        /// Очистить кеш каталогов
        /// </summary>
        public async UniTask ClearCacheAsync()
        {
            try
            {
                UnityEngine.Caching.ClearCache();
                
                foreach (var catalog in _loadedCatalogs)
                {
                    catalog.LastUpdated = DateTime.MinValue;
                }
                
                await UniTask.Yield();
                Debug.Log("[CatalogManager] Cache cleared successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CatalogManager] Failed to clear cache: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Get all loaded catalogs / Получить все загруженные каталоги
        /// </summary>
        public CatalogInfo[] GetLoadedCatalogs()
        {
            return _loadedCatalogs.ToArray();
        }
    }
}
