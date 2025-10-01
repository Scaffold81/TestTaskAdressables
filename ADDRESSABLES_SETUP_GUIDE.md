# 📦 СОЗДАНИЕ ADDRESSABLES АССЕТОВ - Инструкция

## 🎯 Цель: Создать все необходимые ассеты для демо

---

## 📁 СТРУКТУРА ПАПОК (2 минуты)

### 1. Создать папки
```
Project → Assets → Create Folder → "AddressableAssets"

В AddressableAssets создать:
  → Core/
    → UI/
    → Audio/
    → Materials/
  → Characters/
  → Environment/
  → Effects/
  → Levels/
```

---

## 🎨 CORE АССЕТЫ (5 минут)

### 2. UI Button Sprite
```
Assets/AddressableAssets/Core/UI/
→ Right Click → Create → Sprites → Square

Rename: "ui_main_button"

Inspector:
  ✓ Addressable
  Address: "ui_main_button"
  Group: Core_Local
  Labels: ui, core
```

### 3. Default Material
```
Assets/AddressableAssets/Core/Materials/
→ Right Click → Create → Material

Rename: "material_default"

Material Settings:
  - Shader: Standard
  - Albedo: RGB(200, 200, 200)

Inspector:
  ✓ Addressable
  Address: "material_default"
  Group: Core_Local
  Labels: core
```

### 4. Audio Click Sound
```
Assets/AddressableAssets/Core/Audio/
→ Right Click → Create → Audio Clip (пустой)

ИЛИ можем пропустить аудио и создать пустой ScriptableObject

Rename: "audio_click_sound"

Inspector:
  ✓ Addressable
  Address: "audio_click_sound"
  Group: Core_Local
  Labels: audio, core
```

**Альтернатива для аудио:**
```
Если нет аудио файла:
→ Right Click → Create → ScriptableObject (любой)
Rename: "audio_click_sound"
```

---

## 🏗️ LEVELS АССЕТЫ (опционально, 3 минуты)

### 5. Level Scene Placeholders

**Вариант A: Создать сцены**
```
Assets/AddressableAssets/Levels/
→ Create → Scene

Создать 2 сцены:
1. "Level01"
2. "Level02"

Для каждой:
  Inspector:
    ✓ Addressable
    Address: "levels_level01_scene" / "levels_level02_scene"
    Group: Levels_Remote
    Labels: level
```

**Вариант B: Использовать префабы (проще)**
```
Assets/AddressableAssets/Levels/
→ Create → Empty GameObject → Save as Prefab

Создать 2 префаба:
1. "Level01_Prefab"
2. "Level02_Prefab"

Для каждого:
  Inspector:
    ✓ Addressable
    Address: "levels_level01_scene" / "levels_level02_scene"
    Group: Levels_Remote
    Labels: level
```

---

## 🔧 ADDRESSABLES GROUPS НАСТРОЙКА (5 минут)

### 6. Открыть Addressables Groups
```
Window → Asset Management → Addressables → Groups
```

### 7. Проверить/Создать группы

**Core_Local:**
```
Если нет - Create New Group → Local → Core_Local

Settings:
  Build Path: LocalBuildPath
  Load Path: LocalLoadPath
  Bundle Mode: Pack Together
  Compression: LZ4
  ✓ Include In Build
```

**UI_Remote:**
```
Create New Group → Remote → UI_Remote

Settings:
  Build Path: RemoteBuildPath
  Load Path: RemoteLoadPath (http://localhost:8080/[BuildTarget])
  Bundle Mode: Pack Separately
  Compression: LZ4
```

**Levels_Remote:**
```
Create New Group → Remote → Levels_Remote

Settings:
  Build Path: RemoteBuildPath
  Load Path: RemoteLoadPath
  Bundle Mode: Pack Together By Label
  Compression: LZMA
```

---

## 📋 ФИНАЛЬНАЯ ПРОВЕРКА (2 минуты)

### 8. Проверить все ассеты
```
Addressables Groups Window:

Core_Local:
  ✓ ui_main_button
  ✓ material_default
  ✓ audio_click_sound

Levels_Remote (опционально):
  ✓ levels_level01_scene
  ✓ levels_level02_scene
```

### 9. Analyze
```
Addressables → Analyze → Check for Content Update Restrictions
→ Должно быть без ошибок

Addressables → Analyze → Check Duplicate Bundle Dependencies
→ Проверить дубликаты
```

---

## 🏗️ BUILD ADDRESSABLES (3 минуты)

### 10. Build Content
```
Addressables Groups Window:
→ Build → New Build → Default Build Script

Wait for build...
✓ Build completed successfully
```

### 11. Проверить результат
```
Assets/ServerData/[Platform]/
  Должны появиться .bundle файлы:
  - core_local_assets_all.bundle
  - catalog.json
  - catalog.hash
```

---

## 🎮 ТЕСТИРОВАНИЕ (2 минуты)

### 12. Запустить MainMenu
```
Play Mode

Нажать "Download Core Assets"
→ Должно загрузиться успешно! ✓

Проверить:
  ✓ Cache Size обновился
  ✓ Прогресс показался
  ✓ Статус "Core assets downloaded!"
```

---

## 🚀 ГОТОВО!

Теперь у тебя:
- ✅ Правильная структура Addressables
- ✅ 3 Core ассета готовы к загрузке
- ✅ Группы настроены
- ✅ Build успешный
- ✅ MainMenu работает без ошибок

---

## 📝 СЛЕДУЮЩИЕ ШАГИ:

1. **Создать Loading сцену** (10 мин)
2. **Добавить больше ассетов** (по желанию)
3. **Настроить Remote CDN** (для продакшена)
4. **Собрать WebGL билд**

---

**Начинай с пункта 1! Создавай папки и ассеты!** 🎨
