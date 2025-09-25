using System;
using UnityEngine;

namespace Project.Core.Services.Addressable.Models
{
    /// <summary>
    /// Information about Addressable catalog
    /// Информация о каталоге Addressables
    /// </summary>
    [Serializable]
    public class CatalogInfo
    {
        /// <summary>
        /// Catalog name / Имя каталога
        /// </summary>
        public string Name;
        
        /// <summary>
        /// Catalog version / Версия каталога
        /// </summary>
        public string Version;
        
        /// <summary>
        /// Catalog URL / URL каталога
        /// </summary>
        public string Url;
        
        /// <summary>
        /// Is separate catalog / Является ли отдельным каталогом
        /// </summary>
        public bool IsSeparate;
        
        /// <summary>
        /// Associated group names / Связанные группы
        /// </summary>
        public string[] GroupNames;
        
        /// <summary>
        /// Last update time / Время последнего обновления
        /// </summary>
        public DateTime LastUpdated;
        
        /// <summary>
        /// Constructor / Конструктор
        /// </summary>
        public CatalogInfo(string name, string version, string url, bool isSeparate, string[] groupNames)
        {
            Name = name;
            Version = version;
            Url = url;
            IsSeparate = isSeparate;
            GroupNames = groupNames ?? new string[0];
            LastUpdated = DateTime.Now;
        }
        
        /// <summary>
        /// Get display name / Получить отображаемое имя
        /// </summary>
        public string GetDisplayName()
        {
            return $"{Name} v{Version}";
        }
        
        /// <summary>
        /// Get info string / Получить информационную строку
        /// </summary>
        public override string ToString()
        {
            return $"Catalog: {GetDisplayName()} | Groups: {string.Join(", ", GroupNames)} | Updated: {LastUpdated:yyyy-MM-dd HH:mm}";
        }
    }
}