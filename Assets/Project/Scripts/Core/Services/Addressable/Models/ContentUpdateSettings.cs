using System;
using UnityEngine;

namespace Project.Core.Services.Addressable.Models
{
    /// <summary>
    /// Settings for content update workflow
    /// Настройки для процесса обновления контента
    /// </summary>
    [Serializable]
    public class ContentUpdateSettings
    {
        /// <summary>
        /// Enable automatic content update check / Включить автоматическую проверку обновлений контента
        /// </summary>
        public bool EnableAutoCheck = true;
        
        /// <summary>
        /// Check interval in seconds / Интервал проверки в секундах
        /// </summary>
        public int CheckIntervalSeconds = 300;
        
        /// <summary>
        /// Auto download updates / Автоматически загружать обновления
        /// </summary>
        public bool AutoDownload = false;
        
        /// <summary>
        /// Show update notification / Показывать уведомление об обновлении
        /// </summary>
        public bool ShowNotification = true;
        
        /// <summary>
        /// Update timeout in seconds / Таймаут обновления в секундах
        /// </summary>
        public int UpdateTimeoutSeconds = 30;
        
        /// <summary>
        /// Catalogs to update / Каталоги для обновления
        /// </summary>
        public string[] CatalogsToUpdate = { "main", "catalog_levels" };
        
        /// <summary>
        /// Groups that can be updated post-release / Группы, которые можно обновлять после релиза
        /// </summary>
        public string[] UpdatableGroups = 
        {
            "UI_Remote",
            "Characters_Remote", 
            "Environment_Remote",
            "Effects_Remote",
            "Levels_Remote"
        };
        
        public TimeSpan GetCheckInterval() => TimeSpan.FromSeconds(CheckIntervalSeconds);
        public TimeSpan GetUpdateTimeout() => TimeSpan.FromSeconds(UpdateTimeoutSeconds);
        
        public bool IsGroupUpdatable(string groupName)
        {
            if (string.IsNullOrEmpty(groupName) || UpdatableGroups == null)
                return false;
                
            foreach (var group in UpdatableGroups)
            {
                if (group.Equals(groupName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            
            return false;
        }
    }
}