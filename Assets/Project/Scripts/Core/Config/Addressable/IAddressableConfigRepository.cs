using Project.Core.Services.Addressable.Models;

namespace Project.Core.Config.Addressable
{
    /// <summary>
    /// Repository interface for Addressable configuration access
    /// Интерфейс репозитория для доступа к конфигурации Addressables
    /// </summary>
    public interface IAddressableConfigRepository
    {
        /// <summary>
        /// Get Addressable system settings / Получить настройки системы Addressables
        /// </summary>
        AddressableSettings GetSettings();
        
        /// <summary>
        /// Get core asset keys / Получить ключи основных ресурсов
        /// </summary>
        string[] GetCoreAssetKeys();
        
        /// <summary>
        /// Get remote asset keys / Получить ключи удаленных ресурсов
        /// </summary>
        string[] GetRemoteAssetKeys();
        
        /// <summary>
        /// Get all asset keys / Получить все ключи ресурсов
        /// </summary>
        string[] GetAllAssetKeys();
        
        /// <summary>
        /// Get group configurations / Получить конфигурации групп
        /// </summary>
        GroupConfig[] GetGroupConfigs();
        
        /// <summary>
        /// Check if asset key is core / Проверить, является ли ключ ресурса основным
        /// </summary>
        bool IsCoreAsset(string key);
        
        /// <summary>
        /// Get group configuration by name / Получить конфигурацию группы по имени
        /// </summary>
        GroupConfig GetGroupConfig(string groupName);
        
        /// <summary>
        /// Get current profile name based on build settings / Получить имя текущего профиля на основе настроек сборки
        /// </summary>
        string GetCurrentProfileName();
    }
}