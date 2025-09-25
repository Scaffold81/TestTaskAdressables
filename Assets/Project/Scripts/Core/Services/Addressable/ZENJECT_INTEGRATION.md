# Zenject Integration Guide

Руководство по интеграции Addressables с существующей Zenject архитектурой.

## Структура интеграции

### GlobalConfigInstaller
Отвечает за привязку конфигураций:
```csharp
// Конфиги
Container.Bind<AddressableConfig>().FromInstance(addressableConfig).AsSingle();

// Репозитории конфигов  
Container.Bind<IAddressableConfigRepository>().To<AddressableConfigRepository>().AsSingle();
```

### ProjectServiceInstaller  
Отвечает за привязку сервисов:
```csharp
// Менеджер каталогов
Container.Bind<ICatalogManager>().To<CatalogManager>().AsSingle();

// Основной сервис
Container.Bind<IAddressableService>().To<AddressableService>().AsSingle().NonLazy();
```

## Порядок инициализации

1. **GlobalConfigInstaller** - привязывает конфиги
2. **ProjectServiceInstaller** - привязывает сервисы  
3. **AddressableService** - автоматически инициализируется (.NonLazy())
4. **CatalogManager** - инициализируется через AddressableService

## Использование в других сервисах

### Инжекция зависимости
```csharp
public class MyService
{
    private readonly IAddressableService _addressableService;
    
    public MyService(IAddressableService addressableService)
    {
        _addressableService = addressableService;
    }
}
```

### Расширения существующих сервисов
```csharp
// SceneManagerService
public async UniTask LoadSceneFromAddressables(string sceneKey)
{
    await _addressableService.LoadSceneAsync(sceneKey);
}

// GameFactory  
public async UniTask<T> CreateFromAddressable<T>(string key) where T : Component
{
    var prefab = await _addressableService.LoadAssetAsync<GameObject>(key);
    return Instantiate(prefab).GetComponent<T>();
}
```

## Настройка в Unity

### 1. GlobalConfigInstaller
```
ProjectContext (prefab) → GlobalConfigInstaller:
- Audio Config: назначить существующий AudioConfig
- Addressable Config: создать и назначить AddressableConfig asset
```

### 2. AddressableConfig создание
```
Assets/Project/Configs/ → Create → Project Config → Addressable Config
Настроить все параметры согласно техническому заданию
```

### 3. ProjectServiceInstaller
```
Никаких дополнительных настроек не требуется - 
сервисы привяжутся автоматически при наличии конфигов
```

## Валидация интеграции

### Проверки при запуске:
```csharp
[Test]
public void AddressableServices_ShouldBeInjected()
{
    // Arrange & Act
    var addressableService = Container.Resolve<IAddressableService>();
    var catalogManager = Container.Resolve<ICatalogManager>(); 
    var configRepo = Container.Resolve<IAddressableConfigRepository>();
    
    // Assert
    Assert.IsNotNull(addressableService);
    Assert.IsNotNull(catalogManager);
    Assert.IsNotNull(configRepo);
    Assert.IsTrue(addressableService.IsInitialized);
}
```

### Логи успешной интеграции:
```
[GlobalConfigInstaller] Addressable config bound successfully
[GlobalConfigInstaller] Config repositories bound successfully  
[ProjectServiceInstaller] Addressable services bound successfully
[AddressableService] Initialized successfully
[CatalogManager] Initialized with X catalogs
```

## Возможные проблемы

### Проблема: "AddressableConfig is null"
**Решение**: Создать AddressableConfig asset и назначить в GlobalConfigInstaller

### Проблема: "Circular dependency"  
**Решение**: Проверить порядок привязки - конфиги должны идти перед сервисами

### Проблема: "Service not initialized"
**Решение**: Использовать .NonLazy() для критичных сервисов

## Паттерны использования

### 1. Lazy Loading
```csharp
[Inject] private IAddressableService _addressableService;

private async UniTask<Sprite> GetSpriteWhenNeeded(string key)
{
    return await _addressableService.LoadAssetAsync<Sprite>(key);
}
```

### 2. Preloading
```csharp
[Inject] private IAddressableService _addressableService;

public async UniTask Initialize()
{
    // Preload critical assets
    var coreKeys = new[] { "ui_main_menu", "audio_click_sound" };
    await _addressableService.DownloadDependenciesAsync(coreKeys);
}
```

### 3. Observable Events
```csharp
[Inject] private IAddressableService _addressableService;

private void Start()
{
    _addressableService.OnAssetLoaded
        .Subscribe(data => Debug.Log($"Loaded {data.key} in {data.time}s"))
        .AddTo(this);
}
```

Интеграция с Zenject обеспечивает чистую архитектуру с правильным разделением ответственности и автоматическим управлением жизненным циклом сервисов.
