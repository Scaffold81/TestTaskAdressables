using System;
using Cysharp.Threading.Tasks;
using R3;

namespace Project.Core.Services.Loading
{
    /// <summary>
    /// Service for managing loading UI and progress tracking
    /// Сервис для управления UI загрузки и отслеживания прогресса
    /// </summary>
    public interface ILoadingService
    {
        /// <summary>
        /// Show loading screen with title / Показать экран загрузки с заголовком
        /// </summary>
        void ShowProgress(string title, string status = "");
        
        /// <summary>
        /// Update loading progress / Обновить прогресс загрузки
        /// </summary>
        void UpdateProgress(float progress, string status = "");
        
        /// <summary>
        /// Hide loading screen / Скрыть экран загрузки
        /// </summary>
        void HideProgress();
        
        /// <summary>
        /// Show progress for async operation / Показать прогресс для асинхронной операции
        /// </summary>
        UniTask ShowProgressAsync(UniTask task, string title, string status = "");
        
        /// <summary>
        /// Show progress for async operation with result / Показать прогресс для асинхронной операции с результатом
        /// </summary>
        UniTask<T> ShowProgressAsync<T>(UniTask<T> task, string title, string status = "");
        
        /// <summary>
        /// Observable for progress updates / Observable для обновлений прогресса
        /// </summary>
        Observable<ProgressData> OnProgressUpdated { get; }
        
        /// <summary>
        /// Observable for loading state changes / Observable для изменений состояния загрузки
        /// </summary>
        Observable<bool> OnLoadingStateChanged { get; }
        
        /// <summary>
        /// Check if loading is currently active / Проверить, активна ли сейчас загрузка
        /// </summary>
        bool IsLoading { get; }
    }
    
    /// <summary>
    /// Data structure for progress information / Структура данных для информации о прогрессе
    /// </summary>
    public struct ProgressData
    {
        /// <summary>
        /// Progress value (0.0 - 1.0) / Значение прогресса (0.0 - 1.0)
        /// </summary>
        public float Progress;
        
        /// <summary>
        /// Loading title / Заголовок загрузки
        /// </summary>
        public string Title;
        
        /// <summary>
        /// Current status message / Текущее сообщение статуса
        /// </summary>
        public string Status;
        
        /// <summary>
        /// Timestamp of the update / Временная метка обновления
        /// </summary>
        public DateTime Timestamp;
        
        /// <summary>
        /// Constructor / Конструктор
        /// </summary>
        public ProgressData(float progress, string title, string status)
        {
            Progress = progress;
            Title = title;
            Status = status;
            Timestamp = DateTime.Now;
        }
        
        /// <summary>
        /// Get progress as percentage string / Получить прогресс как строку процентов
        /// </summary>
        public string GetProgressPercent()
        {
            return $"{(Progress * 100):F0}%";
        }
        
        /// <summary>
        /// String representation / Строковое представление
        /// </summary>
        public override string ToString()
        {
            return $"{Title}: {GetProgressPercent()} - {Status}";
        }
    }
}