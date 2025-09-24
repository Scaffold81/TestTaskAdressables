using Game.Enums;

namespace Game.Services
{
    /// <summary>
    /// Интерфейс сервиса управления сценами.
    /// Scene management service interface.
    /// </summary>
    public interface ISceneManagerService
    {
        /// <summary>
        /// Идентификатор целевой сцены для загрузки.
        /// Target scene identifier for loading.
        /// </summary>
        SceneId TargetSceneId { get; }

        /// <summary>
        /// Асинхронно загружает сцену по идентификатору.
        /// Asynchronously loads scene by identifier.
        /// </summary>
        /// <param name="scene">Идентификатор сцены / Scene identifier</param>
        void LoadSceneAsync(SceneId scene);
    }
}
