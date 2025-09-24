using UnityEngine;
using UnityEngine.Audio;
using R3;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Zenject;
using System;
using System.Linq;
using Game.Config;
using System.Threading;

namespace Game.Services
{
    /// <summary>
    /// Сервис для управления аудио. Поддерживает музыку, звуковые эффекты и UI звуки с пулами объектов.
    /// Audio management service. Supports music, sound effects and UI sounds with object pooling.
    /// </summary>
    public class AudioService : IAudioService, IDisposable
    {
        // Dependencies
        private readonly IObjectPoolService poolService;
        private readonly ISaveService saveService;
        private readonly IAudioConfigRepository audioConfigRepository;
        
        // Audio Mixer (from config)
        private AudioMixer audioMixer;
        
        // Settings
        private readonly int sfxPoolSize = 10;
        private readonly int uiPoolSize = 5;
        private readonly int maxSFXSources = 20;
        
        // Audio Sources
        private AudioSource musicSource;
        private AudioSource secondaryMusicSource;
        private GameObject audioContainer;
        
        // Volume Properties
        private readonly ReactiveProperty<float> masterVolume = new(1f);
        private readonly ReactiveProperty<float> musicVolume = new(1f);
        private readonly ReactiveProperty<float> sfxVolume = new(1f);
        private readonly ReactiveProperty<float> uiVolume = new(1f);
        
        // State
        private bool isMuted = false;
        private bool isPaused = false;
        private readonly List<AudioSource> activeSFXSources = new();
        private CancellationTokenSource serviceCancellationToken;
        private CancellationTokenSource musicFadeCancellation;
        
        // Save Keys
        private const string MASTER_VOLUME_KEY = "Audio_MasterVolume";
        private const string MUSIC_VOLUME_KEY = "Audio_MusicVolume";
        private const string SFX_VOLUME_KEY = "Audio_SFXVolume";
        private const string UI_VOLUME_KEY = "Audio_UIVolume";
        private const string MUTE_KEY = "Audio_IsMuted";
        
        // Pool Names
        private const string SFX_POOL = "SFX_AudioSource_Pool";
        private const string UI_POOL = "UI_AudioSource_Pool";
        
        // Public Properties
        public float MasterVolume => masterVolume.Value;
        public float MusicVolume => musicVolume.Value;
        public float SFXVolume => sfxVolume.Value;
        public float UIVolume => uiVolume.Value;
        public bool IsMuted => isMuted;
        
        // Observables
        public Observable<float> OnMasterVolumeChanged => masterVolume.AsObservable();
        public Observable<float> OnMusicVolumeChanged => musicVolume.AsObservable();
        public Observable<float> OnSFXVolumeChanged => sfxVolume.AsObservable();
        public Observable<float> OnUIVolumeChanged => uiVolume.AsObservable();
        
        /// <summary>
        /// Конструктор с инъекцией зависимостей.
        /// Constructor with dependency injection.
        /// </summary>
        /// <param name="poolService">Сервис пулов объектов / Object pool service</param>
        /// <param name="saveService">Сервис сохранения / Save service</param>
        /// <param name="audioConfigRepository">Репозиторий конфигурации аудио / Audio config repository</param>
        [Inject]
        public AudioService(
            IObjectPoolService poolService, 
            ISaveService saveService, 
            IAudioConfigRepository audioConfigRepository)
        {
            this.poolService = poolService;
            this.saveService = saveService;
            this.audioConfigRepository = audioConfigRepository;
            
            serviceCancellationToken = new CancellationTokenSource();
            
            Initialize();
        }
        
        /// <summary>
        /// Инициализирует аудио сервис.
        /// Initializes audio service.
        /// </summary>
        private void Initialize()
        {
            LoadConfigFromRepository();
            CreateAudioContainer();
            InitializeAudioSources();
            InitializeAudioPools();
            LoadAudioSettings();
            SubscribeToVolumeChanges();
            
            // Audio service initialized successfully
        }
        
        private void LoadConfigFromRepository()
        {
            // Load audio mixer from config
            audioMixer = audioConfigRepository.GetAudioMixer();
            
            if (audioMixer == null)
            {
                Debug.LogWarning("[AudioService] No Audio Mixer found in config. Audio will work with direct volume control.");
            }
            else
            {
            // Audio Mixer loaded from config. Using for effects only - volume controlled directly.
            }
        }
        
