using UnityEngine;
using System;

namespace Game.Config
{
    /// <summary>
    /// Структура данных для аудиоклипов с настройками воспроизведения.
    /// Structure for audio clip data with playback settings.
    /// </summary>
    [Serializable]
    public struct AudioClipData
    {
        [SerializeField] private string id;
        [SerializeField] private AudioClip audioClip;
        [SerializeField] private AudioType audioType;
        [SerializeField] private string category;
        [SerializeField] [Range(0.1f, 2f)] private float defaultVolume;
        [SerializeField] [Range(0.1f, 3f)] private float defaultPitch;
        [SerializeField] private bool loop;
        
        public string Id => id;
        public AudioClip AudioClip => audioClip;
        public AudioType AudioType => audioType;
        public string Category => category;
        public float DefaultVolume => defaultVolume;
        public float DefaultPitch => defaultPitch;
        public bool Loop => loop;
        
        /// <summary>
        /// Конструктор для создания данных аудиоклипа.
        /// Constructor for creating audio clip data.
        /// </summary>
        /// <param name="id">Уникальный идентификатор клипа / Unique clip identifier</param>
        /// <param name="audioClip">Аудиоклип / Audio clip</param>
        /// <param name="audioType">Тип аудио / Audio type</param>
        /// <param name="category">Категория клипа / Clip category</param>
        /// <param name="defaultVolume">Громкость по умолчанию / Default volume</param>
        /// <param name="defaultPitch">Высота тона по умолчанию / Default pitch</param>
        /// <param name="loop">Зацикливать воспроизведение / Loop playback</param>
        public AudioClipData(string id, AudioClip audioClip, AudioType audioType, string category = "", 
                           float defaultVolume = 1f, float defaultPitch = 1f, bool loop = false)
        {
            this.id = id;
            this.audioClip = audioClip;
            this.audioType = audioType;
            this.category = category;
            this.defaultVolume = defaultVolume;
            this.defaultPitch = defaultPitch;
            this.loop = loop;
        }
        
        /// <summary>
        /// Проверяет корректность данных клипа.
        /// Checks if clip data is valid.
        /// </summary>
        /// <returns>True если данные корректны / True if data is valid</returns>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(id) && audioClip != null;
        }
        
        /// <summary>
        /// Проверяет соответствие типу аудио.
        /// Checks if matches audio type.
        /// </summary>
        /// <param name="type">Тип для сравнения / Type to compare</param>
        /// <returns>True если типы совпадают / True if types match</returns>
        public bool MatchesType(AudioType type)
        {
            return audioType == type;
        }
        
        /// <summary>
        /// Проверяет соответствие категории.
        /// Checks if matches category.
        /// </summary>
        /// <param name="searchCategory">Категория для поиска / Category to search</param>
        /// <returns>True если категории совпадают / True if categories match</returns>
        public bool MatchesCategory(string searchCategory)
        {
            return !string.IsNullOrEmpty(category) && 
                   category.Equals(searchCategory, StringComparison.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// Возвращает строковое представление данных клипа.
        /// Returns string representation of clip data.
        /// </summary>
        /// <returns>Строковое представление / String representation</returns>
        public override string ToString()
        {
            return $"AudioClipData[{id}] - {audioType} - {category}";
        }
    }
    
    /// <summary>
    /// Типы аудио для категоризации звуков.
    /// Audio types for sound categorization.
    /// </summary>
    public enum AudioType
    {
        Music,
        SFX,
        UI,
        Voice,
        Ambient
    }
}