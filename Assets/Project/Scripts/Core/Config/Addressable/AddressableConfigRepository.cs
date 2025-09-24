using UnityEngine;
using Project.Core.Services.Addressable.Models;
using System.Linq;

namespace Project.Core.Config.Addressable
{
    /// <summary>
    /// Repository implementation for Addressable configuration access
    /// Реализация репозитория для доступа к конфигурации Addressables
    /// </summary>
    public class AddressableConfigRepository : IAddressableConfigRepository
    {
        private readonly AddressableConfig _config;
        
        /// <summary>
        /// Constructor with config injection / Конструктор с инжекцией конфига
        /// </summary>
        public AddressableConfigRepository(AddressableConfig config)
        {
            _config = config ?? throw new System.ArgumentNullException(nameof(config));
        }
        
        /// <summary>
        /// Get Addressable system settings / Получить настройки системы Addressables
        /// </summary>
        public AddressableSettings GetSettings()
        {
            return _config.Settings;
        }
        
        /// <summary>
        /// Get core asset keys / Получить ключи основных ресурсов
        /// </summary>
        public string[] GetCoreAssetKeys()
        {
            return _config.CoreAssetKeys;
        }
        
        /// <summary>
        /// Get remote asset keys / Получить ключи удаленных ресурсов
        /// </summary>
        public string[] GetRemoteAssetKeys()
        {
            return _config.RemoteAssetKeys;
        }
        
        /// <summary>
        /// Get all asset keys / Получить все ключи ресурсов
        /// </summary>
        public string[] GetAllAssetKeys()
        {
            return _config.GetAllAssetKeys();
        }
        
        /// <summary>
        /// Get group configurations / Получить конфигурации групп
        /// </summary>
        public GroupConfig[] GetGroupConfigs()
        {
            return _config.GroupConfigs;
        }
        
        /// <summary>
        /// Check if asset key is core / Проверить, является ли ключ ресурса основным
        /// </summary>
        public bool IsCoreAsset(string key)
        {
            return _config.IsCoreAsset(key);
        }
        
        /// <summary>
        /// Get group configuration by name / Получить конфигурацию группы по имени
        /// </summary>
        public GroupConfig GetGroupConfig(string groupName)
        {
            return _config.GroupConfigs.FirstOrDefault(g => g.GroupName == groupName);
        }
        
        /// <summary>
        /// Get current profile name based on build settings / Получить имя текущего профиля на основе настроек сборки
        /// </summary>
        public string GetCurrentProfileName()
        {
            var settings = GetSettings();
            
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            return settings.DevelopmentProfile;
#elif STAGING
            return "Staging";
#else
            return settings.ProductionProfile;
#endif
        }
    }
}