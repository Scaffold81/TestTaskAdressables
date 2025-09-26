using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using R3;

namespace Project.Core.Services.Loading
{
    /// <summary>
    /// Implementation of loading service with UI integration
    /// Реализация сервиса загрузки с интеграцией UI
    /// </summary>
    public class LoadingService : ILoadingService, IDisposable
    {
        private readonly ReactiveProperty<bool> _isLoading = new ReactiveProperty<bool>(false);
        private readonly ReactiveProperty<float> _loadingProgress = new ReactiveProperty<float>(0f);
        private readonly ReactiveProperty<string> _loadingTitle = new ReactiveProperty<string>("");
        private readonly ReactiveProperty<string> _loadingStatus = new ReactiveProperty<string>("");
        
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        /// <summary>
        /// Observable for loading state (true/false) / Observable для состояния загрузки (true/false)
        /// </summary>
        public ReadOnlyReactiveProperty<bool> IsLoading => _isLoading.ToReadOnlyReactiveProperty();
        
        /// <summary>
        /// Observable for progress updates (0.0-1.0) / Observable для обновлений прогресса (0.0-1.0)
        /// </summary>
        public ReadOnlyReactiveProperty<float> LoadingProgress => _loadingProgress.ToReadOnlyReactiveProperty();
        
        /// <summary>
        /// Observable for loading title / Observable для заголовка загрузки
        /// </summary>
        public ReadOnlyReactiveProperty<string> LoadingTitle => _loadingTitle.ToReadOnlyReactiveProperty();
        
        /// <summary>
        /// Observable for loading status / Observable для статуса загрузки
        /// </summary>
        public ReadOnlyReactiveProperty<string> LoadingStatus => _loadingStatus.ToReadOnlyReactiveProperty();

        /// <summary>
        /// Constructor / Конструктор
        /// </summary>
        public LoadingService()
        {
            // Setup logging / Настроить логирование
            _isLoading.Subscribe(isLoading => 
            {
                Debug.Log($"[LoadingService] Loading state changed: {isLoading}");
            }).AddTo(_disposables);
            
            _loadingProgress.Subscribe(progress => 
            {
                Debug.Log($"[LoadingService] Progress: {progress:P1}");
            }).AddTo(_disposables);
        }

        /// <summary>
        /// Show loading screen with title and initial progress / Показать экран загрузки с заголовком и начальным прогрессом
        /// </summary>
        public void ShowProgress(string title, string status = "", float progress = 0f)
        {
            _loadingTitle.Value = title ?? "";
            _loadingStatus.Value = status ?? "";
            _loadingProgress.Value = Mathf.Clamp01(progress);
            _isLoading.Value = true;
            
            Debug.Log($"[LoadingService] Started loading: {title} ({progress:P1})");
        }

        /// <summary>
        /// Update loading progress / Обновить прогресс загрузки
        /// </summary>
        public void UpdateProgress(string status, float progress)
        {
            _loadingStatus.Value = status ?? "";
            _loadingProgress.Value = Mathf.Clamp01(progress);
            
            Debug.Log($"[LoadingService] Progress updated: {status} ({progress:P1})");
        }

        /// <summary>
        /// Update loading status without changing progress / Обновить статус загрузки без изменения прогресса
        /// </summary>
        public void UpdateStatus(string status)
        {
            _loadingStatus.Value = status ?? "";
            
            Debug.Log($"[LoadingService] Status updated: {status}");
        }

        /// <summary>
        /// Hide loading screen / Скрыть экран загрузки
        /// </summary>
        public void HideProgress()
        {
            _isLoading.Value = false;
            _loadingProgress.Value = 1f;
            _loadingStatus.Value = "Completed";
            
            Debug.Log("[LoadingService] Loading completed and hidden");
        }

        /// <summary>
        /// Show progress for async operation / Показать прогресс для асинхронной операции
        /// </summary>
        public async UniTask ShowProgressAsync(UniTask task, string title, string status = "")
        {
            ShowProgress(title, status, 0f);
            
            try
            {
                await task;
                UpdateProgress("Completed", 1f);
            }
            catch (Exception ex)
            {
                UpdateProgress($"Failed: {ex.Message}", 1f);
                Debug.LogError($"[LoadingService] Task failed during loading: {ex.Message}");
                throw;
            }
            finally
            {
                // Small delay to show completion
                await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
                HideProgress();
            }
        }

        /// <summary>
        /// Show progress for async operation with result / Показать прогресс для асинхронной операции с результатом
        /// </summary>
        public async UniTask<T> ShowProgressAsync<T>(UniTask<T> task, string title, string status = "")
        {
            ShowProgress(title, status, 0f);
            
            try
            {
                var result = await task;
                UpdateProgress("Completed", 1f);
                return result;
            }
            catch (Exception ex)
            {
                UpdateProgress($"Failed: {ex.Message}", 1f);
                Debug.LogError($"[LoadingService] Task failed during loading: {ex.Message}");
                throw;
            }
            finally
            {
                // Small delay to show completion
                await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
                HideProgress();
            }
        }

        /// <summary>
        /// Dispose resources / Освободить ресурсы
        /// </summary>
        public void Dispose()
        {
            _disposables?.Dispose();
            _isLoading?.Dispose();
            _loadingProgress?.Dispose();
            _loadingTitle?.Dispose();
            _loadingStatus?.Dispose();
        }
    }
}
