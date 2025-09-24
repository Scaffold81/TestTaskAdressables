using UnityEngine;

namespace Game.Services
{
    /// <summary>
    /// Фабрика для создания игровых объектов. Обертка над Object.Instantiate.
    /// Factory for creating game objects. Wrapper over Object.Instantiate.
    /// </summary>
    public class GameFactory : IGameFactory
    {
        /// <summary>
        /// Создает экземпляр объекта в указанной позиции.
        /// Creates object instance at specified position.
        /// </summary>
        /// <typeparam name="T">Тип объекта / Object type</typeparam>
        /// <param name="prefab">Префаб / Prefab</param>
        /// <param name="position">Позиция / Position</param>
        /// <param name="parent">Родительский объект / Parent object</param>
        /// <returns>Созданный объект / Created object</returns>
        public T Create<T>(T prefab, Vector3 position, Transform parent) where T : UnityEngine.Object
        {
            return Object.Instantiate(prefab, position, Quaternion.identity, parent);
        }
    }
}