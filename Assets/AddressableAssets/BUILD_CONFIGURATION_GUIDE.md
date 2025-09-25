# Build Configuration Guide

Руководство по настройке билд-системы для Addressables.

## Профили и их назначение

### Development Profile
- **Назначение**: Локальная разработка и тестирование
- **RemoteLoadPath**: `http://localhost:8080/[BuildTarget]`
- **BuildPath**: `ServerData/[BuildTarget]`
- **Особенности**:
  - Быстрая итерация
  - Локальный HTTP сервер
  - Полные логи и диагностика

### Staging Profile  
- **Назначение**: Тестирование перед продакшеном
- **RemoteLoadPath**: `https://staging.yourcdn.com/[BuildTarget]`
- **BuildPath**: `ServerData/[BuildTarget]`
- **Особенности**:
  - Имитация продакшен условий
  - Staging CDN сервер
  - QA тестирование

### Production Profile
- **Назначение**: Финальный релиз
- **RemoteLoadPath**: `https://cdn.yourprod.com/[BuildTarget]`
- **BuildPath**: `ServerData/[BuildTarget]`
- **Особенности**:
  - Производственный CDN
  - Минимальные логи
  - Максимальная оптимизация

## Группы и их конфигурация

| Группа | Bundle Mode | Compression | Include in Build | Обоснование |
|--------|-------------|-------------|------------------|-------------|
| **Core_Local** | Pack Together | LZ4 | ✓ | Критичные ресурсы, всегда доступны |
| **UI_Remote** | Pack Separately | LZ4 | ✗ | Независимые UI экраны |
| **Characters_Remote** | Pack Together By Label | LZMA | ✗ | Персонажи одного типа |
| **Environment_Remote** | Pack Together | LZMA | ✗ | Статичные декорации |
| **Effects_Remote** | Pack Separately | LZ4 | ✗ | Динамические эффекты |
| **Levels_Remote** | Pack Together By Label | LZMA | ✗ | Ресурсы уровня |

## Compression Strategy

### LZ4 (быстрая распаковка)
- **Применение**: UI, Effects, Core
- **Плюсы**: Быстрая загрузка в runtime
- **Минусы**: Больший размер файлов
- **Когда использовать**: Частые загрузки, интерактивные элементы

### LZMA (лучшее сжатие) 
- **Применение**: Characters, Environment, Levels
- **Плюсы**: Минимальный размер файлов
- **Минусы**: Медленная распаковка
- **Когда использовать**: Редкие загрузки, большие ресурсы

## Bundle Mode Strategy

### Pack Together
- **Применение**: Core_Local, Environment_Remote
- **Логика**: Ресурсы используются совместно
- **Пример**: Все UI префабы главного меню

### Pack Separately
- **Применение**: UI_Remote, Effects_Remote  
- **Логика**: Независимые компоненты
- **Пример**: Отдельные UI окна, эффекты по требованию

### Pack Together By Label
- **Применение**: Characters_Remote, Levels_Remote
- **Логика**: Группировка по функциональности
- **Пример**: Все ресурсы уровня 1, все враги типа A

## Content Update Workflow

### Настройка групп для обновлений

```
Levels_Remote группа:
- Can Change Post Release: ✓
- Include in Build: ✗  
- Use Separate Catalog: ✓
- Catalog Name: catalog_levels
```

### Процесс обновления

1. **Первичная сборка**:
   ```
   Addressables Groups → Build → New Build → Default Build Script
   ```

2. **Изменение контента**:
   - Обновить ресурсы в Levels_Remote группе
   - Сохранить `addressables_content_state.bin`

3. **Content Update сборка**:
   ```
   Addressables Groups → Build → Update a Previous Build
   Выбрать сохраненный .bin файл
   ```

4. **Деплоймент**:
   - Загрузить новые файлы на CDN
   - Клиенты автоматически получат обновления

## Platform Settings

### WebGL
```csharp
Max Concurrent Web Requests: 6
Catalog Download Timeout: 30s
Disable Catalog Update on Start: false
Bundle Naming Mode: Filename
```

### Android
```csharp
Use Asset Database: false (Production)
Simulate Groups: false (Production)
Bundle Naming Mode: Hashed Filename
Split Application Binary: true
```

## Размерные ограничения

### WebGL Budget
- **Первая загрузка**: ≤ 30 МБ
- **Отдельный бандл**: ≤ 10 МБ  
- **Core группа**: ≤ 5 МБ