        private void CreateAudioContainer()
        {
            audioContainer = new GameObject("AudioService");
            UnityEngine.Object.DontDestroyOnLoad(audioContainer);
        }
        
        private void InitializeAudioSources()
        {
            // Create music audio sources
            musicSource = audioContainer.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            
            secondaryMusicSource = audioContainer.AddComponent<AudioSource>();
            secondaryMusicSource.playOnAwake = false;
            secondaryMusicSource.loop = true;
            secondaryMusicSource.volume = 0f;
            
            // Set output to mixer if assigned
            if (audioMixer != null)
            {
                musicSource.outputAudioMixerGroup = audioMixer.outputAudioMixerGroup;
                secondaryMusicSource.outputAudioMixerGroup = audioMixer.outputAudioMixerGroup;
            }
        }
        
        private void InitializeAudioPools()
        {
            // Create SFX pool
            poolService.CreatePool<AudioSource>(SFX_POOL, sfxPoolSize, maxSFXSources);
            
            // Create UI pool
            poolService.CreatePool<AudioSource>(UI_POOL, uiPoolSize, uiPoolSize);
            
            // Setup audio sources in pools
            SetupPoolAudioSources(SFX_POOL);
            SetupPoolAudioSources(UI_POOL);
            
            // Created audio pools
        }
        
        private void SetupPoolAudioSources(string poolName)
        {
            // Warmup pool to configure audio sources
            poolService.WarmupPool(poolName, poolService.GetPoolSize(poolName));
            
            // Configure all audio sources in the pool
            for (int i = 0; i < poolService.GetInactiveCount(poolName); i++)
            {
                var source = poolService.Get<AudioSource>(poolName);
                ConfigureAudioSource(source);
                poolService.Return(source);
            }
        }
        
        private void ConfigureAudioSource(AudioSource source)
        {
            source.playOnAwake = false;
            source.spatialBlend = 0f; // All sources 2D by default
            
            // Set output to mixer if assigned
            if (audioMixer != null)
            {
                source.outputAudioMixerGroup = audioMixer.outputAudioMixerGroup;
            }
        }
        
        private void LoadAudioSettings()
        {
            // Load from save first, fallback to config defaults
            masterVolume.Value = ParseFloat(saveService.Load(MASTER_VOLUME_KEY), audioConfigRepository.GetDefaultMasterVolume());
            musicVolume.Value = ParseFloat(saveService.Load(MUSIC_VOLUME_KEY), audioConfigRepository.GetDefaultMusicVolume());
            sfxVolume.Value = ParseFloat(saveService.Load(SFX_VOLUME_KEY), audioConfigRepository.GetDefaultSFXVolume());
            uiVolume.Value = ParseFloat(saveService.Load(UI_VOLUME_KEY), audioConfigRepository.GetDefaultUIVolume());
            isMuted = ParseBool(saveService.Load(MUTE_KEY), false);
            
            ApplyVolumeSettings();
            // Audio settings loaded from save with config defaults as fallback
        }
        
        private float ParseFloat(string value, float defaultValue)
        {
            return float.TryParse(value, out float result) ? result : defaultValue;
        }
        
        private bool ParseBool(string value, bool defaultValue)
        {
            return bool.TryParse(value, out bool result) ? result : defaultValue;
        }
        
        private void SubscribeToVolumeChanges()
        {
            masterVolume.Subscribe(_ => { ApplyVolumeSettings(); SaveAudioSettings(); });
            musicVolume.Subscribe(_ => { ApplyVolumeSettings(); SaveAudioSettings(); });
            sfxVolume.Subscribe(_ => { ApplyVolumeSettings(); SaveAudioSettings(); });
            uiVolume.Subscribe(_ => { ApplyVolumeSettings(); SaveAudioSettings(); });
        }
        
        private void ApplyVolumeSettings()
        {
            // Calculate final volume
            float finalMasterVolume = masterVolume.Value * (isMuted ? 0f : 1f);
            
            // Apply volumes directly to music sources for immediate control
            float musicVolumeMultiplier = musicVolume.Value;
            if (musicSource != null) 
                musicSource.volume = finalMasterVolume * musicVolumeMultiplier;
            if (secondaryMusicSource != null) 
                secondaryMusicSource.volume = finalMasterVolume * musicVolumeMultiplier;
                
            // Note: SFX and UI volumes are applied directly when playing sounds
        }
        
