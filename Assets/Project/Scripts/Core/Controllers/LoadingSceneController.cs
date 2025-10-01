using Cysharp.Threading.Tasks;
using Game.Services;
using System;
using UnityEngine;
using Zenject;

namespace Game.Controllers.SceneMenegment
{
    /// <summary>
    /// Контроллер сцены загрузки. Отвечает за показ загрузочного экрана и переход к целевой сцене.
    /// Loading scene controller. Responsible for showing loading screen and transitioning to target scene.
    /// </summary>
    public class LoadingSceneController : MonoBehaviour
    {
        [SerializeField] private float minLoadingTime = 1.0f;
        
        private ISceneManagerService sceneManagerService;

        /// <summary>
        /// Конструктор с инъекцией зависимостей.
        /// Constructor with dependency injection.
        /// </summary>
        /// <param name="sceneManagerService">Сервис управления сценами / Scene manager service</param>
        [Inject]
        private void Construct(ISceneManagerService sceneManagerService)
        {
            this.sceneManagerService = sceneManagerService;
        }

        /// <summary>
        /// Инициализация компонента и запуск процесса загрузки.
        /// Component initialization and loading process start.
        /// </summary>
        private async void Start()
        {
            await LoadTargetSceneAsync();
        }

        /// <summary>
        /// Асинхронно загружает целевую сцену с минимальным временем ожидания.
        /// Asynchronously loads target scene with minimum waiting time.
        /// </summary>
        /// <returns>Операция UniTask / UniTask operation</returns>
        private async UniTask LoadTargetSceneAsync()
        {
            try
            {
                // Минимальное время показа загрузочного экрана
                await UniTask.Delay(TimeSpan.FromSeconds(minLoadingTime));

                await sceneManagerService.LoadSceneAsync(sceneManagerService.TargetSceneId);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LoadingSceneController] Failed to load target scene: {ex.Message}");
            }
        }
    }
}
