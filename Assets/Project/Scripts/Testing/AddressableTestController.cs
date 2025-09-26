using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using Cysharp.Threading.Tasks;
using Project.Core.Services.Addressable;
using Project.Core.Services.Loading;

namespace Project.Testing
{
    /// <summary>
    /// Main test controller for Addressable system demonstration
    /// Основной тестовый контроллер для демонстрации системы Addressables
    /// </summary>
    public class AddressableTestController : MonoBehaviour
    {
        [Header("Test Buttons")]
        [SerializeField] private Button testSpriteButton;
        [SerializeField] private Button testPrefabButton;
        [SerializeField] private Button testSceneButton;
        [SerializeField] private Button testContentUpdateButton;
        [SerializeField] private Button clearCacheButton;
        [SerializeField] private Button showStatsButton;
        
        [Header("UI Display")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private Image testSpriteDisplay;
        [SerializeField] private Transform prefabSpawnPoint;
        
        [Header("Test Configuration")]
        [SerializeField] private string[] testSpriteKeys = { "ui_main_button", "explosion_particle" };
        [SerializeField] private string[] testPrefabKeys = { "characters_test_prefab" };
        [SerializeField] private string[] testSceneKeys = { "levels_test_scene" };
        
        [Inject] private IAddressableService _addressableService;
        [Inject] private ILoadingService _loadingService;
        
        private int _currentTestIndex = 0;
        
        private void Start()
        {
            SetupButtons();
            UpdateStatus("Test Controller Ready");
            ShowInitialStats();
        }
        
        private void SetupButtons()
        {
            if (testSpriteButton != null)
                testSpriteButton.onClick.AddListener(() => TestSpriteLoading());
                
            if (testPrefabButton != null)
                testPrefabButton.onClick.AddListener(() => TestPrefabLoading());
                
            if (testSceneButton != null)
                testSceneButton.onClick.AddListener(() => TestSceneLoading());
                
            if (testContentUpdateButton != null)
                testContentUpdateButton.onClick.AddListener(() => TestContentUpdate());
                
            if (clearCacheButton != null)
                clearCacheButton.onClick.AddListener(ClearCache);
                
            if (showStatsButton != null)
                showStatsButton.onClick.AddListener(ShowStats);
        }
        
        private async void TestSpriteLoading()
        {
            try
            {
                UpdateStatus("Testing sprite loading...");
                
                var spriteKey = testSpriteKeys[_currentTestIndex % testSpriteKeys.Length];
                
                // Get download size first
                var downloadSize = await _addressableService.GetDownloadSizeAsync(new[] { spriteKey });
                UpdateStatus($"Download size: {downloadSize / 1024f:F1} KB");
                
                // Load sprite
                var sprite = await _addressableService.LoadAssetAsync<Sprite>(spriteKey);
                
                if (sprite != null && testSpriteDisplay != null)
                {
                    testSpriteDisplay.sprite = sprite;
                    UpdateStatus($"✓ Sprite '{spriteKey}' loaded successfully");
                }
                else
                {
                    UpdateStatus($"✗ Failed to load sprite '{spriteKey}'");
                }
                
                _currentTestIndex++;
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"✗ Sprite test error: {ex.Message}");
                Debug.LogError($"[AddressableTest] Sprite loading failed: {ex}");
            }
        }
        
        private async void TestPrefabLoading()
        {
            try
            {
                UpdateStatus("Testing prefab loading...");
                
                var prefabKey = testPrefabKeys[0];
                
                // Check if already downloaded
                var downloadSize = await _addressableService.GetDownloadSizeAsync(new[] { prefabKey });
                if (downloadSize > 0)
                {
                    UpdateStatus($"Downloading prefab: {downloadSize / 1024f:F1} KB");
                }
                
                // Load and instantiate prefab
                var prefab = await _addressableService.LoadAssetAsync<GameObject>(prefabKey);
                
                if (prefab != null && prefabSpawnPoint != null)
                {
                    var instance = Instantiate(prefab, prefabSpawnPoint.position, prefabSpawnPoint.rotation);
                    instance.name = $"TestPrefab_{System.DateTime.Now:HHmmss}";
                    
                    UpdateStatus($"✓ Prefab '{prefabKey}' spawned successfully");
                }
                else
                {
                    UpdateStatus($"✗ Failed to load prefab '{prefabKey}'");
                }
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"✗ Prefab test error: {ex.Message}");
                Debug.LogError($"[AddressableTest] Prefab loading failed: {ex}");
            }
        }
        
