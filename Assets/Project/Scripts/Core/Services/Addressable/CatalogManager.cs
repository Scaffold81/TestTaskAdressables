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
        /// Initialize catalog system / Инициализировать систему каталогов
        /// </summary>
        UniTask InitializeAsync();
        
        /// <summary>
        /// Check for content updates / Проверить обновления контента
        /// </summary>
        UniTask<bool> CheckForUpdatesAsync();
        
        /// <summary>
        /// Download content updates / Скачать обновления контента
        /// </summary>
        UniTask DownloadUpdatesAsync(IProgress<float> progress = null);
        
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
                // Initialize main catalog / Инициализировать основной каталог
                await InitializeMainCatalogAsync();
                
                // Initialize separate catalogs / Инициализировать отдельные каталоги
                var settings = _configRepository.GetSettings();
                if (settings.ContentUpdate.LevelsCatalog.IsSeparate)
                {
                    await InitializeSeparateCatalogAsync(settings.ContentUpdate.LevelsCatalog);
                }
                
                // Start automatic update checking if enabled / Запустить автоматическую проверку обновлений
                if (settings.ContentUpdate.EnableAutoUpdates && settings.ContentUpdate.CheckUpdatesOnStartup)
                {
                    _ = StartPeriodicUpdateCheckAsync();
                }
                
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
                var checkHandle = Addressables.CheckForCatalogUpdates(false);
                var catalogsToUpdate = await checkHandle.ToUniTask();
                
                bool hasUpdates = catalogsToUpdate.Count > 0;
                
                if (hasUpdates)
                {
                    Debug.Log($"[CatalogManager] Found {catalogsToUpdate.Count} catalog(s) to update");
                    
                    // Update catalog info / Обновить информацию о каталогах
                    foreach (var catalogId in catalogsToUpdate)
                    {
                        var catalogInfo = _loadedCatalogs.FirstOrDefault(c => c.Name == catalogId);
                        if (catalogInfo != null)
                        {
                            catalogInfo.LastUpdated = DateTime.Now;
                            _catalogUpdatedSubject.OnNext(catalogInfo);
                        }
                    }
                }
                else
                {
                    Debug.Log("[CatalogManager] No catalog updates found");
                }
                
                Addressables.Release(checkHandle);
                return hasUpdates;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CatalogManager] Error checking for updates: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Download content updates / Скачать обновления контента
        /// </summary>
        public async UniTask DownloadUpdatesAsync(IProgress<float> progress = null)
        {
            try
            {
                var updateHandle = Addressables.UpdateCatalogs();
                var catalogs = await updateHandle.ToUniTask();
                
                Debug.Log($"[CatalogManager] Updated {catalogs.Count} catalog(s)");
                
                // Update loaded catalogs info / Обновить информацию о загруженных каталогах
                foreach (var catalog in catalogs)
                {
                    var existingCatalog = _loadedCatalogs.FirstOrDefault(c => c.Name == "main");
                    if (existingCatalog != null)
                    {
                        existingCatalog.LastUpdated = DateTime.Now;
                        existingCatalog.Version = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                    }
                }
                
                progress?.Report(1f);
                Addressables.Release(updateHandle);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CatalogManager] Error downloading updates: {ex.Message}");
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
        
        /// <summary>
        /// Initialize main catalog / Инициализировать основной каталог
        /// </summary>
        private async UniTask InitializeMainCatalogAsync()
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
        }
        
        /// <summary>
        /// Initialize separate catalog / Инициализировать отдельный каталог
        /// </summary>
        private async UniTask InitializeSeparateCatalogAsync(CatalogConfig catalogConfig)
        {
            await UniTask.Yield();
            
            var separateCatalog = new CatalogInfo(
                catalogConfig.FileName,
                "1.0.0",
                $"{catalogConfig.FileName}.json",
                true,
                new[] { "Levels_Remote" }
            );
            
            _loadedCatalogs.Add(separateCatalog);
        }
        
        /// <summary>
        /// Start periodic update checking / Запустить периодическую проверку обновлений
        /// </summary>
        private async UniTask StartPeriodicUpdateCheckAsync()
        {
            var settings = _configRepository.GetSettings();
            var intervalMs = settings.ContentUpdate.UpdateCheckIntervalMinutes * 60 * 1000;
            
            while (Application.isPlaying)
            {
                await UniTask.Delay(intervalMs);
                
                // Re-get settings in case they changed
                var currentSettings = _configRepository.GetSettings();
                if (currentSettings.ContentUpdate.EnableAutoUpdates)
                {
                    await CheckForUpdatesAsync();
                }
            }
        }
    }
}