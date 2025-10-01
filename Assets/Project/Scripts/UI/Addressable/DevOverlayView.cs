using UnityEngine;
using UnityEngine.UI;
using TMPro;
using R3;
using Zenject;
using Project.Core.Services.Addressable;
using Project.Core.Services.Addressable.Memory;
using Project.Core.Services.Loading;

namespace Project.UI.Addressable
{
    /// <summary>
    /// Developer overlay panel for debugging Addressable system
    /// Панель разработчика для отладки системы Addressables
    /// </summary>
    public class DevOverlayView : MonoBehaviour
    {
        [Header("Control Buttons")]
        [SerializeField] private Button clearCacheButton;
        [SerializeField] private Button switchProfileButton;
        [SerializeField] private Button showMemoryButton;
        [SerializeField] private Button downloadAllButton;
        [SerializeField] private Button toggleVisibilityButton;
        
        [Header("Info Display")]
        [SerializeField] private TextMeshProUGUI profileText;
        [SerializeField] private TextMeshProUGUI catalogText;
        [SerializeField] private TextMeshProUGUI assetsText;
        [SerializeField] private TextMeshProUGUI memoryText;
        [SerializeField] private TextMeshProUGUI statusText;
        
        [Header("Settings")]
        [SerializeField] private float autoRefreshInterval = 2f;
        [SerializeField] private bool _autoRefreshEnabled = true;
        
        // Injected services
        [Inject] private IAddressableService _addressableService;
        [Inject] private IAddressableMemoryManager _memoryManager;
        [Inject] private ILoadingService _loadingService;
        
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private int _currentProfileIndex = 0;
        private readonly string[] _profiles = { "Default", "Development", "Staging", "Production" };
        
        private void Start()
        {
            SetupButtons();
            SetupAutoRefresh();
            RefreshInfo();
            
            // Hide by default in builds
            #if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            gameObject.SetActive(false);
            #endif
        }
        
        private void SetupButtons()
        {
            if (clearCacheButton != null)
            {
                clearCacheButton.onClick.AddListener(ClearCache);
            }
            
            if (switchProfileButton != null)
            {
                switchProfileButton.onClick.AddListener(SwitchProfile);
            }
            
            if (showMemoryButton != null)
            {
                showMemoryButton.onClick.AddListener(ShowMemoryInfo);
            }
            
            if (downloadAllButton != null)
            {
                downloadAllButton.onClick.AddListener(DownloadAllGroups);
            }
            
            if (toggleVisibilityButton != null)
            {
                toggleVisibilityButton.onClick.AddListener(ToggleVisibility);
            }
        }
        
        private void SetupAutoRefresh()
        {
            Observable.Interval(System.TimeSpan.FromSeconds(autoRefreshInterval))
                .Where(_ => _autoRefreshEnabled && gameObject.activeInHierarchy)
                .Subscribe(_ => RefreshInfo())
                .AddTo(_disposables);
        }
        
        private void RefreshInfo()
        {
            RefreshProfileInfo();
            RefreshMemoryInfo();
            RefreshAssetsInfo();
            RefreshStatusInfo();
        }
        
        private void RefreshProfileInfo()
        {
            if (profileText != null)
            {
                var currentProfile = _profiles[_currentProfileIndex];
                profileText.text = $"Profile: {currentProfile}";
            }
            
            if (catalogText != null)
            {
                catalogText.text = "Catalog: v1.0.0";
            }
        }
        
        private void RefreshMemoryInfo()
        {
            if (memoryText != null && _memoryManager != null)
            {
                var stats = _memoryManager.GetMemoryStats();
                var currentMemory = stats.UsedMemoryBytes;
                var memoryLimit = stats.TotalMemoryBytes;
                var memoryMB = currentMemory / (1024f * 1024f);
                var limitMB = memoryLimit / (1024f * 1024f);
                
                memoryText.text = $"Memory: {memoryMB:F1}/{limitMB:F1} MB";
            }
        }
        
        private void RefreshAssetsInfo()
        {
            if (assetsText != null)
            {
                var loadedCount = _addressableService?.LoadedAssets?.Count ?? 0;
                var trackedCount = _memoryManager?.GetMemoryStats().TrackedAssetsCount ?? 0;
                
                assetsText.text = $"Assets: {loadedCount} loaded, {trackedCount} tracked";
            }
        }
        
        private void RefreshStatusInfo()
        {
            if (statusText != null && _loadingService != null)
            {
                var isLoading = _loadingService.IsLoading?.CurrentValue ?? false;
                var status = isLoading ? "Loading..." : "Ready";
                
                statusText.text = $"Status: {status}";
            }
        }
        
        private void ClearCache()
        {
            try
            {
                UnityEngine.Caching.ClearCache();
                
                if (_memoryManager != null)
                {
                    _ = _memoryManager.CleanupMemoryAsync();
                }
                
                Debug.Log("[DevOverlay] Cache cleared successfully");
                UpdateStatus("Cache cleared");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[DevOverlay] Failed to clear cache: {ex.Message}");
                UpdateStatus("Cache clear failed");
            }
        }
        
        private void SwitchProfile()
        {
            _currentProfileIndex = (_currentProfileIndex + 1) % _profiles.Length;
            var newProfile = _profiles[_currentProfileIndex];
            
            Debug.Log($"[DevOverlay] Switched to profile: {newProfile}");
            UpdateStatus($"Profile: {newProfile}");
            
            RefreshInfo();
        }
        
        private void ShowMemoryInfo()
        {
            if (_memoryManager != null)
            {
                var stats = _memoryManager.GetMemoryStats();
                var report = GenerateMemoryReport(stats);
                
                Debug.Log($"[DevOverlay] Memory Report:\n{report}");
                UpdateStatus("Memory info logged");
            }
        }
        
        private async void DownloadAllGroups()
        {
            if (_addressableService == null || _loadingService == null)
            {
                UpdateStatus("Services unavailable");
                return;
            }
            
            try
            {
                UpdateStatus("Downloading...");
                
                // Simulate download of all groups
                var testKeys = new[] { "ui_test", "character_test", "effect_test" };
                await _addressableService.DownloadDependenciesAsync(testKeys);
                
                UpdateStatus("Download complete");
                Debug.Log("[DevOverlay] Download all groups completed");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[DevOverlay] Download failed: {ex.Message}");
                UpdateStatus("Download failed");
            }
        }
        
        private void ToggleVisibility()
        {
            var newVisibility = !gameObject.activeInHierarchy;
            gameObject.SetActive(newVisibility);
            
            if (newVisibility)
            {
                RefreshInfo();
            }
        }
        
        private void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = $"Status: {message}";
            }
        }
        
        private string GenerateMemoryReport(MemoryStats stats)
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== MEMORY REPORT ===");
            report.AppendLine($"Addressable Memory: {stats.UsedMemoryBytes / (1024f * 1024f):F1} MB");
            report.AppendLine($"Tracked Assets: {stats.TrackedAssetsCount}");
            report.AppendLine($"Memory Pressure: {stats.MemoryPressure:P1}");
            
            return report.ToString();
        }
        
        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}