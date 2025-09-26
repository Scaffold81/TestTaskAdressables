using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Cysharp.Threading.Tasks;
using R3;

namespace Project.Core.Services.Addressable.Memory
{
    /// <summary>
    /// Memory management implementation for Addressable system
    /// Реализация управления памятью для системы Addressables
    /// </summary>
    public class AddressableMemoryManager : IAddressableMemoryManager
    {
        private readonly Dictionary<string, AssetMemoryInfo> _trackedAssets = new Dictionary<string, AssetMemoryInfo>();
        private readonly Subject<MemoryUsageData> _memoryUsageSubject = new Subject<MemoryUsageData>();
        private readonly Subject<MemoryCleanupData> _memoryCleanupSubject = new Subject<MemoryCleanupData>();
        
        private long _memoryLimit = 100 * 1024 * 1024; // 100MB default
        private long _currentMemoryUsage = 0;
        
        /// <summary>
        /// Current memory usage in bytes / Текущее использование памяти в байтах
        /// </summary>
        public long CurrentMemoryUsage => _currentMemoryUsage;
        
        /// <summary>
        /// Memory usage limit in bytes / Лимит использования памяти в байтах
        /// </summary>
        public long MemoryLimit 
        { 
            get => _memoryLimit; 
            set => _memoryLimit = value; 
        }
        
        /// <summary>
        /// Number of currently loaded assets / Количество загруженных ресурсов
        /// </summary>
        public int LoadedAssetsCount => _trackedAssets.Count;
        
        /// <summary>
        /// Observable for memory usage changes / Observable для изменений использования памяти
        /// </summary>
        public Observable<MemoryUsageData> OnMemoryUsageChanged => _memoryUsageSubject.AsObservable();
        
        /// <summary>
        /// Observable for memory cleanup events / Observable для событий очистки памяти
        /// </summary>
        public Observable<MemoryCleanupData> OnMemoryCleanup => _memoryCleanupSubject.AsObservable();
        
        public AddressableMemoryManager()
        {
            Debug.Log("[AddressableMemoryManager] Initialized with memory tracking enabled");
        }
        
        /// <summary>
        /// Register asset in memory tracking / Зарегистрировать ресурс в отслеживании памяти
        /// </summary>
        public void RegisterAsset(string key, UnityEngine.Object asset, long estimatedSize = 0)
        {
            if (string.IsNullOrEmpty(key) || asset == null) return;
            
            if (estimatedSize <= 0)
                estimatedSize = EstimateAssetSize(asset);
            
            var assetInfo = new AssetMemoryInfo
            {
                Key = key,
                EstimatedSize = estimatedSize,
                LastAccess = DateTime.Now,
                AccessCount = 1,
                Asset = asset
            };
            
            if (_trackedAssets.ContainsKey(key))
            {
                // Update existing
                var existing = _trackedAssets[key];
                _currentMemoryUsage -= existing.EstimatedSize;
                existing.AccessCount++;
                existing.LastAccess = DateTime.Now;
                _trackedAssets[key] = existing;
            }
            else
            {
                _trackedAssets[key] = assetInfo;
            }
            
            _currentMemoryUsage += estimatedSize;
            NotifyMemoryUsageChanged();
            
            Debug.Log($"[AddressableMemoryManager] Registered asset {key}, size: {FormatBytes(estimatedSize)}");
        }
        
        /// <summary>
        /// Unregister asset from memory tracking / Отменить регистрацию ресурса из отслеживания памяти
        /// </summary>
        public void UnregisterAsset(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            
            if (_trackedAssets.TryGetValue(key, out var assetInfo))
            {
                _currentMemoryUsage -= assetInfo.EstimatedSize;
                _trackedAssets.Remove(key);
                
                NotifyMemoryUsageChanged();
                Debug.Log($"[AddressableMemoryManager] Unregistered asset {key}, freed: {FormatBytes(assetInfo.EstimatedSize)}");
            }
        }
        
        /// <summary>
        /// Force memory cleanup / Принудительная очистка памяти
        /// </summary>
        public async UniTask CleanupMemoryAsync()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var initialMemory = _currentMemoryUsage;
            var assetsCleared = 0;
            
