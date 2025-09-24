using UnityEngine;
using Project.Core.Services.Addressable.Models;

namespace Project.Core.Config.Addressable
{
    /// <summary>
    /// ScriptableObject configuration for Addressable system
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
        
        [Header("Asset Keys / Ключи ресурсов")]
        
        /// <summary>
        /// Core asset keys that are always available / Ключи основных ресурсов, всегда доступных
        /// </summary>
        [SerializeField] private string[] _coreAssetKeys = new string[]
        {
            "ui_main_menu",
            "ui_loading_screen", 
            "material_default",
            "audio_click_sound"
        };
        
        /// <summary>
        /// Remote asset keys for different groups / Ключи удаленных ресурсов для разных групп
        /// </summary>
        [SerializeField] private string[] _remoteAssetKeys = new string[]
        {
            "characters_player_prefab",
            "environment_tree_01",
            "effects_explosion_particles",
            "levels_level01_scene"
        };
        
        [Header("Group Configuration / Конфигурация групп")]
        
        /// <summary>
        /// Group names and their priorities / Названия групп и их приоритеты
        /// </summary>
        [SerializeField] private GroupConfig[] _groupConfigs = new GroupConfig[]
        {
            new GroupConfig("Core_Local", 1, true),
            new GroupConfig("UI_Remote", 2, false),
            new GroupConfig("Characters_Remote", 3, false),
            new GroupConfig("Environment_Remote", 4, false),
            new GroupConfig("Effects_Remote", 5, false),
            new GroupConfig("Levels_Remote", 6, false)
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
        /// Get remote asset keys / Получить ключи удаленных ресурсов
        /// </summary>
        public string[] RemoteAssetKeys => _remoteAssetKeys;
        
        /// <summary>
        /// Get group configurations / Получить конфигурации групп
        /// </summary>
        public GroupConfig[] GroupConfigs => _groupConfigs;
        
        /// <summary>
        /// Get all asset keys / Получить все ключи ресурсов
        /// </summary>
        public string[] GetAllAssetKeys()
        {
            var allKeys = new string[_coreAssetKeys.Length + _remoteAssetKeys.Length];
            _coreAssetKeys.CopyTo(allKeys, 0);
            _remoteAssetKeys.CopyTo(allKeys, _coreAssetKeys.Length);
            return allKeys;
        }
        
        /// <summary>
        /// Check if key is core asset / Проверить, является ли ключ основным ресурсом
        /// </summary>
        public bool IsCoreAsset(string key)
        {
            return System.Array.IndexOf(_coreAssetKeys, key) >= 0;
        }
    }
    
    /// <summary>
    /// Configuration for Addressable group / Конфигурация группы Addressables
    /// </summary>
    [System.Serializable]
    public class GroupConfig
    {
        /// <summary>
        /// Group name / Название группы
        /// </summary>
        public string GroupName;
        
        /// <summary>
        /// Loading priority (lower = higher priority) / Приоритет загрузки (меньше = выше приоритет)
        /// </summary>
        public int Priority;
        
        /// <summary>
        /// Is included in build / Включена в сборку
        /// </summary>
        public bool IncludeInBuild;
        
        /// <summary>
        /// Constructor / Конструктор
        /// </summary>
        public GroupConfig(string groupName, int priority, bool includeInBuild)
        {
            GroupName = groupName;
            Priority = priority;
            IncludeInBuild = includeInBuild;
        }
    }
}