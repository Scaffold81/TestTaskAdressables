using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;

namespace Project.Core.Services.Addressable.Memory
{
    /// <summary>
    /// Service for managing Addressable memory usage and cleanup
    /// Сервис для управления использованием памяти Addressables и очистки
    /// </summary>
    public interface IAddressableMemoryManager
    {
        /// <summary>
        /// Current memory usage in bytes / Текущее использование памяти в байтах
        /// </summary>
        long CurrentMemoryUsage { get; }
        
        /// <summary>
        /// Memory usage limit in bytes / Лимит использования памяти в байтах
        /// </summary>
        long MemoryLimit { get; set; }
        
        /// <summary>
        /// Number of currently loaded assets / Количество загруженных ресурсов
        /// </summary>
        int LoadedAssetsCount { get; }
        
        /// <summary>
        /// Observable for memory usage changes / Observable для изменений использования памяти
        /// </summary>
        Observable<MemoryUsageData> OnMemoryUsageChanged { get; }
        
        /// <summary>
        /// Observable for memory cleanup events / Observable для событий очистки памяти
        /// </summary>
        Observable<MemoryCleanupData> OnMemoryCleanup { get; }
        
        /// <summary>
        /// Register asset in memory tracking / Зарегистрировать ресурс в отслеживании памяти
        /// </summary>
        void RegisterAsset(string key, UnityEngine.Object asset, long estimatedSize = 0);
        
        /// <summary>
        /// Unregister asset from memory tracking / Отменить регистрацию ресурса из отслеживания памяти
        /// </summary>
        void UnregisterAsset(string key);
        
        /// <summary>
        /// Force memory cleanup / Принудительная очистка памяти
        /// </summary>
        UniTask CleanupMemoryAsync();
        
        /// <summary>
        /// Cleanup unused assets / Очистка неиспользуемых ресурсов
        /// </summary>
        UniTask CleanupUnusedAssetsAsync();
        
        /// <summary>
        /// Check if memory usage is above threshold / Проверить, превышено ли пороговое значение памяти
        /// </summary>
        bool IsMemoryUsageHigh(float threshold = 0.8f);
        
        /// <summary>
        /// Get memory statistics / Получить статистику памяти
        /// </summary>
        MemoryStats GetMemoryStats();
        
        /// <summary>
        /// Get assets by usage priority / Получить ресурсы по приоритету использования
        /// </summary>
        IEnumerable<AssetMemoryInfo> GetAssetsByUsagePriority();
        
        /// <summary>
        /// Get current active handle count
        /// Получить текущее количество активных handle'ов
        /// </summary>
        int GetActiveHandleCount();
        
        /// <summary>
        /// Generate detailed memory report
        /// Сгенерировать подробный отчет о памяти
        /// </summary>
        string GenerateMemoryReport();
        
        /// <summary>
        /// Perform automatic cleanup based on settings
        /// Выполнить автоматическую очистку на основе настроек
        /// </summary>
        int PerformAutomaticCleanup();
    }
    
    /// <summary>
    /// Data structure for memory usage information / Структура данных для информации об использовании памяти
    /// </summary>
    public struct MemoryUsageData
    {
        /// <summary>
        /// Current memory usage in bytes / Текущее использование памяти в байтах
        /// </summary>
        public long CurrentUsage;
        
        /// <summary>
        /// Memory limit in bytes / Лимит памяти в байтах
        /// </summary>
        public long Limit;
        
        /// <summary>
        /// Usage percentage (0.0 - 1.0) / Процент использования (0.0 - 1.0)
        /// </summary>
        public float UsagePercent => Limit > 0 ? (float)CurrentUsage / Limit : 0f;
        
        /// <summary>
        /// Number of loaded assets / Количество загруженных ресурсов
        /// </summary>
        public int LoadedAssets;
        
        /// <summary>
        /// Timestamp of the measurement / Временная метка измерения
        /// </summary>
        public DateTime Timestamp;
        
        /// <summary>
        /// Constructor / Конструктор
        /// </summary>
        public MemoryUsageData(long currentUsage, long limit, int loadedAssets)
        {
            CurrentUsage = currentUsage;
            Limit = limit;
            LoadedAssets = loadedAssets;
            Timestamp = DateTime.Now;
        }
        
