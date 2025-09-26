using UnityEngine;
using Zenject;
using Project.Core.Services.Addressable;
using Project.Core.Config.Addressable;
using Project.Core.Services.Loading;
using Project.Core.Services.Extensions;
using Cysharp.Threading.Tasks;

namespace Project.Testing
{
    /// <summary>
    /// Simple test script to verify Addressable service integration
    /// Простой тестовый скрипт для проверки интеграции сервиса Addressables
    /// </summary>
    public class AddressableServiceTest : MonoBehaviour
    {
        [Inject] private IAddressableService _addressableService;
        [Inject] private IAddressableConfigRepository _configRepository;
        [Inject] private ILoadingService _loadingService;
        
        private void Start()
        {
            TestServices();
        }
        
        /// <summary>
        /// Test if services are properly injected / Тест правильной инъекции сервисов
        /// </summary>
        private void TestServices()
        {
            if (_addressableService != null)
            {
                Debug.Log("[AddressableServiceTest] ✅ AddressableService injected successfully");
                Debug.Log($"[AddressableServiceTest] Service initialized: {_addressableService.IsInitialized}");
            }
            else
            {
                Debug.LogError("[AddressableServiceTest] ❌ AddressableService is null");
            }
            
            if (_configRepository != null)
            {
                Debug.Log("[AddressableServiceTest] ✅ ConfigRepository injected successfully");
                var settings = _configRepository.GetSettings();
                Debug.Log($"[AddressableServiceTest] Default Profile: {settings.DefaultProfile}");
            }
            else
            {
                Debug.LogError("[AddressableServiceTest] ❌ ConfigRepository is null");
            }
            
            if (_loadingService != null)
            {
                Debug.Log("[AddressableServiceTest] ✅ LoadingService injected successfully");
                Debug.Log($"[AddressableServiceTest] Loading service active: {_loadingService.IsLoading}");
            }
            else
            {
                Debug.LogError("[AddressableServiceTest] ❌ LoadingService is null");
            }
        }
        
        /// <summary>
        /// Test button for manual testing / Кнопка теста для ручного тестирования
        /// </summary>
        [ContextMenu("Test Addressable Service")]
        private void TestAddressableService()
        {
            if (_addressableService == null)
            {
                Debug.LogError("[AddressableServiceTest] Service not available");
                return;
            }
            
            Debug.Log("[AddressableServiceTest] Testing service functionality...");
            
            // Test basic functionality
            var loadedAssets = _addressableService.LoadedAssets;
            Debug.Log($"[AddressableServiceTest] Currently loaded assets: {loadedAssets.Count}");
            
            // Test config access
            if (_configRepository != null)
            {
                var coreKeys = _configRepository.GetCoreAssetKeys();
                Debug.Log($"[AddressableServiceTest] Core asset keys count: {coreKeys.Length}");
                
                foreach (var key in coreKeys)
                {
                    Debug.Log($"[AddressableServiceTest] Core asset key: {key}");
                }
            }
        }
        
        /// <summary>
        /// Test loading service functionality / Тест функциональности сервиса загрузки
        /// </summary>
        [ContextMenu("Test Loading Service")]
        private async void TestLoadingService()
        {
            if (_loadingService == null)
            {
                Debug.LogError("[AddressableServiceTest] LoadingService not available");
                return;
            }
            
            Debug.Log("[AddressableServiceTest] Testing loading service...");
            
            // Test basic loading UI
            _loadingService.ShowProgress("Test Loading", "Starting test...");
            await UniTask.Delay(1000);
            
            for (int i = 0; i <= 10; i++)
            {
                float progress = i / 10f;
                _loadingService.UpdateProgress(progress, $"Step {i}/10");
                await UniTask.Delay(200);
            }
            
            _loadingService.HideProgress();
            Debug.Log("[AddressableServiceTest] Loading service test completed");
        }
        
        /// <summary>
        /// Test asset loading with progress / Тест загрузки ресурсов с прогрессом
        /// </summary>
        [ContextMenu("Test Asset Loading with Progress")]
        private async void TestAssetLoadingWithProgress()
        {
            if (_addressableService == null || _loadingService == null)
            {
                Debug.LogError("[AddressableServiceTest] Required services not available");
                return;
            }
            
            Debug.Log("[AddressableServiceTest] Testing asset loading with progress...");
            
            try
            {
                // Test download size check
                var coreKeys = _configRepository.GetCoreAssetKeys();
                if (coreKeys.Length > 0)
                {
                    var testKey = coreKeys[0];
                    
                    // Test with loading UI
                    await _addressableService.LoadAssetWithProgressAsync<UnityEngine.Object>(
                        testKey, _loadingService, $"Loading {testKey}");
                    
                    Debug.Log($"[AddressableServiceTest] Successfully loaded {testKey} with progress UI");
                }
                else
                {
                    Debug.LogWarning("[AddressableServiceTest] No core asset keys available for testing");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AddressableServiceTest] Asset loading test failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Test download dependencies with progress / Тест скачивания зависимостей с прогрессом
        /// </summary>
        [ContextMenu("Test Download Dependencies")]
        private async void TestDownloadDependencies()
        {
            if (_addressableService == null || _loadingService == null)
            {
                Debug.LogError("[AddressableServiceTest] Required services not available");
                return;
            }
            
            Debug.Log("[AddressableServiceTest] Testing download dependencies...");
            
            try
            {
                var coreKeys = _configRepository.GetCoreAssetKeys();
                
                if (coreKeys.Length > 0)
                {
                    await _addressableService.DownloadDependenciesWithProgressAsync(
                        coreKeys, _loadingService, "Downloading Core Assets");
                    
                    Debug.Log("[AddressableServiceTest] Dependencies download test completed");
                }
                else
                {
                    Debug.LogWarning("[AddressableServiceTest] No asset keys available for download test");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AddressableServiceTest] Download dependencies test failed: {ex.Message}");
            }
        }
    }
}