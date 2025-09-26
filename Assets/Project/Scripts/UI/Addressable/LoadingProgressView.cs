using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using R3;
using Zenject;
using Project.Core.Services.Loading;
using Game.UI;

namespace Project.UI.Addressable
{
    /// <summary>
    /// Loading progress screen with progress bar and status text
    /// Экран загрузки с прогресс-баром и текстом статуса
    /// </summary>
    public class LoadingProgressView : PageBase
    {
        [Header("UI Components")]
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI percentageText;
        [SerializeField] private GameObject loadingSpinner;
        
        [Header("Animation Settings")]
        [SerializeField] private float progressSmoothSpeed = 5f;
        [SerializeField] private float spinnerSpeed = 360f;
        
        private ILoadingService _loadingService;
        private CompositeDisposable _disposables = new CompositeDisposable();
        private float _targetProgress;
        
        [Inject]
        public void Construct(ILoadingService loadingService)
        {
            _loadingService = loadingService;
        }
        
        protected void Start()
        {
            // Initialize as hidden
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        
        /// <summary>
        /// Show loading view and subscribe to events
        /// Показать экран загрузки и подписаться на события
        /// </summary>
        public override void Show(float showTime = 0.1f)
        {
            base.Show(showTime);
            SubscribeToLoadingEvents();
            UpdateProgressBarSmooth();
        }
        
        /// <summary>
        /// Hide loading view and cleanup subscriptions
        /// Скрыть экран загрузки и очистить подписки
        /// </summary>
        public override void Hide(float hideTime = 0.1f)
        {
            base.Hide(hideTime);
            _disposables.Dispose();
            _disposables = new CompositeDisposable();
        }
        
        /// <summary>
        /// Subscribe to loading service events
        /// Подписаться на события сервиса загрузки
        /// </summary>
        private void SubscribeToLoadingEvents()
        {
            if (_loadingService == null) return;
            
            // Subscribe to loading state changes
            _loadingService.IsLoading
                .Subscribe(isLoading =>
                {
                    if (loadingSpinner != null)
                        loadingSpinner.SetActive(isLoading);
                    
                    if (!isLoading)
                        ResetProgress();
                })
                .AddTo(_disposables);
            
            // Subscribe to progress updates
            _loadingService.LoadingProgress
                .Subscribe(progress =>
                {
                    _targetProgress = progress;
                    UpdatePercentageText(progress);
                })
                .AddTo(_disposables);
            
            // Subscribe to loading title updates
            _loadingService.LoadingTitle
                .Subscribe(title =>
                {
                    if (titleText != null && !string.IsNullOrEmpty(title))
                        titleText.text = title;
                })
                .AddTo(_disposables);
            
            // Subscribe to loading status updates
            _loadingService.LoadingStatus
                .Subscribe(status =>
                {
                    if (statusText != null && !string.IsNullOrEmpty(status))
                        statusText.text = status;
                })
                .AddTo(_disposables);
        }
        
        /// <summary>
        /// Update progress bar with smooth animation
        /// Обновить прогресс-бар с плавной анимацией
        /// </summary>
        private void UpdateProgressBarSmooth()
        {
            if (progressBar == null) return;
            
            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    if (progressBar != null)
                    {
                        float currentProgress = progressBar.value;
                        float newProgress = Mathf.MoveTowards(currentProgress, _targetProgress, 
                            Time.deltaTime * progressSmoothSpeed);
                        progressBar.value = newProgress;
                    }
                })
                .AddTo(_disposables);
        }
        
        /// <summary>
        /// Update percentage text display
        /// Обновить отображение процентов
        /// </summary>
        private void UpdatePercentageText(float progress)
        {
            if (percentageText != null)
            {
                int percentage = Mathf.RoundToInt(progress * 100f);
                percentageText.text = $"{percentage}%";
            }
        }
        
        /// <summary>
        /// Reset progress to initial state
        /// Сбросить прогресс в начальное состояние
        /// </summary>
        private void ResetProgress()
        {
            _targetProgress = 0f;
            if (progressBar != null)
                progressBar.value = 0f;
            UpdatePercentageText(0f);
        }
        
        /// <summary>
        /// Animate loading spinner rotation
        /// Анимировать вращение индикатора загрузки
        /// </summary>
        private void Update()
        {
            if (loadingSpinner != null && loadingSpinner.activeInHierarchy)
            {
                loadingSpinner.transform.Rotate(0f, 0f, -spinnerSpeed * Time.deltaTime);
            }
        }
        
        /// <summary>
        /// Set manual loading state (for testing)
        /// Установить ручное состояние загрузки (для тестирования)
        /// </summary>
        public void SetManualLoadingState(string title, string status, float progress)
        {
            if (titleText != null) titleText.text = title;
            if (statusText != null) statusText.text = status;
            _targetProgress = Mathf.Clamp01(progress);
            UpdatePercentageText(_targetProgress);
        }
        
        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}