            try
            {
                // Get assets sorted by cleanup priority
                var assetsToCleanup = GetAssetsByUsagePriority()
                    .Where(a => (DateTime.Now - a.LastAccess).TotalMinutes > 5) // 5 minutes threshold
                    .Take(10) // Cleanup max 10 assets at once
                    .ToArray();
                
                foreach (var asset in assetsToCleanup)
                {
                    UnregisterAsset(asset.Key);
                    assetsCleared++;
                }
                
                await UniTask.Yield();
                stopwatch.Stop();
                
                var memoryFreed = initialMemory - _currentMemoryUsage;
                var cleanupData = new MemoryCleanupData(memoryFreed, assetsCleared, stopwatch.ElapsedMilliseconds, CleanupReason.Manual);
                
                _memoryCleanupSubject.OnNext(cleanupData);
                
                Debug.Log($"[AddressableMemoryManager] Cleanup completed: freed {FormatBytes(memoryFreed)}, cleared {assetsCleared} assets");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AddressableMemoryManager] Cleanup failed: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Cleanup unused assets / Очистка неиспользуемых ресурсов
        /// </summary>
        public async UniTask CleanupUnusedAssetsAsync()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var initialMemory = _currentMemoryUsage;
            var assetsCleared = 0;
            
            try
            {
                // Find unused assets (not accessed in last 10 minutes)
                var unusedAssets = _trackedAssets.Values
                    .Where(a => (DateTime.Now - a.LastAccess).TotalMinutes > 10)
                    .ToArray();
                
                foreach (var asset in unusedAssets)
                {
                    UnregisterAsset(asset.Key);
                    assetsCleared++;
                }
                
                await UniTask.Yield();
                stopwatch.Stop();
                
                var memoryFreed = initialMemory - _currentMemoryUsage;
                var cleanupData = new MemoryCleanupData(memoryFreed, assetsCleared, stopwatch.ElapsedMilliseconds, CleanupReason.Manual);
                
                _memoryCleanupSubject.OnNext(cleanupData);
                
                Debug.Log($"[AddressableMemoryManager] Unused assets cleanup: freed {FormatBytes(memoryFreed)}, cleared {assetsCleared} assets");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AddressableMemoryManager] Unused assets cleanup failed: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Check if memory usage is above threshold / Проверить, превышено ли пороговое значение памяти
        /// </summary>
        public bool IsMemoryUsageHigh(float threshold = 0.8f)
        {
            if (_memoryLimit <= 0) return false;
            
            float usage = (float)_currentMemoryUsage / _memoryLimit;
            return usage > threshold;
        }
        
        /// <summary>
        /// Get memory statistics / Получить статистику памяти
        /// </summary>
        public MemoryStats GetMemoryStats()
        {
            return new MemoryStats
            {
                TotalAllocated = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong(),
                AddressableMemory = _currentMemoryUsage,
                TrackedAssets = _trackedAssets.Count,
                SystemMemory = GC.GetTotalMemory(false),
                GCMemory = GC.GetTotalMemory(false)
            };
        }
        
        /// <summary>
        /// Get assets by usage priority / Получить ресурсы по приоритету использования
        /// </summary>
        public IEnumerable<AssetMemoryInfo> GetAssetsByUsagePriority()
        {
            return _trackedAssets.Values
                .OrderBy(a => a.CleanupPriority) // Lower priority = cleanup first
                .ThenBy(a => a.LastAccess);
        }
        
        /// <summary>
        /// Get current active handle count
        /// Получить текущее количество активных handle'ов
        /// </summary>
        public int GetActiveHandleCount()
        {
            return _trackedAssets.Count;
        }
        
        /// <summary>
        /// Get all tracked asset keys
        /// Получить все отслеживаемые ключи ресурсов
        /// </summary>
        public string[] GetTrackedAssetKeys()
        {
            return _trackedAssets.Keys.ToArray();
        }
        
