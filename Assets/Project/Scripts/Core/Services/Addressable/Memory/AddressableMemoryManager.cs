using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Project.Core.Services.Addressable.Models;
using Project.Core.Config.Addressable;
using R3;

namespace Project.Core.Services.Addressable.Memory
{
    /// <summary>
    /// Implementation of Addressable memory manager
    /// Реализация менеджера памяти Addressables
    /// </summary>
    public class AddressableMemoryManager : IAddressableMemoryManager, IDisposable
    {
        private readonly MemoryManagement _settings;
        private readonly Dictionary<string, long> _trackedAssets = new Dictionary<string, long>();
        private readonly Subject<float> _memoryPressureSubject = new Subject<float>();
        
        private long _currentMemoryUsage = 0;
        private DateTime _lastCleanupTime = DateTime.MinValue;
        private bool _isCleanupInProgress = false;
        
        public long CurrentMemoryUsage => _currentMemoryUsage;
        public long MemoryThreshold => _settings.GetMemoryThresholdBytes();
        public Observable<float> OnMemoryPressure => _memoryPressureSubject.AsObservable();
        
        public AddressableMemoryManager(IAddressableConfigRepository configRepository)
        {
            if (configRepository == null)
                throw new ArgumentNullException(nameof(configRepository));
            
            _settings = new MemoryManagement();
            
            if (_settings.EnableAutoCleanup)
            {
                StartAutoCleanupLoop().Forget();
            }
        }
        
        public async UniTask CleanupMemoryAsync()
        {
            if (_isCleanupInProgress)
            {
                Debug.LogWarning("[MemoryManager] Cleanup already in progress");
                return;
            }
            
            _isCleanupInProgress = true;
            
            try
            {
                var startMemory = _currentMemoryUsage;
                
                await ReleaseUnusedAssetsAsync();
                
                if (_settings.UnloadUnusedAssets)
                {
                    await Resources.UnloadUnusedAssets().ToUniTask();
                }
                
                if (_settings.ForceGCAfterCleanup)
                {
                    GC.Collect();
                    await UniTask.Yield();
                }
                
                _lastCleanupTime = DateTime.Now;
                
                var freedMemory = startMemory - _currentMemoryUsage;
                Debug.Log($"[MemoryManager] Cleaned up {freedMemory / (1024f * 1024f):F1} MB");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MemoryManager] Cleanup failed: {ex.Message}");
            }
            finally
            {
                _isCleanupInProgress = false;
            }
        }
        
        public void TrackAsset(string key, long sizeBytes)
        {
            if (string.IsNullOrEmpty(key)) return;
            
            if (!_trackedAssets.ContainsKey(key))
            {
                _trackedAssets[key] = sizeBytes;
                _currentMemoryUsage += sizeBytes;
                UpdateMemoryPressure();
            }
        }
        
        public void UntrackAsset(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            
            if (_trackedAssets.TryGetValue(key, out var size))
            {
                _trackedAssets.Remove(key);
                _currentMemoryUsage -= size;
                UpdateMemoryPressure();
            }
        }
        
        public bool ShouldCleanup()
        {
            return _settings.ShouldCleanup(_currentMemoryUsage);
        }
        
        public MemoryStats GetMemoryStats()
        {
            var threshold = MemoryThreshold;
            var pressure = threshold > 0 ? Mathf.Clamp01((float)_currentMemoryUsage / threshold) : 0f;
            
            return new MemoryStats
            {
                TotalMemoryBytes = threshold,
                UsedMemoryBytes = _currentMemoryUsage,
                TrackedAssetsCount = _trackedAssets.Count,
                MemoryPressure = pressure
            };
        }
        
        private async UniTaskVoid StartAutoCleanupLoop()
        {
            while (_settings.EnableAutoCleanup)
            {
                await UniTask.Delay(_settings.GetCleanupInterval());
                
                if (ShouldCleanup())
                {
                    await CleanupMemoryAsync();
                }
            }
        }
        
        private async UniTask ReleaseUnusedAssetsAsync()
        {
            var assetsToRelease = new List<string>();
            
            switch (_settings.Strategy)
            {
                case CleanupStrategy.Aggressive:
                    assetsToRelease.AddRange(_trackedAssets.Keys);
                    break;
                    
                case CleanupStrategy.Balanced:
                    var halfCount = _trackedAssets.Count / 2;
                    var index = 0;
                    foreach (var key in _trackedAssets.Keys)
                    {
                        if (index++ >= halfCount) break;
                        assetsToRelease.Add(key);
                    }
                    break;
                    
                case CleanupStrategy.Conservative:
                    var stats = GetMemoryStats();
                    if (stats.MemoryPressure > 0.9f)
                    {
                        var quarter = _trackedAssets.Count / 4;
                        var idx = 0;
                        foreach (var key in _trackedAssets.Keys)
                        {
                            if (idx++ >= quarter) break;
                            assetsToRelease.Add(key);
                        }
                    }
                    break;
            }
            
            foreach (var key in assetsToRelease)
            {
                UntrackAsset(key);
            }
            
            await UniTask.Yield();
        }
        
        private void UpdateMemoryPressure()
        {
            var stats = GetMemoryStats();
            _memoryPressureSubject.OnNext(stats.MemoryPressure);
        }
        
        public void Dispose()
        {
            _memoryPressureSubject?.Dispose();
            _trackedAssets?.Clear();
        }
    }
}