using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using R3;
using Game.Enums;

namespace Game.Services
{
    /// <summary>
    /// Service for managing scene transitions with Addressables support
    /// Сервис для управления переходами между сценами с поддержкой Addressables
    /// </summary>
    public interface ISceneManagerService
    {
        /// <summary>
        /// Target scene to load / Целевая сцена для загрузки
        /// </summary>
        SceneId TargetSceneId { get; set; }
        
        /// <summary>
        /// Current active scene name / Имя текущей активной сцены
        /// </summary>
        string CurrentSceneName { get; }
        
        /// <summary>
        /// Observable for scene loading progress / Observable для прогресса загрузки сцены
        /// </summary>
        Observable<float> OnSceneLoadProgress { get; }
        
        /// <summary>
        /// Observable for scene loaded event / Observable для события загрузки сцены
        /// </summary>
        Observable<string> OnSceneLoaded { get; }
        
        /// <summary>
        /// Load scene asynchronously by SceneId enum / Загрузить сцену асинхронно по SceneId enum
        /// </summary>
        UniTask LoadSceneAsync(SceneId sceneId, LoadSceneMode mode = LoadSceneMode.Single);
        
        /// <summary>
        /// Load scene asynchronously by name / Загрузить сцену асинхронно по имени
        /// </summary>
        UniTask LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single);
        
        /// <summary>
        /// Load scene through Addressables / Загрузить сцену через Addressables
        /// </summary>
        UniTask LoadSceneAddressableAsync(string sceneKey, LoadSceneMode mode = LoadSceneMode.Single);
        
        /// <summary>
        /// Unload scene asynchronously / Выгрузить сцену асинхронно
        /// </summary>
        UniTask UnloadSceneAsync(string sceneName);
        
        /// <summary>
        /// Reload current scene / Перезагрузить текущую сцену
        /// </summary>
        UniTask ReloadCurrentSceneAsync();
    }
}
