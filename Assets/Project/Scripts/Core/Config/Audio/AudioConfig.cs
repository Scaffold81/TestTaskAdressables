using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.Linq;

namespace Game.Config
{
    /// <summary>
    /// Конфигурация аудиосистемы, содержащая все аудиоклипы и настройки.
    /// Audio system configuration containing all audio clips and settings.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "Game/Config/Audio Config")]
    public class AudioConfig : ScriptableObject
    {
        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer audioMixer;
        
        [Header("All Audio Clips")]
        [SerializeField] private List<AudioClipData> audioClips = new List<AudioClipData>();
        
        [Header("Audio Settings")]
        [SerializeField] [Range(0.1f, 3f)] private float defaultMasterVolume = 1f;
        [SerializeField] [Range(0.1f, 3f)] private float defaultMusicVolume = 0.8f;
        [SerializeField] [Range(0.1f, 3f)] private float defaultSFXVolume = 1f;
        [SerializeField] [Range(0.1f, 3f)] private float defaultUIVolume = 1f;
        
        [Header("Music Playback Settings")]
        [SerializeField] private bool shuffleMusicLists = true;
        [SerializeField] private bool loopMusicLists = true;
        
        // Properties
        public AudioMixer AudioMixer => audioMixer;
        public List<AudioClipData> AudioClips => audioClips;
        
        // Settings
        public float DefaultMasterVolume => defaultMasterVolume;
        public float DefaultMusicVolume => defaultMusicVolume;
        public float DefaultSFXVolume => defaultSFXVolume;
        public float DefaultUIVolume => defaultUIVolume;
        
        // Music Playback Settings
        public bool ShuffleMusicLists => shuffleMusicLists;
        public bool LoopMusicLists => loopMusicLists;
        
        // Core Methods
        /// <summary>
        /// Получает данные аудиоклипа по идентификатору.
        /// Gets audio clip data by identifier.
        /// </summary>
        /// <param name="id">Идентификатор клипа / Clip identifier</param>
        /// <returns>Данные клипа или default / Clip data or default</returns>
        public AudioClipData GetAudioClipData(string id)
        {
            return audioClips.FirstOrDefault(clip => clip.Id.Equals(id, System.StringComparison.OrdinalIgnoreCase));
        }
        
        /// <summary>
        /// Получает аудиоклип по идентификатору.
        /// Gets audio clip by identifier.
        /// </summary>
        /// <param name="id">Идентификатор клипа / Clip identifier</param>
        /// <returns>Аудиоклип или null / Audio clip or null</returns>
        public AudioClip GetAudioClip(string id)
        {
            var clipData = GetAudioClipData(id);
            return clipData.AudioClip;
        }
        
        /// <summary>
        /// Получает все клипы определенного типа.
        /// Gets all clips of specific type.
        /// </summary>
        /// <param name="audioType">Тип аудио / Audio type</param>
        /// <returns>Список клипов / List of clips</returns>
        public List<AudioClipData> GetAudioClipsByType(AudioType audioType)
        {
            return audioClips.Where(clip => clip.MatchesType(audioType)).ToList();
        }
        
        /// <summary>
        /// Получает все клипы определенной категории.
        /// Gets all clips of specific category.
        /// </summary>
        /// <param name="category">Категория / Category</param>
        /// <returns>Список клипов / List of clips</returns>
        public List<AudioClipData> GetAudioClipsByCategory(string category)
        {
            return audioClips.Where(clip => clip.MatchesCategory(category)).ToList();
        }
        
        /// <summary>
        /// Получает все клипы определенного типа и категории.
        /// Gets all clips of specific type and category.
        /// </summary>
        /// <param name="audioType">Тип аудио / Audio type</param>
        /// <param name="category">Категория / Category</param>
        /// <returns>Список клипов / List of clips</returns>
        public List<AudioClipData> GetAudioClipsByTypeAndCategory(AudioType audioType, string category)
        {
            return audioClips.Where(clip => clip.MatchesType(audioType) && clip.MatchesCategory(category)).ToList();
        }
        
