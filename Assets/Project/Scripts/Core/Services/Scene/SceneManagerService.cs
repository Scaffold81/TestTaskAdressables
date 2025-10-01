using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Project.Core.Services.Addressable;
using R3;
using System;
using Zenject;
using Game.Enums;

namespace Game.Services
{
    /// <summary>
    /// Implementation of scene manager service with Addressables integration
    /// Реализация сервиса управления сценами с интеграцией Addressables
    /// </summary>
    public class SceneManagerService : ISceneManagerService, IDisposable
    {
        private readonly IAddressableService _addressableService;
        
        private readonly Subject<float> _sceneLoadProgressSubject = new Subject<float>();
        private readonly Subject<string> _sceneLoadedSubject = new Subject<string>();
        
        public SceneId TargetSceneId { get; set; } = SceneId.MainMenu;
        public string CurrentSceneName => SceneManager.GetActiveScene().name;
        
        public Observable<float> OnSceneLoadProgress => _sceneLoadProgressSubject.AsObservable();
        public Observable<string> OnSceneLoaded => _sceneLoadedSubject.AsObservable();
        
        [Inject]
        public SceneManagerService(IAddressableService addressableService)
        {
            _addressableService = addressableService;
        }
        
        /// <summary>
        /// Load scene by SceneId enum using scene NAME
        /// Загрузить сцену по SceneId enum используя ИМЯ сцены
        /// </summary>
        public async UniTask LoadSceneAsync(SceneId sceneId, LoadSceneMode mode = LoadSceneMode.Single)
        {
            // Convert enum to scene name
            string sceneName = sceneId.ToString();
            
            Debug.Log($"[SceneManagerService] Loading scene: {sceneId} (name: {sceneName})");
            
            // Release assets from previous scene if loading in Single mode
            if (mode == LoadSceneMode.Single)
            {
                _addressableService.ReleaseAllAssets();
            }
            
            var operation = SceneManager.LoadSceneAsync(sceneName, mode);
            
            if (operation == null)
            {
                Debug.LogError($"[SceneManagerService] Failed to start loading scene: {sceneName}");
                return;
            }
            
            // Wait for scene to load
            while (!operation.isDone)
            {
                // Unity's progress goes to 0.9, then jumps to isDone
                // Normalize to 0-1 range
                float progress = Mathf.Clamp01(operation.progress / 0.9f);
                _sceneLoadProgressSubject.OnNext(progress);
                await UniTask.Yield();
            }
            
            // Ensure 100% progress
            _sceneLoadProgressSubject.OnNext(1f);
            
            // Notify scene loaded
            string loadedSceneName = SceneManager.GetActiveScene().name;
            _sceneLoadedSubject.OnNext(loadedSceneName);
            
            Debug.Log($"[SceneManagerService] Scene {sceneId} ({loadedSceneName}) loaded successfully");
        }
        
        /// <summary>
        /// Load scene by name / Загрузить сцену по имени
        /// </summary>
        public async UniTask LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[SceneManagerService] Scene name cannot be null or empty");
                return;
            }
            
            Debug.Log($"[SceneManagerService] Loading scene by name: {sceneName}");
            
            // Release assets from previous scene if loading in Single mode
            if (mode == LoadSceneMode.Single)
            {
                _addressableService.ReleaseAllAssets();
            }
            
            var operation = SceneManager.LoadSceneAsync(sceneName, mode);
            
            if (operation == null)
            {
                Debug.LogError($"[SceneManagerService] Failed to start loading scene: {sceneName}");
                return;
            }
            
            // Wait for scene to load
            while (!operation.isDone)
            {
                // Normalize progress to 0-1
                float progress = Mathf.Clamp01(operation.progress / 0.9f);
                _sceneLoadProgressSubject.OnNext(progress);
                await UniTask.Yield();
            }
            
            // Ensure 100% progress
            _sceneLoadProgressSubject.OnNext(1f);
            
            // Notify scene loaded
            _sceneLoadedSubject.OnNext(sceneName);
            
            Debug.Log($"[SceneManagerService] Scene {sceneName} loaded successfully");
        }
        
        /// <summary>
        /// Load scene through Addressables / Загрузить сцену через Addressables
        /// </summary>
        public async UniTask LoadSceneAddressableAsync(string sceneKey, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (string.IsNullOrEmpty(sceneKey))
            {
                Debug.LogError("[SceneManagerService] Scene key cannot be null or empty");
                return;
            }
            
            Debug.Log($"[SceneManagerService] Loading Addressable scene: {sceneKey}");
            
            try
            {
                // Release assets from previous scene if loading in Single mode
                if (mode == LoadSceneMode.Single)
                {
                    _addressableService.ReleaseAllAssets();
                }
                
                // Load scene through Addressables
                await _addressableService.LoadSceneAsync(sceneKey);
                
                // Notify 100% progress
                _sceneLoadProgressSubject.OnNext(1f);
                
                // Notify scene loaded
                _sceneLoadedSubject.OnNext(sceneKey);
                
                Debug.Log($"[SceneManagerService] Addressable scene {sceneKey} loaded successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SceneManagerService] Failed to load Addressable scene {sceneKey}: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Unload scene / Выгрузить сцену
        /// </summary>
        public async UniTask UnloadSceneAsync(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[SceneManagerService] Scene name cannot be null or empty");
                return;
            }
            
            Debug.Log($"[SceneManagerService] Unloading scene: {sceneName}");
            
            var operation = SceneManager.UnloadSceneAsync(sceneName);
            
            if (operation == null)
            {
                Debug.LogWarning($"[SceneManagerService] Scene {sceneName} is not loaded or cannot be unloaded");
                return;
            }
            
            while (!operation.isDone)
            {
                await UniTask.Yield();
            }
            
            // Release addressable assets for unloaded scene
            _addressableService.ReleaseAllAssets();
            
            // Force garbage collection
            await Resources.UnloadUnusedAssets();
            
            Debug.Log($"[SceneManagerService] Scene {sceneName} unloaded successfully");
        }
        
        /// <summary>
        /// Reload current scene / Перезагрузить текущую сцену
        /// </summary>
        public async UniTask ReloadCurrentSceneAsync()
        {
            var currentScene = SceneManager.GetActiveScene();
            Debug.Log($"[SceneManagerService] Reloading current scene: {currentScene.name}");
            
            await LoadSceneAsync(currentScene.name, LoadSceneMode.Single);
        }
        
        /// <summary>
        /// Dispose subjects / Освободить Subject'ы
        /// </summary>
        public void Dispose()
        {
            _sceneLoadProgressSubject?.Dispose();
            _sceneLoadedSubject?.Dispose();
        }
    }
}
