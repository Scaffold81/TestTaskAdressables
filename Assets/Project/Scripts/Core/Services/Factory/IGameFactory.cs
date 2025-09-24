using UnityEngine;

namespace Game.Services
{
    /// <summary>
    /// Интерфейс фабрики для создания игровых объектов.
    /// Interface for factory that creates game objects.
    /// </summary>
    public interface IGameFactory
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
        T Create<T>(T prefab, Vector3 position, Transform parent) where T : Object;
    }
}