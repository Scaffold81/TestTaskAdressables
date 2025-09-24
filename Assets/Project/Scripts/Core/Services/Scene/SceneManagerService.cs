using Game.Enums;
using UnityEngine.SceneManagement;
using Zenject;

namespace Game.Services
{
    /// <summary>
    /// Сервис для управления сценами. Отвечает за загрузку и переходы между сценами.
    /// Scene management service. Responsible for loading and transitioning between scenes.
    /// </summary>
    public class SceneManagerService : ISceneManagerService
    {
        private ISaveService saveService;

        /// <summary>
        /// Идентификатор целевой сцены для загрузки.
        /// Target scene identifier for loading.
        /// </summary>
        public SceneId TargetSceneId { get; private set; } = SceneId.Main;

        /// <summary>
        /// Конструктор с инъекцией зависимостей.
        /// Constructor with dependency injection.
        /// </summary>
        /// <param name="saveService">Сервис сохранения / Save service</param>
        [Inject]
        void Construct(ISaveService saveService)
        {
            this.saveService = saveService;
            TargetSceneId = SceneId.Main;
        }

        /// <summary>
        /// Асинхронно загружает сцену по идентификатору.
        /// Asynchronously loads scene by identifier.
        /// </summary>
        /// <param name="scene">Идентификатор сцены / Scene identifier</param>
        public void LoadSceneAsync(SceneId scene)
        {
            SceneManager.LoadSceneAsync(scene.ToString());
        }

        /// <summary>
        /// Асинхронно загружает промежуточную сцену с указанием целевой сцены.
        /// Asynchronously loads interstitial scene with specified target scene.
        /// </summary>
        /// <param name="targetScene">Целевая сцена после промежуточной / Target scene after interstitial</param>
        public void LoadSceneIntersitialAsync(SceneId targetScene = SceneId.Main)
        {
            TargetSceneId = targetScene;
            SceneManager.LoadSceneAsync(SceneId.Interstitial.ToString());
        }
    }
}
