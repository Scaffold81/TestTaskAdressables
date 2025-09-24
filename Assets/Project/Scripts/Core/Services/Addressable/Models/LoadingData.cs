using System;
using UnityEngine;

namespace Project.Core.Services.Addressable.Models
{
    /// <summary>
    /// Data model for asset loading telemetry
    /// Модель данных для телеметрии загрузки ресурсов
    /// </summary>
    [Serializable]
    public class LoadingData
    {
        /// <summary>
        /// Asset key / Ключ ресурса
        /// </summary>
        public string AssetKey;
        
        /// <summary>
        /// Group name / Название группы
        /// </summary>
        public string GroupName;
        
        /// <summary>
        /// Size in bytes / Размер в байтах
        /// </summary>
        public long SizeBytes;
        
        /// <summary>
        /// Loading time in seconds / Время загрузки в секундах
        /// </summary>
        public float LoadTime;
        
        /// <summary>
        /// Source of loading (Cache/Network) / Источник загрузки (кеш/сеть)
        /// </summary>
        public string Source;
        
        /// <summary>
        /// When asset was loaded / Когда ресурс был загружен
        /// </summary>
        public DateTime LoadedAt;
        
        /// <summary>
        /// Default constructor / Конструктор по умолчанию
        /// </summary>
        public LoadingData()
        {
            LoadedAt = DateTime.Now;
        }
        
        /// <summary>
        /// Constructor with parameters / Конструктор с параметрами
        /// </summary>
        public LoadingData(string assetKey, string groupName, long sizeBytes, float loadTime, string source)
        {
            AssetKey = assetKey;
            GroupName = groupName;
            SizeBytes = sizeBytes;
            LoadTime = loadTime;
            Source = source;
            LoadedAt = DateTime.Now;
        }
        
        /// <summary>
        /// Get formatted size string / Получить отформатированную строку размера
        /// </summary>
        public string GetFormattedSize()
        {
            if (SizeBytes < 1024)
                return $"{SizeBytes} B";
            if (SizeBytes < 1024 * 1024)
                return $"{SizeBytes / 1024f:F1} KB";
            return $"{SizeBytes / (1024f * 1024f):F1} MB";
        }
        
        /// <summary>
        /// Get loading info string / Получить строку информации о загрузке
        /// </summary>
        public override string ToString()
        {
            return $"[{GroupName}] {AssetKey} - {GetFormattedSize()} in {LoadTime:F2}s from {Source}";
        }
    }
}