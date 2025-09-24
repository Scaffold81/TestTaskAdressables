using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

namespace Game.Config
{
    /// <summary>
    /// Интерфейс для работы с конфигурацией аудио.
    /// Interface for working with audio configuration.
    /// </summary>
    public interface IAudioConfigRepository
    {
        // Audio Mixer
        AudioMixer GetAudioMixer();
        
        // Core Audio Methods
        AudioClip GetAudioClip(string id);
        AudioClipData GetAudioClipData(string id);
        
        // Type-based Methods
        List<AudioClipData> GetAudioClipsByType(AudioType audioType);
        List<AudioClipData> GetAudioClipsByCategory(string category);
        List<AudioClipData> GetAudioClipsByTypeAndCategory(AudioType audioType, string category);
        
        // Random Selection
        AudioClip GetRandomAudioClip(AudioType audioType, string category = "");
        AudioClipData GetRandomAudioClipData(AudioType audioType, string category = "");
        
        // Music Convenience Methods
        AudioClip GetRandomMusicByCategory(string category);
        AudioClip GetMenuMusic();
        AudioClip GetGameplayMusic();
        AudioClip GetVictoryMusic();
        AudioClip GetDefeatMusic();
        AudioClip GetAmbientMusic();
        
        // Common Audio IDs (можно настроить под проект)
        AudioClip GetExplosionSound();
        AudioClip GetShootSound();
        AudioClip GetHitSound();
        AudioClip GetPowerUpSound();
        AudioClip GetCoinSound();
        AudioClip GetButtonClickSound();
        AudioClip GetButtonHoverSound();
        AudioClip GetWindowOpenSound();
        AudioClip GetWindowCloseSound();
        AudioClip GetErrorSound();
        AudioClip GetSuccessSound();
        
        // Default Settings
        float GetDefaultMasterVolume();
        float GetDefaultMusicVolume();
        float GetDefaultSFXVolume();
        float GetDefaultUIVolume();
        
        // Music Settings
        bool ShouldShuffleMusicLists();
        bool ShouldLoopMusicLists();
        
        // Validation & Info
        bool HasValidAudioMixer();
        bool HasAudioClip(string id);
        bool HasAudioInCategory(AudioType audioType, string category);
        List<string> GetAllCategories(AudioType audioType);
        List<string> GetAllIds();
        
        // Play with AudioClipData settings
        AudioSource PlayAudioClipData(AudioClipData clipData, AudioSource audioSource = null);
    }
}