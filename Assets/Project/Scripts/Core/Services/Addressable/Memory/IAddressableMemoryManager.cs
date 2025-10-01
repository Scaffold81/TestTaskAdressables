using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;

namespace Project.Core.Services.Addressable.Memory
{
    /// <summary>
    /// Interface for Addressable memory management
    /// Интерфейс для управления памятью Addressables
    /// </summary>
    public interface IAddressableMemoryManager
    {
        long CurrentMemoryUsage { get; }
        long MemoryThreshold { get; }
        Observable<float> OnMemoryPressure { get; }
        
        UniTask CleanupMemoryAsync();
        void TrackAsset(string key, long sizeBytes);
        void UntrackAsset(string key);
        bool ShouldCleanup();
        MemoryStats GetMemoryStats();
    }
    
    /// <summary>
    /// Memory statistics / Статистика памяти
    /// </summary>
    public struct MemoryStats
    {
        public long TotalMemoryBytes;
        public long UsedMemoryBytes;
        public long TrackedAssetsCount;
        public float MemoryPressure;
    }
}