### Android Budget
- **Первая загрузка**: ≤ 15 МБ
- **Отдельный бандл**: ≤ 8 МБ
- **Core группа**: ≤ 3 МБ

## Мониторинг и метрики

### Bundle Analysis
```bash
# После сборки проверить размеры
ls -la ServerData/WebGL/*.bundle

# Анализ зависимостей  
Addressables Groups → Tools → Analyze → Check Duplicate Bundle Dependencies
```

### Runtime метрики
```csharp
// Размер загруженных бандлов
Debug.Log($"Bundle size: {handle.GetDownloadStatus().TotalBytes} bytes");

// Время загрузки
var stopwatch = Stopwatch.StartNew();
await Addressables.LoadAssetAsync<Sprite>(key);
Debug.Log($"Load time: {stopwatch.ElapsedMilliseconds} ms");
```

## Оптимизация производительности

### 1. Дедупликация ресурсов
- Выявить повторяющиеся зависимости
- Вынести общие ресурсы в SharedAssets группу
- Использовать Asset References вместо прямых ссылок

### 2. Предзагрузка критичного контента
```csharp
// Preload Core assets on startup
await addressableService.DownloadDependenciesAsync(coreAssetKeys);

// Preload next level while playing current
_ = addressableService.LoadAssetAsync<Scene>("levels_level02_scene");
```

### 3. Lazy loading стратегия
```csharp
// Load on demand
if (!loadedCharacters.ContainsKey(characterId))
{
    var character = await addressableService.LoadAssetAsync<GameObject>($"characters_{characterId}");
    loadedCharacters[characterId] = character;
}
```

### 4. Memory management
```csharp
// Release unused assets on scene change
private void OnSceneUnloaded(Scene scene)
{
    var sceneAssets = GetAssetsForScene(scene.name);
    foreach (var assetKey in sceneAssets)
    {
        addressableService.ReleaseAsset(assetKey);
    }
}
```

## Troubleshooting

### Проблема: "Missing Reference"
**Причины**:
- Ресурс не назначен в правильную группу
- Неправильный Address key
- Группа не включена в билд

**Решение**:
```
1. Addressables Groups → проверить ресурс в группе
2. Inspector → Addressable → проверить Address
3. Group Settings → Include in Build ✓
```

### Проблема: "Slow Loading"
**Причины**:
- LZMA compression для частых ресурсов
- Большие бандлы
- Медленная сеть

**Решение**:
```
1. Изменить Compression на LZ4
2. Bundle Mode: Pack Separately
3. Preload critical assets
```

### Проблема: "WebGL CORS Error"
**Причины**:
- CDN не настроен для CORS
- Неправильные headers

**Решение**:
```
Access-Control-Allow-Origin: *
Access-Control-Allow-Methods: GET
Access-Control-Allow-Headers: Content-Type
```

## Deployment Checklist

### Pre-deployment
- [ ] Bundle analysis пройден
- [ ] Размеры соответствуют бюджетам
- [ ] Content update протестирован
- [ ] Platform settings применены
- [ ] Duplicate dependencies устранены

### Deployment
- [ ] Addressables build выполнен
- [ ] ServerData загружен на CDN
- [ ] CORS настроен на сервере
- [ ] Profile URLs обновлены
- [ ] Кеш CDN очищен

### Post-deployment  
- [ ] Первая загрузка протестирована
- [ ] Content update работает
- [ ] Метрики производительности собираются
- [ ] Error handling функционирует
- [ ] Fallback режим проверен

## Scripts для автоматизации

### Build Script
```bash
#!/bin/bash
# Automated Addressables build

echo "Building Addressables..."
unity -batchmode -projectPath . -executeMethod BuildScript.BuildAddressables

echo "Uploading to CDN..."
aws s3 sync ServerData/ s3://your-cdn-bucket/

echo "Invalidating CDN cache..."
aws cloudfront create-invalidation --distribution-id YOUR_ID --paths "/*"

echo "Build complete!"
```

### Размер мониторинг
```bash
#!/bin/bash
# Check bundle sizes

echo "Bundle sizes:"
for file in ServerData/WebGL/*.bundle; do
    size=$(du -h "$file" | cut -f1)
    echo "$file: $size"
done

total=$(du -sh ServerData/WebGL/ | cut -f1)
echo "Total: $total"
```

Этот план обеспечивает полную настройку системы Addressables с поддержкой независимых обновлений контента, оптимизацией для разных платформ и комплексным мониторингом производительности.
