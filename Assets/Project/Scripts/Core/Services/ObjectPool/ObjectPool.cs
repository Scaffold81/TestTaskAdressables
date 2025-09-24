using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Services
{
    /// <summary>
    /// Пул объектов для конкретного типа компонентов. Управляет жизненным циклом объектов.
    /// Object pool for specific component type. Manages object lifecycle.
    /// </summary>
    [Serializable]
    public class ObjectPool<T> where T : Component
    {
        [SerializeField] private string poolName;
        [SerializeField] private T prefab;
        [SerializeField] private int maxSize;
        [SerializeField] private Transform poolParent;
        
        private Queue<T> availableObjects = new Queue<T>();
        private HashSet<T> activeObjects = new HashSet<T>();
        private int createdCount = 0;
        
        // Коллбэки для событий
        private Action<string, Component> onObjectCreated;
        
        public string PoolName => poolName;
        public int ActiveCount => activeObjects.Count;
        public int InactiveCount => availableObjects.Count;
        public int TotalCount => createdCount;
        public int MaxSize => maxSize;
        
        public ObjectPool(string name, T prefab, int initialSize, int maxSize, Transform parent = null, Action<string, Component> onObjectCreated = null)
        {
            this.poolName = name;
            this.prefab = prefab;
            this.maxSize = maxSize;
            this.poolParent = parent;
            this.onObjectCreated = onObjectCreated;
            
            // Создаем родительский объект для пула если не указан
            if (this.poolParent == null)
            {
                var poolContainer = new GameObject($"Pool_{name}");
                this.poolParent = poolContainer.transform;
                UnityEngine.Object.DontDestroyOnLoad(poolContainer);
            }
            
            // Предварительное создание объектов
            WarmUp(initialSize);
        }
        
        public T Get()
        {
            T obj;
            
            if (availableObjects.Count > 0)
            {
                // Берем из пула
                obj = availableObjects.Dequeue();
            }
            else if (createdCount < maxSize)
            {
                // Создаем новый объект
                obj = CreateNewObject();
            }
            else
            {
                Debug.LogWarning($"[ObjectPool] Pool '{poolName}' reached max size ({maxSize}). Reusing oldest object.");
                // Возвращаем самый старый активный объект
                var enumerator = activeObjects.GetEnumerator();
                enumerator.MoveNext();
                obj = enumerator.Current;
                activeObjects.Remove(obj);
            }
            
            // Активируем объект
            obj.gameObject.SetActive(true);
            activeObjects.Add(obj);
            
            return obj;
        }
        
        public T Get(Vector3 position, Quaternion rotation)
        {
            var obj = Get();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            return obj;
        }
        
        public T Get(Vector3 position, Quaternion rotation, Transform parent)
        {
            var obj = Get(position, rotation);
            obj.transform.SetParent(parent);
            return obj;
        }
        
        public bool Return(T obj)
        {
            if (obj == null || !activeObjects.Contains(obj))
            {
                return false;
            }
            
            activeObjects.Remove(obj);
            
            // Сбрасываем состояние объекта
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(poolParent);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
            
            // Возвращаем в пул
            availableObjects.Enqueue(obj);
            
            return true;
        }
        
        public void WarmUp(int count)
        {
            for (int i = 0; i < count && createdCount < maxSize; i++)
            {
                var obj = CreateNewObject();
                obj.gameObject.SetActive(false);
                availableObjects.Enqueue(obj);
            }
        }
        
        public void Clear()
        {
            // Уничтожаем все активные объекты
            foreach (var obj in activeObjects)
            {
                if (obj != null)
                {
                    UnityEngine.Object.Destroy(obj.gameObject);
                }
            }
            activeObjects.Clear();
            
            // Уничтожаем все неактивные объекты
            while (availableObjects.Count > 0)
            {
                var obj = availableObjects.Dequeue();
                if (obj != null)
                {
                    UnityEngine.Object.Destroy(obj.gameObject);
                }
            }
            
            createdCount = 0;
        }
        
        private T CreateNewObject()
        {
            T obj;
            
            if (prefab != null)
            {
                obj = UnityEngine.Object.Instantiate(prefab, poolParent);
            }
            else
            {
                // Создаем пустой GameObject с компонентом T
                var go = new GameObject($"{poolName}_Object_{createdCount}");
                go.transform.SetParent(poolParent);
                obj = go.AddComponent<T>();
            }
            
            createdCount++;
            
            // Вызываем событие создания объекта
            onObjectCreated?.Invoke(poolName, obj);
            
            return obj;
        }
    }
}