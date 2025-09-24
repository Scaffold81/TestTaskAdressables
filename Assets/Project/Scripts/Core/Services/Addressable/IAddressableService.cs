using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using R3;

namespace Project.Core.Services.Addressable
{
    /// <summary>
    /// Main service for managing Addressable assets loading and unloading
    /// Основной сервис для управления загрузкой и выгрузкой Addressable ресурсов
    /// </summary>
    public interface IAddressableService
    {
        /// <summary>
        /// Load asset asynchronously by key / Загрузить ресурс асинхронно по ключу
        /// </summary>
        UniTask<T> LoadAssetAsync<T>(string key) where T : UnityEngine.Object;
        
        /// <summary>
        /// Load scene asynchronously / Загрузить сцену асинхронно
        /// </summary>
        UniTask LoadSceneAsync(string sceneKey);
        
        /// <summary>
        /// Release specific asset / Освободить конкретный ресурс
        /// </summary>
        void ReleaseAsset(string key);
        
        /// <summary>
        /// Release all loaded assets / Освободить все загруженные ресурсы
        /// </summary>
        void ReleaseAllAssets();
        
        /// <summary>
        /// Get download size for assets / Получить размер загрузки для ресурсов
        /// </summary>
        UniTask<long> GetDownloadSizeAsync(params string[] keys);
        
        /// <summary>
        /// Download dependencies with progress tracking / Загрузить зависимости с отслеживанием прогресса
        /// </summary>
        UniTask DownloadDependenciesAsync(IEnumerable<string> keys, IProgress<float> progress = null);
        
        /// <summary>
        /// Observable for progress updates / Observable для обновлений прогресса
        /// </summary>
        Observable<float> OnProgressUpdated { get; }
        
        /// <summary>
        /// Observable for asset loading telemetry / Observable для телеметрии загрузки ресурсов
        /// </summary>
        Observable<(string key, float time, long size)> OnAssetLoaded { get; }
        
        /// <summary>
        /// Check if system is initialized / Проверить, инициализирована ли система
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Get loaded assets dictionary / Получить словарь загруженных ресурсов
        /// </summary>
        Dictionary<string, object> LoadedAssets { get; }
    }
}