namespace Project.Core.Services.Addressable
{
    /// <summary>
    /// Constants for Addressable asset keys
    /// Константы для ключей ресурсов Addressables
    /// </summary>
    public static class AddressableKeys
    {
        /// <summary>
        /// Core assets (always in build) / Основные ресурсы (всегда в билде)
        /// </summary>
        public static class Core
        {
            public const string UI_MAIN_MENU = "ui_main_menu";
            public const string UI_LOADING_SCREEN = "ui_loading_screen";
            public const string MATERIAL_DEFAULT = "material_default";
            public const string AUDIO_CLICK_SOUND = "audio_click_sound";
        }
        
        /// <summary>
        /// UI assets (remote) / UI ресурсы (удаленные)
        /// </summary>
        public static class UI
        {
            public const string SETTINGS_PANEL = "ui_settings_panel";
            public const string SHOP_WINDOW = "ui_shop_window";
            public const string INVENTORY_PANEL = "ui_inventory_panel";
            public const string LOADING_PROGRESS = "ui_loading_progress";
            public const string DEV_OVERLAY = "ui_dev_overlay";
        }
        
        /// <summary>
        /// Character assets / Ресурсы персонажей
        /// </summary>
        public static class Characters
        {
            public const string PLAYER_PREFAB = "characters_player_prefab";
            public const string ENEMY_01 = "characters_enemy_01";
            public const string ENEMY_BOSS = "characters_enemy_boss";
            public const string NPC_MERCHANT = "characters_npc_merchant";
        }
        
        /// <summary>
        /// Environment assets / Ресурсы окружения
        /// </summary>
        public static class Environment
        {
            public const string TREE_01 = "environment_tree_01";
            public const string BUILDING_HOUSE = "environment_building_house";
            public const string ROCK_LARGE = "environment_rock_large";
            public const string GRASS_PATCH = "environment_grass_patch";
        }
        
        /// <summary>
        /// Effect assets / Ресурсы эффектов
        /// </summary>
        public static class Effects
        {
            public const string EXPLOSION_PARTICLES = "effects_explosion_particles";
            public const string MAGIC_SHADER = "effects_magic_shader";
            public const string HIT_IMPACT = "effects_hit_impact";
            public const string HEALING_AURA = "effects_healing_aura";
        }
        
        /// <summary>
        /// Level scene assets / Ресурсы сцен уровней
        /// </summary>
        public static class Levels
        {
            public const string LEVEL01_SCENE = "levels_level01_scene";
            public const string LEVEL02_SCENE = "levels_level02_scene";
            public const string TUTORIAL_SCENE = "levels_tutorial_scene";
            public const string BOSS_ARENA_SCENE = "levels_boss_arena_scene";
        }
        
        /// <summary>
        /// Get all keys as array / Получить все ключи как массив
        /// </summary>
        public static string[] GetAllKeys()
        {
            return new string[]
            {
                // Core
                Core.UI_MAIN_MENU, Core.UI_LOADING_SCREEN, Core.MATERIAL_DEFAULT, Core.AUDIO_CLICK_SOUND,
                
                // UI
                UI.SETTINGS_PANEL, UI.SHOP_WINDOW, UI.INVENTORY_PANEL, UI.LOADING_PROGRESS, UI.DEV_OVERLAY,
                
                // Characters
                Characters.PLAYER_PREFAB, Characters.ENEMY_01, Characters.ENEMY_BOSS, Characters.NPC_MERCHANT,
                
                // Environment
                Environment.TREE_01, Environment.BUILDING_HOUSE, Environment.ROCK_LARGE, Environment.GRASS_PATCH,
                
                // Effects
                Effects.EXPLOSION_PARTICLES, Effects.MAGIC_SHADER, Effects.HIT_IMPACT, Effects.HEALING_AURA,
                
                // Levels
                Levels.LEVEL01_SCENE, Levels.LEVEL02_SCENE, Levels.TUTORIAL_SCENE, Levels.BOSS_ARENA_SCENE
            };
        }
    }
    
    /// <summary>
    /// Labels for Addressable assets / Метки для ресурсов Addressables
    /// </summary>
    public static class AddressableLabels
    {
        public const string CORE = "core";
        public const string UI = "ui"; 
        public const string CHARACTER = "character";
        public const string ENVIRONMENT = "environment";
        public const string EFFECT = "effect";
        public const string LEVEL = "level";
        public const string AUDIO = "audio";
        public const string TEXTURE = "texture";
        public const string PREFAB = "prefab";
        public const string SCENE = "scene";
    }
}