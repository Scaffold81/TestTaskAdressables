using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using R3;
using Project.Core.Services.Loading;
using Project.UI.Addressable;

namespace Project.UI.Loading
{
    /// <summary>
    /// Loading screen controller with progress indicator integration
    /// Контроллер экрана загрузки с интеграцией индикатора прогресса
    /// </summary>
    public class LoadingScreenController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LoadingProgressView progressView;
        
        [Header("Settings")]
        [SerializeField] private bool autoShowOnLoading = true;
        [SerializeField] private bool autoHideOnComplete = true;
        [SerializeField] private float autoHideDelay = 0.5f;
        
        [Inject] private ILoadingService _loadingService;
        
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        
        private void Start()
        {
            if (progressView == null)
            {
                Debug.LogError("[LoadingScreen] LoadingProgressView not assigned!");
                return;
            }
            
            if (_loadingService == null)
            {
                Debug.LogError("[LoadingScreen] ILoadingService not injected!");
                return;
            }
            
            SetupObservers();
            
            // Hide initially
            progressView.Hide();
        }
        
        private void SetupObservers()
        {
            // Subscribe to loading state changes
            _loadingService.IsLoading
                .Subscribe(isLoading =>
                {
                    if (isLoading && autoShowOnLoading)
                    {
                        ShowLoadingScreen();
                    }
                    else if (!isLoading && autoHideOnComplete)
                    {
                        HideLoadingScreenDelayed().Forget();
                    }
                })
                .AddTo(_disposables);
            
            // Subscribe to progress updates via LoadingProgress property
            _loadingService.LoadingProgress
                .Subscribe(progress =>
                {
                    UpdateProgressDisplay(progress);
                })
                .AddTo(_disposables);
            
            // Subscribe to title updates via LoadingTitle property
            _loadingService.LoadingTitle
                .Subscribe(title =>
                {
                    if (!string.IsNullOrEmpty(title))
                        UpdateTitle(title);
                })
                .AddTo(_disposables);
            
            // Subscribe to status updates via LoadingStatus property
            _loadingService.LoadingStatus
                .Subscribe(status =>
                {
                    if (!string.IsNullOrEmpty(status))
                        UpdateStatus(status);
                })
                .AddTo(_disposables);
        }
        
        private void ShowLoadingScreen()
        {
            progressView.Show();
        }
        
        private async UniTaskVoid HideLoadingScreenDelayed()
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(autoHideDelay));
            progressView.Hide();
        }
        
        private void UpdateProgressDisplay(float progress)
        {
            // Progress bar updates automatically via LoadingProgressView subscription
            // This method is kept for potential custom logic
        }
        
        private void UpdateTitle(string title)
        {
            // Title updates automatically via LoadingProgressView subscription
            // This method is kept for potential custom logic
        }
        
        private void UpdateStatus(string status)
        {
            // Status updates automatically via LoadingProgressView subscription
            // This method is kept for potential custom logic
        }
        
        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
        
        // Public API for manual control
        
        /// <summary>
        /// Manually show loading screen / Вручную показать экран загрузки
        /// </summary>
        public void Show()
        {
            progressView.Show();
        }
        
        /// <summary>
        /// Manually hide loading screen / Вручную скрыть экран загрузки
        /// </summary>
        public void Hide()
        {
            progressView.Hide();
        }
        
        /// <summary>
        /// Set manual loading state for testing / Установить ручное состояние для тестирования
        /// </summary>
        public void SetManualState(string title, string status, float progress)
        {
            progressView.SetManualLoadingState(title, status, progress);
        }
    }
}
