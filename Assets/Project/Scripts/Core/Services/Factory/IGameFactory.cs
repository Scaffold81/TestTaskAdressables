using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Services
{
    /// <summary>
    /// Factory service for instantiating game objects through Addressables
    /// Фабрика для создания игровых объектов через Addressables
    /// </summary>
    public interface IGameFactory
    {
        /// <summary>
        /// Instantiate prefab by Addressable key / Создать префаб по ключу Addressable
        /// </summary>
        UniTask<GameObject> InstantiateAsync(string key, Transform parent = null);
        
        /// <summary>
        /// Instantiate prefab at position / Создать префаб в позиции
        /// </summary>
        UniTask<GameObject> InstantiateAsync(string key, Vector3 position, Quaternion rotation, Transform parent = null);
        
        /// <summary>
        /// Instantiate and get component / Создать и получить компонент
        /// </summary>
        UniTask<T> InstantiateAsync<T>(string key, Transform parent = null) where T : Component;
        
        /// <summary>
        /// Destroy game object and release Addressable / Уничтожить объект и освободить Addressable
        /// </summary>
        void Destroy(GameObject gameObject);
        
        /// <summary>
        /// Destroy with delay / Уничтожить с задержкой
        /// </summary>
        void Destroy(GameObject gameObject, float delay);
    }
}