        /// <summary>
        /// Check if usage is above threshold / Проверить, превышен ли порог использования
        /// </summary>
        public bool IsAboveThreshold(float threshold) => UsagePercent > threshold;
        
        /// <summary>
        /// Get usage as MB string / Получить использование как строку в МБ
        /// </summary>
        public string GetUsageMB() => $"{CurrentUsage / (1024f * 1024f):F1} MB";
        
        /// <summary>
        /// Get limit as MB string / Получить лимит как строку в МБ
        /// </summary>
        public string GetLimitMB() => $"{Limit / (1024f * 1024f):F1} MB";
    }
    
    /// <summary>
    /// Data structure for memory cleanup information / Структура данных для информации об очистке памяти
    /// </summary>
    public struct MemoryCleanupData
    {
        /// <summary>
        /// Memory freed in bytes / Освобождено памяти в байтах
        /// </summary>
        public long MemoryFreed;
        
        /// <summary>
        /// Number of assets cleaned up / Количество очищенных ресурсов
        /// </summary>
        public int AssetsCleared;
        
        /// <summary>
        /// Cleanup duration in milliseconds / Продолжительность очистки в миллисекундах
        /// </summary>
        public long DurationMs;
        
        /// <summary>
        /// Cleanup trigger reason / Причина запуска очистки
        /// </summary>
        public CleanupReason Reason;
        
        /// <summary>
        /// Timestamp of cleanup / Временная метка очистки
        /// </summary>
        public DateTime Timestamp;
        
        /// <summary>
        /// Constructor / Конструктор
        /// </summary>
        public MemoryCleanupData(long memoryFreed, int assetsCleared, long durationMs, CleanupReason reason)
        {
            MemoryFreed = memoryFreed;
            AssetsCleared = assetsCleared;
            DurationMs = durationMs;
            Reason = reason;
            Timestamp = DateTime.Now;
        }
    }
    
    /// <summary>
    /// Reasons for memory cleanup / Причины очистки памяти
    /// </summary>
    public enum CleanupReason
    {
        Manual,           // Ручная очистка
        MemoryThreshold,  // Превышен порог памяти
        AssetLimit,       // Превышен лимит ресурсов
        SceneChange,      // Смена сцены
        LowMemoryWarning  // Предупреждение о нехватке памяти
    }
    
    /// <summary>
    /// Memory statistics / Статистика памяти
    /// </summary>
    public struct MemoryStats
    {
        /// <summary>
        /// Total allocated memory / Общая выделенная память
        /// </summary>
        public long TotalAllocated;
        
        /// <summary>
        /// Used memory by Addressables / Используемая память Addressables
        /// </summary>
        public long AddressableMemory;
        
        /// <summary>
        /// Number of tracked assets / Количество отслеживаемых ресурсов
        /// </summary>
        public int TrackedAssets;
        
        /// <summary>
        /// Average asset size / Средний размер ресурса
        /// </summary>
        public float AverageAssetSize => TrackedAssets > 0 ? (float)AddressableMemory / TrackedAssets : 0f;
        
        /// <summary>
        /// System memory usage / Использование системной памяти
        /// </summary>
        public long SystemMemory;
        
        /// <summary>
        /// GC memory usage / Использование памяти GC
        /// </summary>
        public long GCMemory;
    }
    
    /// <summary>
    /// Asset memory information / Информация о памяти ресурса
    /// </summary>
    public struct AssetMemoryInfo
    {
        /// <summary>
        /// Asset key / Ключ ресурса
        /// </summary>
        public string Key;
        
        /// <summary>
        /// Estimated size in bytes / Оценочный размер в байтах
        /// </summary>
        public long EstimatedSize;
        
        /// <summary>
        /// Last access time / Время последнего доступа
        /// </summary>
        public DateTime LastAccess;
        
        /// <summary>
        /// Access count / Количество обращений
        /// </summary>
        public int AccessCount;
        
        /// <summary>
        /// Priority for cleanup (lower = cleanup first) / Приоритет для очистки (меньше = очистить первым)
        /// </summary>
        public int CleanupPriority => AccessCount + (int)(DateTime.Now - LastAccess).TotalHours;
        
        /// <summary>
        /// Reference to the asset / Ссылка на ресурс
        /// </summary>
        public UnityEngine.Object Asset;
    }
}