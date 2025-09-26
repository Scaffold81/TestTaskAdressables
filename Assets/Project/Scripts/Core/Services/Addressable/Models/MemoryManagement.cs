using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Core.Services.Addressable.Models
{
    /// <summary>
    /// Memory management settings and statistics / Настройки управления памятью и статистика
    /// </summary>
    [Serializable]
    public class MemoryManagementData
    {
        /// <summary>
        /// Memory budget in MB / Бюджет памяти в МБ
        /// </summary>
        public float MemoryBudgetMB = 512f;
        
        /// <summary>
        /// Warning threshold (0.0-1.0) / Порог предупреждения
        /// </summary>
        public float WarningThreshold = 0.8f;
        
        /// <summary>
        /// Critical threshold for automatic cleanup / Критический порог для автоматической очистки
        /// </summary>
        public float CriticalThreshold = 0.9f;
        
        /// <summary>
        /// Enable automatic memory cleanup / Включить автоматическую очистку памяти
        /// </summary>
        public bool EnableAutoCleanup = true;
        
        /// <summary>
        /// Cleanup check interval in seconds / Интервал проверки очистки в секундах
        /// </summary>
        public float CleanupCheckInterval = 30f;
        
        /// <summary>
        /// Assets that should never be unloaded / Ресурсы, которые никогда не должны выгружаться
        /// </summary>
        public List<string> ProtectedAssets = new List<string>();
    }
    
    /// <summary>
    /// Current memory usage statistics / Текущая статистика использования памяти
    /// </summary>
    [Serializable]
    public struct MemoryStats
    {
        /// <summary>
        /// Used memory in bytes / Используемая память в байтах
        /// </summary>
        public long UsedMemoryBytes;
        
        /// <summary>
        /// Total allocated memory in bytes / Общая выделенная память в байтах
        /// </summary>
        public long AllocatedMemoryBytes;
        
        /// <summary>
        /// Number of loaded assets / Количество загруженных ресурсов
        /// </summary>
        public int LoadedAssetCount;
        
        /// <summary>
        /// Number of cached handles / Количество кешированных handles
        /// </summary>
        public int CachedHandleCount;
        
        /// <summary>
        /// Memory usage percentage (0.0-1.0) / Процент использования памяти
        /// </summary>
        public float UsagePercentage;
        
        /// <summary>
        /// Timestamp of measurement / Временная метка измерения
        /// </summary>
        public DateTime Timestamp;
        
        /// <summary>
        /// Get used memory in MB / Получить используемую память в МБ
        /// </summary>
        public float GetUsedMemoryMB() => UsedMemoryBytes / (1024f * 1024f);
        
        /// <summary>
        /// Get allocated memory in MB / Получить выделенную память в МБ
        /// </summary>
        public float GetAllocatedMemoryMB() => AllocatedMemoryBytes / (1024f * 1024f);
        
        /// <summary>
        /// Check if memory usage is critical / Проверить, критично ли использование памяти
        /// </summary>
        public bool IsCritical(float threshold) => UsagePercentage >= threshold;
        
        /// <summary>
        /// String representation / Строковое представление
        /// </summary>
        public override string ToString()
        {
            return $"Memory: {GetUsedMemoryMB():F1}MB ({UsagePercentage * 100:F1}%), Assets: {LoadedAssetCount}, Handles: {CachedHandleCount}";
        }
    }
    
    /// <summary>
    /// Asset usage tracking data / Данные отслеживания использования ресурсов
    /// </summary>
    [Serializable]
    public class AssetUsageData
    {
        /// <summary>
        /// Asset key / Ключ ресурса
        /// </summary>
        public string Key;
        
        /// <summary>
        /// Last access time / Время последнего доступа
        /// </summary>
        public DateTime LastAccessTime;
        
        /// <summary>
        /// Access count / Количество обращений
        /// </summary>
        public int AccessCount;
        
        /// <summary>
        /// Estimated memory size in bytes / Приблизительный размер в памяти в байтах
        /// </summary>
        public long EstimatedSizeBytes;
        
        /// <summary>
        /// Is asset protected from auto-cleanup / Защищен ли ресурс от автоматической очистки
        /// </summary>
        public bool IsProtected;
        
        /// <summary>
        /// Priority for cleanup (lower = cleaned first) / Приоритет для очистки (ниже = очищается первым)
        /// </summary>
        public int CleanupPriority;
        
        /// <summary>
        /// Constructor / Конструктор
        /// </summary>
        public AssetUsageData(string key)
        {
            Key = key;
            LastAccessTime = DateTime.Now;
            AccessCount = 1;
            EstimatedSizeBytes = 0;
            IsProtected = false;
            CleanupPriority = 0;
        }
        
        /// <summary>
        /// Update access information / Обновить информацию о доступе
        /// </summary>
        public void UpdateAccess()
        {
            LastAccessTime = DateTime.Now;
            AccessCount++;
        }
        
        /// <summary>
        /// Get time since last access / Получить время с последнего доступа
        /// </summary>
        public TimeSpan GetTimeSinceLastAccess()
        {
            return DateTime.Now - LastAccessTime;
        }
        
        /// <summary>
        /// Calculate cleanup score (higher = more likely to be cleaned) / Вычислить очки для очистки
        /// </summary>
        public float CalculateCleanupScore()
        {
            if (IsProtected) return -1f; // Never cleanup protected assets
            
            var hoursSinceAccess = (float)GetTimeSinceLastAccess().TotalHours;
            var sizeScore = EstimatedSizeBytes / (1024f * 1024f); // MB
            var accessFrequency = AccessCount / Math.Max(1f, hoursSinceAccess);
            
            // Higher score = more likely to be cleaned up
            // Factor in: time since access, size, low access frequency
            return (hoursSinceAccess * sizeScore) / Math.Max(0.1f, accessFrequency);
        }
    }
}