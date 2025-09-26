# Loading Service Integration Guide

Руководство по интеграции сервиса загрузки с Addressables и UI.

## Архитектура сервиса

### ILoadingService
Основной интерфейс для управления UI загрузки и отслеживания прогресса.

**Основные методы:**
```csharp
// Показать экран загрузки
void ShowProgress(string title, string status = "");

// Обновить прогресс
void UpdateProgress(float progress, string status = "");

// Скрыть экран загрузки
void HideProgress();

// Обернуть асинхронную операцию
UniTask ShowProgressAsync(UniTask task, string title, string status = "");
```

**Observable события:**
```csharp
// Обновления прогресса
Observable<ProgressData> OnProgressUpdated { get; }

// Изменения состояния загрузки
Observable<bool> OnLoadingStateChanged { get; }
```

### LoadingService
Реализация с интеграцией в GameTemplate архитектуру.

**Зависимости:**
- `IUIPageService` - для управления UI страницами
- Автоматическая регистрация через Zenject
- R3 Observable для реактивных событий

## Интеграция с Addressables

### Extension Methods
Методы расширения для упрощения использования:

```csharp
// Загрузка ресурса с UI
await addressableService.LoadAssetWithProgressAsync<Sprite>(
    "ui_button", loadingService, "Loading UI Assets");

// Загрузка сцены с прогрессом
await addressableService.LoadSceneWithProgressAsync(
    "level01_scene", loadingService, "Loading Level");

// Скачивание зависимостей
await addressableService.DownloadDependenciesWithProgressAsync(
    new[] { "character_hero" }, loadingService, "Downloading Characters");
```

### Интеграция с SceneManagerService
```csharp
// Загрузка сцены через SceneManager с прогрессом
await sceneManagerService.LoadAddressableSceneWithProgressAsync(
    "gameplay_scene", addressableService, loadingService);

// Предзагрузка зависимостей сцены
await sceneManagerService.PreloadSceneDependenciesWithProgressAsync(
    "next_level", addressableService, loadingService);
```

## Observable Integration

### AddressableLoadingIntegration
Автоматическая интеграция событий между сервисами:

```csharp
// Автоматически подписывается на:
addressableService.OnProgressUpdated → loadingService.UpdateProgress()
addressableService.OnAssetLoaded → телеметрия загрузки
loadingService.OnLoadingStateChanged → логирование состояний
```

### Ручная подписка на события
```csharp
loadingService.OnProgressUpdated
    .Subscribe(progress => {
        Debug.Log($"Loading: {progress.Title} - {progress.GetProgressPercent()}");
    })
    .AddTo(disposables);
```

## Использование в коде

### Базовое использование
```csharp
public class GameController : MonoBehaviour
{
    [Inject] private ILoadingService _loadingService;
    
    private async UniTask LoadGameContent()
    {
        // Показать загрузку
        _loadingService.ShowProgress("Loading Game", "Initializing...");
        
        // Имитация загрузки
        for (int i = 0; i < 10; i++)
        {
            await UniTask.Delay(100);
            _loadingService.UpdateProgress(i / 10f, $"Step {i + 1}/10");
        }
        
        // Скрыть загрузку
        _loadingService.HideProgress();
    }
}
```

### С автоматическим прогрессом
```csharp
public class AssetLoader : MonoBehaviour
{
    [Inject] private IAddressableService _addressableService;
    [Inject] private ILoadingService _loadingService;
    
    private async UniTask LoadCharacter(string characterKey)
    {
        // Автоматическое управление прогрессом
        var character = await _addressableService.LoadAssetWithProgressAsync<GameObject>(
            characterKey, _loadingService, "Loading Character");
            
        Instantiate(character);
    }
}
```

### Кастомная обработка прогресса
```csharp
public class LevelLoader : MonoBehaviour
{
    private async UniTask LoadLevelWithCustomProgress()
    {
        var loadingTask = LoadLevelAsync();
        
        // Обернуть в UI загрузки
        await _loadingService.ShowProgressAsync(loadingTask, "Loading Level", "Preparing...");
    }
    
    private async UniTask LoadLevelAsync()
    {
        // Кастомная логика загрузки
        await LoadEnvironment();
        await LoadCharacters();
        await LoadUI();
    }
}
```

## ProgressData структура

```csharp
public struct ProgressData
{
    public float Progress;        // 0.0 - 1.0
    public string Title;         // "Loading Assets"  
    public string Status;        // "Downloading textures..."
    public DateTime Timestamp;   // Время обновления
    
    public string GetProgressPercent(); // "45%"
}
```

## Тестирование

### Встроенные тесты
В `AddressableServiceTest` доступны тестовые методы:

```csharp
[ContextMenu("Test Loading Service")]
private async void TestLoadingService()

[ContextMenu("Test Asset Loading with Progress")]  
private async void TestAssetLoadingWithProgress()

[ContextMenu("Test Download Dependencies")]
private async void TestDownloadDependencies()
```

### Ручное тестирование
1. Добавить `AddressableServiceTest` компонент на GameObject
2. В контекстном меню выбрать нужный тест
3. Наблюдать логи прогресса в Console

## Производительность

### Оптимизации
- Автоматическое управление подписками через CompositeDisposable
- Минимальное количество аллокаций в ProgressData
- Ленивая инициализация UI компонентов

### Мониторинг
```csharp
// Логирование производительности
loadingService.OnProgressUpdated
    .Subscribe(progress => {
        var elapsed = DateTime.Now - progress.Timestamp;
        if (elapsed.TotalMilliseconds > 16) // > 1 frame
        {
            Debug.LogWarning($"Slow progress update: {elapsed.TotalMilliseconds}ms");
        }
    });
```

## Будущие улучшения

### Этап 3.1: UI Integration
- Создание LoadingProgressView префаба
- Интеграция с UIPageService
- DOTween анимации

### Этап 3.2: Advanced Features  
- Множественные одновременные загрузки
- Приоритизация загрузок
- Отмена операций

### Этап 4.1: Telemetry
- Детальная аналитика производительности
- Сетевая статистика
- Memory profiling integration

Система LoadingService обеспечивает единообразный подход к отображению прогресса загрузки во всем приложении с тесной интеграцией с Addressables и GameTemplate архитектурой.
