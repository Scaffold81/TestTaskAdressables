# 🎮 Unity Addressables Demo Project

Демонстрационный проект системы Addressables в Unity, показывающий загрузку, выгрузку и управление контентом с современной архитектурой.

---

## 📋 Содержание

- [Стек технологий](#-стек-технологий)
- [Особенности](#-особенности)
- [Архитектура](#-архитектура)
- [Структура проекта](#-структура-проекта)
- [Установка и запуск](#-установка-и-запуск)
- [Использование](#-использование)
- [Addressables Groups](#-addressables-groups)
- [Производительность](#-производительность)
- [Известные ограничения](#-известные-ограничения)

---

## 🛠 Стек технологий

### Core:
- **Unity**: 2022.3+ LTS
- **Addressable Asset System**: 1.21.19+
- **C#**: .NET Standard 2.1

### Библиотеки и фреймворки:
- **UniTask**: Асинхронное программирование (async/await)
- **Zenject (Extenject)**: Dependency Injection контейнер
- **R3 (Reactive Extensions)**: Реактивное программирование
- **TextMeshPro**: UI текст

### Patterns & Architecture:
- **Service Layer Pattern**: Изолированная бизнес-логика
- **Repository Pattern**: Управление конфигурацией
- **Observer Pattern**: R3 Observable для реактивности
- **Dependency Injection**: Zenject для слабой связанности
- **SOLID принципы**: Чистая масштабируемая архитектура

---

## ✨ Особенности

### 🎯 Addressables функционал:
- ✅ Загрузка и выгрузка ассетов по ключу
- ✅ Асинхронная загрузка сцен через Addressables
- ✅ Предварительная оценка размера загрузки (`GetDownloadSizeAsync`)
- ✅ Управление зависимостями бандлов
- ✅ Автоматическая выгрузка при смене сцены
- ✅ Поддержка профилей (Development, Staging, Production)
- ✅ Local и Remote загрузка контента

### 📊 Мониторинг и отладка:
- ✅ Прогресс-бар загрузки в реальном времени (0-100%)
- ✅ Отображение размера кеша
- ✅ Статистика загруженных ассетов
- ✅ DevOverlay панель для разработчиков
- ✅ Детальное логирование всех операций
- ✅ Телеметрия загрузок (размер, время, источник)

### 🚀 Производительность:
- ✅ Автоматическая выгрузка неиспользуемых ассетов
- ✅ Memory Manager для контроля памяти
- ✅ Оптимизация для WebGL (бюджет 20-30 МБ)
- ✅ LZ4/LZMA компрессия бандлов
- ✅ Нормализованный прогресс загрузки
- ✅ `Resources.UnloadUnusedAssets()` при выгрузке

### 🎨 UI/UX:
- ✅ Плавные переходы между сценами
- ✅ Анимированный loading screen с fade in/out
- ✅ Информативные статусы загрузки
- ✅ Responsive дизайн (1920x1080 base)
- ✅ Управление через CanvasGroup (производительность)

---

## 🏗 Архитектура

### Слоистая архитектура проекта

```
┌─────────────────────────────────────────────────────┐
│                  PRESENTATION LAYER                  │
│                    (UI Controllers)                  │
├─────────────────────────────────────────────────────┤
│                                                      │
│  MainMenuController          LoadingScreenController│
│  ├─ Download Core Assets     ├─ Auto Show/Hide     │
│  ├─ Download Levels          ├─ Progress Display   │
│  ├─ Clear Cache              └─ Smooth Animations  │
│  ├─ DevOverlay Toggle                               │
│  └─ Scene Navigation                                │
│                                                      │
└──────────────────┬──────────────────────────────────┘
                   │
                   ↓
┌─────────────────────────────────────────────────────┐
│                    SERVICE LAYER                     │
│              (Business Logic & State)                │
├─────────────────────────────────────────────────────┤
│                                                      │
│  IAddressableService                                │
│  ├─ LoadAssetAsync<T>(key)                          │
│  ├─ LoadSceneAsync(key)                             │
│  ├─ DownloadDependenciesAsync(keys, progress)       │
│  ├─ GetDownloadSizeAsync(keys)                      │
│  ├─ ReleaseAsset(key)                               │
│  └─ ReleaseAllAssets()                              │
│                                                      │
│  ISceneManagerService                               │
│  ├─ LoadSceneAsync(SceneId)                         │
│  ├─ LoadSceneAddressableAsync(key)                  │
│  ├─ TargetSceneId { get; set; }                     │
│  ├─ OnSceneLoadProgress (Observable<float>)         │
│  └─ OnSceneLoaded (Observable<string>)              │
│                                                      │
│  ILoadingService                                    │
│  ├─ ExecuteWithLoadingAsync(action)                 │
│  ├─ IsLoading (Observable<bool>)                    │
│  ├─ LoadingProgress (Observable<float>)             │
│  ├─ LoadingTitle (Observable<string>)               │
│  └─ LoadingStatus (Observable<string>)              │
│                                                      │
│  IAddressableMemoryManager                          │
│  ├─ CleanupMemoryAsync()                            │
│  ├─ GetMemoryUsage()                                │
│  └─ TrackLoadedAssets()                             │
│                                                      │
└──────────────────┬──────────────────────────────────┘
                   │
                   ↓
┌─────────────────────────────────────────────────────┐
│                   UNITY ADDRESSABLES                 │
│                  (Asset Management)                  │
├─────────────────────────────────────────────────────┤
│                                                      │
│  Addressables API                                   │
│  ├─ LoadAssetAsync<T>()                             │
│  ├─ LoadSceneAsync()                                │
│  ├─ DownloadDependenciesAsync()                     │
│  ├─ GetDownloadSizeAsync()                          │
│  ├─ Release()                                       │
│  └─ ClearDependencyCacheAsync()                     │
│                                                      │
│  ResourceManager                                    │
│  └─ Handle management, Caching, Dependencies        │
│                                                      │
└─────────────────────────────────────────────────────┘
```

---

### Dependency Injection через Zenject

```
ProjectContext (Scene)
    ↓
ProjectServiceInstaller (ScriptableObject)
    ↓
┌───────────────────────────────────────┐
│  Binds Services as Singletons:        │
│                                       │
│  • IAddressableService                │
│    → AddressableService               │
│                                       │
│  • ISceneManagerService               │
│    → SceneManagerService              │
│                                       │
│  • ILoadingService                    │
│    → LoadingService                   │
│                                       │
│  • IAddressableMemoryManager          │
│    → AddressableMemoryManager         │
│                                       │
│  • IGameFactory                       │
│    → GameFactory                      │
│                                       │
└───────────────────────────────────────┘
```

**Все контроллеры получают зависимости через `[Inject]`:**
```csharp
public class MainMenuController : MonoBehaviour
{
    [Inject] private IAddressableService _addressableService;
    [Inject] private ISceneManagerService _sceneManager;
    [Inject] private ILoadingService _loadingService;
    // ... автоматическая инъекция через Zenject
}
```

---

### Reactive Architecture (R3 Observable)

```
┌──────────────────────────────────────────────────┐
│              Event-Driven Flow                    │
└──────────────────────────────────────────────────┘

LoadingService                Observer (UI)
    ↓                              ↓
IsLoading.OnNext(true)  →  Subscribe() → Show UI
    ↓                              ↓
LoadingProgress.OnNext(0.5) → Subscribe() → Update Progress
    ↓                              ↓
IsLoading.OnNext(false) →  Subscribe() → Hide UI
```

**Пример подписки:**
```csharp
_loadingService.LoadingProgress
    .Subscribe(progress => {
        progressBar.value = progress;
    })
    .AddTo(_disposables); // Автоматическая отписка
```

---

### Scene Management Flow

```
MainMenu Scene
    ↓
User clicks button
    ↓
MainMenuController.TestLoadingAsync()
    ↓
_sceneManager.TargetSceneId = SceneId.MainMenu
    ↓
_sceneManager.LoadSceneAsync(SceneId.Loading)
    ↓
════════════════════════════════════
Loading Scene loads
════════════════════════════════════
    ↓
LoadingSceneController.Start()
    ↓
await UniTask.Delay(minLoadingTime)
    ↓
await _sceneManager.LoadSceneAsync(TargetSceneId)
    ↓
════════════════════════════════════
Target Scene loads
════════════════════════════════════
    ↓
Old scene assets released automatically
```

---

## 📂 Структура проекта

```
Assets/
├── Project/
│   ├── Scenes/
│   │   ├── MainMenu.unity          # Главное меню с демо
│   │   └── Loading.unity           # Экран загрузки
│   │
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── Services/
│   │   │   │   ├── Addressable/
│   │   │   │   │   ├── IAddressableService.cs
│   │   │   │   │   ├── AddressableService.cs
│   │   │   │   │   ├── Memory/
│   │   │   │   │   │   ├── IAddressableMemoryManager.cs
│   │   │   │   │   │   └── AddressableMemoryManager.cs
│   │   │   │   │   └── Config/
│   │   │   │   │       ├── AddressableConfig.cs
│   │   │   │   │       └── IAddressableConfigRepository.cs
│   │   │   │   │
│   │   │   │   ├── Scene/
│   │   │   │   │   ├── ISceneManagerService.cs
│   │   │   │   │   └── SceneManagerService.cs
│   │   │   │   │
│   │   │   │   └── Loading/
│   │   │   │       ├── ILoadingService.cs
│   │   │   │       └── LoadingService.cs
│   │   │   │
│   │   │   ├── Controllers/
│   │   │   │   └── LoadingSceneController.cs
│   │   │   │
│   │   │   ├── Enums/
│   │   │   │   └── SceneId.cs
│   │   │   │
│   │   │   └── Installers/
│   │   │       └── ProjectServiceInstaller.cs
│   │   │
│   │   └── UI/
│   │       ├── MainMenu/
│   │       │   └── MainMenuController.cs
│   │       │
│   │       ├── Loading/
│   │       │   └── LoadingScreenController.cs
│   │       │
│   │       └── Addressable/
│   │           ├── LoadingProgressView.cs
│   │           └── DevOverlayView.cs
│   │
│   └── Configs/
│       └── Addressable/
│           └── AddressableConfig.asset
│
├── AddressableAssets/              # Addressable контент
│   ├── Core/
│   │   ├── UI/
│   │   ├── Audio/
│   │   └── Materials/
│   ├── Characters/
│   ├── Environment/
│   ├── Effects/
│   └── Levels/
│
└── ServerData/                     # Build output
    └── [Platform]/
        ├── catalog.json
        ├── catalog.hash
        └── *.bundle
```

---

## 🚀 Установка и запуск

### Требования:
- Unity 2022.3 LTS или новее
- Addressables Package 1.21.19+
- UniTask (через Package Manager)
- Zenject (Extenject)
- R3 (Reactive Extensions)

### Установка:

1. **Клонировать репозиторий:**
```bash
git clone <repository-url>
cd TestTaskAdressables
```

2. **Открыть в Unity:**
```
Unity Hub → Open → Выбрать папку проекта
```

3. **Импортировать пакеты (если нужно):**
```
Window → Package Manager
- Addressables (1.21.19+)
- TextMeshPro
```

4. **Настроить Addressables:**
```
Window → Asset Management → Addressables → Groups
Build → New Build → Default Build Script
```

5. **Запустить:**
```
Open: Assets/Project/Scenes/MainMenu.unity
Play Mode
```

---

## 🎮 Использование

### Главное меню (MainMenu Scene):

**Кнопки:**
- **Download Core Assets** - загрузка основных ассетов (ui_main_button, explosion_particle, characters_test_prefab)
- **Download Levels** - загрузка уровней (если настроены)
- **Clear Cache** - очистка кеша и выгрузка всех ассетов
- **Show Dev Overlay** - показать панель разработчика

**Информация:**
- Catalog Version - версия каталога Addressables
- Profile - текущий профиль (Development/Staging/Production)
- Cache Size - размер занятого кеша в МБ

**Прогресс:**
- Progress Bar - визуальный прогресс загрузки (0-100%)
- Status Text - текущий статус операции

---

### Программное использование:

#### Загрузка ассета:
```csharp
// Инжект сервиса
[Inject] private IAddressableService _addressableService;

// Загрузка спрайта
var sprite = await _addressableService.LoadAssetAsync<Sprite>("ui_main_button");

// Загрузка префаба
var prefab = await _addressableService.LoadAssetAsync<GameObject>("character_prefab");
```

#### Загрузка с прогрессом:
```csharp
var keys = new[] { "asset1", "asset2", "asset3" };

// Получить размер
long size = await _addressableService.GetDownloadSizeAsync(keys);
Debug.Log($"Will download: {size / (1024f * 1024f):F2} MB");

// Загрузить с прогрессом
var progress = new Progress<float>(p => {
    Debug.Log($"Progress: {p * 100:F0}%");
});

await _addressableService.DownloadDependenciesAsync(keys, progress);
```

#### Загрузка сцены:
```csharp
[Inject] private ISceneManagerService _sceneManager;

// По enum
await _sceneManager.LoadSceneAsync(SceneId.MainMenu);

// Через Addressables
await _sceneManager.LoadSceneAddressableAsync("gameplay_scene");
```

#### Выгрузка:
```csharp
// Выгрузить конкретный ассет
_addressableService.ReleaseAsset("ui_main_button");

// Выгрузить все
_addressableService.ReleaseAllAssets();

// С очисткой памяти
await _memoryManager.CleanupMemoryAsync();
```

---

## 📦 Addressables Groups

### Структура групп:

```
Core_Local (встроено в билд)
├─ Bundle Mode: Pack Together
├─ Compression: LZ4
├─ Build Path: LocalBuildPath
├─ Load Path: LocalLoadPath
└─ Assets:
   ├─ ui_main_button
   ├─ material_default
   └─ audio_click_sound

UI_Remote (удаленная загрузка)
├─ Bundle Mode: Pack Separately
├─ Compression: LZ4
├─ Build Path: RemoteBuildPath
├─ Load Path: RemoteLoadPath
└─ Assets:
   └─ (UI элементы для обновления)

Characters_Remote
├─ Bundle Mode: Pack Together By Label
├─ Compression: LZMA
└─ Assets:
   └─ (Персонажи, анимации)

Levels_Remote
├─ Bundle Mode: Pack Together By Label
├─ Compression: LZMA
└─ Assets:
   └─ (Сцены уровней)
```

### Профили:

**Development:**
```
RemoteLoadPath: http://localhost:8080/[BuildTarget]
BuildPath: ServerData/[BuildTarget]
```

**Staging:**
```
RemoteLoadPath: https://staging.yourcdn.com/[BuildTarget]
BuildPath: ServerData/[BuildTarget]
```

**Production:**
```
RemoteLoadPath: https://cdn.yourprod.com/[BuildTarget]
BuildPath: ServerData/[BuildTarget]
```

---

## ⚡ Производительность

### Метрики:

**WebGL Build:**
- First Load Budget: 20-30 MB
- Core Assets: ~5-10 MB
- Loading Time: <2 sec (localhost)

**Memory Management:**
- Automatic asset unloading on scene change
- Manual cleanup via MemoryManager
- Resources.UnloadUnusedAssets() on unload

### Оптимизации:

1. **LZ4 для часто используемых ассетов** (быстрая распаковка)
2. **LZMA для редких больших ассетов** (лучшая компрессия)
3. **Pack Together для связанных ассетов** (меньше запросов)
4. **Pack Separately для независимых** (избежать дубликатов)
5. **Нормализованный прогресс** (плавный UI без рывков)

---

## ⚠️ Известные ограничения

### Текущие ограничения:
- Loading сцена работает только для перехода между локальными сценами
- DevOverlay панель требует дополнительной реализации
- Remote CDN требует настройки сервера
- Content Update Workflow требует дополнительной настройки

### Возможные улучшения:
- [ ] Автоматический Content Update Build
- [ ] Retry логика для network ошибок
- [ ] Детальная телеметрия загрузок
- [ ] Dependency visualization в DevOverlay
- [ ] Automatic profile switching
- [ ] Custom loading animations
- [ ] Bundle size analyzer

---

## 📝 Лицензия

MIT License - используй свободно для обучения и коммерческих проектов.

## 📚 Дополнительные ресурсы

- [Unity Addressables Documentation](https://docs.unity3d.com/Packages/com.unity.addressables@latest)
- [UniTask GitHub](https://github.com/Cysharp/UniTask)
- [Zenject Documentation](https://github.com/modesttree/Zenject)
- [R3 Reactive Extensions](https://github.com/Cysharp/R3)

---

**Создано с ❤️ для демонстрации современной Unity архитектуры**
