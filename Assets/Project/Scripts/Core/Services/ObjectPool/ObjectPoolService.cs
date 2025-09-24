using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Game.Services
{
    /// <summary>
    /// Сервис для управления пулами объектов. Позволяет эффективно переиспользовать объекты.
    /// Service for managing object pools. Allows efficient object reuse.
    /// </summary>
    public class ObjectPoolService : IObjectPoolService
    {
        private Dictionary<string, object> pools = new Dictionary<string, object>();
        private Dictionary<Component, string> objectToPoolMap = new Dictionary<Component, string>();
        private Transform poolsContainer;
        
        // События
        public event Action<string, Component> OnObjectCreated;
        public event Action<string, Component> OnObjectReturned;
        public event Action<string, Component> OnObjectRetrieved;
        
        [Inject]
        private void Construct()
        {
            Debug.Log("[ObjectPoolService] Initializing object pool service...");
            
            // Создаем контейнер для всех пулов
            var containerGO = new GameObject("ObjectPools");
            poolsContainer = containerGO.transform;
            UnityEngine.Object.DontDestroyOnLoad(containerGO);
            
            Debug.Log("[ObjectPoolService] Object pool service initialized successfully!");
        }
        
        #region Pool Creation
        public void CreatePool<T>(string poolName, int initialSize, int maxSize = 100) where T : Component
        {
            CreatePool<T>(poolName, null, initialSize, maxSize);
        }
        
        public void CreatePool<T>(string poolName, T prefab, int initialSize, int maxSize = 100) where T : Component
        {
            if (HasPool(poolName))
            {
                Debug.LogWarning($"[ObjectPoolService] Pool '{poolName}' already exists!");
                return;
            }
            
            var poolParent = new GameObject($"Pool_{poolName}").transform;
            poolParent.SetParent(poolsContainer);
            
            var pool = new ObjectPool<T>(poolName, prefab, initialSize, maxSize, poolParent, OnObjectCreated);
            pools[poolName] = pool;
            
            Debug.Log($"[ObjectPoolService] Created pool '{poolName}' with initial size: {initialSize}, max size: {maxSize}");
        }
        #endregion
        
        #region Object Retrieval
        public T Get<T>(string poolName) where T : Component
        {
            var pool = GetPool<T>(poolName);
            if (pool == null) return null;
            
            var obj = pool.Get();
            objectToPoolMap[obj] = poolName;
            
            OnObjectRetrieved?.Invoke(poolName, obj);
            Debug.Log($"[ObjectPoolService] Retrieved {typeof(T).Name} from pool '{poolName}'");
            
            return obj;
        }
        
        public T Get<T>(string poolName, Vector3 position, Quaternion rotation) where T : Component
        {
            var obj = Get<T>(poolName);
            if (obj != null)
            {
                obj.transform.position = position;
                obj.transform.rotation = rotation;
            }
            return obj;
        }
        
        public T Get<T>(string poolName, Vector3 position, Quaternion rotation, Transform parent) where T : Component
        {
            var obj = Get<T>(poolName, position, rotation);
            if (obj != null)
            {
                obj.transform.SetParent(parent);
            }
            return obj;
        }
        #endregion
        
        #region Object Return
        public void Return<T>(string poolName, T obj) where T : Component
        {
            if (obj == null)
            {
                Debug.LogWarning("[ObjectPoolService] Trying to return null object!");
                return;
            }
            
            var pool = GetPool<T>(poolName);
            if (pool == null) return;
            
            if (pool.Return(obj))
            {
                objectToPoolMap.Remove(obj);
                OnObjectReturned?.Invoke(poolName, obj);
                Debug.Log($"[ObjectPoolService] Returned {typeof(T).Name} to pool '{poolName}'");
            }
            else
            {
                Debug.LogWarning($"[ObjectPoolService] Failed to return {typeof(T).Name} to pool '{poolName}'");
            }
        }
        
        public void Return<T>(T obj) where T : Component
        {
            if (obj == null)
            {
                Debug.LogWarning("[ObjectPoolService] Trying to return null object!");
                return;
            }
            
            if (objectToPoolMap.TryGetValue(obj, out var poolName))
            {
                Return(poolName, obj);
            }
            else
            {
                Debug.LogWarning($"[ObjectPoolService] Object {obj.name} is not tracked by any pool!");
            }
        }
        #endregion
        
        #region Pool Management
        /// <summary>
        /// Прогревает пул, создавая указанное количество объектов заранее.
        /// Warms up pool by creating specified number of objects in advance.
        /// </summary>
        /// <param name="poolName">Имя пула / Pool name</param>
        /// <param name="count">Количество объектов для создания / Number of objects to create</param>
        public void WarmupPool(string poolName, int count)
        {
            if (!pools.TryGetValue(poolName, out var poolObj))
            {
                return;
            }
            
            // Используем рефлексию для вызова WarmUp на правильном типе пула
            var warmupMethod = poolObj.GetType().GetMethod("WarmUp");
            warmupMethod?.Invoke(poolObj, new object[] { count });
        }
        
        /// <summary>
        /// Очищает и удаляет указанный пул.
        /// Clears and removes specified pool.
        /// </summary>
        /// <param name="poolName">Имя пула / Pool name</param>
        public void ClearPool(string poolName)
        {
            if (!pools.TryGetValue(poolName, out var poolObj))
            {
                return;
            }
            
            // Используем рефлексию для вызова Clear на правильном типе пула
            var clearMethod = poolObj.GetType().GetMethod("Clear");
            clearMethod?.Invoke(poolObj, null);
            
            // Удаляем записи из отслеживания объектов
            var keysToRemove = new List<Component>();
            foreach (var kvp in objectToPoolMap)
            {
                if (kvp.Value == poolName)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }
            
            foreach (var key in keysToRemove)
            {
                objectToPoolMap.Remove(key);
            }
            
            pools.Remove(poolName);
        }
        
        /// <summary>
        /// Очищает все пулы объектов.
        /// Clears all object pools.
        /// </summary>
        public void ClearAllPools()
        {
            var poolNames = new List<string>(pools.Keys);
            foreach (var poolName in poolNames)
            {
                ClearPool(poolName);
            }
        }
        #endregion
        
        #region Pool Information
        /// <summary>
        /// Проверяет существование пула с указанным именем.
        /// Checks if pool with specified name exists.
        /// </summary>
        /// <param name="poolName">Имя пула / Pool name</param>
        /// <returns>True если пул существует / True if pool exists</returns>
        public bool HasPool(string poolName)
        {
            return pools.ContainsKey(poolName);
        }
        
        /// <summary>
        /// Получает общий размер пула (активные + неактивные объекты).
        /// Gets total pool size (active + inactive objects).
        /// </summary>
        /// <param name="poolName">Имя пула / Pool name</param>
        /// <returns>Общий размер пула / Total pool size</returns>
        public int GetPoolSize(string poolName)
        {
            if (!pools.TryGetValue(poolName, out var poolObj))
                return 0;
            
            var totalCountProperty = poolObj.GetType().GetProperty("TotalCount");
            return (int)(totalCountProperty?.GetValue(poolObj) ?? 0);
        }
        
        /// <summary>
        /// Получает количество активных объектов в пуле.
        /// Gets number of active objects in pool.
        /// </summary>
        /// <param name="poolName">Имя пула / Pool name</param>
        /// <returns>Количество активных объектов / Number of active objects</returns>
        public int GetActiveCount(string poolName)
        {
            if (!pools.TryGetValue(poolName, out var poolObj))
                return 0;
            
            var activeCountProperty = poolObj.GetType().GetProperty("ActiveCount");
            return (int)(activeCountProperty?.GetValue(poolObj) ?? 0);
        }
        
        /// <summary>
        /// Получает количество неактивных объектов в пуле.
        /// Gets number of inactive objects in pool.
        /// </summary>
        /// <param name="poolName">Имя пула / Pool name</param>
        /// <returns>Количество неактивных объектов / Number of inactive objects</returns>
        public int GetInactiveCount(string poolName)
        {
            if (!pools.TryGetValue(poolName, out var poolObj))
                return 0;
            
            var inactiveCountProperty = poolObj.GetType().GetProperty("InactiveCount");
            return (int)(inactiveCountProperty?.GetValue(poolObj) ?? 0);
        }
        
        /// <summary>
        /// Получает статистику по всем пулам (активные, неактивные, общее количество объектов).
        /// Gets statistics for all pools (active, inactive, total object counts).
        /// </summary>
        /// <returns>Словарь со статистикой пулов / Dictionary with pool statistics</returns>
        public Dictionary<string, (int active, int inactive, int total)> GetPoolStats()
        {
            var stats = new Dictionary<string, (int active, int inactive, int total)>();
            
            foreach (var kvp in pools)
            {
                var poolName = kvp.Key;
                var poolObj = kvp.Value;
                
                var activeCountProperty = poolObj.GetType().GetProperty("ActiveCount");
                var inactiveCountProperty = poolObj.GetType().GetProperty("InactiveCount");
                var totalCountProperty = poolObj.GetType().GetProperty("TotalCount");
                
                var active = (int)(activeCountProperty?.GetValue(poolObj) ?? 0);
                var inactive = (int)(inactiveCountProperty?.GetValue(poolObj) ?? 0);
                var total = (int)(totalCountProperty?.GetValue(poolObj) ?? 0);
                
                stats[poolName] = (active, inactive, total);
            }
            
            return stats;
        }
        #endregion
        
        #region Private Helpers
        /// <summary>
        /// Получает типизированный пул по имени.
        /// Gets typed pool by name.
        /// </summary>
        /// <typeparam name="T">Тип компонента / Component type</typeparam>
        /// <param name="poolName">Имя пула / Pool name</param>
        /// <returns>Типизированный пул или null / Typed pool or null</returns>
        private ObjectPool<T> GetPool<T>(string poolName) where T : Component
        {
            if (!pools.TryGetValue(poolName, out var poolObj))
            {
                return null;
            }
            
            if (poolObj is ObjectPool<T> typedPool)
            {
                return typedPool;
            }
            
            return null;
        }
        #endregion
    }
}