        private async void TestSceneLoading()
        {
            try
            {
                UpdateStatus("Testing scene loading...");
                
                var sceneKey = testSceneKeys[0];
                
                // Check download size
                var downloadSize = await _addressableService.GetDownloadSizeAsync(new[] { sceneKey });
                if (downloadSize > 0)
                {
                    UpdateStatus($"Scene size: {downloadSize / 1024f:F1} KB");
                }
                
                // Load scene additively for testing
                await _addressableService.LoadSceneAsync(sceneKey, UnityEngine.SceneManagement.LoadSceneMode.Additive);
                
                UpdateStatus($"✓ Scene '{sceneKey}' loaded successfully");
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"✗ Scene test error: {ex.Message}");
                Debug.LogError($"[AddressableTest] Scene loading failed: {ex}");
            }
        }
        
        private async void TestContentUpdate()
        {
            try
            {
                UpdateStatus("Testing content update...");
                
                // Simulate content update check
                await UniTask.Delay(1000);
                
                // Check for catalog updates
                var updateSize = await _addressableService.GetDownloadSizeAsync(testSpriteKeys);
                
                if (updateSize > 0)
                {
                    UpdateStatus($"Update available: {updateSize / 1024f:F1} KB");
                    
                    // Download updates
                    await _addressableService.DownloadDependenciesAsync(testSpriteKeys);
                    UpdateStatus("✓ Content update completed");
                }
                else
                {
                    UpdateStatus("✓ Content is up to date");
                }
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"✗ Content update error: {ex.Message}");
                Debug.LogError($"[AddressableTest] Content update failed: {ex}");
            }
        }
        
        private void ClearCache()
        {
            try
            {
                // Clear Unity cache
                UnityEngine.Caching.ClearCache();
                
                // Release all addressable assets
                _addressableService?.ReleaseAllAssets();
                
                // Clear UI
                if (testSpriteDisplay != null)
                {
                    testSpriteDisplay.sprite = null;
                }
                
                // Clear spawned objects
                if (prefabSpawnPoint != null)
                {
                    for (int i = prefabSpawnPoint.childCount - 1; i >= 0; i--)
                    {
                        DestroyImmediate(prefabSpawnPoint.GetChild(i).gameObject);
                    }
                }
                
                UpdateStatus("✓ Cache cleared and assets released");
                ShowStats();
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"✗ Clear cache error: {ex.Message}");
                Debug.LogError($"[AddressableTest] Clear cache failed: {ex}");
            }
        }
        
        private void ShowStats()
        {
            try
            {
                var stats = new System.Text.StringBuilder();
                stats.AppendLine("=== ADDRESSABLE STATS ===");
                
                // Memory info
                var allocatedMemory = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
                stats.AppendLine($"Allocated Memory: {allocatedMemory / (1024f * 1024f):F1} MB");
                
                // Cache info
                stats.AppendLine($"Cache Count: {UnityEngine.Caching.cacheCount}");
                
                // Loaded assets
                if (_addressableService?.LoadedAssets != null)
                {
                    stats.AppendLine($"Loaded Assets: {_addressableService.LoadedAssets.Count}");
                }
                
                // System info
                stats.AppendLine($"Platform: {Application.platform}");
                stats.AppendLine($"Internet: {Application.internetReachability}");
                
                if (statsText != null)
                {
                    statsText.text = stats.ToString();
                }
                
                Debug.Log($"[AddressableTest] Stats:\n{stats}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AddressableTest] Show stats failed: {ex}");
            }
        }
        
        private void ShowInitialStats()
        {
            ShowStats();
        }
        
        private void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = $"Status: {message}";
            }
            
            Debug.Log($"[AddressableTest] {message}");
        }
        
        // Automated test sequence for CI/CD
        [ContextMenu("Run Automated Test Sequence")]
        public async void RunAutomatedTestSequence()
        {
            Debug.Log("[AddressableTest] Starting automated test sequence...");
            
            UpdateStatus("Running automated tests...");
            
            try
            {
                // Test 1: Load sprite
                await TestSpriteLoadingAutomated();
                await UniTask.Delay(1000);
                
                // Test 2: Load prefab
                await TestPrefabLoadingAutomated();
                await UniTask.Delay(1000);
                
                // Test 3: Content update
                await TestContentUpdateAutomated();
                await UniTask.Delay(1000);
                
                // Test 4: Memory cleanup
                ClearCache();
                
                UpdateStatus("✓ All automated tests completed successfully");
                Debug.Log("[AddressableTest] Automated test sequence completed successfully");
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"✗ Automated test failed: {ex.Message}");
                Debug.LogError($"[AddressableTest] Automated test sequence failed: {ex}");
            }
        }
        
        private async UniTask TestSpriteLoadingAutomated()
        {
            var sprite = await _addressableService.LoadAssetAsync<Sprite>(testSpriteKeys[0]);
            if (sprite == null) throw new System.Exception("Sprite loading failed");
        }
        
        private async UniTask TestPrefabLoadingAutomated()
        {
            var prefab = await _addressableService.LoadAssetAsync<GameObject>(testPrefabKeys[0]);
            if (prefab == null) throw new System.Exception("Prefab loading failed");
        }
        
        private async UniTask TestContentUpdateAutomated()
        {
            var updateSize = await _addressableService.GetDownloadSizeAsync(testSpriteKeys);
            Debug.Log($"[AddressableTest] Content update size: {updateSize} bytes");
        }
    }
}
