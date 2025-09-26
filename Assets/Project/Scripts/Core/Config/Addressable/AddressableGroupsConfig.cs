using UnityEngine;

namespace Project.Core.Config.Addressable
{
    /// <summary>
    /// Configuration for Addressable Groups setup
    /// Конфигурация для настройки групп Addressables
    /// </summary>
    [CreateAssetMenu(fileName = "AddressableGroupsConfig", menuName = "Addressables/Groups Config")]
    public class AddressableGroupsConfig : ScriptableObject
    {
        [Header("Group Configuration")]
        [Tooltip("Core assets that must be included in build")]
        public AddressableGroupInfo coreGroup = new AddressableGroupInfo
        {
            groupName = "Core_Local",
            bundleMode = BundlePackingMode.PackTogether,
            compression = BundleCompressionMode.LZ4,
            includeInBuild = true,
            allowWrites = true,
            assetKeys = new string[]
            {
                "ui_main_menu",
                "ui_loading_screen", 
                "material_default",
                "audio_click_sound"
            }
        };
        
        [Tooltip("UI elements loaded remotely")]
        public AddressableGroupInfo uiGroup = new AddressableGroupInfo
        {
            groupName = "UI_Remote",
            bundleMode = BundlePackingMode.PackSeparately,
            compression = BundleCompressionMode.LZ4,
            includeInBuild = false,
            allowWrites = true,
            assetKeys = new string[]
            {
                "ui_game_hud",
                "ui_settings_panel",
                "ui_inventory",
                "ui_shop"
            }
        };
        
        [Tooltip("Character assets")]
        public AddressableGroupInfo charactersGroup = new AddressableGroupInfo
        {
            groupName = "Characters_Remote",
            bundleMode = BundlePackingMode.PackTogetherByLabel,
            compression = BundleCompressionMode.LZMA,
            includeInBuild = false,
            allowWrites = true,
            assetKeys = new string[]
            {
                "characters_player_prefab",
                "characters_enemy_01",
                "characters_enemy_02",
                "characters_npc_merchant"
            }
        };
        
        [Tooltip("Environment assets")]
        public AddressableGroupInfo environmentGroup = new AddressableGroupInfo
        {
            groupName = "Environment_Remote",
            bundleMode = BundlePackingMode.PackTogether,
            compression = BundleCompressionMode.LZMA,
            includeInBuild = false,
            allowWrites = true,
            assetKeys = new string[]
            {
                "environment_tree_01",
                "environment_rock_01",
                "environment_building_01"
            }
        };
        
        [Tooltip("Visual effects")]
        public AddressableGroupInfo effectsGroup = new AddressableGroupInfo
        {
            groupName = "Effects_Remote",
            bundleMode = BundlePackingMode.PackSeparately,
            compression = BundleCompressionMode.LZ4,
            includeInBuild = false,
            allowWrites = true,
            assetKeys = new string[]
            {
                "effects_explosion_particles",
                "effects_fire_shader",
                "effects_water_ripple"
            }
        };
        
        [Tooltip("Level scenes and related assets")]
        public AddressableGroupInfo levelsGroup = new AddressableGroupInfo
        {
            groupName = "Levels_Remote",
            bundleMode = BundlePackingMode.PackTogetherByLabel,
            compression = BundleCompressionMode.LZMA,
            includeInBuild = false,
            allowWrites = true,
            assetKeys = new string[]
            {
                "levels_level01_scene",
                "levels_level02_scene"
            }
        };
        
        [Header("Build Configuration")]
        [Tooltip("Platform-specific settings")]
        public PlatformBuildSettings webGLSettings = new PlatformBuildSettings
        {
            maxConcurrentWebRequests = 6,
            catalogDownloadTimeout = 30,
            disableCatalogUpdateOnStart = false
        };
        
        public PlatformBuildSettings androidSettings = new PlatformBuildSettings
        {
            useAssetDatabase = false,
            simulateGroups = false
        };
        
        /// <summary>
        /// Get all group configurations
        /// Получить все конфигурации групп
        /// </summary>
        public AddressableGroupInfo[] GetAllGroups()
        {
            return new AddressableGroupInfo[]
            {
                coreGroup,
                uiGroup,
                charactersGroup,
                environmentGroup,
                effectsGroup,
                levelsGroup
            };
        }
        
        /// <summary>
        /// Get groups that should be included in build
        /// Получить группы, которые должны быть включены в сборку
        /// </summary>
        public AddressableGroupInfo[] GetLocalGroups()
        {
            return System.Array.FindAll(GetAllGroups(), group => group.includeInBuild);
        }
        
        /// <summary>
        /// Get groups that are loaded remotely
        /// Получить группы, которые загружаются удаленно
        /// </summary>
        public AddressableGroupInfo[] GetRemoteGroups()
        {
            return System.Array.FindAll(GetAllGroups(), group => !group.includeInBuild);
        }
    }
    
    /// <summary>
    /// Information about single Addressable group
    /// Информация об одной группе Addressables
    /// </summary>
    [System.Serializable]
    public class AddressableGroupInfo
    {
        [Tooltip("Name of the addressable group")]
        public string groupName;
        
        [Tooltip("How assets are bundled together")]
        public BundlePackingMode bundleMode;
        
        [Tooltip("Compression algorithm for bundles")]
        public BundleCompressionMode compression;
        
        [Tooltip("Include assets in build or load remotely")]
        public bool includeInBuild;
        
        [Tooltip("Allow content updates for this group")]
        public bool allowWrites;
        
        [Tooltip("Asset keys that belong to this group")]
        public string[] assetKeys;
        
        /// <summary>
        /// Get display name for bundle mode
        /// Получить отображаемое имя для режима пакетирования
        /// </summary>
        public string GetBundleModeDisplayName()
        {
            return bundleMode switch
            {
                BundlePackingMode.PackTogether => "Pack Together",
                BundlePackingMode.PackSeparately => "Pack Separately", 
                BundlePackingMode.PackTogetherByLabel => "Pack Together By Label",
                _ => "Unknown"
            };
        }
        
        /// <summary>
        /// Get display name for compression mode
        /// Получить отображаемое имя для режима сжатия
        /// </summary>
        public string GetCompressionDisplayName()
        {
            return compression switch
            {
                BundleCompressionMode.None => "Uncompressed",
                BundleCompressionMode.LZ4 => "LZ4 (Fast)",
                BundleCompressionMode.LZMA => "LZMA (Small)",
                _ => "Unknown"
            };
        }
    }
    
    /// <summary>
    /// Bundle packing modes for groups
    /// Режимы пакетирования бандлов для групп
    /// </summary>
    public enum BundlePackingMode
    {
        PackTogether,        // Pack all assets in one bundle
        PackSeparately,      // Each asset gets its own bundle
        PackTogetherByLabel  // Group by labels into bundles
    }
    
    /// <summary>
    /// Bundle compression modes
    /// Режимы сжатия бандлов
    /// </summary>
    public enum BundleCompressionMode
    {
        None,   // No compression - fastest loading
        LZ4,    // Fast compression/decompression
        LZMA    // Best compression ratio
    }
    
    /// <summary>
    /// Platform-specific build settings
    /// Настройки сборки для конкретной платформы
    /// </summary>
    [System.Serializable]
    public class PlatformBuildSettings
    {
        [Header("Network Settings")]
        public int maxConcurrentWebRequests = 6;
        public int catalogDownloadTimeout = 30;
        public bool disableCatalogUpdateOnStart = false;
        
        [Header("Development Settings")]
        public bool useAssetDatabase = false;
        public bool simulateGroups = false;
    }
}
