using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using Zenject;

namespace Game.Config
{
    /// <summary>
    /// Репозиторий для работы с конфигурацией аудио. Предоставляет удобные методы для получения аудиоклипов.
    /// Repository for working with audio configuration. Provides convenient methods for getting audio clips.
    /// </summary>
    public class AudioConfigRepository : IAudioConfigRepository
    {
        private readonly AudioConfig audioConfig;
        
        // Common Audio IDs - можно настроить под проект
        private const string EXPLOSION_ID = "explosion";
        private const string BUTTON_CLICK_ID = "button_click";
        private const string ERROR_ID = "error";
        private const string SUCCESS_ID = "success";
        
        // Music Categories
        private const string MENU_CATEGORY = "menu";
        private const string GAMEPLAY_CATEGORY = "gameplay";
        private const string VICTORY_CATEGORY = "victory";
        private const string DEFEAT_CATEGORY = "defeat";
        private const string AMBIENT_CATEGORY = "ambient";
        
        /// <summary>
        /// Конструктор репозитория с инъекцией зависимостей.
        /// Repository constructor with dependency injection.
        /// </summary>
        /// <param name="audioConfig">Конфигурация аудио / Audio configuration</param>
        [Inject]
        public AudioConfigRepository(AudioConfig audioConfig)
        {
            this.audioConfig = audioConfig;
            
            if (audioConfig == null)
            {
                Debug.LogError("[AudioConfigRepository] AudioConfig is null! Please assign it in GlobalConfigInstaller.");
                return;
            }
            
            ValidateConfig();
            // Repository initialized successfully
        }
        
        private void ValidateConfig()
        {
            if (!HasValidAudioMixer())
            {
                Debug.LogWarning("[AudioConfigRepository] Audio Mixer is not assigned!");
            }
            
            if (audioConfig.AudioClips.Count == 0)
            {
                Debug.LogWarning("[AudioConfigRepository] No audio clips found in config!");
            }
            
            // Config validation complete
        }
        
        #region Audio Mixer
        /// <summary>
        /// Получает аудио миксер.
        /// Gets audio mixer.
        /// </summary>
        /// <returns>Аудио миксер / Audio mixer</returns>
        public AudioMixer GetAudioMixer()
        {
            return audioConfig?.AudioMixer;
        }
        
        /// <summary>
        /// Проверяет наличие действительного аудио миксера.
        /// Checks if has valid audio mixer.
        /// </summary>
        /// <returns>True если миксер действителен / True if mixer is valid</returns>
        public bool HasValidAudioMixer()
        {
            return audioConfig?.AudioMixer != null;
        }
        #endregion
        
        #region Core Audio Methods
        /// <summary>
        /// Получает аудиоклип по идентификатору.
        /// Gets audio clip by identifier.
        /// </summary>
        /// <param name="id">Идентификатор клипа / Clip identifier</param>
        /// <returns>Аудиоклип / Audio clip</returns>
        public AudioClip GetAudioClip(string id)
        {
            return audioConfig?.GetAudioClip(id);
        }
        
        /// <summary>
        /// Получает данные аудиоклипа по идентификатору.
        /// Gets audio clip data by identifier.
        /// </summary>
        /// <param name="id">Идентификатор клипа / Clip identifier</param>
        /// <returns>Данные клипа / Clip data</returns>
        public AudioClipData GetAudioClipData(string id)
        {
            return audioConfig?.GetAudioClipData(id) ?? default;
        }
        #endregion
        
        #region Type-based Methods
        public List<AudioClipData> GetAudioClipsByType(AudioType audioType)
        {
            return audioConfig?.GetAudioClipsByType(audioType) ?? new List<AudioClipData>();
        }
        
        public List<AudioClipData> GetAudioClipsByCategory(string category)
        {
            return audioConfig?.GetAudioClipsByCategory(category) ?? new List<AudioClipData>();
        }
        
        public List<AudioClipData> GetAudioClipsByTypeAndCategory(AudioType audioType, string category)
        {
            return audioConfig?.GetAudioClipsByTypeAndCategory(audioType, category) ?? new List<AudioClipData>();
        }
        #endregion
        
        #region Random Selection
        public AudioClip GetRandomAudioClip(AudioType audioType, string category = "")
        {
            return audioConfig?.GetRandomAudioClip(audioType, category);
        }
        
        public AudioClipData GetRandomAudioClipData(AudioType audioType, string category = "")
        {
            return audioConfig?.GetRandomAudioClipData(audioType, category) ?? default;
        }
        #endregion
        