        private void SaveAudioSettings()
        {
            saveService.Save(MASTER_VOLUME_KEY, masterVolume.Value.ToString());
            saveService.Save(MUSIC_VOLUME_KEY, musicVolume.Value.ToString());
            saveService.Save(SFX_VOLUME_KEY, sfxVolume.Value.ToString());
            saveService.Save(UI_VOLUME_KEY, uiVolume.Value.ToString());
            saveService.Save(MUTE_KEY, isMuted.ToString());
        }
        
        public void SetAudioMixer(AudioMixer mixer)
        {
            audioMixer = mixer;
            
            // Update all audio sources to use new mixer for effects
            if (musicSource != null)
                musicSource.outputAudioMixerGroup = mixer?.outputAudioMixerGroup;
            if (secondaryMusicSource != null)
                secondaryMusicSource.outputAudioMixerGroup = mixer?.outputAudioMixerGroup;
                
            // Volume is still controlled directly, mixer is for effects only
            ApplyVolumeSettings();
            // Audio mixer updated - using for effects only
        }
        
        #region Volume Controls
        /// <summary>
        /// Устанавливает общую громкость.
        /// Sets master volume.
        /// </summary>
        /// <param name="volume">Значение громкости (0-1) / Volume value (0-1)</param>
        public void SetMasterVolume(float volume)
        {
            masterVolume.Value = Mathf.Clamp01(volume);
        }
        
        /// <summary>
        /// Устанавливает громкость музыки.
        /// Sets music volume.
        /// </summary>
        /// <param name="volume">Значение громкости (0-1) / Volume value (0-1)</param>
        public void SetMusicVolume(float volume)
        {
            musicVolume.Value = Mathf.Clamp01(volume);
        }
        
        /// <summary>
        /// Устанавливает громкость звуковых эффектов.
        /// Sets sound effects volume.
        /// </summary>
        /// <param name="volume">Значение громкости (0-1) / Volume value (0-1)</param>
        public void SetSFXVolume(float volume)
        {
            sfxVolume.Value = Mathf.Clamp01(volume);
        }
        
        /// <summary>
        /// Устанавливает громкость UI звуков.
        /// Sets UI sounds volume.
        /// </summary>
        /// <param name="volume">Значение громкости (0-1) / Volume value (0-1)</param>
        public void SetUIVolume(float volume)
        {
            uiVolume.Value = Mathf.Clamp01(volume);
        }
        
        /// <summary>
        /// Включает или выключает звук.
        /// Enables or disables sound.
        /// </summary>
        /// <param name="muted">True для отключения звука / True to mute sound</param>
        public void SetMute(bool muted)
        {
            isMuted = muted;
            ApplyVolumeSettings();
            SaveAudioSettings();
            // Audio muted/unmuted
        }
        #endregion
        
        #region Convenience Methods (Config-based)
        // Music from config - now with ID-based system
        /// <summary>
        /// Воспроизводит музыку меню с затуханием.
        /// Plays menu music with fade.
        /// </summary>
        /// <param name="fadeTime">Время затухания в секундах / Fade time in seconds</param>
        public void PlayMenuMusic(float fadeTime = 1f)
        {
            var clip = audioConfigRepository.GetMenuMusic();
            if (clip != null) PlayMusic(clip, true, fadeTime);
        }
        
        /// <summary>
        /// Воспроизводит игровую музыку с затуханием.
        /// Plays gameplay music with fade.
        /// </summary>
        /// <param name="fadeTime">Время затухания в секундах / Fade time in seconds</param>
        public void PlayGameplayMusic(float fadeTime = 1f)
        {
            var clip = audioConfigRepository.GetGameplayMusic();
            if (clip != null) PlayMusic(clip, true, fadeTime);
        }
        
        /// <summary>
        /// Воспроизводит музыку победы с затуханием.
        /// Plays victory music with fade.
        /// </summary>
        /// <param name="fadeTime">Время затухания в секундах / Fade time in seconds</param>
        public void PlayVictoryMusic(float fadeTime = 1f)
        {
            var clip = audioConfigRepository.GetVictoryMusic();
            if (clip != null) PlayMusic(clip, false, fadeTime);
        }
        
