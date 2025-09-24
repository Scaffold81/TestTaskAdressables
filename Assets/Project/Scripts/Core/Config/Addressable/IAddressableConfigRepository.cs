using Project.Core.Services.Addressable.Models;

namespace Project.Core.Config.Addressable
{
    /// <summary>
    /// Repository interface for Addressable configuration
    /// Интерфейс репозитория для конфигурации Addressables
    /// </summary>
    public interface IAddressableConfigRepository
    {
        /// <summary>
        /// Get Addressable settings / Получить настройки Addressables
        /// </summary>
        AddressableSettings GetSettings();
        
        /// <summary>
        /// Get core asset keys / Получить ключи основных ресурсов
        /// </summary>
        string[] GetCoreAssetKeys();
        
        /// <summary>
        /// Get remote groups configuration / Получить конфигурацию удаленных групп
        /// </summary>
        GroupConfig[] GetRemoteGroups();
        
        /// <summary>
        /// Get available labels / Получить доступные метки
        /// </summary>
        string[] GetAvailableLabels();
        
        /// <summary>
        /// Get all asset keys / Получить все ключи ресурсов
        /// </summary>
        string[] GetAllAssetKeys();
        
        /// <summary>
        /// Find group by asset key / Найти группу по ключу ресурса
        /// </summary>
        string FindGroupByAssetKey(string assetKey);
        
        /// <summary>
        /// Get current profile name based on build settings / Получить имя текущего профиля на основе настроек билда
        /// </summary>
        string GetCurrentProfileName();
    }
}