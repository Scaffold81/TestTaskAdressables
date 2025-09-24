using UnityEngine;
using UnityEngine.Audio;
using R3;
using System.Collections;
using Game.Config;

namespace Game.Services
{
    /// <summary>
    /// Интерфейс сервиса аудио для управления музыкой, звуковыми эффектами и UI звуками.
    /// Audio service interface for managing music, sound effects and UI sounds.
    /// </summary>
    public interface IAudioService
    {
        // Volume Settings
        float MasterVolume { get; }
        float MusicVolume { get; }
        float SFXVolume { get; }
        float UIVolume { get; }
        
        // Volume Controls
        void SetMasterVolume(float volume);
        void SetMusicVolume(float volume);
        void SetSFXVolume(float volume);
        void SetUIVolume(float volume);
        
        // Volume Observables
        Observable<float> OnMasterVolumeChanged { get; }
        Observable<float> OnMusicVolumeChanged { get; }
        Observable<float> OnSFXVolumeChanged { get; }
        Observable<float> OnUIVolumeChanged { get; }
        
        // Music Controls
        void PlayMusic(AudioClip clip, bool loop = true, float fadeTime = 1f);
        void StopMusic(float fadeTime = 1f);
        void PauseMusic();
        void ResumeMusic();
        void SetMusicPitch(float pitch);
        
        // SFX Controls
        AudioSource PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f);
        AudioSource PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f);
        void StopSFX(AudioSource source);
        void StopAllSFX();
        
        // UI Audio
        void PlayUISound(AudioClip clip, float volume = 1f);
        
        // Global Controls
        void PauseAll();
        void ResumeAll();
        void StopAll();
        bool IsMuted { get; }
        void SetMute(bool muted);
        
        // Audio Mixer
        void SetAudioMixerParameter(string parameterName, float value);
        float GetAudioMixerParameter(string parameterName);
        void SetAudioMixer(AudioMixer mixer);
    }
}