# AddressableAssets Folder Structure

Эта папка содержит все ресурсы для системы Addressables, организованные по группам.

## Структура папок:

### Core/
**Группа**: Core_Local  
**Описание**: Критичные ресурсы, включаемые в билд  
**Bundle Mode**: Pack Together  
**Compression**: LZ4  
**Содержит**: UI префабы, базовые материалы, критичные звуки

### Characters/
**Группа**: Characters_Remote  
**Описание**: Префабы персонажей, загружаемые удаленно  
**Bundle Mode**: Pack Together By Label  
**Compression**: LZMA  
**Содержит**: Player prefabs, Enemy prefabs, Character textures

### Environment/
**Группа**: Environment_Remote  
**Описание**: Ресурсы окружения  
**Bundle Mode**: Pack Together  
**Compression**: LZMA  
**Содержит**: Props, Terrain assets, Environment textures

### Effects/
**Группа**: Effects_Remote  
**Описание**: Визуальные эффекты  
**Bundle Mode**: Pack Separately  
**Compression**: LZ4  
**Содержит**: Particle systems, Shaders, VFX prefabs

### Levels/
**Группа**: Levels_Remote  
**Описание**: Ресурсы уровней с отдельным каталогом  
**Bundle Mode**: Pack Together By Label  
**Compression**: LZMA  
**Содержит**: Scene assets, Level-specific prefabs

## Правила именования ключей:

- **UI элементы**: `ui_main_menu`, `ui_loading_screen`
- **Материалы**: `material_default`, `material_character_base`
- **Звуки**: `audio_click_sound`, `audio_background_music`
- **Персонажи**: `characters_player_prefab`, `characters_enemy_01`
- **Окружение**: `environment_tree_01`, `environment_building_house`
- **Эффекты**: `effects_explosion_particles`, `effects_magic_shader`
- **Уровни**: `levels_level01_scene`, `levels_tutorial_scene`

## Labels система:
- `core`, `ui`, `character`, `environment`, `effect`, `level`
- `audio`, `texture`, `prefab`, `scene`
