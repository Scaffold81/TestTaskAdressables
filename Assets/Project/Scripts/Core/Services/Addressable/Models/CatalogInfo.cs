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
        /// Catalog name / Название каталога
        /// </summary>
        public string Name;
        
        /// <summary>
        /// Catalog version / Версия каталога
        /// </summary>
        public string Version;
        
        /// <summary>
        /// Catalog file name / Имя файла каталога
        /// </summary>
        public string FileName;
        
        /// <summary>
        /// Is this a remote catalog / Является ли каталог удаленным
        /// </summary>
        public bool IsRemote;
        
        /// <summary>
        /// Last update timestamp / Временная метка последнего обновления
        /// </summary>
        public DateTime LastUpdated;
        
        /// <summary>
        /// Groups included in this catalog / Группы, включенные в этот каталог
        /// </summary>
        public string[] IncludedGroups;
        
        /// <summary>
        /// Default constructor / Конструктор по умолчанию
        /// </summary>
        public CatalogInfo()
        {
            LastUpdated = DateTime.Now;
            IncludedGroups = Array.Empty<string>();
        }
        
        /// <summary>
        /// Constructor with parameters / Конструктор с параметрами
        /// </summary>
        public CatalogInfo(string name, string version, string fileName, bool isRemote, string[] includedGroups)
        {
            Name = name;
            Version = version;
            FileName = fileName;
            IsRemote = isRemote;
            LastUpdated = DateTime.Now;
            IncludedGroups = includedGroups ?? Array.Empty<string>();
        }
        
        /// <summary>
        /// Check if catalog needs update / Проверить, нужно ли обновить каталог
        /// </summary>
        public bool NeedsUpdate(TimeSpan maxAge)
        {
            return (DateTime.Now - LastUpdated) > maxAge;
        }
        
        /// <summary>
        /// Get info string / Получить информационную строку
        /// </summary>
        public override string ToString()
        {
            var location = IsRemote ? "Remote" : "Local";
            return $"{Name} v{Version} ({location}) - {IncludedGroups.Length} groups - Updated: {LastUpdated:g}";
        }
    }
}