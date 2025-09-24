using Project.Core.Services.Addressable.Models;
using UnityEngine;

namespace Project.Core.Config.Addressable
{
    /// <summary>
    /// ScriptableObject configuration for Addressables system
    /// ScriptableObject конфигурация для системы Addressables
    /// </summary>
    [CreateAssetMenu(fileName = "AddressableConfig", menuName = "Project/Config/Addressable Config")]
    public class AddressableConfig : ScriptableObject
    {
        [Header("System Settings / Системные настройки")]
        
        /// <summary>
        /// Main settings for Addressable system / Основные настройки системы Addressables
        /// </summary>
        [SerializeField] private AddressableSettings _settings = new AddressableSettings();
        
        [Header("Group Configuration / Конфигурация групп")]
        
        /// <summary>
        /// Core group assets keys / Ключи ресурсов основной группы
        /// </summary>
        [SerializeField] private string[] _coreAssetKeys = {
            "ui_main_menu",
            "ui_loading_screen", 
            "material_default",
            "audio_click_sound"
        };
        
        /// <summary>
        /// Remote groups configuration / Конфигурация удаленных групп
        /// </summary>
        [SerializeField] private GroupConfig[] _remoteGroups = {
            new GroupConfig("UI_Remote", new[] { "ui_settings_panel", "ui_shop_window" }),
            new GroupConfig("Characters_Remote", new[] { "characters_player_prefab", "characters_enemy_01" }),
            new GroupConfig("Environment_Remote", new[] { "environment_tree_01", "environment_building_house" }),
            new GroupConfig("Effects_Remote", new[] { "effects_explosion_particles", "effects_magic_shader" }),
            new GroupConfig("Levels_Remote", new[] { "levels_level01_scene", "levels_tutorial_scene" })
        };
        
        [Header("Labels Configuration / Конфигурация меток")]
        
        /// <summary>
        /// Available labels for assets / Доступные метки для ресурсов
        /// </summary>
        [SerializeField] private string[] _availableLabels = {
            "core", "ui", "character", "environment", "effect", "level",
            "audio", "texture", "prefab", "scene"
        };
        
        /// <summary>
        /// Get settings / Получить настройки
        /// </summary>
        public AddressableSettings Settings => _settings;
        
        /// <summary>
        /// Get core asset keys / Получить ключи основных ресурсов
        /// </summary>
        public string[] CoreAssetKeys => _coreAssetKeys;
        
        /// <summary>
        /// Get remote groups configuration / Получить конфигурацию удаленных групп
        /// </summary>
        public GroupConfig[] RemoteGroups => _remoteGroups;
        
        /// <summary>
        /// Get available labels / Получить доступные метки
        /// </summary>
        public string[] AvailableLabels => _availableLabels;
        
        /// <summary>
        /// Get all asset keys from all groups / Получить все ключи ресурсов из всех групп
        /// </summary>
        public string[] GetAllAssetKeys()
        {
            var allKeys = new System.Collections.Generic.List<string>();
            allKeys.AddRange(_coreAssetKeys);
            
            foreach (var group in _remoteGroups)
            {
                allKeys.AddRange(group.AssetKeys);
            }
            
            return allKeys.ToArray();
        }
        
        /// <summary>
        /// Find group by asset key / Найти группу по ключу ресурса
        /// </summary>
        public string FindGroupByAssetKey(string assetKey)
        {
            // Check core group first
            foreach (var coreKey in _coreAssetKeys)
            {
                if (coreKey == assetKey)
                    return "Core_Local";
            }
            
            // Check remote groups
            foreach (var group in _remoteGroups)
            {
                foreach (var key in group.AssetKeys)
                {
                    if (key == assetKey)
                        return group.GroupName;
                }
            }
            
            return "Unknown";
        }
    }
    
    /// <summary>
    /// Configuration for asset group / Конфигурация группы ресурсов
    /// </summary>
    [System.Serializable]
    public class GroupConfig
    {
        /// <summary>
        /// Group name / Название группы
        /// </summary>
        public string GroupName;
        
        /// <summary>
        /// Asset keys in this group / Ключи ресурсов в этой группе
        /// </summary>
        public string[] AssetKeys;
        
        /// <summary>
        /// Default constructor / Конструктор по умолчанию
        /// </summary>
        public GroupConfig() { }
        
        /// <summary>
        /// Constructor with parameters / Конструктор с параметрами
        /// </summary>
        public GroupConfig(string groupName, string[] assetKeys)
        {
            GroupName = groupName;
            AssetKeys = assetKeys;
        }
    }
}