        /// <summary>
        /// Воспроизводит музыку поражения с затуханием.
        /// Plays defeat music with fade.
        /// </summary>
        /// <param name="fadeTime">Время затухания в секундах / Fade time in seconds</param>
        public void PlayDefeatMusic(float fadeTime = 1f)
        {
            var clip = audioConfigRepository.GetDefeatMusic();
            if (clip != null) PlayMusic(clip, false, fadeTime);
        }
        
        /// <summary>
        /// Воспроизводит фоновую музыку с затуханием.
        /// Plays ambient music with fade.
        /// </summary>
        /// <param name="fadeTime">Время затухания в секундах / Fade time in seconds</param>
        public void PlayAmbientMusic(float fadeTime = 1f)
        {
            var clip = audioConfigRepository.GetAmbientMusic();
            if (clip != null) PlayMusic(clip, true, fadeTime);
        }
        
        // Play by ID
        /// <summary>
        /// Воспроизводит музыку по идентификатору.
        /// Plays music by identifier.
        /// </summary>
        /// <param name="id">Идентификатор музыкального клипа / Music clip identifier</param>
        /// <param name="loop">Зацикливать воспроизведение / Loop playback</param>
        /// <param name="fadeTime">Время затухания / Fade time</param>
        public void PlayMusicById(string id, bool loop = true, float fadeTime = 1f)
        {
            var clip = audioConfigRepository.GetAudioClip(id);
            if (clip != null) PlayMusic(clip, loop, fadeTime);
        }
        
        /// <summary>
        /// Воспроизводит звуковой эффект по идентификатору.
        /// Plays sound effect by identifier.
        /// </summary>
        /// <param name="id">Идентификатор звукового эффекта / Sound effect identifier</param>
        /// <param name="volume">Громкость / Volume</param>
        /// <param name="pitch">Высота тона / Pitch</param>
        /// <returns>Источник аудио / Audio source</returns>
        public AudioSource PlaySFXById(string id, float volume = 1f, float pitch = 1f)
        {
            var clip = audioConfigRepository.GetAudioClip(id);
            return clip != null ? PlaySFX(clip, volume, pitch) : null;
        }
        
        /// <summary>
        /// Воспроизводит UI звук по идентификатору.
        /// Plays UI sound by identifier.
        /// </summary>
        /// <param name="id">Идентификатор UI звука / UI sound identifier</param>
        /// <param name="volume">Громкость / Volume</param>
        public void PlayUISoundById(string id, float volume = 1f)
        {
            var clip = audioConfigRepository.GetAudioClip(id);
            if (clip != null) PlayUISound(clip, volume);
        }
        
        // Play with AudioClipData settings
        /// <summary>
        /// Воспроизводит аудиоклип с настройками из AudioClipData.
        /// Plays audio clip with settings from AudioClipData.
        /// </summary>
        /// <param name="id">Идентификатор клипа / Clip identifier</param>
        /// <returns>Источник аудио / Audio source</returns>
        public AudioSource PlayAudioClipData(string id)
        {
            var clipData = audioConfigRepository.GetAudioClipData(id);
            if (!clipData.IsValid()) return null;
            
            return clipData.AudioType switch
            {
                Game.Config.AudioType.Music => PlayMusicWithData(clipData),
                Game.Config.AudioType.SFX => PlaySFXWithData(clipData),
                Game.Config.AudioType.UI => PlayUIWithData(clipData),
                Game.Config.AudioType.Voice => PlaySFXWithData(clipData), // Voice как SFX
                Game.Config.AudioType.Ambient => PlayMusicWithData(clipData), // Ambient как Music
                _ => null
            };
        }
        
        private AudioSource PlayMusicWithData(AudioClipData clipData)
        {
            PlayMusic(clipData.AudioClip, clipData.Loop);
            return musicSource.isPlaying ? musicSource : secondaryMusicSource;
        }
        
        private AudioSource PlaySFXWithData(AudioClipData clipData)
        {
            return PlaySFX(clipData.AudioClip, clipData.DefaultVolume, clipData.DefaultPitch);
        }
        
