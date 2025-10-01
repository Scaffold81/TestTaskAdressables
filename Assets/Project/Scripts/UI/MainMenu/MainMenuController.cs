using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using R3;
using System;
using Project.Core.Services.Addressable;
using Project.Core.Services.Addressable.Memory;
using Project.Core.Services.Loading;
using Game.Services;
using Game.Enums;

namespace Project.UI.MainMenu
{
    /// <summary>
    /// Main menu controller for Addressables demo
    /// Контроллер главного меню для демо Addressables
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Control Buttons")]
        [SerializeField] private Button downloadCoreButton;
        [SerializeField] private Button downloadLevelsButton;
        [SerializeField] private Button clearCacheButton;
        [SerializeField] private Button showDevOverlayButton;
        [SerializeField] private Button testLoadingButton;
        
        [Header("Info Display")]
        [SerializeField] private TextMeshProUGUI catalogVersionText;
        [SerializeField] private TextMeshProUGUI profileText;
        [SerializeField] private TextMeshProUGUI cacheSizeText;
        
        [Header("Progress")]
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI statusText;
        
        [Header("Dev Overlay")]
        [SerializeField] private GameObject devOverlayPanel;
        
        [Inject] private IAddressableService _addressableService;
        [Inject] private IAddressableMemoryManager _memoryManager;
        [Inject] private ILoadingService _loadingService;
        [Inject] private ISceneManagerService _sceneManager;
        
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private bool _isLoading = false;
        
        private void Start()
        {
            SetupButtons();
            SetupProgressObservers();
            RefreshInfo();
            
            if (devOverlayPanel != null)
                devOverlayPanel.SetActive(false);
        }
        
        private void SetupButtons()
        {
            if (downloadCoreButton != null)
                downloadCoreButton.onClick.AddListener(() => DownloadCoreAsync().Forget());
            
            if (downloadLevelsButton != null)
                downloadLevelsButton.onClick.AddListener(() => DownloadLevelsAsync().Forget());
            
            if (clearCacheButton != null)
                clearCacheButton.onClick.AddListener(ClearCache);
            
            if (showDevOverlayButton != null)
                showDevOverlayButton.onClick.AddListener(ToggleDevOverlay);
            
            if (testLoadingButton != null)
                testLoadingButton.onClick.AddListener(() => TestLoadingAsync().Forget());
        }
        
        private void SetupProgressObservers()
        {
            if (_loadingService != null)
            {
                _loadingService.IsLoading
                    .Subscribe(isLoading =>
                    {
                        _isLoading = isLoading;
                        UpdateButtonsInteractability();
                    })
                    .AddTo(_disposables);
            }
        }
        
        private async UniTask DownloadCoreAsync()
        {
            if (_isLoading) return;
            
            try
            {
                UpdateStatus("Downloading Core assets...");
                UpdateProgress(0f, "Downloading Core");
                
                // Используем реальные ключи из Core_Local
                var coreKeys = new[] { "ui_main_button", "explosion_particle", "characters_test_prefab" };
                var size = await _addressableService.GetDownloadSizeAsync(coreKeys);
                
                UpdateStatus($"Core size: {size / (1024f * 1024f):F2} MB");
                
                var progress = new Progress<float>(p => UpdateProgress(p, "Downloading Core"));
                await _addressableService.DownloadDependenciesAsync(coreKeys, progress);
                
                UpdateStatus("Core assets downloaded!");
                UpdateProgress(1f, "Complete");
                RefreshCacheSize();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Download failed: {ex.Message}");
            }
        }
        
        private async UniTask DownloadLevelsAsync()
        {
            if (_isLoading) return;
            
            try
            {
                UpdateStatus("Downloading Levels...");
                UpdateProgress(0f, "Downloading Levels");
                
                var levelKeys = new[] { "levels_level01_scene", "levels_level02_scene" };
                var progress = new Progress<float>(p => UpdateProgress(p, "Downloading Levels"));
                
                await _addressableService.DownloadDependenciesAsync(levelKeys, progress);
                
                UpdateStatus("Levels downloaded!");
                UpdateProgress(1f, "Complete");
                RefreshCacheSize();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Download failed: {ex.Message}");
            }
        }
        
        private void ClearCache()
        {
            try
            {
                UpdateStatus("Clearing cache...");
                
                Caching.ClearCache();
                _addressableService?.ReleaseAllAssets();
                
                if (_memoryManager != null)
                    _ = _memoryManager.CleanupMemoryAsync();
                
                UpdateStatus("Cache cleared!");
                UpdateProgress(0f, "Ready");
                RefreshCacheSize();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Clear failed: {ex.Message}");
            }
        }
        
        private async UniTask TestLoadingAsync()
        {
            if (_isLoading) return;
            
            try
            {
                UpdateStatus("Testing Loading scene...");
                
                // Устанавливаем целевую сцену - вернуться в MainMenu
                _sceneManager.TargetSceneId = SceneId.MainMenu;
                
                // Переходим на Loading сцену
                await _sceneManager.LoadSceneAsync(SceneId.Loading);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Failed: {ex.Message}");
            }
        }
        
        private void ToggleDevOverlay()
        {
            if (devOverlayPanel != null)
                devOverlayPanel.SetActive(!devOverlayPanel.activeSelf);
        }
        
        private void RefreshInfo()
        {
            if (catalogVersionText != null)
                catalogVersionText.text = "Catalog: v1.0.0";
            
            if (profileText != null)
            {
                #if UNITY_EDITOR
                profileText.text = "Profile: Development";
                #elif DEVELOPMENT_BUILD
                profileText.text = "Profile: Staging";
                #else
                profileText.text = "Profile: Production";
                #endif
            }
            
            RefreshCacheSize();
        }
        
        private void RefreshCacheSize()
        {
            if (cacheSizeText != null)
            {
                long cacheSize = 0;
                
                try
                {
                    var defaultCache = Caching.defaultCache;
                    if (defaultCache.valid)
                        cacheSize = defaultCache.spaceOccupied;
                }
                catch (Exception)
                {
                    cacheSize = 0;
                }
                
                float cacheSizeMB = cacheSize / (1024f * 1024f);
                cacheSizeText.text = $"Cache: {cacheSizeMB:F2} MB";
            }
        }
        
        private void UpdateProgress(float progress, string operation)
        {
            if (progressBar != null)
                progressBar.value = progress;
            
            if (statusText != null)
                statusText.text = $"{operation}: {progress * 100:F0}%";
        }
        
        private void UpdateStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }
        
        private void UpdateButtonsInteractability()
        {
            bool canInteract = !_isLoading;
            
            if (downloadCoreButton != null)
                downloadCoreButton.interactable = canInteract;
            
            if (downloadLevelsButton != null)
                downloadLevelsButton.interactable = canInteract;
            
            if (testLoadingButton != null)
                testLoadingButton.interactable = canInteract;
        }
        
        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}
