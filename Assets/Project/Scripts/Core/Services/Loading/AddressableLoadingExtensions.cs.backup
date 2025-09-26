using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Project.Core.Services.Addressable;
using Project.Core.Services.Loading;
using R3;

namespace Project.Core.Services.Extensions
{
    /// <summary>
    /// Extension methods for AddressableService integration with LoadingService
    /// Методы расширения для интеграции AddressableService с LoadingService
    /// </summary>
    public static class AddressableLoadingExtensions
    {
        /// <summary>
        /// Load asset with loading progress UI / Загрузить ресурс с UI прогресса
        /// </summary>
        public static async UniTask<T> LoadAssetWithProgressAsync<T>(
            this IAddressableService addressableService,
            string key,
            ILoadingService loadingService,
            string loadingTitle = "Loading Asset") where T : UnityEngine.Object
        {
            if (loadingService == null)
            {
                return await addressableService.LoadAssetAsync<T>(key);
            }

            var task = addressableService.LoadAssetAsync<T>(key);
            return await loadingService.ShowProgressAsync(task, loadingTitle, $"Loading {key}...");
        }

        /// <summary>
        /// Load scene with loading progress UI / Загрузить сцену с UI прогресса
        /// </summary>
        public static async UniTask LoadSceneWithProgressAsync(
            this IAddressableService addressableService,
            string sceneKey,
            ILoadingService loadingService,
            string loadingTitle = "Loading Scene")
        {
            if (loadingService == null)
            {
                await addressableService.LoadSceneAsync(sceneKey);
                return;
            }

            loadingService.ShowProgress(loadingTitle, $"Loading {sceneKey}...");

            try
            {
                // Subscribe to progress updates from AddressableService
                using var progressSubscription = addressableService.OnProgressUpdated
                    .Subscribe(progress =>
                    {
                        loadingService.UpdateProgress(progress, $"Loading {sceneKey}... {(progress * 100):F0}%");
                    });

                await addressableService.LoadSceneAsync(sceneKey);
            }
            finally
            {
                loadingService.HideProgress();
            }
        }

        /// <summary>
        /// Download dependencies with progress UI / Скачать зависимости с UI прогресса
        /// </summary>
        public static async UniTask DownloadDependenciesWithProgressAsync(
            this IAddressableService addressableService,
            string[] keys,
            ILoadingService loadingService,
            string loadingTitle = "Downloading Content")
        {
            if (loadingService == null)
            {
                await addressableService.DownloadDependenciesAsync(keys);
                return;
            }

            // Get download size first
            var totalSize = await addressableService.GetDownloadSizeAsync(keys);
            var sizeText = totalSize > 0 ? $" ({totalSize / (1024f * 1024f):F1} MB)" : "";

            loadingService.ShowProgress(loadingTitle, $"Preparing download{sizeText}...");

            try
            {
                var progressReporter = new Progress<float>(progress =>
                {
                    var statusText = $"Downloading{sizeText}... {(progress * 100):F0}%";
                    loadingService.UpdateProgress(progress, statusText);
                });

                await addressableService.DownloadDependenciesAsync(keys, progressReporter);
            }
            finally
            {
                loadingService.HideProgress();
            }
        }
    }
}