        private AudioSource PlayUIWithData(AudioClipData clipData)
        {
            PlayUISound(clipData.AudioClip, clipData.DefaultVolume);
            return null; // UI sounds не возвращают source
        }
        
        // SFX from config
        /// <summary>
        /// Воспроизводит звук взрыва.
        /// Plays explosion sound.
        /// </summary>
        /// <param name="volume">Громкость / Volume</param>
        /// <param name="pitch">Высота тона / Pitch</param>
        /// <returns>Источник аудио / Audio source</returns>
        public AudioSource PlayExplosionSFX(float volume = 1f, float pitch = 1f)
        {
            var clip = audioConfigRepository.GetExplosionSound();
            return clip != null ? PlaySFX(clip, volume, pitch) : null;
        }
        
        /// <summary>
        /// Воспроизводит звук выстрела.
        /// Plays shoot sound.
        /// </summary>
        /// <param name="volume">Громкость / Volume</param>
        /// <param name="pitch">Высота тона / Pitch</param>
        /// <returns>Источник аудио / Audio source</returns>
        public AudioSource PlayShootSFX(float volume = 1f, float pitch = 1f)
        {
            var clip = audioConfigRepository.GetShootSound();
            return clip != null ? PlaySFX(clip, volume, pitch) : null;
        }
        
        /// <summary>
        /// Воспроизводит звук удара.
        /// Plays hit sound.
        /// </summary>
        /// <param name="volume">Громкость / Volume</param>
        /// <param name="pitch">Высота тона / Pitch</param>
        /// <returns>Источник аудио / Audio source</returns>
        public AudioSource PlayHitSFX(float volume = 1f, float pitch = 1f)
        {
            var clip = audioConfigRepository.GetHitSound();
            return clip != null ? PlaySFX(clip, volume, pitch) : null;
        }
        
        /// <summary>
        /// Воспроизводит звук бонуса.
        /// Plays power up sound.
        /// </summary>
        /// <param name="volume">Громкость / Volume</param>
        /// <param name="pitch">Высота тона / Pitch</param>
        /// <returns>Источник аудио / Audio source</returns>
        public AudioSource PlayPowerUpSFX(float volume = 1f, float pitch = 1f)
        {
            var clip = audioConfigRepository.GetPowerUpSound();
            return clip != null ? PlaySFX(clip, volume, pitch) : null;
        }
        
        /// <summary>
        /// Воспроизводит звук монеты.
        /// Plays coin sound.
        /// </summary>
        /// <param name="volume">Громкость / Volume</param>
        /// <param name="pitch">Высота тона / Pitch</param>
        /// <returns>Источник аудио / Audio source</returns>
        public AudioSource PlayCoinSFX(float volume = 1f, float pitch = 1f)
        {
            var clip = audioConfigRepository.GetCoinSound();
            return clip != null ? PlaySFX(clip, volume, pitch) : null;
        }
        
        // UI Sounds from config
        /// <summary>
        /// Воспроизводит звук клика кнопки.
        /// Plays button click sound.
        /// </summary>
        /// <param name="volume">Громкость / Volume</param>
        public void PlayButtonClickUI(float volume = 1f)
        {
            var clip = audioConfigRepository.GetButtonClickSound();
            if (clip != null) PlayUISound(clip, volume);
        }
        
        /// <summary>
        /// Воспроизводит звук наведения на кнопку.
        /// Plays button hover sound.
        /// </summary>
        /// <param name="volume">Громкость / Volume</param>
        public void PlayButtonHoverUI(float volume = 1f)
        {
            var clip = audioConfigRepository.GetButtonHoverSound();
            if (clip != null) PlayUISound(clip, volume);
        }
        
        /// <summary>
        /// Воспроизводит звук открытия окна.
        /// Plays window open sound.
        /// </summary>
        /// <param name="volume">Громкость / Volume</param>
        public void PlayWindowOpenUI(float volume = 1f)
        {
            var clip = audioConfigRepository.GetWindowOpenSound();
            if (clip != null) PlayUISound(clip, volume);
        }
        
        /// <summary>
        /// Воспроизводит звук закрытия окна.
        /// Plays window close sound.
        /// </summary>
        /// <param name="volume">Громкость / Volume</param>
        public void PlayWindowCloseUI(float volume = 1f)
        {
            var clip = audioConfigRepository.GetWindowCloseSound();
            if (clip != null) PlayUISound(clip, volume);
        }
        
