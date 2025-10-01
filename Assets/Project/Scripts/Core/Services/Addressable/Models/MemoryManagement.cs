using System;
using UnityEngine;

namespace Project.Core.Services.Addressable.Models
{
    /// <summary>
    /// Strategy for memory cleanup / Стратегия очистки памяти
    /// </summary>
    public enum CleanupStrategy
    {
        /// <summary>
        /// Aggressive cleanup - release all unused immediately
        /// Агрессивная очистка - освобождать все неиспользуемое немедленно
        /// </summary>
        Aggressive,
        
        /// <summary>
        /// Balanced cleanup - release after threshold
        /// Балансированная очистка - освобождать после превышения порога
        /// </summary>
        Balanced,
        
        /// <summary>
        /// Conservative cleanup - keep cache as long as possible
        /// Консервативная очистка - сохранять кеш как можно дольше
        /// </summary>
        Conservative
    }
    
    /// <summary>
    /// Memory management settings for Addressables
    /// Настройки управления памятью для Addressables
    /// </summary>
    [Serializable]
    public class MemoryManagement
    {
        /// <summary>
        /// Enable automatic memory cleanup / Включить автоматическую очистку памяти
        /// </summary>
        public bool EnableAutoCleanup = true;
        
        /// <summary>
        /// Memory threshold in MB for cleanup trigger / Порог памяти в МБ для запуска очистки
        /// </summary>
        public int MemoryThresholdMB = 400;
        
        /// <summary>
        /// Cleanup strategy / Стратегия очистки
        /// </summary>
        public CleanupStrategy Strategy = CleanupStrategy.Balanced;
        
        /// <summary>
        /// Release assets on scene change / Освобождать ресурсы при смене сцены
        /// </summary>
        public bool ReleaseOnSceneChange = true;
        
        /// <summary>
        /// Unload unused assets after cleanup / Выгружать неиспользуемые ресурсы после очистки
        /// </summary>
        public bool UnloadUnusedAssets = true;
        
        /// <summary>
        /// Run garbage collection after cleanup / Запускать сборку мусора после очистки
        /// </summary>
        public bool ForceGCAfterCleanup = false;
        
        /// <summary>
        /// Time between automatic cleanups in seconds / Время между автоматическими очистками в секундах
        /// </summary>
        public int AutoCleanupIntervalSeconds = 60;
        
        /// <summary>
        /// Maximum cache age in hours / Максимальный возраст кеша в часах
        /// </summary>
        public int MaxCacheAgeHours = 24;
        
        /// <summary>
        /// Track memory allocation per asset / Отслеживать выделение памяти для каждого ресурса
        /// </summary>
        public bool TrackMemoryPerAsset = true;
        
        /// <summary>
        /// Get memory threshold in bytes / Получить порог памяти в байтах
        /// </summary>
        public long GetMemoryThresholdBytes()
        {
            return (long)MemoryThresholdMB * 1024 * 1024;
        }
        
        /// <summary>
        /// Get cleanup interval as TimeSpan / Получить интервал очистки как TimeSpan
        /// </summary>
        public TimeSpan GetCleanupInterval()
        {
            return TimeSpan.FromSeconds(AutoCleanupIntervalSeconds);
        }
        
        /// <summary>
        /// Get max cache age as TimeSpan / Получить максимальный возраст кеша как TimeSpan
        /// </summary>
        public TimeSpan GetMaxCacheAge()
        {
            return TimeSpan.FromHours(MaxCacheAgeHours);
        }
        
        /// <summary>
        /// Check if cleanup is needed based on current memory / Проверить, нужна ли очистка на основе текущей памяти
        /// </summary>
        public bool ShouldCleanup(long currentMemoryBytes)
        {
            if (!EnableAutoCleanup)
                return false;
                
            return currentMemoryBytes > GetMemoryThresholdBytes();
        }
        
        /// <summary>
        /// Get cleanup priority multiplier based on strategy / Получить множитель приоритета очистки на основе стратегии
        /// </summary>
        public float GetCleanupPriorityMultiplier()
        {
            return Strategy switch
            {
                CleanupStrategy.Aggressive => 1.5f,
                CleanupStrategy.Balanced => 1.0f,
                CleanupStrategy.Conservative => 0.5f,
                _ => 1.0f
            };
        }
    }
}