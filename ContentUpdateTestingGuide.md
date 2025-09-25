# Content Update Testing Guide

Руководство по тестированию обновлений контента без релиза приложения.

## Тестовый сценарий "Content Update без релиза клиента"

### Шаг 1: Подготовка первой версии

1. **Создать тестовый ресурс**:
   ```
   Assets/AddressableAssets/UI/
   └── test_button_sprite.png (версия 1 - красная кнопка)
   ```

2. **Назначить Addressable настройки**:
   - Address: `ui_test_button`
   - Group: `UI_Remote`
   - Labels: `ui`, `prefab`

3. **Первая сборка Addressables**:
   ```
   Addressables Groups → Build → New Build → Default Build Script
   Profile: Development
   ```

4. **Запуск локального сервера**:
   ```bash
   cd ServerData
   python -m http.server 8080
   ```

### Шаг 2: Тестирование загрузки v1

1. **Запустить приложение**
2. **Загрузить ресурс**: `ui_test_button`
3. **Проверить**: отображается красная кнопка
4. **Проверить логи**: ресурс загружен из сети

### Шаг 3: Создание обновленной версии

1. **Заменить ресурс**:
   ```
   Assets/AddressableAssets/UI/
   └── test_button_sprite.png (версия 2 - синяя кнопка)
   ```

2. **Content Update сборка**:
   ```
   Addressables Groups → Build → Update a Previous Build
   Выбрать предыдущий addressables_content_state.bin
   ```

3. **Проверить изменения**:
   ```
   ServerData/WebGL/
   ├── catalog_*.json (новая версия)
   └── новые bundle файлы
   ```

### Шаг 4: Тестирование обновления

1. **БЕЗ перезапуска приложения**
2. **Вызвать проверку обновлений**:
   ```csharp
   await catalogManager.CheckForUpdatesAsync();
   ```

3. **Скачать обновления**:
   ```csharp
   await catalogManager.DownloadUpdatesAsync();
   ```

4. **Перезагрузить ресурс**:
   ```csharp
   addressableService.ReleaseAsset("ui_test_button");
   await addressableService.LoadAssetAsync<Sprite>("ui_test_button");
   ```

5. **Проверить результат**: 
   - ✅ Отображается синяя кнопка (версия 2)
   - ✅ Приложение не перезапускалось
   - ✅ В логах видно обновление каталога

## Диагностика проблем

### Проблема: "Ресурс не обновляется"

**Возможные причины**:
1. Кеш не очищен после обновления
2. Handle не был released перед перезагрузкой  
3. Content Update build не создал новые бандлы

**Решение**:
```csharp
// Очистить кеш
Addressables.ClearDependencyCacheAsync();

// Освободить handle
Addressables.Release(handle);

// Перезагрузить
var newHandle = Addressables.LoadAssetAsync<Sprite>(key);
```

### Проблема: "Каталог не обновляется"

**Проверить**:
1. `Can Change Post Release = true` для группы
2. Правильный путь к `addressables_content_state.bin`
3. Remote Load Path доступен и корректен

## Автоматизация тестирования

```csharp
[Test]
public async Task ContentUpdate_WithoutAppRestart_ShouldLoadNewAsset()
{
    // Arrange
    var originalAsset = await addressableService.LoadAssetAsync<Sprite>("ui_test_button");
    var originalInstanceId = originalAsset.GetInstanceID();
    
    // Simulate content update
    await catalogManager.DownloadUpdatesAsync();
    
    // Act
    addressableService.ReleaseAsset("ui_test_button");
    var updatedAsset = await addressableService.LoadAssetAsync<Sprite>("ui_test_button");
    var updatedInstanceId = updatedAsset.GetInstanceID();
    
    // Assert
    Assert.AreNotEqual(originalInstanceId, updatedInstanceId);
}
```

## Метрики для измерения

1. **Размер обновления**: сколько байт скачано
2. **Время обновления**: от проверки до применения  
3. **Успешность**: процент успешных обновлений
4. **Влияние на производительность**: FPS во время обновления

## Чек-лист проверки Content Update

- [ ] Группы настроены с `Can Change Post Release = true`
- [ ] Remote Load Path корректный и доступен
- [ ] Локальный сервер запущен и отдает файлы
- [ ] `addressables_content_state.bin` из предыдущего билда найден
- [ ] Новые bundle файлы созданы в ServerData
- [ ] Catalog обновился (новая дата/версия)
- [ ] Handle освобожден перед перезагрузкой ресурса
- [ ] Приложение получило новый ресурс без перезапуска