        /// <summary>
        /// Воспроизводит звук ошибки.
        /// Plays error sound.
        /// </summary>
        /// <param name="volume">Громкость / Volume</param>
        public void PlayErrorUI(float volume = 1f)
        {
            var clip = audioConfigRepository.GetErrorSound();
            if (clip != null) PlayUISound(clip, volume);
        }
        
        /// <summary>
        /// Воспроизводит звук успеха.
        /// Plays success sound.
        /// </summary>
        /// <param name="volume">Громкость / Volume</param>
        public void PlaySuccessUI(float volume = 1f)
        {
            var clip = audioConfigRepository.GetSuccessSound();
            if (clip != null) PlayUISound(clip, volume);
        }
        #endregion
        
        #region Music Controls
        /// <summary>
        /// Воспроизводит музыку с затуханием.
        /// Plays music with fade.
        /// </summary>
        /// <param name="clip">Аудиоклип / Audio clip</param>
        /// <param name="loop">Зацикливать / Loop</param>
        /// <param name="fadeTime">Время затухания / Fade time</param>
        public void PlayMusic(AudioClip clip, bool loop = true, float fadeTime = 1f)
        {
            if (clip == null) return;
            
            if (musicFadeCancellation != null)
            {
                musicFadeCancellation.Cancel();
                musicFadeCancellation.Dispose();
            }
            
            musicFadeCancellation = new CancellationTokenSource();
            FadeToNewMusicAsync(clip, loop, fadeTime, musicFadeCancellation.Token).Forget();
        }
        
        /// <summary>
        /// Останавливает музыку с затуханием.
        /// Stops music with fade.
        /// </summary>
        /// <param name="fadeTime">Время затухания / Fade time</param>
        public void StopMusic(float fadeTime = 1f)
        {
            if (musicFadeCancellation != null)
            {
                musicFadeCancellation.Cancel();
                musicFadeCancellation.Dispose();
            }
            
            musicFadeCancellation = new CancellationTokenSource();
            FadeOutMusicAsync(fadeTime, musicFadeCancellation.Token).Forget();
        }
        
        /// <summary>
        /// Приостанавливает музыку.
        /// Pauses music.
        /// </summary>
        public void PauseMusic()
        {
            musicSource.Pause();
            secondaryMusicSource.Pause();
        }
        
        /// <summary>
        /// Возобновляет музыку.
        /// Resumes music.
        /// </summary>
        public void ResumeMusic()
        {
            if (!isPaused)
            {
                musicSource.UnPause();
                secondaryMusicSource.UnPause();
            }
        }
        
        /// <summary>
        /// Устанавливает высоту тона музыки.
        /// Sets music pitch.
        /// </summary>
        /// <param name="pitch">Высота тона / Pitch</param>
        public void SetMusicPitch(float pitch)
        {
            musicSource.pitch = pitch;
            secondaryMusicSource.pitch = pitch;
        }
        
        private async UniTask FadeToNewMusicAsync(AudioClip newClip, bool loop, float fadeTime, CancellationToken cancellationToken)
        {
            try
            {
                AudioSource fadeInSource = musicSource.isPlaying ? secondaryMusicSource : musicSource;
                AudioSource fadeOutSource = musicSource.isPlaying ? musicSource : secondaryMusicSource;
                
                fadeInSource.clip = newClip;
                fadeInSource.loop = loop;
                fadeInSource.volume = 0f;
                fadeInSource.Play();
                
                float startVolume = fadeOutSource.volume;
                float targetVolume = masterVolume.Value * musicVolume.Value * (isMuted ? 0f : 1f);
                
                var fadeTimeMs = (int)(fadeTime * 1000);
                var startTime = Time.time;
                
                while (Time.time - startTime < fadeTime)
                {
                    if (cancellationToken.IsCancellationRequested) return;
                    
                    float progress = (Time.time - startTime) / fadeTime;
                    
                    fadeOutSource.volume = Mathf.Lerp(startVolume, 0f, progress);
                    fadeInSource.volume = Mathf.Lerp(0f, targetVolume, progress);
                    
                    await UniTask.NextFrame(cancellationToken);
                }
                
                fadeOutSource.Stop();
                fadeOutSource.volume = 0f;
                fadeInSource.volume = targetVolume;
            }
            catch (OperationCanceledException)
            {
                // Task was cancelled, this is expected
            }
            finally
            {
                if (musicFadeCancellation != null)
                {
                    musicFadeCancellation.Dispose();
                    musicFadeCancellation = null;
                }
            }
        }
        
