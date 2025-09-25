using UnityEngine;
using Zenject;
using Project.Core.Services.Addressable;
using Project.Core.Config.Addressable;

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
    }
}