        #region Music Convenience Methods
        public AudioClip GetRandomMusicByCategory(string category)
        {
            return GetRandomAudioClip(AudioType.Music, category);
        }
        
        public AudioClip GetMenuMusic()
        {
            return GetRandomMusicByCategory(MENU_CATEGORY);
        }
        
        public AudioClip GetGameplayMusic()
        {
            return GetRandomMusicByCategory(GAMEPLAY_CATEGORY);
        }
        
        public AudioClip GetVictoryMusic()
        {
            return GetRandomMusicByCategory(VICTORY_CATEGORY);
        }
        
        public AudioClip GetDefeatMusic()
        {
            return GetRandomMusicByCategory(DEFEAT_CATEGORY);
        }
        
        public AudioClip GetAmbientMusic()
        {
            return GetRandomMusicByCategory(AMBIENT_CATEGORY);
        }
        #endregion
        
        #region Common Audio IDs
        public AudioClip GetExplosionSound()
        {
            return GetAudioClip(EXPLOSION_ID);
        }
        
        public AudioClip GetShootSound()
        {
            return GetAudioClip("shoot");
        }
        
        public AudioClip GetHitSound()
        {
            return GetAudioClip("hit");
        }
        
        public AudioClip GetPowerUpSound()
        {
            return GetAudioClip("powerup");
        }
        
        public AudioClip GetCoinSound()
        {
            return GetAudioClip("coin");
        }
        
        public AudioClip GetButtonClickSound()
        {
            return GetAudioClip(BUTTON_CLICK_ID);
        }
        
        public AudioClip GetButtonHoverSound()
        {
            return GetAudioClip("button_hover");
        }
        
        public AudioClip GetWindowOpenSound()
        {
            return GetAudioClip("window_open");
        }
        
        public AudioClip GetWindowCloseSound()
        {
            return GetAudioClip("window_close");
        }
        
        public AudioClip GetErrorSound()
        {
            return GetAudioClip(ERROR_ID);
        }
        
        public AudioClip GetSuccessSound()
        {
            return GetAudioClip(SUCCESS_ID);
        }
        #endregion
        
        #region Default Settings
        public float GetDefaultMasterVolume()
        {
            return audioConfig?.DefaultMasterVolume ?? 1f;
        }
        
        public float GetDefaultMusicVolume()
        {
            return audioConfig?.DefaultMusicVolume ?? 0.8f;
        }
        
        public float GetDefaultSFXVolume()
        {
            return audioConfig?.DefaultSFXVolume ?? 1f;
        }
        
        public float GetDefaultUIVolume()
        {
            return audioConfig?.DefaultUIVolume ?? 1f;
        }
        #endregion
        
        #region Music Settings
        public bool ShouldShuffleMusicLists()
        {
            return audioConfig?.ShuffleMusicLists ?? true;
        }
        
        public bool ShouldLoopMusicLists()
        {
            return audioConfig?.LoopMusicLists ?? true;
        }
        #endregion
        
        #region Validation & Info
        public bool HasAudioClip(string id)
        {
            return audioConfig?.HasAudioClip(id) ?? false;
        }
        
        public bool HasAudioInCategory(AudioType audioType, string category)
        {
            return audioConfig?.HasAudioInCategory(audioType, category) ?? false;
        }
        
        public List<string> GetAllCategories(AudioType audioType)
        {
            return audioConfig?.GetAllCategories(audioType) ?? new List<string>();
        }
        
        public List<string> GetAllIds()
        {
            return audioConfig?.GetAllIds() ?? new List<string>();
        }
        #endregion
        
        #region Play with AudioClipData settings
        public AudioSource PlayAudioClipData(AudioClipData clipData, AudioSource audioSource = null)
        {
            if (!clipData.IsValid()) return null;
            
            if (audioSource == null)
            {
                // Create temporary AudioSource
                var tempGO = new GameObject("TempAudioSource");
                audioSource = tempGO.AddComponent<AudioSource>();
                Object.Destroy(tempGO, clipData.AudioClip.length + 0.1f);
            }
            
            audioSource.clip = clipData.AudioClip;
            audioSource.volume = clipData.DefaultVolume;
            audioSource.pitch = clipData.DefaultPitch;
            audioSource.loop = clipData.Loop;
            audioSource.Play();
            
            return audioSource;
        }
        #endregion
    }
}