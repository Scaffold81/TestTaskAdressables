using R3;
using UnityEngine;
using Zenject;

namespace Game.Services
{
    /// <summary>
    /// Сервис для управления данными игрока. Может содержать логику уровня, опыта, достижений и т.д.
    /// Service for managing player data. Can contain logic for level, experience, achievements, etc.
    /// </summary>
    public class PlayerDataService : IPlayerDataService
    {
        private ISaveService saveService;

        /// <summary>
        /// Конструктор с инъекцией сервиса сохранения.
        /// Constructor with save service injection.
        /// </summary>
        /// <param name="saveService">Сервис сохранения / Save service</param>
        [Inject]
        private void Construct(ISaveService saveService)
        {
            this.saveService = saveService;
           
        }
    }
}