        private async UniTask FadeOutMusicAsync(float fadeTime, CancellationToken cancellationToken)
        {
            try
            {
                AudioSource activeSource = musicSource.isPlaying ? musicSource : secondaryMusicSource;
                float startVolume = activeSource.volume;
                var startTime = Time.time;
                
                while (Time.time - startTime < fadeTime)
                {
                    if (cancellationToken.IsCancellationRequested) return;
                    
                    float progress = (Time.time - startTime) / fadeTime;
                    activeSource.volume = Mathf.Lerp(startVolume, 0f, progress);
                    
                    await UniTask.NextFrame(cancellationToken);
                }
                
                activeSource.Stop();
                activeSource.volume = 0f;
            }
            catch (OperationCanceledException)
            {
                // Task was cancelled, this is expected
            }
            finally
            {
                if (musicFadeCancellation != null)
                {
                    musicFadeCancellation.Dispose();
                    musicFadeCancellation = null;
                }
            }
        }
        #endregion
        
        #region SFX Controls
        /// <summary>
        /// Воспроизводит звуковой эффект.
        /// Plays sound effect.
        /// </summary>
        /// <param name="clip">Аудиоклип / Audio clip</param>
        /// <param name="volume">Громкость / Volume</param>
        /// <param name="pitch">Высота тона / Pitch</param>
        /// <returns>Источник аудио / Audio source</returns>
        public AudioSource PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null) return null;
            
            var source = poolService.Get<AudioSource>(SFX_POOL);
            if (source == null) return null;
            
            source.clip = clip;
            source.volume = volume * sfxVolume.Value * masterVolume.Value * (isMuted ? 0f : 1f);
            source.pitch = pitch;
            source.spatialBlend = 0f; // 2D sound
            source.Play();
            
            activeSFXSources.Add(source);
            ReturnSFXSourceWhenFinishedAsync(source, clip.length / pitch).Forget();
            