        // Random Selection
        /// <summary>
        /// Получает случайные данные клипа по типу и категории.
        /// Gets random clip data by type and category.
        /// </summary>
        /// <param name="audioType">Тип аудио / Audio type</param>
        /// <param name="category">Категория (необязательно) / Category (optional)</param>
        /// <returns>Случайные данные клипа / Random clip data</returns>
        public AudioClipData GetRandomAudioClipData(AudioType audioType, string category = "")
        {
            var clips = string.IsNullOrEmpty(category) 
                ? GetAudioClipsByType(audioType)
                : GetAudioClipsByTypeAndCategory(audioType, category);
                
            return clips.Count > 0 ? clips[Random.Range(0, clips.Count)] : default;
        }
        
        /// <summary>
        /// Получает случайный аудиоклип по типу и категории.
        /// Gets random audio clip by type and category.
        /// </summary>
        /// <param name="audioType">Тип аудио / Audio type</param>
        /// <param name="category">Категория (необязательно) / Category (optional)</param>
        /// <returns>Случайный аудиоклип / Random audio clip</returns>
        public AudioClip GetRandomAudioClip(AudioType audioType, string category = "")
        {
            var clipData = GetRandomAudioClipData(audioType, category);
            return clipData.AudioClip;
        }
        
        // Convenience Methods for Common IDs
        /// <summary>
        /// Получает случайную музыку по категории.
        /// Gets random music by category.
        /// </summary>
        /// <param name="category">Категория музыки / Music category</param>
        /// <returns>Аудиоклип музыки / Music audio clip</returns>
        public AudioClip GetMusicByCategory(string category)
        {
            return GetRandomAudioClip(AudioType.Music, category);
        }
        
        // Validation
        /// <summary>
        /// Проверяет наличие клипа по идентификатору.
        /// Checks if clip exists by identifier.
        /// </summary>
        /// <param name="id">Идентификатор клипа / Clip identifier</param>
        /// <returns>True если клип существует / True if clip exists</returns>
        public bool HasAudioClip(string id)
        {
            return audioClips.Any(clip => clip.Id.Equals(id, System.StringComparison.OrdinalIgnoreCase));
        }
        
        /// <summary>
        /// Проверяет наличие аудио в категории.
        /// Checks if audio exists in category.
        /// </summary>
        /// <param name="audioType">Тип аудио / Audio type</param>
        /// <param name="category">Категория / Category</param>
        /// <returns>True если аудио есть в категории / True if audio exists in category</returns>
        public bool HasAudioInCategory(AudioType audioType, string category)
        {
            return audioClips.Any(clip => clip.MatchesType(audioType) && clip.MatchesCategory(category));
        }
        
        /// <summary>
        /// Получает все категории для указанного типа аудио.
        /// Gets all categories for specified audio type.
        /// </summary>
        /// <param name="audioType">Тип аудио / Audio type</param>
        /// <returns>Список категорий / List of categories</returns>
        public List<string> GetAllCategories(AudioType audioType)
        {
            return audioClips
                .Where(clip => clip.MatchesType(audioType))
                .Select(clip => clip.Category)
                .Where(category => !string.IsNullOrEmpty(category))
                .Distinct()
                .ToList();
        }
        
        /// <summary>
        /// Получает все идентификаторы клипов.
        /// Gets all clip identifiers.
        /// </summary>
        /// <returns>Список идентификаторов / List of identifiers</returns>
        public List<string> GetAllIds()
        {
            return audioClips.Select(clip => clip.Id).ToList();
        }
        
        private void OnValidate()
        {
            ValidateAudioClips();
            
            // Debug info removed - only warnings and errors are logged
        }
        
        private void ValidateAudioClips()
        {
            // Remove invalid clips
            audioClips.RemoveAll(clip => !clip.IsValid());
            
            // Check for duplicate IDs
            var duplicateIds = audioClips
                .GroupBy(clip => clip.Id.ToLower())
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);
                
            foreach (var duplicateId in duplicateIds)
            {
                Debug.LogWarning($"[AudioConfig] Duplicate ID found: {duplicateId}");
            }
        }
        
        private int GetMusicClipsCount()
        {
            return audioClips.Count(clip => clip.MatchesType(AudioType.Music));
        }
        
        private int GetSFXClipsCount()
        {
            return audioClips.Count(clip => clip.MatchesType(AudioType.SFX));
        }
        
        private int GetUIClipsCount()
        {
            return audioClips.Count(clip => clip.MatchesType(AudioType.UI));
        }
    }
}