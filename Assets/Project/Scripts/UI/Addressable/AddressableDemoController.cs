using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using Cysharp.Threading.Tasks;
using Project.Core.Services.Addressable;
using Project.Core.Services.Loading;

namespace Project.UI.Addressable
{
    /// <summary>
    /// Demo controller for testing Addressable functionality
    /// Демо контроллер для тестирования функциональности Addressables
    /// </summary>
    public class AddressableDemoController : MonoBehaviour
    {
        [Header("UI Controls")]
        [SerializeField] private Button loadSpriteButton;
        [SerializeField] private Button loadPrefabButton;
        [SerializeField] private Button loadSceneButton;
        [SerializeField] private Button clearCacheButton;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Image testImage;
        [SerializeField] private Transform spawnPoint;
        
        [Inject] private IAddressableService _addressableService;
        [Inject] private ILoadingService _loadingService;
        
        private void Start()
        {
            SetupButtons();
            UpdateStatus("Demo ready");
        }
        
        private void SetupButtons()
        {
            if (loadSpriteButton != null)
                loadSpriteButton.onClick.AddListener(() => LoadSprite());
                
            if (loadPrefabButton != null)  
                loadPrefabButton.onClick.AddListener(() => LoadPrefab());
                
            if (loadSceneButton != null)
                loadSceneButton.onClick.AddListener(() => LoadScene());
                
            if (clearCacheButton != null)
                clearCacheButton.onClick.AddListener(ClearCache);
        }
        
        private async void LoadSprite()
        {
            try
            {
                UpdateStatus("Loading sprite...");
                
                var sprite = await _addressableService.LoadAssetAsync<Sprite>("ui_main_button");
                
                if (testImage != null && sprite != null)
                {
                    testImage.sprite = sprite;
                    UpdateStatus("Sprite loaded successfully");
                }
                else
                {
                    UpdateStatus("Failed to load sprite");
                }
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"Error: {ex.Message}");
                Debug.LogError($"[AddressableDemo] Load sprite failed: {ex.Message}");
            }
        }
        
        private async void LoadPrefab()
        {
            try
            {
                UpdateStatus("Loading prefab...");
                
                var prefab = await _addressableService.LoadAssetAsync<GameObject>("characters_test_prefab");
                
                if (prefab != null && spawnPoint != null)
                {
                    var instance = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
                    UpdateStatus("Prefab spawned successfully");
                }
                else
                {
                    UpdateStatus("Failed to load prefab");
                }
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"Error: {ex.Message}");
                Debug.LogError($"[AddressableDemo] Load prefab failed: {ex.Message}");
            }
        }
        
        private async void LoadScene()
        {
            try
            {
                UpdateStatus("Loading scene...");
                
                await _addressableService.LoadSceneAsync("levels_test_scene");
                
                UpdateStatus("Scene loaded successfully");
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"Error: {ex.Message}");
                Debug.LogError($"[AddressableDemo] Load scene failed: {ex.Message}");
            }
        }
        
        private void ClearCache()
        {
            try
            {
                UnityEngine.Caching.ClearCache();
                _addressableService.ReleaseAllAssets();
                
                UpdateStatus("Cache cleared");
                Debug.Log("[AddressableDemo] Cache cleared successfully");
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"Clear failed: {ex.Message}");
                Debug.LogError($"[AddressableDemo] Clear cache failed: {ex.Message}");
            }
        }
        
        private void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = $"Status: {message}";
            }
            
            Debug.Log($"[AddressableDemo] {message}");
        }
    }
}