            return source;
        }
        
        /// <summary>
        /// Воспроизводит звуковой эффект в определенной позиции (3D).
        /// Plays sound effect at specific position (3D).
        /// </summary>
        /// <param name="clip">Аудиоклип / Audio clip</param>
        /// <param name="position">Позиция в пространстве / Position in space</param>
        /// <param name="volume">Громкость / Volume</param>
        /// <param name="pitch">Высота тона / Pitch</param>
        /// <returns>Источник аудио / Audio source</returns>
        public AudioSource PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
        {
            if (clip == null) return null;
            
            var source = poolService.Get<AudioSource>(SFX_POOL);
            if (source == null) return null;
            
            source.transform.position = position;
            source.clip = clip;
            source.volume = volume * sfxVolume.Value * masterVolume.Value * (isMuted ? 0f : 1f);
            source.pitch = pitch;
            source.spatialBlend = 1f; // 3D sound
            source.Play();
            
            activeSFXSources.Add(source);
            ReturnSFXSourceWhenFinishedAsync(source, clip.length / pitch).Forget();
            
            return source;
        }
        
        /// <summary>
        /// Останавливает звуковой эффект.
        /// Stops sound effect.
        /// </summary>
        /// <param name="source">Источник аудио / Audio source</param>
        public void StopSFX(AudioSource source)
        {
            if (source != null && activeSFXSources.Contains(source))
            {
                source.Stop();
                activeSFXSources.Remove(source);
                poolService.Return(source);
            }
        }
        
        /// <summary>
        /// Останавливает все звуковые эффекты.
        /// Stops all sound effects.
        /// </summary>
        public void StopAllSFX()
        {
            foreach (var source in activeSFXSources.ToArray())
            {
                StopSFX(source);
            }
        }
        
        private async UniTask ReturnSFXSourceWhenFinishedAsync(AudioSource source, float duration)
        {
            try
            {
                await UniTask.Delay((int)(duration * 1000 + 100), cancellationToken: serviceCancellationToken.Token);
                
                if (source != null && activeSFXSources.Contains(source))
                {
                    activeSFXSources.Remove(source);
                    poolService.Return(source);
                }
            }
            catch (OperationCanceledException)
            {
                // Task was cancelled, this is expected
            }
        }
        #endregion
        
        #region UI Audio
        /// <summary>
        /// Воспроизводит UI звук.
        /// Plays UI sound.
        /// </summary>
        /// <param name="clip">Аудиоклип / Audio clip</param>
        /// <param name="volume">Громкость / Volume</param>
        public void PlayUISound(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            
            var source = poolService.Get<AudioSource>(UI_POOL);
            if (source == null) return;
            
            source.clip = clip;
            source.volume = volume * uiVolume.Value * masterVolume.Value * (isMuted ? 0f : 1f);
            source.pitch = 1f;
            source.spatialBlend = 0f; // Always 2D
            source.Play();
            
            ReturnUISourceWhenFinishedAsync(source, clip.length).Forget();
        }
        
        private async UniTask ReturnUISourceWhenFinishedAsync(AudioSource source, float duration)
        {
            try
            {
                await UniTask.Delay((int)(duration * 1000 + 100), cancellationToken: serviceCancellationToken.Token);
                
                if (source != null)
                {
                    poolService.Return(source);
                }
            }
            catch (OperationCanceledException)
            {
                // Task was cancelled, this is expected
            }
        }
        #endregion
        
        #region Global Controls
        /// <summary>
        /// Приостанавливает все аудио.
        /// Pauses all audio.
        /// </summary>
        public void PauseAll()
        {
            isPaused = true;
            PauseMusic();
            
            foreach (var source in activeSFXSources)
            {
                if (source != null && source.isPlaying)
                {
                    source.Pause();
                }
            }
        }
        
        /// <summary>
        /// Возобновляет все аудио.
        /// Resumes all audio.
        /// </summary>
        public void ResumeAll()
        {
            isPaused = false;
            ResumeMusic();
            
            foreach (var source in activeSFXSources)
            {
                if (source != null)
                {
                    source.UnPause();
                }
            }
        }
        
        /// <summary>
        /// Останавливает все аудио.
        /// Stops all audio.
        /// </summary>
        public void StopAll()
        {
            StopMusic(0f);
            StopAllSFX();
        }
        #endregion
        
        #region Audio Mixer
        /// <summary>
        /// Устанавливает параметр аудио миксера.
        /// Sets audio mixer parameter.
        /// </summary>
        /// <param name="parameterName">Имя параметра / Parameter name</param>
        /// <param name="value">Значение / Value</param>
        public void SetAudioMixerParameter(string parameterName, float value)
        {
            if (audioMixer != null)
            {
                try
                {
                    audioMixer.SetFloat(parameterName, value);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[AudioService] Failed to set mixer parameter '{parameterName}': {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// Получает параметр аудио миксера.
        /// Gets audio mixer parameter.
        /// </summary>
        /// <param name="parameterName">Имя параметра / Parameter name</param>
        /// <returns>Значение параметра / Parameter value</returns>
        public float GetAudioMixerParameter(string parameterName)
        {
            if (audioMixer != null)
            {
                try
                {
                    if (audioMixer.GetFloat(parameterName, out float value))
                    {
                        return value;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[AudioService] Failed to get mixer parameter '{parameterName}': {ex.Message}");
                }
            }
            return 0f;
        }
        #endregion
        
        /// <summary>
        /// Освобождает ресурсы аудио сервиса.
        /// Disposes audio service resources.
        /// </summary>
        public void Dispose()
        {
            // Cancel all running tasks
            serviceCancellationToken?.Cancel();
            serviceCancellationToken?.Dispose();
            
            musicFadeCancellation?.Cancel();
            musicFadeCancellation?.Dispose();
            
            // Dispose reactive properties
            masterVolume?.Dispose();
            musicVolume?.Dispose();
            sfxVolume?.Dispose();
            uiVolume?.Dispose();
            
            // Destroy audio container
            if (audioContainer != null)
            {
                UnityEngine.Object.Destroy(audioContainer);
            }
        }
    }
}