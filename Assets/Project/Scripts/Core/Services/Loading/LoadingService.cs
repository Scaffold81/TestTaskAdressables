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
        private readonly Subject<ProgressData> _progressSubject = new Subject<ProgressData>();
        private readonly Subject<bool> _loadingStateSubject = new Subject<bool>();
        
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        
        private bool _isLoading = false;
        private ProgressData _currentProgress;

        /// <summary>
        /// Observable for progress updates / Observable для обновлений прогресса
        /// </summary>
        public Observable<ProgressData> OnProgressUpdated => _progressSubject.AsObservable();
        
        /// <summary>
        /// Observable for loading state changes / Observable для изменений состояния загрузки
        /// </summary>
        public Observable<bool> OnLoadingStateChanged => _loadingStateSubject.AsObservable();
        
        /// <summary>
        /// Check if loading is currently active / Проверить, активна ли сейчас загрузка
        /// </summary>
        public bool IsLoading => _isLoading;

        /// <summary>
        /// Constructor / Конструктор
        /// </summary>
        public LoadingService()
        {
            // Setup automatic disposal / Настроить автоматическое освобождение ресурсов
            OnProgressUpdated.Subscribe(progress => 
            {
                Debug.Log($"[LoadingService] {progress}");
            }).AddTo(_disposables);
        }

        /// <summary>
        /// Show loading screen with title / Показать экран загрузки с заголовком
        /// </summary>
        public void ShowProgress(string title, string status = "")
        {
            if (_isLoading)
            {
                Debug.LogWarning("[LoadingService] Loading is already active, updating current progress");
            }

            SetLoadingState(true);
            UpdateProgressInternal(0f, title, status);
            
            // Show loading UI if available / Показать UI загрузки, если доступен
            ShowLoadingUI();
            
            Debug.Log($"[LoadingService] Started loading: {title}");
        }

        /// <summary>
        /// Update loading progress / Обновить прогресс загрузки
        /// </summary>
        public void UpdateProgress(float progress, string status = "")
        {
            if (!_isLoading)
            {
                Debug.LogWarning("[LoadingService] Trying to update progress when loading is not active");
                return;
            }

            progress = Mathf.Clamp01(progress);
            UpdateProgressInternal(progress, _currentProgress.Title, status);
        }

        /// <summary>
        /// Hide loading screen / Скрыть экран загрузки
        /// </summary>
        public void HideProgress()
        {
            if (!_isLoading)
            {
                Debug.LogWarning("[LoadingService] Trying to hide progress when loading is not active");
                return;
            }

            SetLoadingState(false);
            UpdateProgressInternal(1f, _currentProgress.Title, "Completed");
            
            // Hide loading UI / Скрыть UI загрузки
            HideLoadingUI();
            
            Debug.Log("[LoadingService] Loading completed");
        }

        /// <summary>
        /// Show progress for async operation / Показать прогресс для асинхронной операции
        /// </summary>
        public async UniTask ShowProgressAsync(UniTask task, string title, string status = "")
        {
            ShowProgress(title, status);
            
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LoadingService] Task failed during loading: {ex.Message}");
                throw;
            }
            finally
            {
                HideProgress();
            }
        }

        /// <summary>
        /// Show progress for async operation with result / Показать прогресс для асинхронной операции с результатом
        /// </summary>
        public async UniTask<T> ShowProgressAsync<T>(UniTask<T> task, string title, string status = "")
        {
            ShowProgress(title, status);
            
            try
            {
                var result = await task;
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LoadingService] Task failed during loading: {ex.Message}");
                throw;
            }
            finally
            {
                HideProgress();
            }
        }

        /// <summary>
        /// Update progress internal implementation / Внутренняя реализация обновления прогресса
        /// </summary>
        private void UpdateProgressInternal(float progress, string title, string status)
        {
            _currentProgress = new ProgressData(progress, title, status);
            _progressSubject.OnNext(_currentProgress);
        }

        /// <summary>
        /// Set loading state / Установить состояние загрузки
        /// </summary>
        private void SetLoadingState(bool isLoading)
        {
            if (_isLoading == isLoading) return;
            
            _isLoading = isLoading;
            _loadingStateSubject.OnNext(_isLoading);
        }

        /// <summary>
        /// Show loading UI / Показать UI загрузки
        /// </summary>
        private void ShowLoadingUI()
        {
            try
            {
                // Try to show loading UI through UIPageService
                // This will be implemented when LoadingProgressView is created
                Debug.Log("[LoadingService] Loading UI would be shown here");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LoadingService] Could not show loading UI: {ex.Message}");
            }
        }

        /// <summary>
        /// Hide loading UI / Скрыть UI загрузки
        /// </summary>
        private void HideLoadingUI()
        {
            try
            {
                // Try to hide loading UI through UIPageService
                Debug.Log("[LoadingService] Loading UI would be hidden here");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LoadingService] Could not hide loading UI: {ex.Message}");
            }
        }

        /// <summary>
        /// Dispose resources / Освободить ресурсы
        /// </summary>
        public void Dispose()
        {
            _disposables?.Dispose();
            _progressSubject?.Dispose();
            _loadingStateSubject?.Dispose();
        }
    }
}