        /// <summary>
        /// Generate detailed memory report
        /// Сгенерировать подробный отчет о памяти
        /// </summary>
        public string GenerateMemoryReport()
        {
            var report = new StringBuilder();
            
            report.AppendLine("=== ADDRESSABLE MEMORY REPORT ===");
            report.AppendLine($"Generated at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();
            
            // Summary
            report.AppendLine("SUMMARY:");
            report.AppendLine($"Tracked Assets: {_trackedAssets.Count}");
            report.AppendLine($"Total Memory: {FormatBytes(_currentMemoryUsage)}");
            report.AppendLine($"Memory Limit: {FormatBytes(_memoryLimit)}");
            report.AppendLine($"Usage: {(_memoryLimit > 0 ? (_currentMemoryUsage / (float)_memoryLimit * 100) : 0):F1}%");
            report.AppendLine();
            
            // Asset Details
            if (_trackedAssets.Count > 0)
            {
                report.AppendLine("TRACKED ASSETS:");
                foreach (var kvp in _trackedAssets.OrderByDescending(a => a.Value.EstimatedSize))
                {
                    var asset = kvp.Value;
                    var age = DateTime.Now - asset.LastAccess;
                    
                    report.AppendLine($"  {asset.Key}");
                    report.AppendLine($"    Size: {FormatBytes(asset.EstimatedSize)}, Access: {asset.AccessCount}x");
                    report.AppendLine($"    Last Access: {age.TotalMinutes:F1}m ago");
                }
                report.AppendLine();
            }
            
            return report.ToString();
        }
        
        /// <summary>
        /// Perform automatic cleanup based on settings
        /// Выполнить автоматическую очистку на основе настроек
        /// </summary>
        public int PerformAutomaticCleanup()
        {
            var cleanedCount = 0;
            var cutoffTime = DateTime.Now - TimeSpan.FromMinutes(10);
            var keysToCleanup = new List<string>();

            // Find assets that haven't been accessed recently
            foreach (var kvp in _trackedAssets)
            {
                var asset = kvp.Value;
                if (asset.LastAccess < cutoffTime)
                {
                    keysToCleanup.Add(kvp.Key);
                }
            }

            // Cleanup identified assets
            foreach (var key in keysToCleanup)
            {
                UnregisterAsset(key);
                cleanedCount++;
            }

            if (cleanedCount > 0)
            {
                Debug.Log($"[AddressableMemoryManager] Automatic cleanup removed {cleanedCount} assets");
            }

            return cleanedCount;
        }
        
        /// <summary>
        /// Notify about memory usage changes
        /// Уведомить об изменениях использования памяти
        /// </summary>
        private void NotifyMemoryUsageChanged()
        {
            var usageData = new MemoryUsageData(_currentMemoryUsage, _memoryLimit, _trackedAssets.Count);
            _memoryUsageSubject.OnNext(usageData);
        }
        
        /// <summary>
        /// Estimate asset size in bytes
        /// Оценить размер ресурса в байтах
        /// </summary>
        private long EstimateAssetSize(UnityEngine.Object asset)
        {
            if (asset == null) return 0;

            // Basic size estimation - this is very rough!
            switch (asset)
            {
                case Texture2D texture:
                    return texture.width * texture.height * GetBytesPerPixel(texture.format);
                
                case AudioClip audio:
                    return (long)(audio.length * audio.frequency * audio.channels * 2); // 16-bit assumption
                
                case Mesh mesh:
                    return (long)(mesh.vertexCount * (3 * 4 + 3 * 4 + 2 * 4)); // pos + normal + uv approximation
                
                case GameObject go:
                    // Very rough estimate based on components
                    return go.GetComponentsInChildren<Component>().Length * 1024;
                
                default:
                    return 1024; // 1KB default estimate
            }
        }
        
        /// <summary>
        /// Get bytes per pixel for texture format
        /// Получить байт на пиксель для формата текстуры
        /// </summary>
        private int GetBytesPerPixel(TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat.ARGB32:
                case TextureFormat.RGBA32:
                case TextureFormat.BGRA32:
                    return 4;
                    
                case TextureFormat.RGB24:
                    return 3;
                    
                case TextureFormat.ARGB4444:
                case TextureFormat.RGBA4444:
                case TextureFormat.RGB565:
                    return 2;
                    
                case TextureFormat.Alpha8:
                case TextureFormat.R8:
                    return 1;
                    
                default:
                    return 4; // Default assumption
            }
        }
        
        /// <summary>
        /// Format bytes to human readable string
        /// Форматировать байты в читаемую строку
        /// </summary>
        private string FormatBytes(long bytes)
        {
            const long KB = 1024;
            const long MB = KB * 1024;
            const long GB = MB * 1024;

            if (bytes >= GB)
                return $"{bytes / (float)GB:F2} GB";
            if (bytes >= MB)
                return $"{bytes / (float)MB:F2} MB";
            if (bytes >= KB)
                return $"{bytes / (float)KB:F2} KB";
            
            return $"{bytes} B";
        }
    }
}
