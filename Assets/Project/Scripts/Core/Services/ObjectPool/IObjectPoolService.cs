using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Services
{
    /// <summary>
    /// Интерфейс сервиса пулов объектов для эффективного управления памятью.
    /// Object pool service interface for efficient memory management.
    /// </summary>
    public interface IObjectPoolService
    {
        // Создание пулов
        void CreatePool<T>(string poolName, int initialSize, int maxSize = 100) where T : Component;
        void CreatePool<T>(string poolName, T prefab, int initialSize, int maxSize = 100) where T : Component;
        
        // Получение объектов из пула
        T Get<T>(string poolName) where T : Component;
        T Get<T>(string poolName, Vector3 position, Quaternion rotation) where T : Component;
        T Get<T>(string poolName, Vector3 position, Quaternion rotation, Transform parent) where T : Component;
        
        // Возврат объектов в пул
        void Return<T>(string poolName, T obj) where T : Component;
        void Return<T>(T obj) where T : Component; // Автоопределение пула
        
        // Управление пулами
        void WarmupPool(string poolName, int count);
        void ClearPool(string poolName);
        void ClearAllPools();
        
        // Информация о пулах
        bool HasPool(string poolName);
        int GetPoolSize(string poolName);
        int GetActiveCount(string poolName);
        int GetInactiveCount(string poolName);
        Dictionary<string, (int active, int inactive, int total)> GetPoolStats();
        
        // События
        event Action<string, Component> OnObjectCreated;
        event Action<string, Component> OnObjectReturned;
        event Action<string, Component> OnObjectRetrieved;
    }
}