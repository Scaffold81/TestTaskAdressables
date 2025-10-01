using Cysharp.Threading.Tasks;
using UnityEngine;
using Project.Core.Services.Addressable;
using Zenject;
using System;
using System.Collections.Generic;

namespace Game.Services
{
    /// <summary>
    /// Implementation of game factory with Addressables and Zenject integration
    /// Реализация игровой фабрики с интеграцией Addressables и Zenject
    /// </summary>
    public class GameFactory : IGameFactory
    {
        private readonly IAddressableService _addressableService;
        private readonly DiContainer _container;
        private readonly Dictionary<GameObject, string> _instantiatedObjects = new Dictionary<GameObject, string>();
        
        [Inject]
        public GameFactory(IAddressableService addressableService, DiContainer container)
        {
            _addressableService = addressableService ?? throw new ArgumentNullException(nameof(addressableService));
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }
        
        public async UniTask<GameObject> InstantiateAsync(string key, Transform parent = null)
        {
            var prefab = await _addressableService.LoadAssetAsync<GameObject>(key);
            
            var instance = _container.InstantiatePrefab(prefab, parent);
            _instantiatedObjects[instance] = key;
            
            Debug.Log($"[GameFactory] Instantiated {key} with Zenject injection");
            return instance;
        }
        
        public async UniTask<GameObject> InstantiateAsync(string key, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            var prefab = await _addressableService.LoadAssetAsync<GameObject>(key);
            
            var instance = _container.InstantiatePrefab(prefab, position, rotation, parent);
            _instantiatedObjects[instance] = key;
            
            Debug.Log($"[GameFactory] Instantiated {key} at position {position}");
            return instance;
        }
        
        public async UniTask<T> InstantiateAsync<T>(string key, Transform parent = null) where T : Component
        {
            var gameObject = await InstantiateAsync(key, parent);
            
            var component = gameObject.GetComponent<T>();
            if (component == null)
            {
                Debug.LogError($"[GameFactory] Component {typeof(T).Name} not found on {key}");
                Destroy(gameObject);
                return null;
            }
            
            return component;
        }
        
        public void Destroy(GameObject gameObject)
        {
            if (gameObject == null) return;
            
            if (_instantiatedObjects.TryGetValue(gameObject, out var key))
            {
                _instantiatedObjects.Remove(gameObject);
                Debug.Log($"[GameFactory] Destroying {key}");
            }
            
            UnityEngine.Object.Destroy(gameObject);
        }
        
        public void Destroy(GameObject gameObject, float delay)
        {
            if (gameObject == null) return;
            
            if (_instantiatedObjects.ContainsKey(gameObject))
            {
                _instantiatedObjects.Remove(gameObject);
            }
            
            UnityEngine.Object.Destroy(gameObject, delay);
        }
    }
}