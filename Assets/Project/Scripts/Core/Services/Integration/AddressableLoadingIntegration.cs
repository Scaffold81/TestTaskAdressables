using System;
using UnityEngine;
using R3;
using Project.Core.Services.Addressable;
using Project.Core.Services.Loading;

namespace Project.Core.Services.Integration
{
    /// <summary>
    /// Manager for integrating Addressable events with Loading service
    /// Менеджер для интеграции событий Addressables с сервисом Loading
    /// </summary>
    public class AddressableLoadingIntegration : IDisposable
    {
        private readonly IAddressableService _addressableService;
        private readonly ILoadingService _loadingService;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        
        /// <summary>
        /// Constructor / Конструктор
        /// </summary>
        public AddressableLoadingIntegration(IAddressableService addressableService, ILoadingService loadingService)
        {
            _addressableService = addressableService ?? throw new ArgumentNullException(nameof(addressableService));
            _loadingService = loadingService ?? throw new ArgumentNullException(nameof(loadingService));
            
            SetupEventIntegration();
        }
        
        /// <summary>
        /// Setup event integration between services / Настроить интеграцию событий между сервисами
        /// </summary>
        private void SetupEventIntegration()
        {
            // Subscribe to Addressable progress updates
            _addressableService.OnProgressUpdated
                .Subscribe(progress =>
                {
                    if (_loadingService.IsLoading)
                    {
                        _loadingService.UpdateProgress(progress, "Loading assets...");
                    }
                })
                .AddTo(_disposables);
            
            // Subscribe to asset loaded events for telemetry
            _addressableService.OnAssetLoaded
                .Subscribe(data =>
                {
                    Debug.Log($"[AddressableLoadingIntegration] Asset loaded: {data.key} in {data.time:F2}s, size: {data.size / 1024f:F1} KB");
                })
                .AddTo(_disposables);
                
            // Subscribe to loading state changes
            _loadingService.OnLoadingStateChanged
                .Subscribe(isLoading =>
                {
                    Debug.Log($"[AddressableLoadingIntegration] Loading state changed: {isLoading}");
                })
                .AddTo(_disposables);
                
            Debug.Log("[AddressableLoadingIntegration] Event integration setup completed");
        }
        
        /// <summary>
        /// Dispose resources / Освободить ресурсы
        /// </summary>
        public void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}